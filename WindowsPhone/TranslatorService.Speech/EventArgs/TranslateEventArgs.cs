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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TranslatorService.Speech
{
    /// <summary>
    /// Provides data for the <strong>TranslateCompleted</strong> event.
    /// </summary>
    /// <seealso cref="SpeechEventArgs"/>
    /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateCompleted"/>
    public class TranslateEventArgs : SpeechEventArgs
    {
        private string translatedText;
        /// <summary>
        /// Gets the translation of the text passed to the <see cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/> method.
        /// </summary>
        /// <value>The translation of the text passed to the <see cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/> method.</value>
        /// <see cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/>
        public string TranslatedText
        {
            get
            { 
                return base.ReturnValue(translatedText); 
            }
            internal set 
            { 
                translatedText = value; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <strong>TranslateEventArgs</strong> class.
        /// </summary>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/>
        public TranslateEventArgs()
            : this(null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>TranslateEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/>
        public TranslateEventArgs(Exception error)
            : this(null, error)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>TranslateEventArgs</strong> class using the specified text.
        /// </summary>
        /// <param name="translatedText">A string representing the translated text.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/>
        public TranslateEventArgs(string translatedText)
            : this(translatedText, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>TranslateEventArgs</strong> class using the specified text and <see cref="Exception"/>.
        /// </summary>
        /// <param name="translatedText">A string representing the translated text.</param>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string, string)"/>
        public TranslateEventArgs(string translatedText, Exception error)
            : base(error)
        {
            this.translatedText = translatedText;
        }
    }
}
