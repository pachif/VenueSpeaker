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
    /// Provides data for the <strong>GetLanguagesCompleted</strong> event.
    /// </summary>
    /// <seealso cref="SpeechEventArgs"/>
    /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.GetLanguagesCompleted"/>
    public class GetLanguagesEventArgs : SpeechEventArgs
    {
        private string[] languages;
        /// <summary>
        /// Gets the list of the language codes supported for speech synthesis by <strong>Microsoft Translator Service</strong>.
        /// </summary>
        /// <value>Gets the list of the language codes supported for speech synthesis by <strong>Microsoft Translator Service</strong>.</value>
        public string[] Languages
        {
            get 
            { 
                return base.ReturnValue(languages); 
            }
            internal set 
            { 
                languages = value; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <strong>GetLanguagesEventArgs</strong> class.
        /// </summary>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.GetLanguagesAsync"/>
        public GetLanguagesEventArgs()
            : this(null, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetLanguagesEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.GetLanguagesAsync"/>
        public GetLanguagesEventArgs(Exception error)
            : this(null, error)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetLanguagesEventArgs</strong> class using the specified language codes array.
        /// </summary>
        /// <param name="languages">An array of language codes that are supported for speech synthesis by <strong>Microsoft Translator Service</strong>.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.GetLanguagesAsync"/>
        public GetLanguagesEventArgs(string[] languages)
            : this(languages, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetLanguagesEventArgs</strong> class setting the specified language codes array and <see cref="Exception"/> object.
        /// </summary>
        /// <param name="languages">An array of language codes that are supported for speech synthesis by <strong>Microsoft Translator Service</strong>.</param>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="TranslatorService.Speech.SpeechSynthesizer.GetLanguagesAsync"/>
        public GetLanguagesEventArgs(string[] languages, Exception error)
            : base(error)
        {
            this.languages = languages;
        }
    }
}
