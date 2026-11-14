// Turns the JSON models under tools/model into a WebSharper WIG assembly.
//
// Nothing here is CapGo-specific: a package is described by its root namespace,
// its root class and the plugin models it contains.
module CapGoGeneratorCommon.Generator

open System.Collections.Generic
open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator
open CapGoGeneratorCommon.Model

type Package =
    {
        /// e.g. "WebSharper.Capacitor.CapGo"
        RootNamespace: string
        /// e.g. "CapacitorCapGo"
        RootClass: string
        Plugins: Plugin list
    }

// --- type ordering --------------------------------------------------------

let rec private refsOf (t: MType) =
    match t with
    | TRef n -> [ n ]
    | TArray x
    | TMap x -> refsOf x
    | TUnion xs -> List.collect refsOf xs
    | TFunc (args, ret) -> List.collect refsOf (ret :: args)
    | TPromise x -> refsOf x
    | TExt _ -> []
    | _ -> []

let private depsOf (d: TypeDef) =
    match d.Shape with
    | Config (required, optional) ->
        required @ optional
        |> List.collect (fun m -> refsOf m.Type)
        |> List.filter (fun n -> n <> d.Name)
        |> List.distinct
    | UnionOf ns -> ns
    | Map v -> refsOf v
    | Strings _
    | Numbers _ -> []

/// Topological order; definitions in a dependency cycle are emitted last (their
/// refs still resolve because every type is registered before use only where
/// possible — a real cycle would fail loudly at build time).
let private orderTypes (defs: TypeDef list) =
    let known = defs |> List.map (fun d -> d.Name) |> Set.ofList
    let rec loop doneNames remaining =
        match remaining with
        | [] -> doneNames
        | _ ->
            let ready, blocked =
                remaining
                |> List.partition (fun d ->
                    depsOf d |> List.forall (fun n -> not (Set.contains n known) || List.contains n doneNames)
                )
            match ready with
            | [] -> doneNames @ (blocked |> List.map (fun d -> d.Name))
            | _ -> loop (doneNames @ (ready |> List.map (fun d -> d.Name))) blocked
    loop [] defs

// --- WIG construction -----------------------------------------------------

/// DOM types that WebSharper.JavaScript already models; anything else that comes
/// from outside the plugin package falls back to `obj` (and is logged).
let private knownExternal =
    dict [
        "Blob", T<Blob>
        "Uint8Array", T<Uint8Array>
        "HTMLImageElement", T<HTMLImageElement>
        "HTMLVideoElement", T<HTMLVideoElement>
        "File", T<File>
        "Worker", T<Worker>
    ]

let rec private conv (types: Dictionary<string, Type.Type>) (t: MType) : Type.Type =
    match t with
    | TString -> T<string>
    | TFloat -> T<float>
    | TBool -> T<bool>
    | TUnit -> T<unit>
    | TObj -> T<obj>
    | TRef n ->
        match types.TryGetValue n with
        | true, x -> x
        | _ ->
            // Only reachable for recursive type graphs, where the back-edge is
            // cut. Surface it instead of silently emitting a wrong shape.
            printfn "capgo-generator: cyclic reference '%s' falls back to obj" n
            T<obj>
    | TArray x -> !| (conv types x)
    | TMap _ -> T<obj>
    | TUnion [] -> T<unit>
    | TUnion (x :: xs) ->
        xs
        |> List.fold (fun acc y -> acc + (conv types y)) (conv types x)
    | TPromise x -> T<Promise<_>>[ conv types x ]
    | TFunc ([], ret) -> T<unit> ^-> conv types ret
    | TFunc (args, ret) ->
        let ps : Type.Parameters =
            {
                This = None
                Arguments =
                    [
                        for a in args ->
                            { Type.Parameter.Name = None; Optional = false; Type = conv types a }
                    ]
                Variable = None
            }
        (ps :> Type.IParameters) ^-> conv types ret
    | TExt n ->
        match knownExternal.TryGetValue n with
        | true, x -> x
        | _ ->
            printfn "capgo-generator: external type '%s' falls back to obj" n
            T<obj>

let private mkParameters (types: Dictionary<string, Type.Type>) (ps: Param list) : Type.Parameters =
    {
        This = None
        Arguments =
            [
                for p in ps ->
                    {
                        Type.Parameter.Name = Some(if System.String.IsNullOrWhiteSpace p.Name then "arg" else p.Name)
                        Optional = p.Optional
                        Type = conv types p.Type
                    }
            ]
        Variable = None
    }

