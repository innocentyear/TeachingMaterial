using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
namespace TeachingMaterial.Models
{
    public class GradeMajorViewModel
    {

        [Display(Name = "年级专业ID")]
        public int? GradeMajorID { get; set; }
        
        [Display(Name ="年级")]
        public SelectList GradesList { get; set; } //select 定义在System.Web.Mvc命名空间下。

        [Display(Name ="专业")]
        public SelectList MajorsList { get; set; }

        [Display(Name = "是否有效")]
        public bool GradeMajorIsValidate { get; set; }
    }
}