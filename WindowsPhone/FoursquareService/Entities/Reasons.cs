using System;
using System.Collections.Generic;
using System.Linq;


namespace FourSquare.SharpSquare.Entities
{
    public class Reasons : FourSquareEntity
    {
        public string type
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }
    }
}