using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace TeachingMaterial.Models
{
    //学期课程，指哪个专业哪个年级开设的课程。没有将课程单独建为一个实体，考虑到方便。
    //学期课程 包括 学期，年级专业和课程
    public class SemesterCourse
    {
        [Display(Name = "学期课程ID")]
        public int SemesterCourseID { get; set; }

        [Display(Name = "学期课程代码")]
        [StringLength(10, MinimumLength = 4, ErrorMessage = "{0}字符数必须大于{2},小于{1},课程代码必须与信息平台一致")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]

        public string SemesterCourseNumber { get; set; }

        [Display(Name = "学期课程名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string SemesterCourseName { get; set; }

        [Display(Name = "课程类型")]
        public CourseType CourseType { get; set; }

        [Display(Name = "年级专业ID")]
        public int GradeMajorID { get; set; }

        [Display(Name = "年级专业")]
        public virtual GradeMajor GradeMajor { get; set; }

        [Display(Name = "学期ID")]
        public int SemesterID { get; set; }

        [Display(Name = "学期")]
        public virtual Semester Semester { get; set; }

        [Display(Name = "更改教师")]
        [StringLength(10, ErrorMessage = "{0}的字符数必须小于{1}")]
        public string AuthorName { get; set; }

        [Display(Name = "更改时间")]
        public DateTime PostTime { get; set; }

        //补加的字段

        [Display(Name = "是否需要学生教材")]
        public bool? IsNeedStudentBook { get; set; }

        [Display(Name = "是否需要教师用书")]
        public bool? IsNeedTeacherBook { get; set; }

        [Display(Name = "征订教师")]
        [StringLength(10, ErrorMessage = "{0}的字符数必须小于{1}")]
        public string SubscriptionUserName { get; set; }
       

        [Display(Name = "征订时间")]
        public DateTime? SubscriptionTime { get; set; }

        [Display(Name = "提交状态")]
        public bool SubmmitState { get; set; }


        [Display(Name ="课程关联的征订单")]
        public virtual ICollection<BookSubscription> BookSubscriptions { get; set; }
        
        
    }
  

    public enum CourseType
    {
        专业课,公共课,思政课
    }
}