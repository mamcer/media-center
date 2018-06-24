using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MobileClient.Models
{
    public class SearchModel
    {
        [Required]
        public string SearchString { get; set; }
    }
}