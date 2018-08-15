using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TeachingMaterial.Models
{
    public class Book
    {
        [Display(Name = "教材ID")]
        public int BookID { get; set; }

        [Display(Name = "教材名称（版次）")]
        [Required]
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
        

        [Display(Name ="教材类别")]
        public int BookTypeID { get; set; }

        [Display(Name = "教材类别")]
        public virtual BookType BookType { get; set; }

        [Display(Name ="教材汇总信息")]
        public string BookInfo
        {
            get
            {
                return BookName + "/" + AuthorName + "/" + ISBN + "/" + Press + "/" + BookType.BookTypeName + "/" + Price + "元";
            }
           
        }

        [Display(Name = "教材关联的征订单")]
        public virtual ICollection<BookSubscription> BookSubscriptions { get; set; }


    }
}