/// A JS string literal.
let private quote (s: string) =
    let b = System.Text.StringBuilder("\"")
    for ch in s do
        match ch with
        | '"' -> b.Append("\\\"") |> ignore
        | '\\' -> b.Append("\\\\") |> ignore
        | '\n' -> b.Append("\\n") |> ignore
        | '\r' -> b.Append("\\r") |> ignore
        | '\t' -> b.Append("\\t") |> ignore
        | c -> b.Append(c) |> ignore
    b.Append("\"").ToString()

/// Turns raw string cases into unique, valid member names. CapGo enums repeat
/// values with different casing ("VISA"/"visa") and use "" as a value.
let private enumMemberNames (cases: string list) =
    let sanitize (s: string) =
        let s = System.Text.RegularExpressions.Regex.Replace(s, "[^A-Za-z0-9_]", "_")
        let s = if s = "" then "Unspecified" else s
        if System.Char.IsDigit s.[0] then "_" + s else s
    let seen = System.Collections.Generic.HashSet<string>()
    [
        for c in cases ->
            let baseName = sanitize c
            let mutable name = baseName
            let mutable i = 2
            while not (seen.Add(name.ToLowerInvariant())) do
                name <- baseName + string i
                i <- i + 1
            name, c
    ]

let private cap (s: string) =
    if System.String.IsNullOrEmpty s then s
    else System.Char.ToUpperInvariant(s.[0]).ToString() + s.Substring(1)

