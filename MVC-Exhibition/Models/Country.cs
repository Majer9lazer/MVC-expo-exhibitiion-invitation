using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LiteDB;

namespace MVC_Exhibition.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public List<City> Cities { get; set; }
        
    }
}