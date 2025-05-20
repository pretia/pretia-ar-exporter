using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace PretiaEditor
{
    public class CMC
    {
        public static string API_URL 
        {
            get { return Authentication.PUBLISHER_URL + "/api/v1"; }
        }
        
        public static void Get(string url, System.Action<(bool, string)> onRequestDone, string token = null)
        {
            var finalURL = Path.Combine(API_URL, url);
            UnityWebRequest request = UnityWebRequest.Get(finalURL);
            ProcessPostRequest(request, onRequestDone);
        }
        
        public static void Post(string url, string data, System.Action<(bool, string)> onRequestDone, string method = "POST")
        {
            var finalURL = Path.Combine(API_URL, url);
            UnityWebRequest request = UnityWebRequest.Post(finalURL, data);
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = method;
            request.downloadHandler = new DownloadHandlerBuffer();
            ProcessPostRequest(request, onRequestDone);
        }
        
        public static void PostFileData(string url, 
            string filename, 
            byte[] data, 
            System.Action<(bool, string)> onRequestDone, 
            string method = "POST")
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>()
            {
                new MultipartFormFileSection(filename, data, filename, "text/plain")
            };
            
            var finalURL = Path.Combine(API_URL, url);
            UnityWebRequest request = UnityWebRequest.Post(finalURL, formData);
            request.method = method;
            ProcessPostRequest(request, onRequestDone);
        }
        
        private static void ProcessPostRequest(UnityWebRequest request,
            System.Action<(bool, string)> onRequestDone)
        {
            var token = Authentication.Token;

            if (token != null)
                request.SetRequestHeader("Authorization", "Bearer " + token);

            var asyncOp = request.SendWebRequest();
            asyncOp.completed += (AsyncOperation op) =>
            {
                request = (op as UnityWebRequestAsyncOperation).webRequest;

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error getting response: " + request.error + "\n" + request.downloadHandler.text);
                    onRequestDone.Invoke((false, request.downloadHandler.text));
                }
                else
                {
                    onRequestDone.Invoke((true, request.downloadHandler.text));
                }
                
                request.Dispose();
            };
        }
    }
}