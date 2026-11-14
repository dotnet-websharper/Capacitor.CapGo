// The flat plugin surface model produced by tools/extract.mjs.
//
// Everything here is fully resolved: no generics, no conditional types, no
// anonymous types. `obj` only appears for index-signature maps over unknown
// JSON.
module CapGoGeneratorCommon.Model

open System
open System.IO
open System.Text.Json

/// A type in the model DSL.
type MType =
    | TString
    | TFloat
    | TBool
    | TUnit
    | TObj
    | TRef of string
    | TArray of MType
    | TMap of MType
    | TUnion of MType list
    /// A Promise returned as a value (property or field), not a method return.
    | TPromise of MType
    /// A callback value: argument types and result type.
    | TFunc of MType list * MType
    /// A type from outside the plugin package that the WIG may not model.
    | TExt of string

type Member =
    {
        Name: string
        Type: MType
    }

type TypeShape =
    | Config of required: Member list * optional: Member list
    | Strings of string list
    | Numbers of (string * int) list
    | UnionOf of string list
    /// A string-indexed map `{ [key: string]: T }`.
    | Map of MType

type TypeDef =
    {
        Name: string
        Shape: TypeShape
    }

type Param =
    {
        Name: string
        Type: MType
        Optional: bool
    }

type Method =
    {
        Name: string
        Params: Param list
        Returns: MType
        Promise: bool
        Comment: string option
    }

type Event =
    {
        Name: string
        Listener: MType
    }

type Plugin =
    {
        Namespace: string
        Family: string
        Package: string
        Version: string
        Export: string
        Types: TypeDef list
        Methods: Method list
        Events: Event list
        Unsupported: (string * string) list
    }

// --- JSON helpers ---------------------------------------------------------

let private prop (e: JsonElement) (name: string) =
    match e.TryGetProperty name with
    | true, v -> Some v
    | _ -> None

let private str (e: JsonElement) = e.GetString()

let private optStr (e: JsonElement) name =
    prop e name |> Option.map str

let private arr (e: JsonElement) name =
    match prop e name with
    | Some v -> v.EnumerateArray() |> Seq.toList
    | None -> []

let rec private recType (e: JsonElement) : MType =
    match str (e.GetProperty "k") with
    | "string" -> TString
    | "float" | "int" -> TFloat
    | "bool" -> TBool
    | "unit" -> TUnit
    | "obj" -> TObj
    | "ref" -> TRef (str (e.GetProperty "name"))
    | "array" -> TArray (recType (e.GetProperty "of"))
    | "map" -> TMap (recType (e.GetProperty "of"))
    | "union" -> TUnion (arr e "of" |> List.map recType)
    | "promise" -> TPromise (recType (e.GetProperty "of"))
    | "func" -> TFunc (arr e "args" |> List.map recType, recType (e.GetProperty "ret"))
    | "ext" -> TExt (str (e.GetProperty "name"))
    | other -> failwithf "unknown type kind '%s'" other

let rec private recTypeDef (e: JsonElement) : TypeDef =
    let name = str (e.GetProperty "name")
    let shape =
        match str (e.GetProperty "kind") with
        | "config" ->
            let members n =
                arr e n
                |> List.map (fun m -> { Name = str (m.GetProperty "name"); Type = recType (m.GetProperty "type") })
            Config (members "required", members "optional")
        | "enumStrings" -> Strings (arr e "cases" |> List.map str)
        | "enum" ->
            Numbers (
                arr e "cases"
                |> List.map (fun c -> str (c.GetProperty "name"), c.GetProperty("value").GetInt32())
            )
        | "union" -> UnionOf (arr e "of" |> List.map str)
        | "map" -> Map (recType (e.GetProperty "of"))
        | other -> failwithf "unknown type shape '%s' in %s" other name
    { Name = name; Shape = shape }

let private recParam (e: JsonElement) : Param =
    {
        Name = str (e.GetProperty "name")
        Type = recType (e.GetProperty "type")
        Optional =
            match prop e "optional" with
            | Some v -> v.GetBoolean()
            | None -> false
    }

let private recMethod (e: JsonElement) : Method =
    {
        Name = str (e.GetProperty "name")
        Params = arr e "params" |> List.map recParam
        Returns = recType (e.GetProperty "returns")
        Promise =
            match prop e "promise" with
            | Some v -> v.GetBoolean()
            | None -> false
        Comment = optStr e "comment"
    }

let private recEvent (e: JsonElement) : Event =
    {
        Name = str (e.GetProperty "name")
        Listener = recType (e.GetProperty "listener")
    }

/// Parses one plugin model file.
let load (path: string) : Plugin =
    use doc = JsonDocument.Parse(File.ReadAllText path)
    let root = doc.RootElement
    {
        Namespace = str (root.GetProperty "namespace")
        Family = str (root.GetProperty "family")
        Package = str (root.GetProperty "package")
        Version = str (root.GetProperty "version")
        Export = str (root.GetProperty "export")
        Types = arr root "types" |> List.map recTypeDef
        Methods = arr root "methods" |> List.map recMethod
        Events = arr root "events" |> List.map recEvent
        Unsupported =
            arr root "unsupported"
            |> List.map (fun u -> str (u.GetProperty "method"), str (u.GetProperty "reason"))
    }

/// Loads every `*.json` model in a directory.
let loadDir (path: string) : Plugin list =
    Directory.GetFiles(path, "*.json")
    |> Array.sort
    |> Array.map load
    |> List.ofArray