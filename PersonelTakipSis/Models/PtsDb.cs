using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PersonelTakipSis.Models
{
    public static class PtsDb
    {
        static PetaPoco.Database db;

        public static PetaPoco.Database Instance 
        {
            get
            {
                if (db == null)
	            {
		            db= new PetaPoco.Database("PTSConnection");
	            }
                return db;
            }                           
        }
    }
}