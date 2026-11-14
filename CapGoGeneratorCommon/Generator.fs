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
    | _ -> []

let private depsOf (d: TypeDef) =
    match d.Shape with
    | Config (required, optional) ->
        required @ optional
        |> List.collect (fun m -> refsOf m.Type)
        |> List.filter (fun n -> n <> d.Name)
        |> List.distinct
    | UnionOf ns -> ns
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
        | _ -> failwithf "unresolved type reference '%s'" n
    | TArray x -> !| (conv types x)
    | TMap _ -> T<obj>
    | TUnion [] -> T<unit>
    | TUnion (x :: xs) ->
        xs
        |> List.fold (fun acc y -> acc + (conv types y)) (conv types x)

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
                let cls = Pattern.EnumStrings def.Name cases
                types.[def.Name] <- cls.Type
                members.Add(cls :> CodeModel.NamespaceEntity)
            | Numbers cases ->
                let cls = Pattern.EnumInlines def.Name [ for n, v in cases -> n, string v ]
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

    let pluginClass =
        Class (p.Namespace + "Plugin")
        |+> Instance [
                for m in p.Methods -> mkMethod types m
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