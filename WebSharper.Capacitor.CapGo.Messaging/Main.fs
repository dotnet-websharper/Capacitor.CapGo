namespace WebSharper.Capacitor.CapGo.Messaging

open WebSharper
open WebSharper.InterfaceGenerator
open CapGoGeneratorCommon

module Definition =
    /// The plugins of this package. The surface itself comes from
    /// tools/model/*.json (see tools/README.md).
    let plugins =
        Model.loadDir (__SOURCE_DIRECTORY__ + "/../tools/model")
        |> List.filter (fun p -> p.Family = "Messaging")

    let Assembly =
        Generator.getAssembly {
            RootNamespace = "WebSharper.Capacitor.CapGo.Messaging"
            RootClass = "CapacitorCapGoMessaging"
            Plugins = plugins
        }

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly = Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
