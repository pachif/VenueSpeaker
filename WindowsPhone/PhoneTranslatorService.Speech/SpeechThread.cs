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
//using Microsoft.SPOT;
using System.IO;

namespace MicroTranslatorService.Speech
{
    internal class SpeechThread
    {
        private SpeechSynthesizer speechSynthesizer;

        #region Events

        internal event SpeechSynthesizer.GetSpeakStreamEventHandler GetSpeakStreamCompleted;
        internal event SpeechSynthesizer.GetSpeakBytesEventHandler GetSpeakBytesCompleted;

        #endregion

        #region Properties

        public string Text { get; set; }
        public string Language { get; set; }

        #endregion

        public SpeechThread(SpeechSynthesizer speechSynthesizer, string text)
            : this(speechSynthesizer, text, null)
        { }        

        public SpeechThread(SpeechSynthesizer speechSynthesizer, string text, string language)
        {
            this.speechSynthesizer = speechSynthesizer;
            Text = text;
            Language = language;
        }

        public void GetSpeakStream()
        {
            Stream stream = null;
            Exception error = null;

            try
            {
                stream = speechSynthesizer.GetSpeakStream(Text, Language);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (GetSpeakStreamCompleted != null)
            {
                GetSpeakStreamEventArgs args = new GetSpeakStreamEventArgs(stream, error);
                GetSpeakStreamCompleted(this, args);
            }
        }

        public void GetSpeakBytes()
        {
            Byte[] data = null;
            Exception error = null;

            try
            {
                using (MemoryStream stream = speechSynthesizer.GetSpeakStream(Text, Language) as MemoryStream)
                {
                    data = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (GetSpeakBytesCompleted != null)
            {
                GetSpeakBytesEventArgs args = new GetSpeakBytesEventArgs(data, error);
                GetSpeakBytesCompleted(this, args);
            }
        }
    }
}
