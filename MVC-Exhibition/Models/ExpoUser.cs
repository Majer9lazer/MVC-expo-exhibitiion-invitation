using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Exhibition.Models
{
    public class ExpoUser
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string UserEmail { get; set; }
        [Required]
        public string UserPhoneNumber { get; set; }
        [Required]
        public string UserCompanyName { get; set; }
        
        public int UserCountryId { get; set; }
        //public Country UserCountry { get; set; }
        
        public int UserCityId { get; set; }
        //public City UserCity { get; set; }
        [Required]
        public string StoAddress { get; set; }
        public string KindOfActivity { get; set; }
        public string WorkPosition { get; set; }
    }
}