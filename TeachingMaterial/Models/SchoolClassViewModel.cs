using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace TeachingMaterial.Models
{
    public class SchoolClassViewModel
    {
        [Display(Name = "班级ID")]
        public int? SchoolClassID { get; set; }

        [Display(Name = "年级")]
        public SelectList GradesList { get; set; } //select 定义在System.Web.Mvc命名空间下。

        [Display(Name = "部门")]
        public SelectList DepartmentList { get; set; }

        [Display(Name = "专业")]
        public SelectList MajorsList { get; set; }


        [Display(Name = "班级代码")]
        [StringLength(12, MinimumLength = 5, ErrorMessage = "{0}字符数必须大于{2},小于{1},班级代码必须与信息平台一致")] //考虑到中专没有代码啊，五年制有的8位，10位，大专代码为9位。
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string SchoolClassNumber { get; set; }

        [Display(Name = "班级名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string SchoolClassName { get; set; }

        [Display(Name = "学生人数")]
        [Range(0, 8000, ErrorMessage = "{0}必须大于{1},并且小于{2}")] //{0}表示字段的显示名称，{1}为Range的第一个参数，最小值。{2}为Range的第二个参数，最大值。
        public int StudentNumber { get; set; }
    }
}