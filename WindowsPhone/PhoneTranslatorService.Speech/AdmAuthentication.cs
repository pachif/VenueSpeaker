using System;
using System.Text;
using System.Net;
using System.IO;
using MicroTranslatorService.Speech.Utils;
using System.Threading;
using System.Diagnostics;

namespace MicroTranslatorService.Speech
{
    internal class AdmAccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }       
    }

    internal class AdmAuthentication
    {
        private static AutoResetEvent waitEvent = new AutoResetEvent(false);

        private const string DATAMARKET_ACCESS_URI = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        private string clientID;
        private string cientSecret;
        private string request;
        AdmAccessToken token;
        private bool isReady;

        public AdmAuthentication(string clientID, string clientSecret)
        {
            this.clientID = clientID;
            this.cientSecret = clientSecret;
            
            //If clientid or client secret has special characters, encode before sending request
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", Uri.EscapeDataString(clientID), Uri.EscapeDataString(clientSecret));
        }

        public AdmAccessToken GetAccessToken()
        {
            //Prepare OAuth request 
            isReady = false;
            Debug.WriteLine("Prepare OAuth request ");
            WebRequest webRequest = WebRequest.Create(DATAMARKET_ACCESS_URI);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            webRequest.BeginGetRequestStream(MsTranslateRequestCallBack, webRequest);

            while (!isReady)
            {
                // Do Nothing ... wait
            }

            return token;
        }

        private void MsTranslateRequestCallBack(IAsyncResult asyncResult)
        {
            HttpWebRequest webRequest = asyncResult.AsyncState as HttpWebRequest;
            Debug.WriteLine("### MsTranslateTextRequestCallBack");
            byte[] bytes = Encoding.UTF8.GetBytes(request);
            using (System.IO.Stream outputStream = webRequest.EndGetRequestStream(asyncResult))
            {
                outputStream.Write(bytes, 0, bytes.Length);
            }
            webRequest.BeginGetResponse(new AsyncCallback(MsTranslateResponseCallBack), webRequest);
        }

        private void MsTranslateResponseCallBack(IAsyncResult result)
        {
            var response = result.AsyncState as HttpWebRequest;
            WebResponse webResponse = response.EndGetResponse(result);
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AdmAccessToken));
            Debug.WriteLine("### MsTranslateTextResponseCallBack");
            using (Stream stream = webResponse.GetResponseStream())
            {
                // Get deserialized object from JSON stream 
                token = (AdmAccessToken)serializer.ReadObject(stream);
            }
            isReady = true;
        }
    }
}
