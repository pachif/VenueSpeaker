using System;
using System.Collections.Generic;
using System.Linq;


namespace FourSquare.SharpSquare.Entities
{
    public class VenueExplore : FourSquareEntity
    {
        public FourSquareEntityItems<Reasons> reasons
        {
            get;
            set;
        }

        public Venue venue
        {
            get;
            set;
        }
	}
}