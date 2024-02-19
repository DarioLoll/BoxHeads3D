using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Models;
using PlayFab;
using PlayFab.ClientModels;
using Services;
using UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance;

        public bool IsLoggedIn { get; private set; }
        public bool IsOffline { get; private set; }
        public PlayFabPlayer Player { get; private set; }

        public event Action<PlayFabPlayer> LoggedIn;
        public event Action LoggedOut;
        public event Action<PlayFabPlayer> DisplayNameUpdated;
        public event Action<string> PasswordResetRequested;
        public event Action<string> EmailVerificationRequested;
        public event Action LoadingComplete;
        private string _emailAddress;
        
    
        // Start is called before the first frame update
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoginWithDeviceId();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Log(string message)
        {
            Debug.Log(message);
        }
        
        #region When the game starts, try to log the player in with the device id

            private async void LoginWithDeviceId()
            {
                if(Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Log("No internet connection");
                    IsOffline = true;
                    OnLoadingComplete();
                    return;
                }

                await StartUnityServices();
                var request = new LoginWithCustomIDRequest
                {
                    TitleId = PlayFabSettings.TitleId,
                    CustomId = SystemInfo.deviceUniqueIdentifier,
                    CreateAccount = false,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
                    {
                        GetPlayerProfile = true,
                        GetUserAccountInfo = true,
                        ProfileConstraints = new PlayerProfileViewConstraints()
                        {
                            ShowDisplayName = true
                        }
                    }
                };
                PlayFabClientAPI.LoginWithCustomID(request, OnLoginWithDeviceIdSuccess, OnLoginWithDeviceIdFailure);
            }
            
            private async Task StartUnityServices()
            {
                try
                {
                    await UnityServices.InitializeAsync();
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                catch (Exception e)
                {
                    Log("Failed to initialize Unity Services: " + e.Message);
                }
            }
            
            private void OnLoginWithDeviceIdSuccess(LoginResult obj)
            {
                string displayName = obj.InfoResultPayload.PlayerProfile?.DisplayName ?? string.Empty;
                string username = obj.InfoResultPayload.AccountInfo?.Username ?? "Guest";
                Log("Logged in as " + username);
                Player = new PlayFabPlayer
                {
                    Username = username,
                    IsGuest = username == "Guest",
                    DisplayName = displayName
                };
                OnLoggedIn(Player);
                OnLoadingComplete();
            }
            
            private void OnLoginWithDeviceIdFailure(PlayFabError error)
            {
                Log("Failed to log in with device id");
                if(error.Error == PlayFabErrorCode.AccountNotFound)
                {
                    Log("No saved account on this device...");
                }
                else
                {
                    Log("Login failed with error: " + error.ErrorMessage);
                    IsOffline = true;
                }
                OnLoadingComplete();
            }
        
        #endregion
        
        
        #region Sign Up (Email Verification)
        
            public void SignUp(string email)
            {
                if (IsLoggedIn && !Player.IsGuest)
                {
                    Log("Already logged in");
                    return;
                }
                _emailAddress = email.Trim();
                if (!ValidateEmail(_emailAddress)) return;
                Log("Inputs are valid. Checking if email is taken...");
                var loginRequest = new LoginWithEmailAddressRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    Email = _emailAddress,
                    Password = "6chars" //password that won't work
                };
                PlayFabClientAPI.LoginWithEmailAddress(loginRequest, _ => {}, OnEmailCheckDone);
            }

            private void OnEmailCheckDone(PlayFabError obj)
            {
                if (obj.Error == PlayFabErrorCode.AccountNotFound)
                {
                    Log("Email is not taken. Sending verification...");
                    var loginWithDeviceIdRequest = new LoginWithCustomIDRequest
                    {
                        TitleId = PlayFabSettings.TitleId,
                        CustomId = SystemInfo.deviceUniqueIdentifier,
                        CreateAccount = true
                    };
                    PlayFabClientAPI.LoginWithCustomID(loginWithDeviceIdRequest, _ =>
                    {
                        Log("Logged in with device id");
                        SendVerification(_emailAddress);
                    }, OnPlayFabError);
                }
                else
                {
                    Log("Email is taken");
                    PopupBox.Instance.DisplayError("Email is already taken");
                }
            }
            
            private void SendVerification(string email)
            {
                var request = new AddOrUpdateContactEmailRequest
                {
                    EmailAddress = email
                };
                PlayFabClientAPI.AddOrUpdateContactEmail(request, OnContactEmailSuccess, OnPlayFabError);
            }
            
            private void OnContactEmailSuccess(AddOrUpdateContactEmailResult obj)
            {
                //Switch to the email verification canvas
                Log("Contact email added successfully");
                OnEmailVerificationRequested(_emailAddress);
            }
        
        #endregion
        
        #region Sign Up (Adding Username, Password and Email)
        
            public void AddAccountData(string username, string password, string confirmPassword)
            {
                if (!ValidateUsername(username)) return;
                if (!ValidatePassword(password, confirmPassword)) return;
                Log("Inputs are valid. Adding account data...");
                var request = new AddUsernamePasswordRequest
                {
                    Username = username.Trim(),
                    Email = _emailAddress,
                    Password = password.Trim()
                };
                PlayFabClientAPI.AddUsernamePassword(request, OnAddAccountDataSuccess, OnPlayFabError);
            }
            private void OnAddAccountDataSuccess(AddUsernamePasswordResult obj)
            {
                Log("Account data added successfully " + obj.Username);
                Player ??= new PlayFabPlayer();
                Player.IsGuest = false;
                Player.Username = obj.Username;
                OnLoggedIn(Player);
            }
        
        #endregion
        

        #region Login
        
            public void Login(string username, string password)
            {
                username = username.Trim();
                password = password.Trim();
                if(!ValidateUsername(username) || !ValidatePassword(password)) 
                    return;
                Debug.Log("Inputs are valid. Logging in...");
                var request = new LoginWithPlayFabRequest
                {
                    Username = username,
                    Password = password,
                    TitleId = PlayFabSettings.TitleId,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
                    {
                        GetPlayerProfile = true,
                        GetUserAccountInfo = true,
                        ProfileConstraints = new PlayerProfileViewConstraints()
                        {
                            ShowDisplayName = true
                        }
                    }
                };
                PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnPlayFabError);
            }
            
            private void OnLoginSuccess(LoginResult obj)
            {
                string displayName = obj.InfoResultPayload.PlayerProfile.DisplayName;
                Debug.Log("Login successful. Linking device id...");
                var request = new LinkCustomIDRequest
                {
                    CustomId = SystemInfo.deviceUniqueIdentifier,
                    ForceLink = true
                };
                PlayFabClientAPI.LinkCustomID(request, _ =>
                {
                    Debug.Log("Device id linked successfully");
                    OnDeviceIdLinked(obj, displayName);
                }, OnPlayFabError);
                
            }

            private void OnDeviceIdLinked(LoginResult loginResult, string displayName)
            {
                Player = new PlayFabPlayer
                {
                    Username = loginResult.InfoResultPayload.AccountInfo.Username,
                    DisplayName = displayName
                };
                OnLoggedIn(Player);
            }

        #endregion

        #region Log Out

            public void LogOut()
            {
                var unlinkRequest = new UnlinkCustomIDRequest
                {
                    CustomId = SystemInfo.deviceUniqueIdentifier
                };
                PlayFabClientAPI.UnlinkCustomID(unlinkRequest, OnDeviceIdUnlinked, OnPlayFabError);
            }

            private void OnDeviceIdUnlinked(UnlinkCustomIDResult obj)
            {
                PlayFabClientAPI.ForgetAllCredentials();
                Player = null;
                IsLoggedIn = false;
                OnLoggedOut();
            }

        #endregion

        #region Continue as Guest

            public void ContinueAsGuest()
            {
                if(IsLoggedIn)
                {
                    Log("Already logged in");
                    return;
                }
                var request = new LoginWithCustomIDRequest
                {
                    TitleId = PlayFabSettings.TitleId,
                    CustomId = SystemInfo.deviceUniqueIdentifier,
                    CreateAccount = true
                };
                PlayFabClientAPI.LoginWithCustomID(request, _ =>
                {
                    Log("Logged in as Guest");
                    Player = new PlayFabPlayer
                    {
                        Username = "Guest",
                        IsGuest = true
                    };
                    OnLoggedIn(Player);
                }, OnPlayFabError);
            }

        #endregion

        #region Reset Password

            public void ResetPassword(string email)
            {
                email = email.Trim();
                if (!ValidateEmail(email)) return;
                Debug.Log("Inputs are valid. Sending email...");
                _emailAddress = email;
                var request = new SendAccountRecoveryEmailRequest
                {
                    Email = _emailAddress,
                    TitleId = PlayFabSettings.TitleId
                };
                PlayFabClientAPI.SendAccountRecoveryEmail(request, _ =>
                {
                    Debug.Log("Password reset email sent successfully");
                    OnPasswordResetRequested(_emailAddress);
                }, OnPlayFabError);
                
            }

        #endregion
        
        public void CheckVerificationStatus(string email, Action<bool> callback)
        {
            email = email.Trim();
            if (!ValidateEmail(email)) return;
            Debug.Log("Inputs are valid. Checking verification status...");
            _emailAddress = email;
            var request = new GetPlayerProfileRequest()
            {
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowContactEmailAddresses = true
                }
            };
            PlayFabClientAPI.GetPlayerProfile(request, result =>
            {
                var emailList = result.PlayerProfile.ContactEmailAddresses;
                foreach (var emailInfo in emailList)
                {
                    if (emailInfo.EmailAddress == _emailAddress)
                    {
                        callback(emailInfo.VerificationStatus == EmailVerificationStatus.Confirmed);
                        return;
                    }
                }
                callback(false);
            }, OnPlayFabError);
        }
        
        public void UpdateDisplayName(string displayName)
        {
            displayName = displayName.Trim();
            if (!ValidateDisplayName(displayName)) return;
            Debug.Log("Inputs are valid. Setting display name...");
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
            {
                Debug.Log("Display name set successfully " + result.DisplayName);
                Player.DisplayName = result.DisplayName;
                OnDisplayNameUpdated(Player);
            }, OnPlayFabError);
        }
        
        
        #region Private Methods

            private void OnPlayFabError(PlayFabError error)
            {
                UIManager.Instance.DisplayError(error);
            }
            
            private bool ValidatePassword(string password, string confirmPassword)
            {
                string passwordText = password.Trim();
                if (passwordText != confirmPassword.Trim())
                {
                    PopupBox.Instance.DisplayError("Passwords do not match");
                    return false;
                }
                return ValidatePassword(passwordText);
            }
            
            private bool ValidateUsername(string username)
            {
                username = username.Trim();
                if (username.Length is < 3 or > 20)
                {
                    PopupBox.Instance.DisplayError("Username must be between 3 and 20 characters");
                    return false;
                }
                if (!Regex.IsMatch(username, "^[a-zA-Z0-9]+$"))
                {
                    PopupBox.Instance.DisplayError("Username may only contain numbers and letters");
                    return false;
                }
                return true;
            }

            private bool ValidateEmail(string email)
            {
                string emailText = email.Trim();
                if (!Regex.IsMatch(emailText, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                {
                    PopupBox.Instance.DisplayError("Invalid email address");
                    return false;
                }
                return true;
            }

            private bool ValidatePassword(string passwordText)
            {
                passwordText = passwordText.Trim();
                if (passwordText.Length < 8)
                {
                    PopupBox.Instance.DisplayError("Password must be at least 8 characters long");
                    return false;
                }
                return true;
            }

            private bool ValidateDisplayName(string displayNameText)
            {
                displayNameText = displayNameText.Trim();
                if (displayNameText.Length is < 3 or > 20)
                {
                    PopupBox.Instance.DisplayError("Display name must be between 3 and 20 characters");
                    return false;
                }
                return true;
            }

            
        #endregion

        #region Event Invocations

            private void OnLoggedIn(PlayFabPlayer obj)
            {
                IsLoggedIn = true;
                LoggedIn?.Invoke(obj);
            }

            private void OnDisplayNameUpdated(PlayFabPlayer obj)
            {
                DisplayNameUpdated?.Invoke(obj);
            }
            
            protected virtual void OnLoadingComplete()
            {
                SceneLoader.LoadScene(Scenes.Menu);
                LoadingComplete?.Invoke();
            }

            protected virtual void OnPasswordResetRequested(string email)
            {
                PasswordResetRequested?.Invoke(email);
            }

            protected virtual void OnEmailVerificationRequested(string email)
            {
                EmailVerificationRequested?.Invoke(email);
            }

            protected virtual void OnLoggedOut()
            {
                IsLoggedIn = false;
                LoggedOut?.Invoke();
            }

        #endregion

        
    }
    
}
