using System;
using System.Collections.Generic;
using FourSquare.SharpSquare.Entities;

namespace FourSquare.SharpSquare.Core
{
    public class FourSquareMultipleResponse<T> : FourSquareResponse where T : FourSquareEntity
    {
        public FourSquareMultipleResponseObject<T> response { get; set; }
    }

    public class FourSquareMultipleResponseObject<T>
    {
        public List<T> venues
        {
            get;
            set;
        }

        public bool confident { get; set; }
    }
}