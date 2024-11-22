namespace WebSharper.Capacitor.CapGo

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =
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

    let CapacitorCapGo = 
        Class "CapacitorCapGo"
        |+> Static [
            "SocialLogin" =? SocialLoginPlugin
            |> Import "SocialLogin" "@capgo/capacitor-social-login"
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.Capacitor.CapGo" [
                 CapacitorCapGo
                 SocialLoginPlugin
            ]
            Namespace "WebSharper.Capacitor.CapGo.SocialLogin" [
                IsLoggedInResult; LogoutOptions; LoginResult; LoginOptions; AppleProviderResponse; AppleProfile
                GoogleLoginResponse; GoogleProfile; FacebookLoginResponse; FacebookProfile; Hometown; Location
                AgeRange; AccessToken; AppleProviderOptions; GoogleLoginOptions; FacebookLoginOptions
                InitializeOptions; IsLoggedInOptions; AuthorizationCodeOptions; AuthorizationCode; AppleOptions
                GoogleOptions; FacebookOptions; Provider
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
