namespace WebSharper.Capacitor.CapGo

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
    let PluginListenerHandle =
        Class "PluginListenerHandle"
        |+> Instance ["remove" => T<unit> ^-> T<Promise<unit>>]

    [<AutoOpen>]
    module CapacitorSocialLogin = 
        let Provider = 
            Pattern.EnumStrings "Provider" [
                "apple"
                "google"
                "facebook"
                "twitter"
            ]
        
        let FacebookOptions =
            Pattern.Config "FacebookOptions" {
                Required = [
                    "appId", T<string>
                    "clientToken", T<string>
                ]
                Optional = []
            }

        let GoogleOptions =
            Pattern.Config "GoogleOptions" {
                Required = []
                Optional = [
                    "iOSClientId", T<string>
                    "iOSServerClientId", T<string>
                    "webClientId", T<string>
                ]
            }

        let AppleOptions =
            Pattern.Config "AppleOptions" {
                Required = []
                Optional = [
                    "clientId", T<string>
                    "redirectUrl", T<string>
                ]
            }

        let AuthorizationCode =
            Pattern.Config "AuthorizationCode" {
                Required = [
                    "jwt", T<string>
                ]
                Optional = []
            }

        let AuthorizationCodeOptions =
            Pattern.Config "AuthorizationCodeOptions" {
                Required = [
                    "provider", Provider.Type
                ]
                Optional = []
            }

        let IsLoggedInOptions =
            Pattern.Config "IsLoggedInOptions" {
                Required = [
                    "provider", Provider.Type
                ]
                Optional = []
            }

        let InitializeOptions =
            Pattern.Config "InitializeOptions" {
                Required = []
                Optional = [
                    "facebook", FacebookOptions.Type
                    "google", GoogleOptions.Type
                    "apple", AppleOptions.Type
                ]
            }

        let FacebookLoginOptions =
            Pattern.Config "FacebookLoginOptions" {
                Required = [
                    "permissions", !| T<string>
                ]
                Optional = [
                    "limitedLogin", T<bool>
                    "nonce", T<string>
                ]
            }

        let GoogleLoginOptions =
            Pattern.Config "GoogleLoginOptions" {
                Required = []
                Optional = [
                    "scopes", !| T<string>
                    "nonce", T<string>
                    "grantOfflineAccess", T<bool>
                ]
            }
            
        let AppleProviderOptions =
            Pattern.Config "AppleProviderOptions" {
                Required = []
                Optional = [
                    "scopes", !| T<string>
                    "nonce", T<string>
                    "state", T<string>
                ]
            }
            
        let AccessToken =
            Pattern.Config "AccessToken" {
                Required = [
                    "token", T<string>
                ]
                Optional = [
                    "applicationId", T<string>
                    "declinedPermissions", !| T<string>
                    "expires", T<string>
                    "isExpired", T<bool>
                    "lastRefresh", T<string>
                    "permissions", !| T<string>
                    "refreshToken", T<string>
                    "userId", T<string>
                ]
            }

        let AgeRange = 
            Pattern.Config "AgeRange" {
                Required = []
                Optional = [
                    "min", T<uint>
                    "max", T<uint>
                ]
            }

        let Location = 
            Pattern.Config "Location" {
                Required = []
                Optional = [
                    "id", T<uint>
                    "name", T<uint>
                ]
            }

        let Hometown = 
            Pattern.Config "Hometown" {
                Required = []
                Optional = [
                    "id", T<uint>
                    "name", T<uint>
                ]
            }

        let FacebookProfile =
            Pattern.Config "FacebookProfile" {
                Required = []
                Optional = [
                    "userID", T<string>
                    "email", T<string>
                    "friendIDs", !| T<string>                    
                    "birthday", T<string>
                    "ageRange", AgeRange.Type
                    "gender", T<string>
                    "location", Location.Type
                    "hometown", Hometown.Type
                    "profileURL", T<string>
                    "name", T<string>
                    "imageURL", T<string>
                ]
            }

        let FacebookLoginResponse =
            Pattern.Config "FacebookLoginResponse" {
                Required = []
                Optional = [
                    "accessToken", AccessToken.Type
                    "profile", FacebookProfile.Type
                    "idToken", T<string>
                ]
            }       

        let GoogleProfile =  
            Pattern.Config "GoogleProfile" {
                Required = []
                Optional = [
                    "email", T<string>
                    "familyName", T<string>
                    "givenName", T<string>
                    "id", T<string>
                    "name", T<string>
                    "imageUrl", T<string>
                ]
            } 

        let GoogleLoginResponse =
            Pattern.Config "GoogleLoginResponse" {
                Required = []
                Optional = [
                    "accessToken", AccessToken.Type
                    "idToken", T<string>
                    "profile", GoogleProfile.Type
                ]
            }

        let AppleProfile =  
            Pattern.Config "AppleProfile" {
                Required = []
                Optional = [
                    "user", T<string>
                    "email", T<string>
                    "familyName", T<string>
                    "givenName", T<string>
                ]
            } 

        let AppleProviderResponse =
            Pattern.Config "AppleProviderResponse" {
                Required = []
                Optional = [
                    "accessToken", AccessToken.Type
                    "idToken", T<string>
                    "profile", AppleProfile.Type
                ]
            }

        let LoginOptions =
            Pattern.Config "LoginOptions" {
                Required = [
                    "provider", Provider.Type
                    "options", FacebookLoginOptions + GoogleLoginOptions + AppleProviderOptions
                ]
                Optional = []
            }

        let LoginResult =
            Pattern.Config "LoginResult" {
                Required = [
                    "provider", Provider.Type
                    "result", FacebookLoginResponse + GoogleLoginResponse + AppleProviderResponse
                ]
                Optional = []
            }

        let LogoutOptions = 
            Pattern.Config "LogoutOptions" {
                Required = [
                    "provider", Provider.Type
                ]
                Optional = []
            }

        let IsLoggedInResult = 
            Pattern.Config "IsLoggedInResult" {
                Required = [
                    "isLoggedIn", T<bool>
                ]
                Optional = []
            }

        let SocialLoginPlugin =
            Class "SocialLoginPlugin"
            |+> Instance [
                "initialize" => InitializeOptions?options ^-> T<Promise<unit>>
                "login" => LoginOptions?options ^-> T<Promise<_>>[LoginResult]
                "logout" =>LogoutOptions?options ^-> T<Promise<unit>>
                "isLoggedIn" => IsLoggedInOptions?options ^-> T<Promise<_>>[IsLoggedInResult]
                "getAuthorizationCode" => AuthorizationCodeOptions?options ^-> T<Promise<_>>[AuthorizationCode]
                "refresh" => LoginOptions?options ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module CapacitorUploader = 
        let UploadOptionMethod = 
            Pattern.EnumStrings "UploadOptionMethod" [
                "PUT"; "POST"
            ]

        let UploadOption =
            Pattern.Config "UploadOption" {
                Required = [
                    "filePath", T<string>
                    "serverUrl", T<string>
                    "headers", T<obj>
                ]
                Optional = [
                    "notificationTitle", T<int>
                    "method", UploadOptionMethod.Type
                    "mimeType", T<string>
                    "parameters", T<obj>
                    "maxRetries", T<int>
                ]
            }

        let UploadEventPayload =
            Pattern.Config "UploadEventPayload" {
                Required = []
                Optional = [
                    "percent", T<int>
                    "error", T<string>
                    "statusCode", T<int>
                ]
            }

        let UploadEventName = 
            Pattern.EnumStrings "UploadEventName" [
                "uploading"; "completed"; "failed"
            ]

        let UploadEvent =
            Pattern.Config "UploadEvent" {
                Required = [
                    "name", UploadEventName.Type
                    "payload", UploadEventPayload.Type
                    "id", T<string>
                ]
                Optional = []
            }

        let RemoveUploadOptions =
            Pattern.Config "RemoveUploadOptions" {
                Required = [
                    "id", T<string>
                ]
                Optional = []
            }

        let UploadAddListener = 
            Pattern.EnumStrings "UploadAddListener" [
                "events"
            ]

        let StartUploadResult =
            Pattern.Config "StartUploadResult" {
                Required = [
                    "id", T<string>
                ]
                Optional = []
            }

        let UploaderPlugin =
            Class "UploaderPlugin"
            |+> Instance [
                "startUpload" => UploadOption?options ^-> T<Promise<_>>[StartUploadResult]
                "removeUpload" => RemoveUploadOptions?options ^-> T<Promise<unit>>
                "addListener" =>
                    UploadAddListener?eventName *
                    (UploadEvent.Type?state ^-> T<unit>)?listenerFunc
                    ^-> T<Promise<_>>[PluginListenerHandle]
            ]

    [<AutoOpen>]
    module CapacitorNavigationBarPlugin =
        let SetNavigationBarColorOptions =
            Pattern.Config "SetNavigationBarColorOptions" {
                Required = [
                    "color", T<string>
                ]
                Optional = []
            }

        let GetNavigationBarColorResult =
            Pattern.Config "GetNavigationBarColorResult" {
                Required = [
                    "color", T<string>
                ]
                Optional = []
            }

        let NavigationBarPlugin =
            Class "NavigationBarPlugin"
            |+> Instance [
                "setNavigationBarColor" =>
                    SetNavigationBarColorOptions?options ^-> T<Promise<unit>>
                "getNavigationBarColor" =>
                    T<unit> ^-> T<Promise<_>>[GetNavigationBarColorResult]
            ]

    [<AutoOpen>]
    module CapacitorDataStorageSqlitePlugin =
        let CapEchoOptions =
            Pattern.Config "CapEchoOptions" {
                Required = []
                Optional = [
                    "value", T<string>
                ]
            }

        let CapOpenStorageOptions =
            Pattern.Config "CapOpenStorageOptions" {
                Required = []
                Optional = [
                    "database", T<string>
                    "table", T<string>
                    "encrypted", T<bool>
                    "mode", T<string>
                ]
            }

        let CapDataStorageOptions =
            Pattern.Config "CapDataStorageOptions" {
                Required = [
                    "key", T<string>
                ]
                Optional = [
                    "value", T<string>
                ]
            }

        let CapStorageOptions =
            Pattern.Config "CapStorageOptions" {
                Required = [
                    "database", T<string>
                ]
                Optional = []
            }

        let CapTableStorageOptions =
            Pattern.Config "CapTableStorageOptions" {
                Required = [
                    "table", T<string>
                ]
                Optional = []
            }

        let CapFilterStorageOptions =
            Pattern.Config "CapFilterStorageOptions" {
                Required = [
                    "filter", T<string>
                ]
                Optional = []
            }

        let CapEchoResult =
            Pattern.Config "CapEchoResult" {
                Required = [
                    "value", T<string>
                ]
                Optional = []
            }

        let CapDataStorageResult =
            Pattern.Config "CapDataStorageResult" {
                Required = []
                Optional = [
                    "result", T<bool>
                    "message", T<string>
                ]
            }

        let CapValueResult =
            Pattern.Config "CapValueResult" {
                Required = [
                    "value", T<string>
                ]
                Optional = []
            }

        let CapKeysResult =
            Pattern.Config "CapKeysResult" {
                Required = [
                    "keys", !| T<string>
                ]
                Optional = []
            }

        let CapValuesResult =
            Pattern.Config "CapValuesResult" {
                Required = [
                    "values", !| T<string>
                ]
                Optional = []
            }

        let CapKeysValuesResult =
            Pattern.Config "CapKeysValuesResult" {
                Required = [
                    "keysvalues", !| T<obj>
                ]
                Optional = []
            }

        let CapTablesResult =
            Pattern.Config "CapTablesResult" {
                Required = [
                    "tables", !| T<string>
                ]
                Optional = []
            }

        let JsonTable =
            Pattern.Config "JsonTable" {
                Required = [
                    "name", T<string>
                ]
                Optional = [
                    "values", !| CapDataStorageOptions.Type
                ]
            }

        let JsonStore =
            Pattern.Config "JsonStore" {
                Required = [
                    "database", T<string>
                    "encrypted", T<bool>
                    "tables", !| JsonTable.Type
                ]
                Optional = []
            }

        let CapDataStorageChanges =
            Pattern.Config "CapDataStorageChanges" {
                Required = []
                Optional = [
                    "changes", T<int>
                ]
            }

        let CapStoreImportOptions =
            Pattern.Config "CapStoreImportOptions" {
                Required = []
                Optional = [
                    "jsonstring", T<string>
                ]
            }

        let CapStoreJson =
            Pattern.Config "CapStoreJson" {
                Required = []
                Optional = [
                    "export", JsonStore.Type
                ]
            }

        let CapacitorDataStorageSqlitePlugin =
            Class "CapacitorDataStorageSqlitePlugin"
            |+> Instance [
                "echo" => CapEchoOptions?options ^-> T<Promise<_>>[CapEchoResult]
                "openStore" => CapOpenStorageOptions?options ^-> T<Promise<unit>>
                "closeStore" => CapStorageOptions?options ^-> T<Promise<unit>>
                "isStoreOpen" => CapStorageOptions?options ^-> T<Promise<_>>[CapDataStorageResult]
                "isStoreExists" => CapStorageOptions?options ^-> T<Promise<_>>[CapDataStorageResult]
                "deleteStore" => CapOpenStorageOptions?options ^-> T<Promise<unit>>
                "setTable" => CapTableStorageOptions?options ^-> T<Promise<unit>>
                "set" => CapDataStorageOptions?options ^-> T<Promise<unit>>
                "get" => CapDataStorageOptions?options ^-> T<Promise<_>>[CapValueResult]
                "remove" => CapDataStorageOptions?options ^-> T<Promise<unit>>
                "clear" => T<unit> ^-> T<Promise<unit>>
                "iskey" => CapDataStorageOptions?options ^-> T<Promise<_>>[CapDataStorageResult]
                "keys" => T<unit> ^-> T<Promise<_>>[CapKeysResult]
                "values" => T<unit> ^-> T<Promise<_>>[CapValuesResult]
                "filtervalues" => CapFilterStorageOptions?options ^-> T<Promise<_>>[CapValuesResult]
                "keysvalues" => T<unit> ^-> T<Promise<_>>[CapKeysValuesResult]
                "isTable" => CapTableStorageOptions?options ^-> T<Promise<_>>[CapDataStorageResult]
                "tables" => T<unit> ^-> T<Promise<_>>[CapTablesResult]
                "deleteTable" => CapTableStorageOptions?options ^-> T<Promise<unit>>
                "importFromJson" => CapStoreImportOptions?options ^-> T<Promise<_>>[CapDataStorageChanges]
                "isJsonValid" => CapStoreImportOptions?options ^-> T<Promise<_>>[CapDataStorageResult]
                "exportToJson" => T<unit> ^-> T<Promise<_>>[CapStoreJson]
            ]

    [<AutoOpen>]
    module ScreenRecorderPlugin = 
        let ScreenRecorderPlugin =
            Class "ScreenRecorderPlugin"
            |+> Instance [
                "start" => T<unit> ^-> T<Promise<unit>>
                "stop" => T<unit> ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module NativePurchasesPlugin = 
        let ATTRIBUTION_NETWORK =
            Pattern.EnumStrings "ATTRIBUTION_NETWORK" [
                "APPLE_SEARCH_ADS"
                "ADJUST"
                "APPSFLYER"
                "BRANCH"
                "TENJIN"
                "FACEBOOK"
            ]

        let PURCHASE_TYPE =
            Pattern.EnumStrings "PURCHASE_TYPE" [
                "INAPP"; "SUBS"
            ]

        let BILLING_FEATURE =
            Pattern.EnumStrings "BILLING_FEATURE" [
                "SUBSCRIPTIONS"
                "SUBSCRIPTIONS_UPDATE"
                "IN_APP_ITEMS_ON_VR"
                "SUBSCRIPTIONS_ON_VR"
                "PRICE_CHANGE_CONFIRMATION"
            ]

        let PRORATION_MODE =
            Pattern.EnumStrings "PRORATION_MODE" [
                "UNKNOWN_SUBSCRIPTION_UPGRADE_DOWNGRADE_POLICY"
                "IMMEDIATE_WITH_TIME_PRORATION"
                "IMMEDIATE_AND_CHARGE_PRORATED_PRICE"
                "IMMEDIATE_WITHOUT_PRORATION"
                "DEFERRED"
            ]

        let PACKAGE_TYPE =
            Pattern.EnumStrings "PACKAGE_TYPE" [
                "UNKNOWN"
                "CUSTOM"
                "LIFETIME"
                "ANNUAL"
                "SIX_MONTH"
                "THREE_MONTH"
                "TWO_MONTH"
                "MONTHLY"
                "WEEKLY"
            ]

        let INTRO_ELIGIBILITY_STATUS =
            Pattern.EnumStrings "INTRO_ELIGIBILITY_STATUS" [
                "INTRO_ELIGIBILITY_STATUS_UNKNOWN"
                "INTRO_ELIGIBILITY_STATUS_INELIGIBLE"
                "INTRO_ELIGIBILITY_STATUS_ELIGIBLE"
            ]

        let SubscriptionPeriod =
            Pattern.Config "SubscriptionPeriod" {
                Required = [
                    "numberOfUnits", T<int>
                    "unit", T<int>
                ]
                Optional = []
            }

        let SKProductDiscount =
            Pattern.Config "SKProductDiscount" {
                Required = [
                    "identifier", T<string>
                    "type", T<int>
                    "price", T<float>
                    "priceString", T<string>
                    "currencySymbol", T<string>
                    "currencyCode", T<string>
                    "paymentMode", T<int>
                    "numberOfPeriods", T<int>
                    "subscriptionPeriod", SubscriptionPeriod.Type
                ]
                Optional = []
            }

        let Transaction =
            Pattern.Config "Transaction" {
                Required = [
                    "transactionId", T<string>
                ]
                Optional = []
            }

        let CustomerInfo =
            Pattern.Config "CustomerInfo" {
                Required = [
                    "activeSubscriptions", T<string[]>
                    "allPurchasedProductIdentifiers", T<string[]>
                    "nonSubscriptionTransactions", !| Transaction.Type
                    "firstSeen", T<string>
                    "originalAppUserId", T<string>
                    "requestDate", T<string>
                ]
                Optional = [
                    "latestExpirationDate", T<string>
                    "originalApplicationVersion", T<string>
                    "originalPurchaseDate", T<string>
                    "managementURL", T<string>
                ]
            }

        let Product =
            Pattern.Config "Product" {
                Required = [
                    "identifier", T<string>
                    "description", T<string>
                    "title", T<string>
                    "price", T<float>
                    "priceString", T<string>
                    "currencyCode", T<string>
                    "currencySymbol", T<string>
                    "isFamilyShareable", T<bool>
                    "subscriptionGroupIdentifier", T<string>
                    "subscriptionPeriod", SubscriptionPeriod.Type
                    "discounts", !| SKProductDiscount.Type
                ]
                Optional = [
                    "introductoryPrice", SKProductDiscount.Type
                ]
            }

        let PurchaseProductOptions = 
            Pattern.Config "PurchaseProductOptions" {
                Required = [
                    "productIdentifier", T<string>
                ]
                Optional = [
                    "planIdentifier", T<string>
                    "productType", PURCHASE_TYPE.Type
                    "quantity", T<int>
                ]
            }

        let GetProductsOptions = 
            Pattern.Config "GetProductsOptions" {
                Required = [
                    "productIdentifiers", T<string[]>
                ]
                Optional = [
                    "productType", PURCHASE_TYPE.Type
                ]
            }
               
        let GetProductOptions =
            Pattern.Config "GetProductOptions" {
                Required = [
                    "productIdentifier", T<string>
                ]
                Optional = [
                    "productType", PURCHASE_TYPE.Type
                ]
            }

        let NativePurchasesPlugin =
            Class "NativePurchasesPlugin"
            |+> Instance [
                "restorePurchases" => T<unit> ^-> T<Promise<_>>[CustomerInfo]

                "purchaseProduct" => PurchaseProductOptions.Type ^-> T<Promise<_>>[Transaction]

                "getProducts" => GetProductsOptions.Type ^-> T<Promise<_>>[!| Product]

                "getProduct" => GetProductOptions.Type ^-> T<Promise<_>>[Product]

                "isBillingSupported" => T<unit> ^-> T<Promise<_>>[T<bool>]

                "getPluginVersion" => T<unit> ^-> T<Promise<_>>[T<string>]
            ]
            
    let CapacitorCapGo = 
        Class "CapacitorCapGo"
        |+> Static [
            "SocialLogin" =? SocialLoginPlugin
            |> Import "SocialLogin" "@capgo/capacitor-social-login"
            "Uploader" =? UploaderPlugin
            |> Import "Uploader" "@capgo/capacitor-uploader"
            "NavigationBar" =? NavigationBarPlugin 
            |> Import "NavigationBar" "@capgo/capacitor-navigation-bar"
            "CapacitorDataStorageSqlite" =? CapacitorDataStorageSqlitePlugin 
            |> Import "CapacitorDataStorageSqlite" "@capgo/capacitor-data-storage-sqlite"
            "ScreenRecorder" =? ScreenRecorderPlugin 
            |> Import "ScreenRecorder" "@capgo/capacitor-screen-recorder"
            "NativePurchases" =? NativePurchasesPlugin 
            |> Import "NativePurchases" "@capgo/native-purchases"
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.Capacitor.CapGo" [
                    CapacitorCapGo
                    PluginListenerHandle

                    SocialLoginPlugin
                    UploaderPlugin                 
                    NavigationBarPlugin
                    CapacitorDataStorageSqlitePlugin
                    ScreenRecorderPlugin
                    NativePurchasesPlugin
            ]
            Namespace "WebSharper.Capacitor.CapGo.SocialLogin" [
                IsLoggedInResult; LogoutOptions; LoginResult; LoginOptions; AppleProviderResponse; AppleProfile
                GoogleLoginResponse; GoogleProfile; FacebookLoginResponse; FacebookProfile; Hometown; Location
                AgeRange; AccessToken; AppleProviderOptions; GoogleLoginOptions; FacebookLoginOptions
                InitializeOptions; IsLoggedInOptions; AuthorizationCodeOptions; AuthorizationCode; AppleOptions
                GoogleOptions; FacebookOptions; Provider
            ]
            Namespace "WebSharper.Capacitor.CapGo.Uploader" [
                StartUploadResult; UploadAddListener; RemoveUploadOptions; UploadEvent
                UploadEventPayload; UploadOption; UploadOptionMethod; UploadEventName
            ]
            Namespace "WebSharper.Capacitor.CapGo.NavigationBar" [
                GetNavigationBarColorResult; SetNavigationBarColorOptions
            ]
            Namespace "WebSharper.Capacitor.CapGo.CapacitorDataStorageSqlite" [
                CapStoreJson; CapStoreImportOptions; CapDataStorageChanges; JsonStore; JsonTable
                CapTablesResult; CapKeysValuesResult; CapValuesResult; CapKeysResult; CapValueResult
                CapDataStorageResult; CapEchoResult; CapFilterStorageOptions; CapTableStorageOptions
                CapStorageOptions; CapEchoOptions; CapOpenStorageOptions; CapDataStorageOptions
            ]
            Namespace "WebSharper.Capacitor.CapGo.NativePurchases" [
                GetProductOptions; GetProductsOptions; PurchaseProductOptions; Product
                CustomerInfo; Transaction; SKProductDiscount; SubscriptionPeriod
                INTRO_ELIGIBILITY_STATUS; PACKAGE_TYPE; PRORATION_MODE
                PURCHASE_TYPE; ATTRIBUTION_NETWORK; BILLING_FEATURE
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
