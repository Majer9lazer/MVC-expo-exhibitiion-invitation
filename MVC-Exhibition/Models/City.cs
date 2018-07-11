using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Exhibition.Models
{
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public Country CitiesCountry { get; set; }

    }
}