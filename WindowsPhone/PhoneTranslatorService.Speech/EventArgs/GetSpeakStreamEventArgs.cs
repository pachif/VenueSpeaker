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
using System.Text;
using System.IO;

namespace MicroTranslatorService.Speech
{
    /// <summary>
    /// Provides data for the <strong>GetSpeakStreamEventArgs</strong> event.
    /// </summary>
    /// <seealso cref="SpeechEventArgs"/>
    public class GetSpeakStreamEventArgs : SpeechEventArgs
    {
        /// <summary>
        /// Gets a<see cref="System.IO.Stream"/> object that contains a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/> method.
        /// </summary>
        /// <value>A <see cref="System.IO.Stream"/> object that contains a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/> method.</value>
        /// <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/>
        public Stream Stream { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakStreamEventArgs</strong> class.
        /// </summary>
        public GetSpeakStreamEventArgs()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakStreamEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        public GetSpeakStreamEventArgs(Exception error)
            : base(error)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakStreamEventArgs</strong> class using the specified <see cref="System.IO.Stream"/> object.
        /// </summary>
        /// <param name="stream">A <see cref="System.IO.Stream"/> object that contains a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/> method.</param>
        /// <seealso cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/>
        public GetSpeakStreamEventArgs(Stream stream)
            : this(stream, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakStreamEventArgs</strong> class using the specified <see cref="System.IO.Stream"/> object and <see cref="Exception"/>.
        /// </summary>
        /// <param name="stream">A <see cref="System.IO.Stream"/> object that contains a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/> method.</param>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakStreamAsync(string, string)"/>
        public GetSpeakStreamEventArgs(Stream stream, Exception error)
            : base(error)
        {
            Stream = stream;
        }
    }
}
