using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class ChooseBookViewModel
    {

        [Display(Name = "教材ID")]
        public int BookID { get; set; }

        [Display(Name = "教材汇总信息")]
        public string BookInfo { get; set; }
       
    }
}
