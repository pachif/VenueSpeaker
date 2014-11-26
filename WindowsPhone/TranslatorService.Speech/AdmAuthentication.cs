using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace TranslatorService.Speech
{
    public class AdmAccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
    }

    public class AdmAuthentication
    {
        public event EventHandler<AuthEventArgs> TokenAknowledgeCompleted;

        private const string DATAMARKET_ACCESS_URI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientID;
        private string cientSecret;
        private string request;
        AdmAccessToken token;

        public AdmAuthentication(string clientID, string clientSecret)
        {
            this.clientID = clientID;
            this.cientSecret = clientSecret;

            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", Uri.EscapeDataString(clientID), Uri.EscapeDataString(clientSecret));
        }

        public void GetAccessToken()
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(DATAMARKET_ACCESS_URI);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.BeginGetRequestStream(MsTranslateTextRequestCallBack, webRequest);
        }

        public object Source { get; set; }

        public Action Procedure { get; set; }

        private void MsTranslateTextRequestCallBack(IAsyncResult asyncResult)
        {
            HttpWebRequest requestWeb = asyncResult.AsyncState as HttpWebRequest;

            byte[] bytes = Encoding.UTF8.GetBytes(request);
            using (System.IO.Stream outputStream = requestWeb.EndGetRequestStream(asyncResult))
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            requestWeb.BeginGetResponse(ResponseCallBack, requestWeb);
        }

        private void ResponseCallBack(IAsyncResult result)
        {
            var response = result.AsyncState as HttpWebRequest;
            WebResponse webResponse = response.EndGetResponse(result);
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AdmAccessToken));

            using (Stream stream = webResponse.GetResponseStream())
            {
                // Get deserialized object from JSON stream 
                token = (AdmAccessToken)serializer.ReadObject(stream);
                TriggerCompleted(token);
            }
        }

        private void TriggerCompleted(AdmAccessToken token)
        {
            if (TokenAknowledgeCompleted!=null)
            {
                TokenAknowledgeCompleted(Source, new AuthEventArgs(token, Procedure));
            }
        }
    }
}
