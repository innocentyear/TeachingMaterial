using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class Grade
    {
        [Display(Name = "年级ID")]
        public int GradeID { get; set; }


        [Display(Name = "年级名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string GradeName { get; set; }

        [Display(Name = "拥有的年级专业")]
        public ICollection<GradeMajor> GradeMajors { get; set; }

        [Display(Name = "是否有效")]
        public bool GradeIsValidate { get; set; }

        //行不通  年级人数计算列 //如果该年级下的 年级专业为非空，则遍历该年级下的所有的年级专业。
        //不知道什么原因，二次遍历 无法成功，也就是说始终判定 gradeMajor为空,无法用作计算列。
        [Display(Name = "年级学生人数")]
        public int GradeStudentNumber { get; set; }
       /* {
            get
            {
                int gradeStudentNumber = 0;
                if (this.GradeMajors != null)
                {
                    foreach (var gradeMajorItem in this.GradeMajors)
                    {
                        gradeStudentNumber += gradeMajorItem.GradeMajorStudentNumber;
                    }
                }
                return gradeStudentNumber;
            }
        }
        */

    }
}