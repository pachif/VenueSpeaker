using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Globalization;
using System.Configuration;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.Threading;

namespace TranslatorService.Speech
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
    /// The <strong>SpeechSynthesizer</strong> class provides methods to retrieve stream of audio file speaking text in various supported languages.
    /// </summary>
    /// <remarks>
    /// <para>To use this library, you need to go to <strong>Azure DataMarket</strong> at https://datamarket.azure.com/developer/applications and register your application. In this way, you'll obtain the <see cref="ClientID"/> and <see cref="ClientSecret"/> that are necessary to use <strong>Microsoft Translator Service</strong>.</para>
    /// <para>You also need to go to https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb and subscribe the <strong>Microsoft Translator Service</strong>. There are many options, based on the amount of characters per month. The service is free up to 2 million characters per month.</para>
    /// </remarks>
    public class SpeechSynthesizer
    {
        private const string BASE_URL = "http://api.microsofttranslator.com/v2/Http.svc/";
        private const string LANGUAGES_URI = BASE_URL + "GetLanguagesForSpeak";
        private const string SPEAK_URI = BASE_URL + "Speak?text={0}&language={1}&format={2}&options={3}";
        private const string TRANSLATE_URI = BASE_URL + "Translate?text={0}&to={1}&contentType=text/plain";
        private const string DETECT_URI = BASE_URL + "Detect?text={0}";
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const int MAX_TEXT_LENGTH = 1000;
        private const int MAX_TEXT_LENGTH_FOR_AUTODETECTION = 100;
        private string CLIENT_ID = "loadvenues";
        private string CLIENT_SECRET = "9RUanzXB529CyiGQPpCilZIricwZ4lQTa4UfVgmR7RE=";

        private DateTime tokenRequestTime;
        private int tokenValiditySeconds;
        private string headerValue;
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        #region Events

        /// <summary>
        /// Occurs when an asynchronous request for supported languages completes;
        /// </summary>
        /// <seealso cref="GetLanguagesAsync"/>
        public event EventHandler<GetLanguagesEventArgs> GetLanguagesCompleted;

        /// <summary>
        /// Occurs when an asynchronous request for a stream with speech completes.
        /// </summary>
        /// <seealso cref="GetSpeakStreamAsync(string, string)"/>
        public event EventHandler<GetSpeakStreamEventArgs> GetSpeakStreamCompleted;

        /// <summary>
        /// Occurs when speaking of the text completes.
        /// </summary>
        /// <seealso cref="SpeakAsync(string, string)"/>
        public event EventHandler<SpeechEventArgs> SpeakCompleted;

        /// <summary>
        /// Occurs when translation completes.
        /// </summary>
        /// <seealso cref="TranslateAsync(string, string)"/>
        public event EventHandler<TranslateEventArgs> TranslateCompleted;

        /// <summary>
        /// Occurs when language detection completes.
        /// </summary>
        /// <seealso cref="DetectLanguageAsync(string)"/>
        public event EventHandler<DetectLanguageEventArgs> DetectLanguageCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Application Client ID that is necessary to use <strong>Microsoft Translator Service</strong>.
        /// </summary>
        /// <value>The Application Client ID.</value>
        /// <remarks>
        /// <para>Go to <strong>Azure DataMarket</strong> at https://datamarket.azure.com/developer/applications to register your application and obtain a Client ID.</para>
        /// <para>You also need to go to https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb and subscribe the <strong>Microsoft Translator Service</strong>. There are many options, based on the amount of characters per month. The service is free up to 2 million characters per month.</para>
        /// </remarks>        
        public string ClientID { get; set; }

        /// <summary>
        /// Gets or sets the Application Client Secret that is necessary to use <strong>Microsoft Translator Service</strong>.
        /// </summary>
        /// <remarks>
        /// <value>The Application Client Secret.</value>
        /// <para>Go to <strong>Azure DataMaket</strong> at https://datamarket.azure.com/developer/applications to register your application and obtain a Client Secret.</para>
        /// <para>You also need to go to https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb and subscribe the <strong>Microsoft Translator Service</strong>. There are many options, based on the amount of characters per month. The service is free up to 2 million characters per month.</para>
        /// </remarks>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the string representing the supported language code to speak the text in.
        /// </summary>
        /// <value>The string representing the supported language code to speak the text in. The code must be present in the list of codes returned from the method <see cref="GetLanguages"/>.</value>
        /// <seealso cref="GetLanguages"/>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the audio format of the retrieved audio stream. Currently, <strong>Wav</strong> and <strong>MP3</strong> are supported.
        /// </summary>
        /// <value>The audio format of the retrieved audio stream. Currently, <strong>Wav</strong> and <strong>MP3</strong> are supported.</value>
        /// <remarks>The default value is <strong>Wave</strong>.</remarks>        
        public SpeakStreamFormat AudioFormat { get; set; }

        /// <summary>
        /// Gets or sets the audio quality of the retrieved audio stream. Currently, <strong>MaxQuality</strong> and <strong>MinSize</strong> are supported.
        /// </summary>
        /// <value>The audio quality of the retrieved audio stream. Currently, <strong>MaxQuality</strong> and <strong>MinSize</strong> are supported.</value>
        /// <remarks>
        /// With <strong>MaxQuality</strong>, you can get the voice with the highest quality, and with <strong>MinSize</strong>, you can get the voices with the smallest size. The default value is <strong>MinSize</strong>.
        /// </remarks>
        public SpeakStreamQuality AudioQuality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sentence to be spoken must be translated in the specified language.
        /// </summary>
        /// <value><strong>true</strong> if the sentence to be spoken must be translated in the specified language; otherwise, <strong>false</strong>.</value>
        /// <remarks>If you don't need to translate to text to be spoken, you can speed-up the the library setting the <strong>AutomaticTranslation</strong> property to <strong>false</strong>. In this way, the specified text is passed as is to the other methods, without performing any translation. The default value is <strong>true</strong>.</remarks>
        public bool AutomaticTranslation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language of the text must be automatically detected before text-to-speech.
        /// </summary>
        /// <value><strong>true</strong> if the language of the text must be automatically detected; otherwise, <strong>false</strong>.</value>
        /// <remarks>The <strong>AutoDetectLanguage</strong> property is used when the following methods are invoked:
        /// <list type="bullet">
        /// <term><see cref="GetSpeakStream(string)"/></term>
        /// <term><see cref="Speak(string)"/></term>
        /// <term><see cref="GetSpeakStreamAsync(string)"/></term>
        /// <term><see cref="SpeakAsync(string)"/></term>
        /// </list>
        /// <para>When these methods are called, if the <strong>AutoDetectLanguage</strong> property is set to <strong>true</strong>, the language of the text is auto-detected before speech stream request. Otherwise, the language specified in the <seealso cref="Language"/> property is used.</para>
        /// <para>If the language to use is explicitly specified, using the versions of the methods that accept it, no auto-detection is performed.</para>
        /// <para>The default value is <strong>true</strong>.</para>
        /// </remarks>
        /// <seealso cref="Language"/>
        public bool AutoDetectLanguage { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechSynthesizer</strong> class, using the specified Client ID and Client Secret and the current system language.
        /// </summary>
        /// <param name="clientID">The Application Client ID.
        /// </param>
        /// <param name="clientSecret">The Application Client Secret.
        /// </param>
        /// <remarks><para>You must register your application on <strong>Azure DataMarket</strong> at https://datamarket.azure.com/developer/applications to obtain the Client ID and Client Secret needed to use the service.</para>
        /// <para>You also need to go to https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb and subscribe the <strong>Microsoft Translator Service</strong>. There are many options, based on the amount of characters per month. The service is free up to 2 million characters per month.</para>
        /// </remarks>
        /// <seealso cref="ClientID"/>
        /// <seealso cref="ClientSecret"/>        
        /// <seealso cref="Language"/>
        public SpeechSynthesizer()
            : this(CultureInfo.CurrentCulture.Name.ToLower())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechSynthesizer</strong> class, using the specified Client ID and Client Secret and the desired language.
        /// </summary>
        /// <param name="clientID">The Application Client ID.
        /// </param>
        /// <param name="clientSecret">The Application Client Secret.
        /// </param>
        /// <param name="language">A string representing the supported language code to speak the text in. The code must be present in the list of codes returned from the method <see cref="GetLanguages"/>.</param>
        /// <remarks><para>You must register your application on <strong>Azure DataMarket</strong> at https://datamarket.azure.com/developer/applications to obtain the Client ID and Client Secret needed to use the service.</para>
        /// <para>You also need to go to https://datamarket.azure.com/dataset/1899a118-d202-492c-aa16-ba21c33c06cb and subscribe the <strong>Microsoft Translator Service</strong>. There are many options, based on the amount of characters per month. The service is free up to 2 million characters per month.</para>
        /// </remarks>
        /// <seealso cref="ClientID"/>
        /// <seealso cref="ClientSecret"/>        
        /// <seealso cref="Language"/>
        public SpeechSynthesizer(string language)
        {
            ClientID = CLIENT_ID;
            ClientSecret = CLIENT_SECRET;
            Language = language;
            AudioFormat = SpeakStreamFormat.Wave;
            AudioQuality = SpeakStreamQuality.MinSize;
            AutomaticTranslation = true;
            AutoDetectLanguage = true;
        }

        #region Get Languages

        /// <summary>
        /// Retrieves the languages available for speech synthesis.
        /// </summary>
        /// <returns>A string array containing the language codes supported for speech synthesis by <strong>Microsoft Translator Service</strong>.</returns>        
        /// <exception cref="ArgumentException">The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</exception>
        /// <remarks><para>This method will block until the array is returned. If you want to perform a non-blocking request and to be notified when the operation is completed, use the <see cref="GetLanguagesAsync"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512415.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetLanguagesAsync"/>
        public string[] GetLanguages()
        {
            GetLanguagesAsync(true);
            autoResetEvent.WaitOne();

            return Languages;
        }

        /// <summary>
        /// Retrieves the languages available for speech synthesis.
        /// </summary>
        /// <exception cref="ArgumentException">The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</exception>
        /// <remarks><para>This method performs a non-blocking request. When the operation completes, the <see cref="GetLanguagesAsync"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512415.aspx.
        /// </para>
        /// </remarks>
        public void GetLanguagesAsync(bool isSyncCall = false)
        {
            Action result = () => GetLanguagesAsyncCallback(isSyncCall);
            // Check if it is necessary to obtain/update access token.
            ExecuteUpdateTokenAsync(result);
        }

        private void GetLanguagesAsyncCallback(bool isSyncCall)
        {
            string uri = LANGUAGES_URI;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = headerValue;

            httpWebRequest.BeginGetResponse((asyncResult) =>
            {
                using (WebResponse response = httpWebRequest.EndGetResponse(asyncResult))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(string[]));
                        var results = (string[])dcs.ReadObject(stream);
                        if (isSyncCall)
                        {
                            Languages = results;
                            autoResetEvent.Set();
                        }
                        else
                        {
                            OnGetLanguagesCompleted(new AsyncCompletedEventArgs(null, false, results));
                        }
                    }
                }
            }, null);
        }

        private void OnGetLanguagesCompleted(AsyncCompletedEventArgs e)
        {
            if (GetLanguagesCompleted != null)
            {
                GetLanguagesEventArgs args = new GetLanguagesEventArgs(e.UserState as string[], e.Error);
                GetLanguagesCompleted(this, args);
            }
        }

        #endregion

        #region Translate

        /// <summary>
        /// Translates a text string into the language specified in the <seealso cref="Language"/> property.
        /// </summary>
        /// <returns>A string representing the translated text.</returns>
        /// <param name="text">A string representing the text to translate.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method will block until the translated text is returned. If you want to perform a non-blocking request and to be notified when the operation is completed, use the <see cref="TranslateAsync(string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512421.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="Language"/> 
        /// <seealso cref="TranslateAsync(string)"/>
        public string Translate(string text)
        {
            return this.Translate(text, Language);
        }

        /// <summary>
        /// Translates a text string into the specified language. 
        /// </summary>
        /// <returns>A string representing the translated text.</returns>
        /// <param name="text">A string representing the text to translate.</param>
        /// <param name="to">A string representing the language code to translate the text into. The code must be present in the list of codes returned from the <see cref="GetLanguages"/> method.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method will block until the translated text is returned. If you want to perform a non-blocking request and to be notified when the operation is completed, use the <see cref="TranslateAsync(string, string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512421.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="Language"/> 
        /// <seealso cref="TranslateAsync(string, string)"/>
        public string Translate(string text, string to)
        {
            TranslateAsync(text, to, true);
            autoResetEvent.WaitOne();
            return TranslatedText;
        }

        /// <summary>
        /// Translates a text string into the language specified in the <seealso cref="Language"/> property.
        /// </summary>
        /// <returns>A string representing the translated text.</returns>
        /// <param name="text">A string representing the text to translate.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for translation. When the operation completes, the <see cref="TranslateCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512421.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="Language"/> 
        /// <seealso cref="TranslateCompleted"/>
        public void TranslateAsync(string text)
        {
            this.TranslateAsync(text, Language);
        }

        /// <summary>
        /// Translates a text string into the specified language.
        /// </summary>
        /// <returns>A string representing the translated text.</returns>
        /// <param name="text">A string representing the text to translate.</param>
        /// <param name="to">A string representing the language code to translate the text into. The code must be present in the list of codes returned from the <see cref="GetLanguages"/> method.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for translation. When the operation completes, the <see cref="TranslateCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512421.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="Language"/> 
        /// <seealso cref="TranslateCompleted"/>
        public void TranslateAsync(string text, string to, bool isAsyncCall = false)
        {
            // Check if it is necessary to obtain/update access token.
            Action proc = ()=> TranslateAsyncCallback(text, to, isAsyncCall);
            ExecuteUpdateTokenAsync(proc);
        }

        private void TranslateAsyncCallback(string text, string to, bool isAsyncCall)
        {
            string results = null;

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");

            if (text.Length > MAX_TEXT_LENGTH)
                throw new ArgumentException("text parameter cannot be longer than " + MAX_TEXT_LENGTH + " characters");

            if (string.IsNullOrEmpty(to))
                to = Language;

            string uri = string.Format(TRANSLATE_URI, Uri.EscapeDataString(text), to);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers[AUTHORIZATION_HEADER] = headerValue;

            httpWebRequest.BeginGetResponse((asyncResult) =>
            {
                using (WebResponse response = httpWebRequest.EndGetResponse(asyncResult))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(string));
                        results = (string)dcs.ReadObject(stream);
                        if (isAsyncCall)
                        {
                            TranslatedText = results;
                            autoResetEvent.Set();
                        }
                        else
                        {
                            OnTranslateCompleted(new AsyncCompletedEventArgs(null, false, results));
                        }
                    }
                }
            }, null);
        }

        private void OnTranslateCompleted(AsyncCompletedEventArgs e)
        {
            if (TranslateCompleted != null)
            {
                TranslateEventArgs args = new TranslateEventArgs(e.UserState as string, e.Error);
                TranslateCompleted(this, args);
            }
        }

        #endregion

        #region Detect Language

        /// <summary>
        /// Detects the language of a text. 
        /// </summary>
        /// <param name="text">A string represeting the text whose language must be detected.</param>
        /// <returns>A string containing a two-character Language code for the given text.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method will block until the language code is returned. If you want to perform a non-blocking request for the language of the text and to be notified when the operation is completed, use the <see cref="DetectLanguageAsync(string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512427.aspx.
        /// </para></remarks>
        /// <seealso cref="DetectLanguageAsync"/>
        /// <seealso cref="GetLanguages"/>
        /// <seealso cref="Language"/>        
        public string DetectLanguage(string text)
        {
            DetectLanguageAsync(text, true);
            autoResetEvent.WaitOne();
            return DetectedLanguage;
        }

        /// <summary>
        /// Detects the language of a text. 
        /// </summary>
        /// <param name="text">A string represeting the text whose language must be detected.</param>
        /// <returns>A string containing a two-character Language code for the given text. </returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for language code. When the operation completes, the <see cref="DetectLanguageCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512427.aspx.
        /// </para></remarks>
        /// <seealso cref="DetectLanguage"/>
        /// <seealso cref="GetLanguages"/>
        /// <seealso cref="Language"/>    
        public void DetectLanguageAsync(string text, bool isSyncCall = false)
        {
            Action proc = () => DetectLanguageAsyncCallback(text, isSyncCall);
            // Check if it is necessary to obtain/update access token.
            ExecuteUpdateTokenAsync(proc);
        }

        private void DetectLanguageAsyncCallback(string text, bool isSyncCall)
        {
            string results = null;

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");

            text = text.Substring(0, Math.Min(text.Length, MAX_TEXT_LENGTH_FOR_AUTODETECTION));

            string uri = string.Format(DETECT_URI, text);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = headerValue;

            httpWebRequest.BeginGetResponse((ar) =>
            {
                using (WebResponse response = httpWebRequest.EndGetResponse(ar))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        DataContractSerializer dcs = new DataContractSerializer(typeof(string));
                        results = (string)dcs.ReadObject(stream);

                        if (isSyncCall)
                        {
                            DetectedLanguage = results;
                            autoResetEvent.Set();
                        }
                        else
                        {
                            OnDetectLanguageCompleted(new AsyncCompletedEventArgs(null, false, results));
                        }
                    }
                }
            }, null);
        }

        private void OnDetectLanguageCompleted(AsyncCompletedEventArgs e)
        {
            if (DetectLanguageCompleted != null)
            {
                DetectLanguageEventArgs args = new DetectLanguageEventArgs(e.UserState as string, e.Error);
                DetectLanguageCompleted(this, args);
            }
        }

        #endregion

        #region Get Speak Stream

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text. If <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <see cref="Language"/> property is used. 
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <returns>A <see cref="System.IO.Stream"/> object that contains a file speaking the passed-in text.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method will block until the <see cref="System.IO.Stream"/> object is returned. If you want to perform a non-blocking request for the stream and to be notified when the operation is completed, use the <see cref="GetSpeakStreamAsync(string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <see cref="AutoDetectLanguage"/>
        /// <see cref="DetectLanguage"/>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakStreamAsync(string)"/>
        public Stream GetSpeakStream(string text)
        {
            return GetSpeakStream(text, null);
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the desired language. If <paramref name="language"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) and the <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <see cref="Language"/> property is used.
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <param name="language">A string representing the language code to speak the text in. The code must be present in the list of codes returned from the method <see cref="GetLanguages"/>.</param>
        /// <returns>A <see cref="System.IO.Stream"/> object that contains a file speaking the passed-in text in the desired language.</returns>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks>
        /// <para>This method will block until the <see cref="System.IO.Stream"/> object is returned. If you want to perform a non-blocking request for the stream and to be notified when the operation is completed, use the <see cref="GetSpeakStreamAsync(string, string)"/> method instead.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        /// <seealso cref="GetLanguages"/>
        /// <seealso cref="GetSpeakStreamAsync(string, string)"/>
        public Stream GetSpeakStream(string text, string language)
        {
            GetSpeakStreamAsync(text, true, language);

            autoResetEvent.WaitOne();

            return SpeakMemoryStream;
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text. If <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <see cref="Language"/> property is used.  
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the stream. When the operation completes, the <see cref="GetSpeakStreamCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakStreamCompleted"/>
        public void GetSpeakStreamAsync(string text)
        {
            this.GetSpeakStreamAsync(text, false);
        }

        /// <summary>
        /// Returns a stream of a file speaking the passed-in text in the desired language. If <paramref name="language"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) and the <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <see cref="Language"/> property is used.
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <param name="language">A string representing the language code to speak the text in. The code must be present in the list of codes returned from the method <see cref="GetLanguages"/>.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <remarks><para>This method perform a non-blocking request for the stream. When the operation completes, the <see cref="GetSpeakStreamCompleted"/> event is raised.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="System.IO.Stream"/>
        /// <seealso cref="Language"/>
        /// <seealso cref="GetLanguages"/>
        /// <seealso cref="GetSpeakStreamCompleted"/>
        public void GetSpeakStreamAsync(string text, bool isSyncCall, string language = null)
        {
            Action proc = () => GetSpeakStreamAsyncCallBack(text, isSyncCall, language);
            ExecuteUpdateTokenAsync(proc);
        }

        private void ExecuteUpdateTokenAsync(Action callback)
        {
            // Check if it is necessary to obtain/update access token.
            if (string.IsNullOrWhiteSpace(ClientID))
                throw new ArgumentException("Invalid Client ID. Go to Azure Marketplace and sign up for Microsoft Translator: https://datamarket.azure.com/developer/applications");

            if (string.IsNullOrWhiteSpace(ClientSecret))
                throw new ArgumentException("Invalid Client Secret. Go to Azure Marketplace and sign up for Microsoft Translator: https://datamarket.azure.com/developer/applications");

            if ((DateTime.Now - tokenRequestTime).TotalSeconds > tokenValiditySeconds)
            {
                // Token has expired. Make a request for a new one.
                AdmAuthentication admAuth = new AdmAuthentication(ClientID, ClientSecret);
                admAuth.Source = this;
                admAuth.Procedure = callback;
                admAuth.TokenAknowledgeCompleted += ObtainTokenCompleted;
                admAuth.GetAccessToken();
            }
            else
            {
                callback.Invoke();
            }
        }

        private void ObtainTokenCompleted(object sender, AuthEventArgs e)
        {
            // now update the token
            tokenRequestTime = DateTime.Now;
            tokenValiditySeconds = int.Parse(e.Token.expires_in);
            headerValue = "Bearer " + e.Token.access_token;
            e.ToExecute.Invoke();
        }

        private void GetSpeakStreamAsyncCallBack(string text, bool isSyncCall, string language)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException("text");

            if (text.Length > MAX_TEXT_LENGTH)
                throw new ArgumentException("text parameter cannot be longer than " + MAX_TEXT_LENGTH + " characters");

            bool languageDetected = false;
            if (string.IsNullOrEmpty(language))
            {
                if (AutoDetectLanguage)
                {
                    language = this.DetectLanguage(text);
                    languageDetected = true;
                }
            }
            else
            {
                language = Language;
                languageDetected = true;
            }

            if (AutomaticTranslation && !languageDetected)
                text = this.Translate(text, language);

            string audioFormat = (AudioFormat == SpeakStreamFormat.Wave ? "audio/wav" : "audio/mp3");
            string audioQuality = (AudioQuality == SpeakStreamQuality.MinSize ? "MinSize" : "MaxQuality");
            string uri = string.Format(SPEAK_URI, Uri.EscapeDataString(text), language, Uri.EscapeDataString(audioFormat), audioQuality);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = headerValue;


            httpWebRequest.BeginGetResponse((ar) =>
            {
                using (WebResponse response = httpWebRequest.EndGetResponse(ar))
                {

                    using (Stream stream = response.GetResponseStream())
                    {
                        var memoryStream = new MemoryStream();
                        // Read the response in chunks and save it in a MemoryStream.
                        byte[] buffer = new byte[32768];
                        int bytesRead, totalBytesRead = 0;
                        do
                        {
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                            totalBytesRead += bytesRead;
                            memoryStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);

                        memoryStream.Position = 0;

                        if (isSyncCall)
                        {
                            SpeakMemoryStream = memoryStream;
                            autoResetEvent.Set();
                        }
                        else
                        {
                            OnGetSpeakStreamCompleted(new AsyncCompletedEventArgs(null, false, memoryStream));
                        }
                    }
                }
            }, null);
        }

        private void OnGetSpeakStreamCompleted(AsyncCompletedEventArgs e)
        {
            if (GetSpeakStreamCompleted != null)
            {
                GetSpeakStreamEventArgs args = new GetSpeakStreamEventArgs(e.UserState as Stream, e.Error);
                GetSpeakStreamCompleted(this, args);
            }
        }

        #endregion

        #region Speak

        /// <summary>
        /// Speaks the passed-in text. If <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <seealso cref="Language"/> property is used. Only streams in WAVE audio format are directly supported by this method.
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <exception cref="NotSupportedException">The <see cref="AudioFormat"/> property is set to MP3.</exception>
        /// <remarks><para>This method  first call the <see cref="GetSpeakStream(string)"/> method, then it automatically speaks the received stream. It will block until the entire operation is completed. If you want to perform a non-blocking speaking and to be notified when the operation completes, use the <see cref="SpeakAsync(string)"/> method instead.</para>
        /// <para>Only streams in WAVE audio format are directly supported by this method. If you call this method while the <see cref="AudioFormat"/> property is set to <strong>MP3</strong>, a <see cref="NotSupportedException"/> error will be generated.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para>
        /// </remarks>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakStream(string)"/>
        /// <seealso cref="SpeakAsync(string)"/>
        public void Speak(string text)
        {
            this.Speak(text, null);
        }

        /// <summary>
        /// Speaks the passed-in text in the language specified in the desired language. If <paramref name="language"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) and the <see cref="AutoDetectLanguage"/> property is set to <strong>true</strong>, the <see cref="DetectLanguage"/> method is used to detect the language of the speech stream. Otherwise, the language specified in the <see cref="Language"/> property is used. Only streams in WAVE audio format are directly supported by this method.
        /// </summary>
        /// <param name="text">A string containing the sentence to be spoken.</param>
        /// <param name="language">A string representing the language code to speak the text in. The code must be present in the list of codes returned from the method <see cref="GetLanguages"/>.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <term>The <see cref="ClientID"/> or <see cref="ClientSecret"/> properties haven't been set.</term>
        /// <term>The <paramref name="text"/> parameter is longer than 1000 characters.</term>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="text"/> parameter is <strong>null</strong> (<strong>Nothing</strong> in Visual Basic) or empty.</exception>
        /// <exception cref="NotSupportedException">The <see cref="AudioFormat"/> property is set to MP3.</exception>
        /// <remarks>
        /// <para>This method first call the <see cref="GetSpeakStream(string, string)"/> method, then it automatically speaks the received stream. It will block until the entire operation is completed. If you want to perform a non-blocking speaking and to be notified when the operation completes, use the <see cref="SpeakAsync(string, string)"/> method instead.</para>
        /// <para>Only streams in WAVE audio format are directly supported by this method. If you call this method while the <see cref="AudioFormat"/> property is set to <strong>MP3</strong>, a <see cref="NotSupportedException"/> error will be generated.</para>
        /// <para>For more information, go to http://msdn.microsoft.com/en-us/library/ff512420.aspx.
        /// </para></remarks>
        /// <seealso cref="Language"/>        
        /// <seealso cref="GetSpeakStream(string, string)"/>
        /// <seealso cref="SpeakAsync(string, string)"/>
        public void Speak(string text, string language)
        {
            if (AudioFormat == SpeakStreamFormat.MP3)
                throw new NotSupportedException("Speak method supports WAVE audio format only");

            using (Stream stream = this.GetSpeakStream(text, language))
            {
                using (SoundEffect player = SoundEffect.FromStream(stream))
                {
                    FrameworkDispatcher.Update();
                    player.Play();
                }
            }
        }

        private void OnSpeakCompleted(AsyncCompletedEventArgs e)
        {
            if (SpeakCompleted != null)
            {
                SpeechEventArgs args = new SpeechEventArgs(e.Error);
                SpeakCompleted(this, args);
            }
        }

        #endregion

        public string[] Languages { get; set; }

        public Stream SpeakMemoryStream { get; set; }

        public string DetectedLanguage { get; set; }

        public string TranslatedText { get; set; }
    }
}
