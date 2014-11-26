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
    /// Provides data for the <strong>GetSpeakBytesEventArgs</strong> event.
    /// </summary>
    /// <seealso cref="SpeechEventArgs"/>
    public class GetSpeakBytesEventArgs : SpeechEventArgs
    {
        /// <summary>
        /// Gets a byte array containg the stream of a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/> method.
        /// </summary>
        /// <value>A byte array object that contains a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/> method.</value>
        /// <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/>
        public byte[] Data { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakBytesEventArgs</strong> class.
        /// </summary>
        public GetSpeakBytesEventArgs()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakBytesEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        public GetSpeakBytesEventArgs(Exception error)
            : base(error)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakBytesEventArgs</strong> class using the specified <see cref="System.IO.Stream"/> object.
        /// </summary>
        /// <param name="data">A byte array containg the stream of a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/> method.</param>
        /// <seealso cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/>
        public GetSpeakBytesEventArgs(byte[] data)
            : this(data, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>GetSpeakBytesEventArgs</strong> class using the specified byte array and <see cref="Exception"/>.
        /// </summary>
        /// <param name="data">A byte array containg the stream of a wave-file speaking the text passed to the <see cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/> method.</param>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        /// <seealso cref="MicroTranslatorService.Speech.SpeechSynthesizer.GetSpeakBytesAsync(string, string)"/>
        public GetSpeakBytesEventArgs(byte[] data, Exception error)
            : base(error)
        {
            Data = data;
        }
    }
}
