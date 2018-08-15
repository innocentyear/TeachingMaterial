using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class DepartmentViewModel
    {
        public int DepartmentID { get; set; }

        [Display(Name = "部门名称")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2}，小于{1}")]
        public string DepartmentName { get; set; }
    }


    public class AdministratorViewModel
    {
        [Display(Name = "用户ID")]
        public string Id { get; set; }


        [Required]
        [StringLength(20, ErrorMessage = "{0}少于{2}个字符", MinimumLength = 2)]
        [Display(Name = "姓名")]
        public string RealName { get; set; }

        public int? DepartmentID { get; set; }

    }
}