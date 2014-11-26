/* Copyright 2012 Marco Minerva, marco.minerva@gmail.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using MicroTranslatorService.Speech.Utils;

namespace MicroTranslatorService.Speech
{
    /// <summary>
    /// Specifies the audio format of the retrieved audio stream.
    /// </summary>
    public enum SpeakStreamFormat
    {
        /// <summary>
        /// Uses the WAVE file format.
        /// </summary>
        Wave,
        /// <summary>
        /// Uses the MP3 file format.
        /// </summary>
        MP3
    }

    /// <summary>
    /// Specifies the audio quality of the retrieved audio stream.
    /// </summary>
    public enum SpeakStreamQuality
    {
        /// <summary>
        /// Uses the max available quality.
        /// </summary>
        MaxQuality,
        /// <summary>
        /// Retrieves audio file with the minimum size.
        /// </summary>
        MinSize
    }

    /// <summary>
    /// The <strong>SpeechSynthesizer</strong> class provides methods to retrieve stream of file speaking text in various supported languages.
    /// </summary>
    public class SpeechSynthesizer
    {
        private const string BASE_URL = "http://api.microsofttranslator.com/v2/Http.svc/";
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const int MAX_TEXT_LENGTH = 1000;

        private DateTime tokenRequestTime;
        private long tokenValidityTicks;
        private string headerValue;

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        #region Delegates

        /// <summary>
        /// Delegates of the event that is raised when the <see cref="GetSpeakStreamAsync(string, string)"/> method completes.
        /// </summary>
        /// <param name="sender">The object that generates the event.</param>
        /// <param name="e">An object of type <see cref="MicroTranslatorService.Speech.GetSpeakStreamEventArgs"/> that contains information related to the event.</param>
        public delegate void GetSpeakStreamEventHandler(object sender, GetSpeakStreamEventArgs e);

        /// <summary>
        /// Delegates of the event that is raised when the <see cref="GetSpeakBytesAsync(string, string)"/> method completes.
        /// </summary>
        /// <param name="sender">The object that generates the event.</param>
        /// <param name="e">An object of type <see cref="MicroTranslatorService.Speech.GetSpeakBytesEventArgs"/> that contains information related to the event.</param>
        public delegate void GetSpeakBytesEventHandler(object sender, GetSpeakBytesEventArgs e);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an asynchronous request for a stream with speech completes.
        /// </summary>
        /// <seealso cref="GetSpeakStreamAsync(string, string)"/>
        public event GetSpeakStreamEventHandler GetSpeakStreamCompleted;

        /// <summary>
        /// Occurs when speeching of the text completes.
        /// </summary>
        /// <seealso cref="GetSpeakBytesAsync(string, string)"/>
        public event GetSpeakBytesEventHandler GetSpeakBytesCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Application Client ID.
        /// </summary>
        /// <remarks>
        /// Go to https://datamarket.azure.com/developer/applications/ to register your application and obtain a Client ID.
        /// </remarks>        
        public string ClientID { get; set; }

        /// <summary>
        /// Gets or sets the Application Client ID.
        /// </summary>
        /// <remarks>
        /// Go to https://datamarket.azure.com/developer/applications/ to register your application and obtain a Client Secret.
        /// </remarks>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the string representing the supported language code to speak the text in.
        /// </summary>
        public string Language { get; set; }

        public MemoryStream SpeakStream { get; set; }

        /*
        /// <summary>
        /// Gets or sets a <see cref="System.Net.WebProxy"/> class that contains the proxy definition to be used to send request over the Internet.
        /// </summary>
        public WebProxy Proxy { get; set; }
        */

        /// <summary>
        /// Gets or sets the audio format of the retrieved audio stream. Currently, <strong>Wav</strong> and <strong>MP3</strong> are supported.
        /// </summary>
        /// <remarks>The default value is <strong>Wave</strong>.</remarks>        
        public SpeakStreamFormat AudioFormat { get; set; }

        /// <summary>
        /// Gets or sets the audio quality of the retrieved audio stream. Currently, MaxQuality and MinSize are supported.
        /// </summary>
        /// <remarks>
        /// With <strong>MaxQuality</strong>, you can get the voice with the highest quality, and with <strong>MinSize</strong>, you can get the voices with the smallest size. The default value is <strong>MinSize</strong>.
        /// </remarks>
        public SpeakStreamQuality AudioQuality { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechSynthesizer</strong> class.
        /// </summary>
        public SpeechSynthesizer()
            : this(null, null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechSynthesizer</strong> class, using the specified Client ID and Client Secret.
        /// </summary>
        /// <param name="clientID">The Application Client ID.
        /// </param>
        /// <param name="clientSecret">The Application Client Secret.
        /// </param>
        /// <remarks>You must register your application on Azure DataMarket, https://datamarket.azure.com/developer/applications, to obtain the Client ID and Client Secret needed to use the service.
        /// </remarks>
        /// <seealso cref="ClientID"/>
        /// <seealso cref="ClientSecret"/>        
        /// <seealso cref="Language"/>
        public SpeechSynthesizer(string clientID, string clientSecret)
            : this(clientID, clientSecret, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechSynthesizer</strong> class, using the specified Client ID and Client Secret and the desired language.
        /// </summary>
        /// <param name="clientID">The Application Client ID.
        /// </param>
        /// <param name="clientSecret">The Application Client Secret.
        /// </param>
        /// <param name="language">A string representing the supported language code to speak the text in.</param>
        /// <remarks>You must register your application on Azure DataMarket, https://datamarket.azure.com/developer/applications, to obtain the Client ID and Client Secret needed to use the service.
        /// </remarks>
        /// <seealso cref="ClientID"/>
        /// <seealso cref="ClientSecret"/>        
        /// <seealso cref="Language"/>
        public SpeechSynthesizer(string clientID, string clientSecret, string language)
        {
            ClientID = clientID;
            ClientSecret = clientSecret;
            Language = language;
            AudioFormat = SpeakStreamFormat.Wave;
            AudioQuality = SpeakStreamQuality.MinSize;
        }

        #region Get Speak Stream

        private string TextEncode(string str)
        {
            string ret = null;
            foreach (char c in str)
            {
                switch (c)
                {
                    case ' ':
                        ret += "+";
                        break;

                    default:
                        ret += c;
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the language specified in the <seealso cref="Language"/> property. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <returns>A <see cref="System.IO.Stream"/> object that contains a wave-file speaking the passed-in text in the language specified in the <seealso cref="Language"/> property.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method will block until the <see cref="System.IO.Stream"/> object is returned.If you want to perform a non-blocking request for the stream and to be notified when the operation is completed, use the <see cref="GetSpeakStreamAsync(string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/> 
        /// <seealso cref="GetSpeakStreamAsync(string)"/>
        public Stream GetSpeakStream(string text)
        {
            return GetSpeakStream(text, Language);
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the desired language. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <param name="language">A string representing the supported language codes to speak the text in.</param>
        /// <returns>A <see cref="System.IO.Stream"/> object that contains a file speaking the passed-in text in the desired language.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>If <paramref name="language"/> parameter is <strong>null</strong> it will be used the language specified in the <see cref="Language"/> property.</para>
        /// <para>This method will block until the <see cref="System.IO.Stream"/> object is returned. If you want to perform a non-blocking request for the stream and to be notified when the operation is completed, use the <see cref="GetSpeakStreamAsync(string, string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        /// <seealso cref="GetSpeakStreamAsync(string, string)"/>
        public Stream GetSpeakStream(string text, string language)
        {
            if (text == null || text.Trim().Length == 0)
                throw new ArgumentNullException("text");

            if (text.Length > MAX_TEXT_LENGTH)
                throw new ArgumentException("text parameter cannot be longer than " + MAX_TEXT_LENGTH + " characters");

            // Check if it is necessary to obtain/update access token.
            this.UpdateToken();

            if (language == null || language.Trim() == string.Empty)
                language = Language;

            string audioFormat = (AudioFormat == SpeakStreamFormat.Wave ? "audio/wav" : "audio/mp3");
            string audioQuality = (AudioQuality == SpeakStreamQuality.MinSize ? "MinSize" : "MaxQuality");
            string uri = BASE_URL + "Speak?text=" + TextEncode(text) + "&language=" + language + "&format=" + HttpServerUtility.UrlEncode(audioFormat) + "&options=" + audioQuality;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = headerValue;
            
            //if (Proxy != null)
            //    httpWebRequest.Proxy = Proxy;

            IAsyncResult asyncResult = httpWebRequest.BeginGetRequestStream(Callback, httpWebRequest);

            // Wait until the call is finished
            asyncResult.AsyncWaitHandle.WaitOne();

            return SpeakStream;
        }

        private void Callback(IAsyncResult asyncResult)
        {
            var httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
            try
            {
                using (WebResponse response = httpWebRequest.EndGetResponse(asyncResult))
                {
                    MemoryStream ms = null;

                    using (Stream stream = response.GetResponseStream())
                    {
                        // Read the response in chunks and save it in a MemoryStream.
                        ms = new MemoryStream();
                        byte[] buffer = new byte[32768];
                        int bytesRead, totalBytesRead = 0;
                        do
                        {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            totalBytesRead += bytesRead;

                            ms.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);

                        ms.Position = 0;
                    }
                    SpeakStream = ms;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                SpeakStream = null;
            }
            autoResetEvent.Set();
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the language specified in the <seealso cref="Language"/> property. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the stream. When the operation completes, the <see cref="GetSpeakStreamCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakStreamCompleted"/>
        public void GetSpeakStreamAsync(string text)
        {
            this.GetSpeakStreamAsync(text, Language);
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the desired language. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <param name="language">A string representing the supported language codes to speak the text in.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the stream. When the operation completes, the <see cref="GetSpeakStreamCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        /// <seealso cref="GetSpeakStreamCompleted"/>
        public void GetSpeakStreamAsync(string text, string language)
        {
            SpeechThread st = new SpeechThread(this, text, language);
            st.GetSpeakStreamCompleted += new GetSpeakStreamEventHandler(st_GetSpeakStreamCompleted);
            Thread t = new Thread(st.GetSpeakStream);
            t.Start();
        }

        private void st_GetSpeakStreamCompleted(object sender, GetSpeakStreamEventArgs e)
        {
            if (GetSpeakStreamCompleted != null)
                GetSpeakStreamCompleted(this, e);
        }

        #endregion

        #region Get Speak Bytes

        /// <summary>
        /// Returns a byte array containing the stream of a file speaking the passed-in text in the language specified in the <seealso cref="Language"/> property.
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <returns>A byte array that contains the stream of a wave-file speaking the passed-in text in the desired language.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method will block until the byte array is returned. If you want to perform a non-blocking request for the array and to be notified when the operation is completed, use the <see cref="GetSpeakBytesAsync(string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        public byte[] GetSpeakBytes(string text)
        {
            return this.GetSpeakBytes(text, Language);
        }

        /// <summary>
        /// Returns a byte array containing the stream of a file speaking the passed-in text in the desired language. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <param name="language">A string representing the supported language codes to speak the text in.</param>
        /// <returns>A byte array that contains the stream of a wave-file speaking the passed-in text in the desired language.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>If <paramref name="language"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic), it will be used the language specified in the <see cref="Language"/> property.</para>
        /// <para>This method will block until the byte array is returned. If you want to perform a non-blocking request for the array and to be notified when the operation is completed, use the <see cref="GetSpeakBytesAsync(string, string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        public byte[] GetSpeakBytes(string text, string language)
        {
            using (MemoryStream ms = this.GetSpeakStream(text, language) as MemoryStream)
                return ms.ToArray();
        }

        /// <summary>
        /// Returns a byte array containing the stream of a wave-file speaking the passed-in text in the language specified in the <seealso cref="Language"/> property. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the byte array. When the operation completes, the <see cref="GetSpeakBytesCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakBytesCompleted"/>
        public void GetSpeakBytesAsync(string text)
        {
            this.GetSpeakBytesAsync(text, Language);
        }

        /// <summary>
        /// Returns a byte array containg the stream of a wave-file speaking the passed-in text in the desired language. 
        /// </summary>
        /// <param name="text">A string containing a sentence to be spoken.</param>
        /// <param name="language">A string representing the supported language codes to speak the text in.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the byte array. When the operation completes, the <see cref="GetSpeakStreamCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        /// <seealso cref="GetSpeakBytesCompleted"/>
        public void GetSpeakBytesAsync(string text, string language)
        {
            SpeechThread st = new SpeechThread(this, text, language);
            st.GetSpeakBytesCompleted += new GetSpeakBytesEventHandler(st_GetSpeakBytesCompleted);
            Thread t = new Thread(st.GetSpeakBytes);
            t.Start();
        }

        private void st_GetSpeakBytesCompleted(object sender, GetSpeakBytesEventArgs e)
        {
            if (GetSpeakBytesCompleted != null)
                GetSpeakBytesCompleted(this, e);
        }

        #endregion

        private void UpdateToken()
        {
            if (ClientID == null || ClientID.Trim().Length == 0)
                throw new ArgumentException("Invalid Client ID. Go to Azure Marketplace and sign up for Microsoft Translator: https://datamarket.azure.com/developer/applications");

            if (ClientSecret == null || ClientSecret.Trim().Length == 0)
                throw new ArgumentException("Invalid Client Secret. Go to Azure Marketplace and sign up for Microsoft Translator: https://datamarket.azure.com/developer/applications");

            if ((DateTime.Now - tokenRequestTime).Ticks > tokenValidityTicks)
            {
                // Token has expired. Make a request for a new one.
                tokenRequestTime = DateTime.Now;
                AdmAuthentication admAuth = new AdmAuthentication(ClientID, ClientSecret);
                var admToken = admAuth.GetAccessToken();

                tokenValidityTicks = admToken.expires_in * TimeSpan.TicksPerSecond;
                headerValue = "Bearer " + admToken.access_token;
            }
        }
    }
}
