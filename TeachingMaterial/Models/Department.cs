using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [Display(Name ="部门名称")]
        [StringLength(10,MinimumLength =2,ErrorMessage ="{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings =false,ErrorMessage ="{0}不能全为空字符")]
        public string DepartmentName { get; set; }

        [Display(Name = "所在地点")]
        [StringLength(50, ErrorMessage = "{0}不能超过{1}个字符")]
        public string DepartmentLocation { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public virtual ICollection<ApplicationUser> Administrators { get; set; }

        public virtual ICollection<Major>  Majors { get; set; }

    }
}