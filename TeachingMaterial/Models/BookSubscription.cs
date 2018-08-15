using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TeachingMaterial.Models
{
    //生成的原始订单数据。系部征订人员征订学期课程，生成的订单。
    public class BookSubscription
    {
        [Display(Name ="征订单ID")]
        public int BookSubscriptionID { get; set; }

        [Display(Name = "教材ID")]
        public int BookID { get; set; }

        [Display(Name = "教材")]
        public virtual Book Book { get; set; }


        [Display(Name = "学期课程ID")]
        public int SemesterCourseID { get; set; }


        [Display(Name = "学期课程")]
        public virtual SemesterCourse SemesterCourse { get; set; }
       

        [Display(Name = "征订数量")]  //来自年级专业表的人数，但是可以修改。
        public int BookSubscriptionNumber { get; set; }

        [Display(Name = "征订单价格")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal SubscriptionPrice { get; set; }


        [Display(Name ="征订类别")]
        //征订的是学生教材 还是教师用书
        public SubscriptionType SubscriptionType { get; set; }


        //后来方便汇总，加入冗余列
        [Display(Name ="学期名称")]
        public string SemesterName { get; set; }

        [Display(Name ="年级名称")]
        public string GradeName { get; set; }

        [Display(Name = "系部名称")]
        public string DepartmentName { get; set; }

        [Display(Name = "专业名称")]
        public string MajorName { get; set; }

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

        [Display(Name = "教材类别")]
        public string BookTypeName{ get; set; }

        [Display(Name = "征订类别名字")]
        //征订的是学生教材 还是教师用书
        public string SubscriptionTypeName { get; set; }

        
        [Display(Name ="班级征订记录")]
        public virtual ICollection<SchoolClassBookOrder> SchoolClassBookOrders { get; set; }
      

    }
    public enum SubscriptionType
    { 学生教材,教师用书}
}