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
    /// Provides data for the <strong>DetectLanguageCompleted</strong> event.
    /// </summary>
    /// <seealso cref="SpeechEventArgs"/>
    /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageCompleted"/>
    public class DetectLanguageEventArgs : SpeechEventArgs
    {
        private string language;
        /// <summary>
        /// Gets a string containing a two-character Language code for the text passed to the <see cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/> method.
        /// </summary>
        /// <value>A string containing a two-character Language code for the text passed to the <see cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/> method.</value>
        /// <see cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/>
        public string Language
        {
            get
            { 
                return base.ReturnValue(language); 
            }
            internal set 
            { 
                language = value; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <strong>DetectLanguageEventArgs</strong> class.
        /// </summary>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/>
        public DetectLanguageEventArgs()
            : this(null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>DetectLanguageEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/>
        public DetectLanguageEventArgs(Exception error)
            : this(null, error)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>DetectLanguageEventArgs</strong> class using the specified language code.
        /// </summary>
        /// <param name="language">A string containing a two-character Language code for the given text.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.TranslateAsync(string)"/>
        public DetectLanguageEventArgs(string language)
            : this(language, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>DetectLanguageEventArgs</strong> class using the language code and <see cref="Exception"/>.
        /// </summary>
        /// <param name="language">A string containing a two-character Language code for the given text.</param>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.DetectLanguageAsync(string)"/>
        public DetectLanguageEventArgs(string language, Exception error)
            : base(error)
        {
            this.language = language;
        }
    }
}
