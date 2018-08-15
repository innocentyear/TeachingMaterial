using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class Major
    {
        [Display(Name = "专业ID")]
        public int MajorID { get; set; }

        [Display(Name = "专业名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string MajorName { get; set; }

        [Display(Name = "部门ID")]
        public int DepartmentID { get; set; }

        [Display(Name = "部门")]
        public virtual Department Department { get; set; }

        [Display(Name = "拥有的年级专业")]
        public ICollection<GradeMajor> GradeMajors { get; set; }

        [Display(Name = "专业学生人数")]
        //专业人数计算列 //如果该专业下的 年级专业为非空，则遍历该专业下的所有的年级专业。
        // 不知道什么原因，二次遍历 无法成功，也就是说始终判定 gradeMajor为空,无法用作计算列。
        public int MajorStudentNumber { get; set; }
        /*{
            get
            {
                int majorStudentNumber = 0;
                if (this.GradeMajors != null)
                {
                    foreach (var gradeMajorItem in this.GradeMajors)
                    {
                        majorStudentNumber += gradeMajorItem.GradeMajorStudentNumber;
                    }
                }

                return majorStudentNumber;
            }
        }
        */

        public virtual ICollection<TeacherBookOrder> TeacherBookOrders { get; set; } //一个部门可以对应多条 教师用书的 征订记录。

    }
}