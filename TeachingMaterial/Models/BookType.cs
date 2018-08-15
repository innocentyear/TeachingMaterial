using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace TeachingMaterial.Models
{
    public class BookType
    {
        [Display(Name ="教材类别ID")]
        public int BookTypeID { get; set; }

        [Display(Name ="教材类别名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string BookTypeName { get; set; }
        
        [Display(Name ="属于此类别的教材")]
        public virtual ICollection<Book> Books { get; set; }


    }
}