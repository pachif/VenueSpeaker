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
using System.Reflection;

namespace TranslatorService.Speech
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
        /// <remarks><para>If an exception is raised during an asynchronous operation, the class will assign the exception to the <see cref="Error"/> property. The client application's event-handler delegate should check the <see cref="Error"/> property before accessing any properties in a class derived from <see cref="SpeechEventArgs"/>; otherwise, the property will raise a <see cref="System.Reflection.TargetInvocationException"/>with its <see cref="System.Exception.InnerException"/> property holding a reference to <see cref="Error"></see>.</para>
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

        /// <summary>
        /// Returns the passed value, if the <see cref="Error"/> property is a <strong>null</strong> reference (<strong>Nothing</strong> in Visual Basic); otherwise, it will raise a <see cref="System.Reflection.TargetInvocationException"/>with its <see cref="System.Exception.InnerException"/> property holding a reference to <see cref="Error"></see>.
        /// </summary>
        /// <typeparam name="T">The type of the property to be returned.</typeparam>
        /// <param name="value">The value of the property to be return if <see cref="Error"/> property is a <strong>null</strong> reference (<strong>Nothing</strong> in Visual Basic).</param>
        /// <returns>The passed <paramref name="value"/>, if the <see cref="Error"/> property is a <strong>null</strong> reference (<strong>Nothing</strong> in Visual Basic); otherwise, it will raise a <see cref="System.Reflection.TargetInvocationException"/>with its <see cref="System.Exception.InnerException"/> property holding a reference to <see cref="Error"></see>.</returns>
        protected T ReturnValue<T>(T value)
        {
            if (Error != null)
                throw new TargetInvocationException(Error);
            
            return value;
        }
    }
}
