namespace WebSharper.Capacitor.CapGo.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Capacitor.CapGo
open WebSharper.Capacitor.CapGo.Auth
open WebSharper.Capacitor.CapGo.Media
open WebSharper.Capacitor.CapGo.Files
open WebSharper.Capacitor.CapGo.Device
open WebSharper.Capacitor.CapGo.UI
open WebSharper.Capacitor.CapGo.Commerce
open WebSharper.Capacitor.CapGo.Analytics
open WebSharper.Capacitor.CapGo.Messaging
open WebSharper.Capacitor.CapGo.AI
open WebSharper.Capacitor.CapGo.Tooling

[<JavaScript>]
module Helpers =
    // Probe the Capacitor bridge without depending on the core binding. `?` is
    // safe for reading properties; calling needs an inline helper.
    [<Inline "$0 && $0.isNativePlatform ? $0.isNativePlatform() : false">]
    let private platformIsNative (cap: obj) = X<bool>

    [<Inline "$0 && $0.getPlatform ? $0.getPlatform() : 'web'">]
    let private platformName (cap: obj) = X<string>

    let IsNative () = platformIsNative (JS.Global?Capacitor)

    let Platform () = platformName (JS.Global?Capacitor)

[<JavaScript>]
module Client =
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let themeStorageKey = "websharper-capacitor-capgo-theme"
    let theme = Var.Create "light"
    let themeLabel = Var.Create "🌙 Dark mode"

    let applyTheme () =
        JS.Document.DocumentElement.SetAttribute("data-theme", theme.Value)
        themeLabel.Value <- if theme.Value = "dark" then "☀️ Light mode" else "🌙 Dark mode"

    let toggleTheme () =
        theme.Value <- (if theme.Value = "dark" then "light" else "dark")
        try JS.Window.LocalStorage.SetItem(themeStorageKey, theme.Value) with _ -> ()
        applyTheme ()

    type Demo =
        {
            Name: string
            Group: string
            Description: string
            Run: unit -> Promise<string>
        }

    /// One demo per family package; the plugin calls are real calls against the
    /// generated surface, not stubs. On the web they reject and the log reports
    /// that, which is the expected behaviour for native-only plugins.
    let demos : Demo list =
        [
            {
                Name = "CapacitorFlash"
                Group = "Core"
                Description = "Detect the torch and its current state (native only)."
                Run = fun () ->
                    promise {
                        let! available = CapacitorCapGo.CapacitorFlash.IsAvailable()
                        let! on = CapacitorCapGo.CapacitorFlash.IsSwitchedOn()
                        return sprintf "available=%b, switched on=%b" available.Value on.Value
                    }
            }
            {
                Name = "CapacitorMute"
                Group = "Core"
                Description = "Read the ringer mute switch."
                Run = fun () ->
                    promise {
                        let! muted = CapacitorCapGo.CapacitorMute.IsMuted()
                        return sprintf "muted=%b" muted.Value
                    }
            }
            {
                Name = "HomeIndicator"
                Group = "Core"
                Description = "Query the iOS home indicator visibility."
                Run = fun () ->
                    promise {
                        let! hidden = CapacitorCapGo.HomeIndicator.IsHidden()
                        return sprintf "hidden=%b" hidden.Hidden
                    }
            }
            {
                Name = "NavigationBar"
                Group = "Core"
                Description = "Read the Android navigation bar colour."
                Run = fun () ->
                    promise {
                        let! color = CapacitorCapGo.NavigationBar.GetNavigationBarColor()
                        return sprintf "colour=%s" color.Color
                    }
            }
            {
                Name = "Passkey"
                Group = "Auth"
                Description = "Check passkey (WebAuthn) support and configuration."
                Run = fun () ->
                    promise {
                        let! supported = CapacitorCapGoAuth.Passkey.IsSupported()
                        let! config = CapacitorCapGoAuth.Passkey.GetConfiguration()
                        return sprintf "available=%b, domains=%d" supported.Available config.Domains.Length
                    }
            }
            {
                Name = "Permissions"
                Group = "Auth"
                Description = "Request precise location permission."
                Run = fun () ->
                    promise {
                        let! _ = CapacitorCapGoAuth.Permissions.RequestPreciseLocation()
                        return "precise-location permission requested"
                    }
            }
            {
                Name = "Brightness"
                Group = "Device"
                Description = "Read the screen brightness (0–1)."
                Run = fun () ->
                    promise {
                        let! b = CapacitorCapGoDevice.Brightness.GetBrightness()
                        return sprintf "brightness=%.2f" b.Brightness
                    }
            }
            {
                Name = "KeepAwake"
                Group = "Device"
                Description = "Report whether the screen is being kept awake."
                Run = fun () ->
                    promise {
                        let! kept = CapacitorCapGoDevice.KeepAwake.IsKeptAwake()
                        return sprintf "kept awake=%b" kept.IsKeptAwake
                    }
            }
            {
                Name = "AudioSession"
                Group = "Media"
                Description = "Enumerate the current audio outputs."
                Run = fun () ->
                    promise {
                        let! outputs = CapacitorCapGoMedia.AudioSession.CurrentOutputs()
                        return sprintf "%d audio output(s)" outputs.Length
                    }
            }
            {
                Name = "CameraPreview"
                Group = "Media"
                Description = "Report whether the camera preview is running."
                Run = fun () ->
                    promise {
                        let! running = CapacitorCapGoMedia.CameraPreview.IsRunning()
                        return sprintf "running=%b" running.IsRunning
                    }
            }
            {
                Name = "File"
                Group = "Files"
                Description = "Read free disk space through the native file plugin."
                Run = fun () ->
                    promise {
                        let! space = CapacitorCapGoFiles.File.GetFreeDiskSpace()
                        return sprintf "free=%.0f bytes" space.Free
                    }
            }
            {
                Name = "AndroidKiosk"
                Group = "UI"
                Description = "Check whether the app is in kiosk mode."
                Run = fun () ->
                    promise {
                        let! kiosk = CapacitorCapGoUI.AndroidKiosk.IsInKioskMode()
                        return sprintf "kiosk=%b" kiosk.IsInKioskMode
                    }
            }
            {
                Name = "LaunchNavigator"
                Group = "UI"
                Description = "Read the default navigation app."
                Run = fun () ->
                    promise {
                        let! app = CapacitorCapGoUI.LaunchNavigator.GetDefaultApp()
                        return sprintf "default app=%s" app.App
                    }
            }
            {
                Name = "InAppReview"
                Group = "Commerce"
                Description = "Ask the platform for an in-app review prompt."
                Run = fun () ->
                    promise {
                        do! CapacitorCapGoCommerce.InAppReview.RequestReview()
                        return "review prompt requested"
                    }
            }
            {
                Name = "Appinsights"
                Group = "Analytics"
                Description = "Read the App Insights SDK state."
                Run = fun () ->
                    promise {
                        let! state = CapacitorCapGoAnalytics.Appinsights.GetState()
                        return sprintf "init=%b" state.InitCompleted
                    }
            }
            {
                Name = "Intercom"
                Group = "Messaging"
                Description = "Read the unread Intercom conversation count."
                Run = fun () ->
                    promise {
                        let! count = CapacitorCapGoMessaging.Intercom.GetUnreadConversationCount()
                        return sprintf "unread=%d" (int count.Count)
                    }
            }
            {
                Name = "RichNotifications"
                Group = "Messaging"
                Description = "List the notifications currently displayed."
                Run = fun () ->
                    promise {
                        let! pending = CapacitorCapGoMessaging.RichNotifications.GetPending()
                        return sprintf "%d displayed notification(s)" pending.Notifications.Length
                    }
            }
            {
                Name = "SpeechSynthesis"
                Group = "AI"
                Description = "Check on-device speech synthesis availability."
                Run = fun () ->
                    promise {
                        let! available = CapacitorCapGoAI.SpeechSynthesis.IsAvailable()
                        return sprintf "available=%b" available.IsAvailable
                    }
            }
            {
                Name = "Auto"
                Group = "Tooling"
                Description = "Check Android Auto / CarPlay availability."
                Run = fun () ->
                    promise {
                        let! available = CapacitorCapGoTooling.Auto.IsAvailable()
                        return sprintf "available=%b, connected=%b" available.Available available.Connected
                    }
            }
            {
                Name = "Env"
                Group = "Tooling"
                Description = "Read the plugin version marker."
                Run = fun () ->
                    promise {
                        let! v = CapacitorCapGoTooling.Env.GetPluginVersion()
                        return sprintf "version=%s" v.Version
                    }
            }
        ]

    let output = Var.Create ""

    let log (line: string) =
        output.Value <- output.Value + line + "\n"

    let run (demo: Demo) =
        log (sprintf "> %s …" demo.Name)
        demo.Run()
            .Then(fun text ->
                log (sprintf "  ✓ %s" text)
                ())
            .Catch(fun err ->
                log (sprintf "  ✗ %s" (string err))
                ())
        |> ignore

    [<SPAEntryPoint>]
    let Main () =
        let saved =
            try JS.Window.LocalStorage.GetItem(themeStorageKey) with _ -> null
        if saved = "dark" || saved = "light" then
            theme.Value <- saved
        applyTheme ()

        let hint =
            if Helpers.IsNative () then
                sprintf "Running natively on %s — plugin calls go to the native bridge." (Helpers.Platform())
            else
                "Running in a web browser. Most CapGo plugins are native-only, so calls reject and the log reports the error."

        let cards =
            demos
            |> List.map (fun (demo: Demo) ->
                IndexTemplate.PluginItem()
                    .name(demo.Name)
                    .group(demo.Group)
                    .description(demo.Description)
                    .run(fun _ -> run demo)
                    .Doc())
            |> Doc.Concat

        IndexTemplate()
            .platform(Helpers.Platform())
            .themeLabel(themeLabel.V)
            .count(string demos.Length)
            .hint(hint)
            .output(output.V)
            .PluginList(cards)
            .ToggleTheme(fun _ -> toggleTheme ())
            .Bind()