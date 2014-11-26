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
using System.Reflection;
//using Microsoft.SPOT;

namespace MicroTranslatorService.Speech
{
    /// <summary>
    /// Provides data for the <em>MethodName</em><strong>Completed</strong> event.
    /// </summary>
    public class SpeechEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating which error occurred during an asynchronous operation.
        /// </summary>
        /// <value>An <see cref="Exception"/> instance, if an error occurred during an asynchronous operation; otherwise a <strong>null</strong> reference (<strong>Nothing</strong> in Visual Basic).</value>
        /// <remarks><para>If an exception is raised during an asynchronous operation, the class will assign the exception to the <see cref="Error"/> property. The client application's event-handler delegate should check the <see cref="Error"/> property before accessing any properties in a class derived from <see cref="SpeechEventArgs"/>.</para>
        /// <para>The value of the <see cref="Error"></see> property is a <strong>null</strong> reference (<strong>Nothing</strong> in Visual Basic) if no error has been raised.</para></remarks>
        public Exception Error { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechEventArgs</strong> class.
        /// </summary>
        public SpeechEventArgs()
            : this(null)
        { }

        /// <summary>
        /// Initializes a new instance of the <strong>SpeechEventArgs</strong> class setting the specified <see cref="Exception"/> object.
        /// </summary>
        /// <param name="error">Any error that occurred during the asynchronous operation.</param>
        public SpeechEventArgs(Exception error)
        {
            Error = error;
        }
    }
}
