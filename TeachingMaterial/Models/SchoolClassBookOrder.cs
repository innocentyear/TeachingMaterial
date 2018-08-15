using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TeachingMaterial.Models
{
    //班级征订记录
    public class SchoolClassBookOrder
    {
        public int SchoolClassBookOrderID { get; set; }

        [Display(Name ="班级ID")]
        public int? SchoolClassID { get; set; }

        [Display(Name = "班级ID")]
        public virtual SchoolClass SchoolClass { get; set; }

        [Display(Name = "征订ID")]
        public int? BookSubscriptionID { get; set; } //如果不为空将引发级联删除 和循环引用的错误。

        [Display(Name ="征订")]
        public virtual BookSubscription BookSubscription { get; set; }

        [Display(Name ="生成时间")]
        public DateTime GenerateTime { get; set; }



        //后来方便汇总，加入冗余列

       

        [Display(Name = "学期名称")]
        public string SemesterName { get; set; }

        [Display(Name = "年级名称")]
        public string GradeName { get; set; }

        [Display(Name = "系部名称")]
        public string DepartmentName { get; set; }

        [Display(Name = "专业名称")]
        public string MajorName { get; set; }

        [Display(Name = "班级名称")]
        public string SchoolClassName { get; set; }  //班级

        [Display(Name = "课程编号")]
        public string SemesterCourseNumber { get; set; }

        [Display(Name = "学期课程名称")]
        public string SemesterCourseName { get; set; }

        [Display(Name = "教材名称（版次）")]
        public string BookName { get; set; }

        [Display(Name = "作者")]
        public string AuthorName { get; set; }

        [Display(Name = "ISBN号")]
        public string ISBN { get; set; }
        [Display(Name = "出版社")]
        public string Press { get; set; }

        [Display(Name = "出版日期（年）")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PublishingDate { get; set; }

        [Display(Name = "定价")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Display(Name = "征订数量")]  
        public int? BookSubscriptionNumber { get; set; }    //征订数量

        [Display(Name = "教材类别")]
        public string BookTypeName { get; set; }

        [Display(Name = "征订类别名字")]
        //征订的是学生教材 还是教师用书
        public string SubscriptionTypeName { get; set; }


        [Display(Name = "征订单价格")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal? SubscriptionPrice { get; set; }   //价格




    }
}