let private firstLine (s: string) =
    s.Split([| '\n'; '\r' |], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.tryHead
    |> Option.map (fun s -> s.Trim())

let private comment (c: string option) =
    match c with
    | Some c -> firstLine c
    | None -> None

let rec private typeKey (t: MType) =
    match t with
    | TString -> "string"
    | TFloat -> "float"
    | TBool -> "bool"
    | TUnit -> "unit"
    | TObj -> "obj"
    | TRef n -> "ref:" + n
    | TArray x -> "array:" + typeKey x
    | TMap x -> "map:" + typeKey x
    | TUnion xs -> "union:" + (xs |> List.map typeKey |> String.concat "|")
    | TPromise x -> "promise:" + typeKey x
    | TFunc (args, ret) -> "func:(" + (args |> List.map typeKey |> String.concat ",") + ")->" + typeKey ret
    | TExt n -> "ext:" + n

/// Two members that differ only by casing (e.g. `getReferrer` / `GetReferrer`
/// aliases) collapse to one CLR signature; keep the first.
let private methodKey (m: Method) =
    let paramKey (p: Param) =
        (if p.Optional then "?" else "") + typeKey p.Type
    [
        m.Name.ToLowerInvariant()
        yield! m.Params |> List.map paramKey
        typeKey m.Returns
        string m.Promise
    ]
    |> String.concat "|"

let private mkMethod
    (types: Dictionary<string, Type.Type>)
    (m: Method)
    : CodeModel.Method =
    let ret =
        if m.Promise then
            match m.Returns with
            | TUnit -> T<Promise<unit>>
            | t -> T<Promise<_>>[ conv types t ]
        else
            conv types m.Returns
    let sigType = (mkParameters types m.Params :> Type.IParameters) ^-> ret
    let decl = m.Name => sigType
    match comment m.Comment with
    | Some c -> decl |> WithComment c
    | None -> decl

let private mkEvent
    (types: Dictionary<string, Type.Type>)
    (handle: Type.Type)
    (eventNameType: Type.Type)
    (e: Event)
    : CodeModel.Method =
    // The event name is a shared enum type so the overloads stay discoverable;
    // the listener is the payload-specific callback.
    let nameType = eventNameType
    let listener =
        match e.Listener with
        | TUnit -> T<unit> ^-> T<unit>
        | t -> (conv types t) ^-> T<unit>
    let ps : Type.Parameters =
        {
            This = None
            Arguments =
                [
                    { Type.Parameter.Name = Some "eventName"; Optional = false; Type = nameType }
                    { Type.Parameter.Name = Some "listenerFunc"; Optional = false; Type = listener }
                ]
            Variable = None
        }
    "addListener" => ((ps :> Type.IParameters) ^-> T<Promise<_>>[ handle ])
    |> WithComment ("For event '" + e.Name + "'.")

/// Builds one plugin's namespace members and plugin class.
let private buildPlugin (handle: Type.Type) (p: Plugin) =
    let types = Dictionary<string, Type.Type>()
    types.["PluginListenerHandle"] <- handle
    let members = ResizeArray<CodeModel.NamespaceEntity>()

    let ordered = orderTypes p.Types
    let byName = p.Types |> List.map (fun d -> d.Name, d) |> Map.ofList

    for name in ordered do
        match Map.tryFind name byName with
        | None -> ()
        | Some def ->
            match def.Shape with
            | Config (required, optional) ->
                let cls =
                    Pattern.Config def.Name {
                        Required = [ for m in required -> m.Name, conv types m.Type ]
                        Optional = [ for m in optional -> m.Name, conv types m.Type ]
                    }
                types.[def.Name] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
            | Strings cases ->
                let cls =
                    Pattern.EnumInlines def.Name [
                        for name, value in enumMemberNames cases -> name, quote value
                    ]
                types.[def.Name] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
            | Map value ->
                let cls =
                    Class def.Name
                    |+> Instance [ Property "" (conv types value) |> Indexed T<string> ]
                    |+> Static [ Getter "Empty" TSelf |> WithGetterInline "({})" ]
                types.[def.Name] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
            | Numbers cases ->
                let cls =
                    Pattern.EnumInlines def.Name [
                        for name, value in enumMemberNames (cases |> List.map fst) -> name, string value
                    ]
                types.[def.Name] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
            | UnionOf ns ->
                let ts = [ for n in ns -> conv types (TRef n) ]
                match ts with
                | [] -> ()
                | x :: xs -> types.[def.Name] <- xs |> List.fold (fun acc y -> acc + y) x

    // One single-case enum per event: it names the event in F# completion and
    // keeps the overload signatures distinct for the CLR.
    let eventNameTypes =
        [
            for e in p.Events ->
                let enumName = p.Namespace + cap (e.Name) + "EventName"
                let cls = Pattern.EnumStrings enumName [ e.Name ]
                types.[enumName] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
                e, cls.Type
        ]

    let methods = p.Methods |> List.distinctBy methodKey
    if methods.Length <> p.Methods.Length then
        printfn "capgo-generator: %s dropped %d duplicate overload(s)" p.Namespace (p.Methods.Length - methods.Length)

    let pluginClass =
        Class (p.Namespace + "Plugin")
        |+> Instance [
                for m in methods -> mkMethod types m
                for e, nameType in eventNameTypes -> mkEvent types handle nameType e
            ]

    members, pluginClass, types

/// Builds the assembly for one package.
let getAssembly (pkg: Package) : CodeModel.Assembly =
    if pkg.Plugins |> List.isEmpty then
        failwithf "package %s has no plugins" pkg.RootNamespace

    let handle =
        Class "PluginListenerHandle"
        |+> Instance [ "remove" => (T<unit> ^-> T<Promise<unit>>) ]

    let rootMembers = ResizeArray<CodeModel.NamespaceEntity>()
    rootMembers.Add(handle :> CodeModel.NamespaceEntity)

    let rootClass = Class pkg.RootClass

    let namespaces = ResizeArray<CodeModel.Namespace>()
    let properties = ResizeArray<CodeModel.IClassMember>()

    for p in pkg.Plugins do
        if not (List.isEmpty p.Unsupported) then
            for (method, reason) in p.Unsupported do
                printfn "capgo-generator: %s.%s is not bound: %s" p.Namespace method reason

        let members, pluginClass, _ = buildPlugin handle.Type p

        let nsName = pkg.RootNamespace + "." + p.Namespace
        namespaces.Add(Namespace nsName (List.ofSeq members))
        rootMembers.Add(pluginClass :> CodeModel.NamespaceEntity)

        properties.Add(
            (Getter p.Namespace pluginClass |> Import p.Export p.Package) :> CodeModel.IClassMember
        )

    let root =
        rootClass
        |+> Static (List.ofSeq properties)

    rootMembers.Insert(0, root :> CodeModel.NamespaceEntity)
    namespaces.Insert(0, Namespace pkg.RootNamespace (List.ofSeq rootMembers))

    Assembly(List.ofSeq namespaces)