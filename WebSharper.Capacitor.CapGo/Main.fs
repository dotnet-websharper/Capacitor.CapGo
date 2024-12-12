namespace WebSharper.Capacitor.CapGo

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let TypeToUnitFunc schemaType = schemaType ^-> T<unit> 

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
                "addListener" => T<string>?eventName * (TypeToUnitFunc UploadEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
            ]

    [<AutoOpen>]
    module CapacitorNavigationBar =
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
    module CapacitorDataStorageSqlite=
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
    module ScreenRecorder= 
        let ScreenRecorderPlugin =
            Class "ScreenRecorderPlugin"
            |+> Instance [
                "start" => T<unit> ^-> T<Promise<unit>>
                "stop" => T<unit> ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module NativePurchases= 
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

    [<AutoOpen>]
    module CapacitorFlash=
        let IsAvailableResult =
            Pattern.Config "IsAvailableResult" {
                Required = [
                    "value", T<bool>
                ]
                Optional = []
            }

        let IsSwitchedOnResult =
            Pattern.Config "IsSwitchedOnResult" {
                Required = [
                    "value", T<bool>
                ]
                Optional = []
            }

        let ToggleResult =
            Pattern.Config "ToggleResult" {
                Required = [
                    "value", T<bool>
                ]
                Optional = []
            }

        let SwitchOnOptions =
            Pattern.Config "SwitchOnOptions" {
                Required = []
                Optional = [
                    "intensity", T<float>
                ]
            }

        let CapacitorFlashPlugin =
            Class "CapacitorFlashPlugin"
            |+> Instance [
                "isAvailable" => T<unit> ^-> T<Promise<_>>[IsAvailableResult]
                "switchOn" => SwitchOnOptions?options ^-> T<Promise<unit>>
                "switchOff" => T<unit> ^-> T<Promise<unit>>
                "isSwitchedOn" => T<unit> ^-> T<Promise<_>>[IsSwitchedOnResult]
                "toggle" => T<unit> ^-> T<Promise<_>>[ToggleResult]
            ]

    [<AutoOpen>]
    module CapacitorUpdater =

        let BundleStatus =
            Pattern.EnumStrings "BundleStatus" [
                "success"
                "error"
                "pending"
                "downloading"
            ]

        let DelayUntilNext =
            Pattern.EnumStrings "DelayUntilNext" [
                "background"
                "kill"
                "nativeVersion"
                "date"
            ]

        let ManifestEntry =
            Pattern.Config "ManifestEntry" {
                Required = []
                Optional = [
                    "file_name", T<string>
                    "file_hash", T<string>
                    "download_url", T<string>
                ]
            }

        let BundleInfo =
            Pattern.Config "BundleInfo" {
                Required = [
                    "id", T<string>
                    "version", T<string>
                    "downloaded", T<string>
                    "checksum", T<string>
                    "status", BundleStatus.Type
                ]
                Optional = []
            }

        let LatestVersion =
            Pattern.Config "LatestVersion" {
                Required = [
                    "version", T<string>
                ]
                Optional = [
                    "checksum", T<string>
                    "major", T<bool>
                    "message", T<string>
                    "sessionKey", T<string>
                    "error", T<string>
                    "old", T<string>
                    "url", T<string>
                    "manifest", !| ManifestEntry.Type
                ]
            }

        let NoNeedEvent =
            Pattern.Config "NoNeedEvent" {
                Required = [
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let UpdateAvailableEvent =
            Pattern.Config "UpdateAvailableEvent" {
                Required = [
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let ChannelRes =
            Pattern.Config "ChannelRes" {
                Required = [
                    "status", T<string>
                ]
                Optional = [
                    "error", T<string>
                    "message", T<string>
                ]
            }

        let GetChannelRes =
            Pattern.Config "GetChannelRes" {
                Required = []
                Optional = [
                    "channel", T<string>
                    "error", T<string>
                    "message", T<string>
                    "status", T<string>
                    "allowSet", T<bool>
                ]
            }

        let DownloadEvent =
            Pattern.Config "DownloadEvent" {
                Required = [
                    "percent", T<int>
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let MajorAvailableEvent =
            Pattern.Config "MajorAvailableEvent" {
                Required = [
                    "version", T<string>
                ]
                Optional = []
            }

        let DownloadFailedEvent =
            Pattern.Config "DownloadFailedEvent" {
                Required = [
                    "version", T<string>
                ]
                Optional = []
            }

        let DownloadCompleteEvent =
            Pattern.Config "DownloadCompleteEvent" {
                Required = [
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let UpdateFailedEvent =
            Pattern.Config "UpdateFailedEvent" {
                Required = [
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let AppReadyEvent =
            Pattern.Config "AppReadyEvent" {
                Required = [
                    "bundle", BundleInfo.Type
                    "status", T<string>
                ]
                Optional = []
            }

        let SetChannelOptions =
            Pattern.Config "SetChannelOptions" {
                Required = [
                    "channel", T<string>
                ]
                Optional = [
                    "triggerAutoUpdate", T<bool>
                ]
            }

        let UnsetChannelOptions =
            Pattern.Config "UnsetChannelOptions" {
                Required = []
                Optional = [
                    "triggerAutoUpdate", T<bool>
                ]
            }

        let SetCustomIdOptions =
            Pattern.Config "SetCustomIdOptions" {
                Required = [
                    "customId", T<string>
                ]
                Optional = []
            }

        let DelayCondition =
            Pattern.Config "DelayCondition" {
                Required = [
                    "kind", DelayUntilNext.Type
                ]
                Optional = [
                    "value", T<string>
                ]
            }

        let AppReadyResult =
            Pattern.Config "AppReadyResult" {
                Required = [
                    "bundle", BundleInfo.Type
                ]
                Optional = []
            }

        let UpdateUrl =
            Pattern.Config "UpdateUrl" {
                Required = [
                    "url", T<string>
                ]
                Optional = []
            }

        let StatsUrl =
            Pattern.Config "StatsUrl" {
                Required = [
                    "url", T<string>
                ]
                Optional = []
            }

        let ChannelUrl =
            Pattern.Config "ChannelUrl" {
                Required = [
                    "url", T<string>
                ]
                Optional = []
            }

        let DownloadOptions =
            Pattern.Config "DownloadOptions" {
                Required = [
                    "url", T<string>
                    "version", T<string>
                ]
                Optional = [
                    "sessionKey", T<string>
                    "checksum", T<string>
                ]
            }

        let BundleId =
            Pattern.Config "BundleId" {
                Required = [
                    "id", T<string>
                ]
                Optional = []
            }

        let BundleListResult =
            Pattern.Config "BundleListResult" {
                Required = [
                    "bundles", !| BundleInfo.Type
                ]
                Optional = []
            }

        let ResetOptions =
            Pattern.Config "ResetOptions" {
                Required = [
                    "toLastSuccessful", T<bool>
                ]
                Optional = []
            }

        let CurrentBundleResult =
            Pattern.Config "CurrentBundleResult" {
                Required = [
                    "bundle", BundleInfo.Type
                    "naative", T<string>
                ]
                Optional = []
            }

        let MultiDelayConditions =
            Pattern.Config "MultiDelayConditions" {
                Required = [
                    "delayConditions", !| DelayCondition.Type
                ]
                Optional = []
            }

        let BuiltinVersion =
            Pattern.Config "BuiltinVersion" {
                Required = [
                    "version", T<string>
                ]
                Optional = []
            }

        let DeviceId =
            Pattern.Config "DeviceId" {
                Required = [
                    "deviceId", T<string>
                ]
                Optional = []
            }

        let PluginVersion =
            Pattern.Config "PluginVersion" {
                Required = [
                    "version", T<string>
                ]
                Optional = []
            }

        let AutoUpdateEnabled =
            Pattern.Config "AutoUpdateEnabled" {
                Required = [
                    "enabled", T<bool>
                ]
                Optional = []
            }

        let AutoUpdateAvailable =
            Pattern.Config "AutoUpdateAvailable" {
                Required = [
                    "available", T<bool>
                ]
                Optional = []
            }

        let CapacitorUpdaterPlugin =
            Class "CapacitorUpdaterPlugin"
            |+> Instance [
                "notifyAppReady" => T<unit> ^-> T<Promise<_>>[AppReadyResult]
                "setUpdateUrl" => UpdateUrl?options ^-> T<Promise<unit>>
                "setStatsUrl" => StatsUrl?options ^-> T<Promise<unit>>
                "setChannelUrl" => ChannelUrl?options ^-> T<Promise<unit>>
                "download" => DownloadOptions?options ^-> T<Promise<_>>[BundleInfo]
                "next" => BundleId?options ^-> T<Promise<_>>[BundleInfo]
                "set" => BundleId?options ^-> T<Promise<unit>>
                "delete" => BundleId?options ^-> T<Promise<unit>>
                "list" => T<unit> ^-> T<Promise<_>>[BundleListResult]
                "reset" => !?ResetOptions?options ^-> T<Promise<unit>>
                "current" => T<unit> ^-> T<Promise<_>>[CurrentBundleResult]
                "reload" => T<unit> ^-> T<Promise<unit>>
                "setMultiDelay" => MultiDelayConditions?options ^-> T<Promise<unit>>
                "cancelDelay" => T<unit> ^-> T<Promise<unit>>
                "getLatest" => T<unit> ^-> T<Promise<_>>[LatestVersion]
                "setChannel" => SetChannelOptions?options ^-> T<Promise<_>>[ChannelRes]
                "unsetChannel" => UnsetChannelOptions?options ^-> T<Promise<unit>>
                "getChannel" => T<unit> ^-> T<Promise<_>>[GetChannelRes]
                "setCustomId" => SetCustomIdOptions?options ^-> T<Promise<unit>>
                "getBuiltinVersion" => T<unit> ^-> T<Promise<_>>[BuiltinVersion]
                "getDeviceId" => T<unit> ^-> T<Promise<_>>[DeviceId]
                "getPluginVersion" => T<unit> ^-> T<Promise<_>>[PluginVersion]
                "isAutoUpdateEnabled" => T<unit> ^-> T<Promise<_>>[AutoUpdateEnabled]
                "removeAllListeners" => T<unit> ^-> T<Promise<unit>>
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'download'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc NoNeedEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'noNeedUpdate'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc UpdateAvailableEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'updateAvailable'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadCompleteEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'downloadComplete'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc MajorAvailableEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'majorAvailable'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc UpdateFailedEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'updateFailed'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadFailedEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'downloadFailed'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc T<unit>)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'appReloaded'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc AppReadyEvent)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'appReady'."
                "isAutoUpdateAvailable" => T<unit> ^-> T<Promise<_>>[AutoUpdateAvailable]
            ]    

    [<AutoOpen>]
    module HomeIndicator =

        let IsHiddenResult = 
            Pattern.Config "IsHiddenResult" {
                Required = ["hidden", T<bool>]
                Optional = []
            }

        let PluginVersion = 
            Pattern.Config "PluginVersion" {
                Required = ["version", T<string>]
                Optional = []
            }

        let HomeIndicatorPlugin =
            Class "HomeIndicatorPlugin"
            |+> Instance [
                "hide" => T<unit> ^-> T<Promise<unit>>
                "show" => T<unit> ^-> T<Promise<unit>>
                "isHidden" => T<unit> ^-> T<Promise<_>>[IsHiddenResult]
                "getPluginVersion" => T<unit> ^-> T<Promise<_>>[PluginVersion]
            ]

    [<AutoOpen>]
    module NativeGeocoder=

        let Address =
            Pattern.Config "Address" {
                Required = [
                    "latitude", T<float>
                    "longitude", T<float>
                    "countryCode", T<string>
                    "countryName", T<string>
                    "postalCode", T<string>
                    "administrativeArea", T<string>
                    "subAdministrativeArea", T<string>
                    "locality", T<string>
                    "subLocality", T<string>
                    "thoroughfare", T<string>
                    "subThoroughfare", T<string>
                    "areasOfInterest", T<string[]>
                ]
                Optional = []
            }

        let ForwardOptions =
            Pattern.Config "ForwardOptions" {
                Required = [
                    "addressString", T<string>
                ]
                Optional = [
                    "useLocale", T<bool>
                    "defaultLocale", T<string>
                    "maxResults", T<int>
                    "apiKey", T<string>
                ]
            }

        let ReverseOptions =
            Pattern.Config "ReverseOptions" {
                Required = [
                    "latitude", T<float>
                    "longitude", T<float>
                ]
                Optional = [
                    "useLocale", T<bool>
                    "defaultLocale", T<string>
                    "maxResults", T<int>
                    "apiKey", T<string>
                    "resultType", T<string>
                ]
            }

        let ReverseGeocodeResult = 
            Pattern.Config "ReverseGeocodeResult" {
                Required = [
                    "addresses", !| Address.Type
                ]
                Optional = []
            }

        let ForwardGeocodeResult = 
            Pattern.Config "ForwardGeocodeResult" {
                Required = [
                    "addresses", !| Address.Type
                ]
                Optional = []
            }

        let NativeGeocoderPlugin =
            Class "NativeGeocoderPlugin"
            |+> Instance [
                "reverseGeocode" => ReverseOptions?options ^-> T<Promise<_>>[ReverseGeocodeResult]
                "forwardGeocode" => ForwardOptions?options ^-> T<Promise<_>>[ForwardGeocodeResult]
            ]

    [<AutoOpen>]
    module CapacitorCrisp=

        let EventColor =
            Pattern.EnumStrings "EventColor" [
                "red"
                "orange"
                "yellow"
                "green"
                "blue"
                "purple"
                "pink"
                "brown"
                "grey"
                "black"
            ]

        let ConfigureData =
            Pattern.Config "ConfigureData" {
                Required = [
                    "websiteID", T<string>
                ]
                Optional = []
            }

        let SetTokenIDData =
            Pattern.Config "SetTokenIDData" {
                Required = [
                    "tokenID", T<string>
                ]
                Optional = []
            }

        let SetUserData =
            Pattern.Config "SetUserData" {
                Required = []
                Optional = [
                    "nickname", T<string>
                    "phone", T<string>
                    "email", T<string>
                    "avatar", T<string>
                ]
            }

        let PushEventData =
            Pattern.Config "PushEventData" {
                Required = [
                    "name", T<string>
                    "color", EventColor.Type
                ]
                Optional = []
            }

        let Employment = 
            Pattern.Config "Employment" {
                Required = [
                    "title", T<string>
                    "role", T<string>
                ]
                Optional = []
            }

        let Geolocation = 
            Pattern.Config "Geolocation" {
                Required = [
                    "country", T<string>
                    "city", T<string>
                ]
                Optional = []
            }

        let SetCompanyData =
            Pattern.Config "SetCompanyData" {
                Required = [
                    "name", T<string>
                ]
                Optional = [
                    "url", T<string>
                    "description", T<string>
                    "employment", !| Employment.Type
                    "geolocation", !| Geolocation.Type
                ]
            }

        let SetIntData =
            Pattern.Config "SetIntData" {
                Required = [
                    "key", T<string>
                    "value", T<int>
                ]
                Optional = []
            }

        let SetStringData =
            Pattern.Config "SetStringData" {
                Required = [
                    "key", T<string>
                    "value", T<string>
                ]
                Optional = []
            }

        let SendMessageData =
            Pattern.Config "SendMessageData" {
                Required = [
                    "value", T<string>
                ]
                Optional = []
            }

        let SetSegmentData =
            Pattern.Config "SetSegmentData" {
                Required = [
                    "segment", T<string>
                ]
                Optional = []
            }

        let CapacitorCrispPlugin =
            Class "CapacitorCrispPlugin"
            |+> Instance [
                "configure" => ConfigureData?data ^-> T<Promise<unit>>
                "openMessenger" => T<unit> ^-> T<Promise<unit>>
                "setTokenID" => SetTokenIDData?data ^-> T<Promise<unit>>
                "setUser" => SetUserData?data ^-> T<Promise<unit>>
                "pushEvent" => PushEventData?data ^-> T<Promise<unit>>
                "setCompany" => SetCompanyData?data ^-> T<Promise<unit>>
                "setInt" => SetIntData?data ^-> T<Promise<unit>>
                "setString" => SetStringData?data ^-> T<Promise<unit>>
                "sendMessage" => SendMessageData?data ^-> T<Promise<unit>>
                "setSegment" => SetSegmentData?data ^-> T<Promise<unit>>
                "reset" => T<unit> ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module CapacitorMute = 
        let MuteResponse =
            Pattern.Config "MuteResponse" {
                Required = [
                    "value", T<bool>
                ]
                Optional = []
            }

        let CapacitorMutePlugin =
            Class "CapacitorMutePlugin"
            |+> Instance [
                "isMuted" => T<unit> ^-> T<Promise<_>>[MuteResponse]
            ]

    [<AutoOpen>]
    module NativeAudio = 
        let CompletedEvent = 
            Pattern.Config "CompletedEvent" {
                Required = [
                    "assetId", T<string>
                ]
                Optional = []
            }

        let CompletedListener = TypeToUnitFunc CompletedEvent

        let Assets = 
            Pattern.Config "Assets" {
                Required = [
                    "assetId", T<string>
                ]
                Optional = []
            }

        let AssetVolume = 
            Pattern.Config "AssetVolume" {
                Required = [
                    "assetId", T<string>
                    "volume", T<int>
                ]
                Optional = []
            }

        let AssetPlayOptions = 
            Pattern.Config "AssetPlayOptions" {
                Required = [
                    "assetId", T<string>
                ]
                Optional = [
                    "time", T<int>
                    "delay", T<int>
                ]
            }

        let ConfigureOptions = 
            Pattern.Config "ConfigureOptions" {
                Required = []
                Optional = [
                    "fade", T<bool>
                    "focus", T<bool>
                    "background", T<bool>
                ]
            }

        let PreloadOptions = 
            Pattern.Config "PreloadOptions" {
                Required = [
                    "assetPath", T<string>
                    "assetId", T<string>
                ]
                Optional = [
                    "volume", T<int>
                    "audioChannelNum", T<int>
                    "isUrl", T<bool>
                ]
            }

        let isPreloadedResult = 
            Pattern.Config "isPreloadedResult" {
                Required = [
                    "found", T<bool>
                ]
                Optional = []
            }

        let PlayOptions = 
            Pattern.Config "PlayOptions" {
                Required = [
                    "assetId", T<string>
                ]
                Optional = [
                    "time", T<int>
                    "delay", T<int>
                ]
            }

        let SetVolumeOptions = 
            Pattern.Config "SetVolumeOptions" {
                Required = [
                    "assetId", T<string>
                    "volume", T<int>
                ]
                Optional = []
            }

        let SetRateOptions = 
            Pattern.Config "SetRateOptions" {
                Required = [
                    "assetId", T<string>
                    "rate", T<int>
                ]
                Optional = []
            }   

        let GetCurrentTimeResult = 
            Pattern.Config "GetCurrentTimeResult" {
                Required = [
                    "currentTime", T<int>
                ]
                Optional = []
            }

        let GetDurationResult = 
            Pattern.Config "GetDurationResult" {
                Required = [
                    "duration", T<int>
                ]
                Optional = []
            }

        let IsPlayingResult = 
            Pattern.Config "IsPlayingResult" {
                Required = [
                    "isPlaying", T<bool>
                ]
                Optional = []
            }

        let NativeAudioPlugin = 
            Class "NativeAudioPlugin"
            |+> Instance [
                "configure" => ConfigureOptions?options ^-> T<Promise<unit>>
                "preload" => PreloadOptions?options ^-> T<Promise<unit>>
                "isPreloaded" => PreloadOptions?options ^-> T<Promise<_>>[isPreloadedResult]
                "play" => PlayOptions?options ^-> T<Promise<unit>>
                "pause" => Assets?options ^-> T<Promise<unit>>
                "resume" => Assets?options ^-> T<Promise<unit>>
                "loop" => Assets?options ^-> T<Promise<unit>>
                "stop" => Assets?options ^-> T<Promise<unit>>
                "unload" => Assets?options ^-> T<Promise<unit>>
                "setVolume" => SetVolumeOptions?options ^-> T<Promise<unit>>
                "setRate" => SetRateOptions?options ^-> T<Promise<unit>>
                "getCurrentTime" => Assets?options ^-> T<Promise<_>>[GetCurrentTimeResult]
                "getDuration" => Assets?options ^-> T<Promise<_>>[GetDurationResult]
                "isPlaying" => Assets?options ^-> T<Promise<_>>[IsPlayingResult]
                "addListener" => T<string>?eventName * CompletedListener?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'complete'."
            ]

    [<AutoOpen>]
    module CapacitorDownloader=

        let DownloadTaskState =
            Pattern.EnumStrings "DownloadTaskState" [
                "PENDING"
                "RUNNING"
                "PAUSED"
                "DONE"
                "ERROR"
            ]

        let NetworkType =
            Pattern.EnumStrings "NetworkType" [
                "cellular"
                "wifi-only"
            ]

        let PriorityType =
            Pattern.EnumStrings "PriorityType" [
                "high"
                "normal"
                "low"
            ]

        let DownloadTask =
            Pattern.Config "DownloadTask" {
                Required = [
                    "id", T<string>
                    "progress", T<float>
                    "state", DownloadTaskState.Type
                ]
                Optional = []
            }

        let DownloadOptions =
            Pattern.Config "DownloadOptions" {
                Required = [
                    "id", T<string>
                    "url", T<string>
                    "destination", T<string>
                ]
                Optional = [
                    "headers", T<obj[]>
                    "network", NetworkType.Type
                    "priority", PriorityType.Type
                ]
            }

        let DownloadProgress =
            Pattern.Config "DownloadProgress" {
                Required = [
                    "id", T<string>
                    "progress", T<float>
                ]
                Optional = []
            }

        let DownloadCompleted =
            Pattern.Config "DownloadCompleted" {
                Required = [
                    "id", T<string>
                ]
                Optional = []
            }

        let DownloadFailed =
            Pattern.Config "DownloadFailed" {
                Required = [
                    "id", T<string>
                    "error", T<string>
                ]
                Optional = []
            }

        let FileInfo =
            Pattern.Config "FileInfo" {
                Required = [
                    "size", T<int>
                    "type", T<string>
                ]
                Optional = []
            }

        let CapacitorDownloaderPlugin =
            Class "CapacitorDownloaderPlugin"
            |+> Instance [
                "download" => DownloadOptions?options ^-> T<Promise<_>>[DownloadTask]
                "pause" => T<string>?id ^-> T<Promise<unit>>
                "resume" => T<string>?id ^-> T<Promise<unit>>
                "stop" => T<string>?id ^-> T<Promise<unit>>
                "checkStatus" => T<string>?id ^-> T<Promise<_>>[DownloadTask]
                "getFileInfo" => T<string>?path ^-> T<Promise<_>>[FileInfo]
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadProgress)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'downloadProgress'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadCompleted)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'downloadCompleted'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc DownloadFailed)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'downloadFailed'."
                "removeAllListeners" => T<unit> ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module CapacitorShake = 
        let CapacitorShakePlugin = 
            Class "CapacitorShakePlugin"
            |+> Instance [
                "addListener" => T<string>?eventName * (TypeToUnitFunc T<unit>)?listenerFunc ^-> T<Promise<_>>[PluginListenerHandle]
            ]

    [<AutoOpen>]
    module IvsPlayer=
        let CapacitorIvsPlayerState =
            Pattern.EnumStrings "CapacitorIvsPlayerState" [
                "IDLE"
                "BUFFERING"
                "READY"
                "PLAYING"
                "ENDED"
                "UNKNOWN"
            ]

        let CapacitorIvsPlayerBackgroundState =
            Pattern.EnumStrings "CapacitorIvsPlayerBackgroundState" [
                "PAUSED"
                "PLAYING"
            ]

        let CapacitorFrame =
            Pattern.Config "CapacitorFrame" {
                Required = [
                    "x", T<float>
                    "y", T<float>
                    "width", T<float>
                    "height", T<float>
                ]
                Optional = []
            }

        let DownloadProgress =
            Pattern.Config "DownloadProgress" {
                Required = [
                    "id", T<string>
                    "progress", T<float>
                ]
                Optional = []
            }

        let CreateOptions = 
            Pattern.Config "CreateOptions" {
                Required = [
                    "url", T<string>
                ]
                Optional = [
                    "pip", T<bool>
                    "title", T<string>
                    "subtitle", T<string>
                    "cover", T<string>
                    "autoPlay", T<bool>
                    "toBack", T<bool>
                    "x", T<float>
                    "y", T<float>
                    "width", T<float>
                    "height", T<float>
                ]
            }

        let CastStatus = 
            Pattern.Config "CastStatus" {
                Required = [
                    "isActive", T<bool>
                ]
                Optional = []
            }

        let UrlResult = 
            Pattern.Config "UrlResult" {
                Required = [
                    "url", T<string>
                ]
                Optional = []
            }

        let StateResult = 
            Pattern.Config "StateResult" {
                Required = [
                    "state", CapacitorIvsPlayerState.Type
                ]
                Optional = []
            }

        let PlayerPositionOptions = 
            Pattern.Config "PlayerPositionOptions" {
                Optional = [
                    "toBack", T<bool>
                ]
                Required = []
            } 

        let PlayerPositionResult = 
            Pattern.Config "PlayerPositionResult" {
                Required = [
                    "toBack", T<bool>
                ]
                Optional = []
            }

        let AutoQualityOptions = 
             Pattern.Config "AutoQualityOptions" {
                Required = []
                Optional = [
                    "autoQuality", T<bool>
                ]
            }

        let AutoQualityResult = 
            Pattern.Config "AutoQualityResult" {
                Required = [
                    "autoQuality", T<bool>
                ]
                Optional = []
            }

        let PipOptions = 
            Pattern.Config "PipOptions" {
                Required = []
                Optional = [
                    "pip", T<bool>
                ]
            }

        let PipResult = 
            Pattern.Config "PipResult" {
                Required = [
                    "pip", T<bool>
                ]
                Optional = []
            }

        let FrameOptions = 
            Pattern.Config "FrameOptions" {
                Required = []
                Optional = [
                    "x", T<float>
                    "y", T<float>
                    "width", T<float>
                    "height", T<float>
                ]
            }

        let BackgroundStateOptions = 
            Pattern.Config "BackgroundStateOptions" {
                Required = [
                    "backgroundState", CapacitorIvsPlayerBackgroundState.Type
                ]
                Optional = []
            }

        let BackgroundStateResult = 
            Pattern.Config "BackgroundStateResult" {
                Required = [
                    "backgroundState", CapacitorIvsPlayerBackgroundState.Type
                ]
                Optional = []
            }

        let MuteOptions = 
            Pattern.Config "MuteOptions" {
                Required = []
                Optional = [
                    "muted", T<bool>
                ]
            }

        let MuteResult = 
            Pattern.Config "MuteResult" {
                Required = [
                    "mute", T<bool>
                ]
                Optional = []
            }

        let QualityOptions = 
            Pattern.Config "QualityOptions" {
                Optional = [
                    "quality", T<string>
                ]
                Required = []
            }

        let QualityResult = 
            Pattern.Config "QualityResult" {
                Required = [
                    "quality", T<string>
                ]
                Optional = []
            }

        let QualitiesResult = 
            Pattern.Config "QualitiesResult" {
                Required = [
                    "qualities", T<string[]>
                ]
                Optional = []
            }

        let PluginVersionResult = 
            Pattern.Config "PluginVersionResult" {
                Required = [
                    "version", T<string>
                ]
                Optional = []
            }

        let OnStateListener = 
            Pattern.Config "OnStateListener" {
                Required = [
                    "state", CapacitorIvsPlayerState.Type
                ]
                Optional = []
            }

        let OnCuesListener = 
            Pattern.Config "OnCuesListener" {
                Required = [
                    "cues", T<string>
                ]
                Optional = []
            }

        let OnDurationListener = 
            Pattern.Config "OnDurationListener" {
                Required = [
                    "duration", T<int>
                ]
                Optional = []
            }

        let OnErrorListener = 
            Pattern.Config "OnErrorListener" {
                Required = [
                    "error", T<string>
                ]
                Optional = []
            }

        let OnSeekCompletedListener = 
            Pattern.Config "OnSeekCompletedListener" {
                Required = [
                    "position", T<int>
                ]
                Optional = []
            }

        let OnVideoSizeListener = 
            Pattern.Config "onVideoSizeListener" {
                Required = [
                    "width", T<int>
                    "height", T<int>
                ]
                Optional = []
            }

        let OnQualityListener = 
            Pattern.Config "OnQualityListener" {
                Required = [
                    "quality", T<string>
                ]
                Optional = []
            }

        let OnCastStatusListener = 
            Pattern.Config "OnCastStatusListener" {
                Required = [
                    "isActive", T<bool>
                ]
                Optional = []
            }

        let IvsPlayerPlugin =
            Class "CapacitorIvsPlayerPlugin"
            |+> Instance [
                "create" => CreateOptions?options ^-> T<Promise<unit>>
                "start" => T<unit> ^-> T<Promise<unit>>
                "cast" => T<unit> ^-> T<Promise<unit>>
                "getCastStatus" => T<unit> ^-> T<Promise<_>>[CastStatus]
                "pause" => T<unit> ^-> T<Promise<unit>>
                "delete" => T<unit> ^-> T<Promise<unit>>
                "getUrl" => T<unit> ^-> T<Promise<_>>[UrlResult]
                "getState" => T<unit> ^-> T<Promise<_>>[StateResult]
                "setPlayerPosition" => !? PlayerPositionOptions?options ^-> T<Promise<unit>>
                "getPlayerPosition" => T<unit> ^-> T<Promise<_>>[PlayerPositionResult]
                "setAutoQuality" => !? AutoQualityOptions?options ^-> T<Promise<unit>>
                "getAutoQuality" => T<unit> ^-> T<Promise<_>>[AutoQualityResult]
                "setPip" => !? PipOptions?options^-> T<Promise<unit>>
                "getPip" => T<unit> ^-> T<Promise<_>>[PipResult]
                "setFrame" => !? FrameOptions?options ^-> T<Promise<unit>>
                "getFrame" => T<unit> ^-> T<Promise<_>>[CapacitorFrame.Type]
                "setBackgroundState" => BackgroundStateOptions?options ^-> T<Promise<unit>>
                "getBackgroundState" => T<unit> ^-> T<Promise<_>>[BackgroundStateResult]
                "setMute" => !? MuteOptions?options ^-> T<Promise<unit>>
                "getMute" => T<unit> ^-> T<Promise<_>>[MuteResult]
                "setQuality" => !? QualityOptions?options ^-> T<Promise<unit>>
                "getQuality" => T<unit> ^-> T<Promise<_>>[QualityResult]
                "getQualities" => T<unit> ^-> T<Promise<_>>[QualitiesResult]
                "getPluginVersion" => T<unit> ^-> T<Promise<_>>[PluginVersionResult]
                "addListener" => T<string>?eventName * (TypeToUnitFunc T<unit>)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event names 'startPip', 'stopPip', 'expandPip', 'closePip' and 'onRebuffering'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnStateListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onState'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnCuesListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onCues'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnDurationListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onDuration'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnErrorListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onError'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnSeekCompletedListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onSeekCompleted'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnVideoSizeListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onVideoSize'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnQualityListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onQuality'."
                "addListener" => T<string>?eventName * (TypeToUnitFunc OnCastStatusListener)?listenerFunc^-> T<Promise<_>>[PluginListenerHandle]
                |> WithComment "For event name 'onCastStatus'."
                "removeAllListeners" => T<unit> ^-> T<Promise<unit>>
            ]

    [<AutoOpen>]
    module NativeMarketPlugin =
        let OpenStoreListingOptions =
            Pattern.Config "OpenStoreListingOptions" {
                Required = [
                    "appId", T<string>
                ]
                Optional = [
                    "country", T<string>
                ]
            }

        let OpenDevPageOptions =
            Pattern.Config "OpenDevPageOptions" {
                Required = [
                    "devId", T<string>
                ]
                Optional = []
            }

        let OpenCollectionOptions =
            Pattern.Config "OpenCollectionOptions" {
                Required = [
                    "name", T<string>
                ]
                Optional = []
            }

        let OpenEditorChoicePageOptions =
            Pattern.Config "OpenEditorChoicePageOptions" {
                Required = [
                    "editorChoice", T<string>
                ]
                Optional = []
            }

        let SearchOptions =
            Pattern.Config "SearchOptions" {
                Required = [
                    "terms", T<string>
                ]
                Optional = []
            }

        let NativeMarketPlugin =
            Class "NativeMarketPlugin"
            |+> Instance [
                "openStoreListing" => OpenStoreListingOptions?options ^-> T<Promise<unit>>
                "openDevPage" => OpenDevPageOptions?options ^-> T<Promise<unit>>
                "openCollection" => OpenCollectionOptions?options ^-> T<Promise<unit>>
                "openEditorChoicePage" => OpenEditorChoicePageOptions?options ^-> T<Promise<unit>>
                "search" => SearchOptions?options ^-> T<Promise<unit>>
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
            "CapacitorFlash" =? CapacitorFlashPlugin 
            |> Import "CapacitorFlash" "@capgo/capacitor-flash"
            "CapacitorUpdater" =? CapacitorUpdaterPlugin 
            |> Import "CapacitorUpdater" "@capgo/capacitor-updater"
            "HomeIndicator" =? HomeIndicatorPlugin 
            |> Import "HomeIndicator" "@capgo/home-indicator"
            "NativeGeocoder" =? NativeGeocoderPlugin 
            |> Import "NativeGeocoder" "@capgo/nativegeocoder"
            "CapacitorCrisp" =? CapacitorCrispPlugin 
            |> Import "CapacitorCrisp" "@capgo/capacitor-crisp"
            "CapacitorMute" =? CapacitorMutePlugin 
            |> Import "CapacitorMute" "@capgo/capacitor-mute"
            "NativeAudio" =? NativeAudioPlugin 
            |> Import "NativeAudio" "@capgo/native-audio"
            "CapacitorDownloader" =? CapacitorDownloaderPlugin
            |> Import "CapacitorDownloader" "@capgo/capacitor-downloader"
            "CapacitorShake" =? CapacitorShakePlugin
            |> Import "CapacitorShake" "@capgo/capacitor-shake"
            "IvsPlayer" =? IvsPlayerPlugin
            |> Import "IvsPlayer" "@capgo/ivs-player"
            "NativeMarket" =? NativeMarketPlugin
            |> Import "NativeMarket" "@capgo/native-market"
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
                    CapacitorFlashPlugin
                    CapacitorUpdaterPlugin
                    HomeIndicatorPlugin
                    NativeGeocoderPlugin
                    CapacitorCrispPlugin
                    CapacitorMutePlugin
                    NativeAudioPlugin
                    CapacitorDownloaderPlugin
                    CapacitorShakePlugin
                    IvsPlayerPlugin
                    NativeMarketPlugin
            ]
            Namespace "WebSharper.Capacitor.CapGo.SocialLogin" [
                IsLoggedInResult; LogoutOptions; LoginResult; LoginOptions; AppleProviderResponse; AppleProfile
                GoogleLoginResponse; GoogleProfile; FacebookLoginResponse; FacebookProfile; Hometown; Location
                AgeRange; AccessToken; AppleProviderOptions; GoogleLoginOptions; FacebookLoginOptions
                InitializeOptions; IsLoggedInOptions; AuthorizationCodeOptions; AuthorizationCode; AppleOptions
                GoogleOptions; FacebookOptions; Provider
            ]
            Namespace "WebSharper.Capacitor.CapGo.Uploader" [
                StartUploadResult; RemoveUploadOptions; UploadEvent
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
            Namespace "WebSharper.Capacitor.CapGo.CapacitorFlash" [
                SwitchOnOptions; ToggleResult; IsSwitchedOnResult; IsAvailableResult
            ]
            Namespace "WebSharper.Capacitor.CapGo.CapacitorUpdater" [
                AppReadyEvent; UpdateFailedEvent; DownloadCompleteEvent; DownloadFailedEvent
                MajorAvailableEvent; DownloadEvent; GetChannelRes; ChannelRes; UpdateAvailableEvent
                NoNeedEvent; LatestVersion; BundleInfo; ManifestEntry; DelayUntilNext
                BundleStatus; AutoUpdateAvailable; AutoUpdateEnabled; CapacitorUpdater.PluginVersion
                DeviceId; BuiltinVersion; MultiDelayConditions; CurrentBundleResult
                ResetOptions; BundleListResult; BundleId; CapacitorUpdater.DownloadOptions; ChannelUrl
                StatsUrl; UpdateUrl; AppReadyResult; DelayCondition; SetCustomIdOptions
                UnsetChannelOptions; SetChannelOptions
            ]
            Namespace "WebSharper.Capacitor.CapGo.HomeIndicator" [
                HomeIndicator.PluginVersion; IsHiddenResult
            ]
            Namespace "WebSharper.Capacitor.CapGo.NativeGeocoder" [
                Address; ForwardOptions; ReverseOptions; ReverseGeocodeResult; ForwardGeocodeResult
            ]
            Namespace "WebSharper.Capacitor.CapGo.CapacitorCrisp" [
                SetSegmentData; SendMessageData; SetStringData; SetIntData; SetCompanyData
                Geolocation; Employment; PushEventData; SetUserData; SetTokenIDData
                ConfigureData; EventColor
            ]
            Namespace "WebSharper.Capacitor.CapGo.CapacitorMute" [
                MuteResponse
            ]
            Namespace "WebSharper.Capacitor.CapGo.NativeAudio" [
                IsPlayingResult; GetDurationResult; GetCurrentTimeResult
                SetRateOptions; SetVolumeOptions; PlayOptions; isPreloadedResult
                PreloadOptions; ConfigureOptions; AssetPlayOptions
                AssetVolume; Assets; CompletedEvent
            ]
            Namespace "WebSharper.Capacitor.CapGo.CapacitorDownloader" [
                FileInfo; DownloadFailed; DownloadCompleted;NetworkType
                CapacitorDownloader.DownloadOptions; DownloadTask; PriorityType
                DownloadTaskState; CapacitorDownloader.DownloadProgress
            ]
            Namespace "WebSharper.Capacitor.CapGo.IvsPlayer" [
                PluginVersionResult; QualitiesResult; QualityResult
                QualityOptions; MuteResult; MuteOptions; BackgroundStateResult
                BackgroundStateOptions; FrameOptions; PipResult; PipOptions
                AutoQualityResult; AutoQualityOptions; PlayerPositionResult
                PlayerPositionOptions; StateResult; UrlResult; CastStatus
                CreateOptions; IvsPlayer.DownloadProgress
                CapacitorIvsPlayerBackgroundState; CapacitorIvsPlayerState
                OnCastStatusListener; OnQualityListener; OnVideoSizeListener
                OnSeekCompletedListener; OnErrorListener; OnDurationListener
                OnCuesListener; OnStateListener; CapacitorFrame
            ]
            Namespace "WebSharper.Capacitor.CapGo.NativeMarket" [
                OpenStoreListingOptions; OpenDevPageOptions
                OpenCollectionOptions; OpenEditorChoicePageOptions
                SearchOptions
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
