using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TranslatorService.Speech
{
    public class AuthEventArgs : EventArgs
    {
        public AuthEventArgs(AdmAccessToken token, Action procedure)
        {
            Token = token;
            ToExecute = procedure;
        }

        public AdmAccessToken Token { get; set; }

        public Action ToExecute { get; set; }
    }
}
