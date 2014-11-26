using System;
using System.Collections.Generic;
using System.Linq;


namespace FourSquare.SharpSquare.Entities
{
    public class Price : FourSquareEntity
	{
        public string tier
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