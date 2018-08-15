using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace TeachingMaterial.Models
{
    public class SemesterCourseViewModel
    {
        [Display(Name = "学期课程ID")]
        public int? SemesterCourseID { get; set; }

        [Display(Name ="学期")]
        public SelectList SemesterList { get; set; }

        [Display(Name = "年级")]
        public SelectList GradesList { get; set; } //select 定义在System.Web.Mvc命名空间下。

        [Display(Name = "部门")]
        public SelectList DepartmentList { get; set; }

        [Display(Name = "专业")]
        public SelectList MajorsList { get; set; }

        [Display(Name = "课程类型")]
        public CourseType CourseType { get; set; }

        [Display(Name = "学期课程代码")]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "{0}字符数必须大于{2},小于{1},课程代码必须与信息平台一致")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]

        public string SemesterCourseNumber { get; set; }

        [Display(Name = "学期课程名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string SemesterCourseName { get; set; }
    }
}