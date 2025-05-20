using System.Text;
using Needle.Deeplink;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace PretiaEditor
{
    public class Authentication : MonoBehaviour
    {
        // PROD
        public const string AUTH_URL = "https://accounts.pretiaar.com";
        public const string PUBLISHER_URL = "https://publisher.pretiaar.com";
        public const string CLIENT_KEY = "0195d53f-e06e-7f7f-a7bd-059e6a1c143b";
        
        private static string _clientSecret;
        private static string _authState;
        private static string _token;
        private static string _status = "";
        
        public static string Token
        {
            get
            {
                if (_token == null)
                    _token = EditorPrefs.GetString("API_TOKEN", "");
                return _token;
            }
        }
        
        public static string Status
        {
            get { return _status; }
        }
        
        private class TokenResponse
        {
            public string access_token;
        }

        public static void Logout(bool setStatus = true)
        {
            _token = null;
            EditorPrefs.SetString("API_TOKEN", null);
            if (setStatus)
                _status = "Logged out";
        }

        /// <summary>
        /// Open browser and retrieve authCode from publisher via handleAuth
        /// </summary>
        public static void Login()
        {
            Logout();
            _status = "Logging in...";
            
            _clientSecret = Crypt.RandomString(32);
            _authState = Crypt.RandomString(32);

            byte[] sha256str = Crypt.CalculateSHA256(Encoding.UTF8.GetBytes(_clientSecret));
            string codeChallenge = System.Convert.ToBase64String(sha256str);
            
            codeChallenge = codeChallenge.Replace('+', '-').Replace('/', '_');
            codeChallenge = codeChallenge.Trim('=');
            
            string url = AUTH_URL + "/api/v1/authorize?" +
                "response_type=code&" +
                "client_id=" + CLIENT_KEY + "&" +
                "state=" + _authState + "&" +
                "redirect_uri=" + "com.unity3d.kharma:pretia-auth&" + 
                "scope=openid&" +
                "code_challenge=" + codeChallenge + "&" +
                "code_challenge_method=S256&" +
                "audience=" + PUBLISHER_URL;

            Application.OpenURL(url);
        }

        /// <summary>
        /// Deep link handler for publisher auth response
        /// </summary>
        [DeepLink(RegexFilter = @"com.unity3d.kharma:pretia-auth(.*)")]
        private static bool HandleAuth(string data)
        {
            ProcessUrlParams(data);
            return true;
        }
        
        private static void ProcessUrlParams(string url)
        {
            string[] urlStrings = url.Split('?');
            
            if (urlStrings.Length > 1)
            {
                string[] parameters = urlStrings[1].Split('&');
                foreach (string p in parameters)
                {
                    string[] keyValue = p.Split('=');
                    if (keyValue.Length == 2)
                    {
                        if (keyValue[0] == "code")
                            GetToken(keyValue[1], ((bool noError, string response) result) =>
                            {
                                if (result.noError)
                                {
                                    _token = result.response;
                                    //Debug.Log("Token: " + _token);
                                    EditorPrefs.SetString("API_TOKEN", _token);
                                }
                            });
                    }
                }
            }
        }

        private static void SendAndRetry(
            UnityWebRequest request, 
            System.Action<UnityWebRequest> onRequestDone, 
            int requestCount = 0)
        {
            var asyncOp = request.SendWebRequest();
            asyncOp.completed += (AsyncOperation op) =>
            {
                request = (op as UnityWebRequestAsyncOperation).webRequest;
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (requestCount < 2)
                        SendAndRetry(request, onRequestDone, requestCount++);
                    else
                    {
                        onRequestDone?.Invoke(request);
                        request.Dispose();
                    }
                }
                else
                {
                    onRequestDone?.Invoke(request);
                    request.Dispose();
                }
            };
        }
        
        private static void GetToken(string authCode, System.Action<(bool, string)> onRequestDone)
        {
            var url = AUTH_URL + "/api/v1/oauth/token";
            WWWForm form = new WWWForm ();
            form.AddField("grant_type", "authorization_code");
            form.AddField("client_id", CLIENT_KEY);
            form.AddField("code_verifier", _clientSecret);
            form.AddField("code", authCode);
            form.AddField("redirect_uri", "com.unity3d.kharma:pretia-auth");
            
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.method = "POST";
            request.downloadHandler = new DownloadHandlerBuffer();
            
            SendAndRetry(request, (UnityWebRequest request) =>
            {
                _status = "";
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error getting token: " + request.error + "\n" + request.downloadHandler.text);
                    onRequestDone.Invoke((false, request.downloadHandler.text));
                }
                else
                {
                    try
                    {
                        string token = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text).access_token;
                        onRequestDone.Invoke((true, token));
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError("Exception getting token: " + request.error + "\n" + request.downloadHandler.text);
                        onRequestDone.Invoke((false, "Exception while parsing token"));
                    }
                }
            });
        }
    }
}