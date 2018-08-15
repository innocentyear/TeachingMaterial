using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    //年级专业
    public class GradeMajor
    {

        [Display(Name = "年级专业ID")]
        public int GradeMajorID { get; set; }

        [Display(Name = "年级ID")]
        public int GradeID { get; set; }

        [Display(Name = "专业ID")]
        public int MajorID { get; set; }

        [Display(Name = "所属年级")]
        public virtual Grade Grade { get; set; }

        [Display(Name = "所属专业")]
        public virtual Major Major { get; set; }

        //决定还是不要这个 年级字段
        // public string GradeName { get; private set; }  将年级名称和专业名称设为私有字段和公有属性 为空值，设置不起，不知道为什么;

        //决定还是不要这个 专业字段
        // public string MajorName { get; private set; }


        [Display(Name = "班级")]
        public virtual ICollection<SchoolClass> SchoolClasses { get; set; }


        [Display(Name = "开设的学期课程")]
        public virtual ICollection<SemesterCourse> SemesterCourses { get; set; }


        [Display(Name = "是否有效")]
        public bool GradeMajorIsValidate { get; set; }

        public int GradeMajorStudentCount { get; set; }

        [Display(Name = "学生人数")]
        //年级专业人数计算列 //如果该年级专业的班级不为空，则遍历该年级专业中的所有班级，
        //采用过滤器的方式也失败了。
        public int GradeMajorStudentNumber
        {
            get
            {
                int gradeMajorStudentNumber = 0;
                if (this.SchoolClasses != null)
                {
                    foreach (var classItem in this.SchoolClasses)
                    {
                        gradeMajorStudentNumber += classItem.StudentNumber;
                    }
                }
                return gradeMajorStudentNumber;
            }
        }



    }
}