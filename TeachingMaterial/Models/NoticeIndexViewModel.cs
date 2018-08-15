using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace TeachingMaterial.Models
{
    //定义一个用于显示公告列表的视图模型对象，主要是用于返回不用生成 公告内容的字段，这样会提高系统访问速度；
    public class NoticeIndexViewModel
    {
        [Display(Name = "公告ID")]
        public int? NoticeID { get; set; }
        

        [Display(Name = "标题")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(50, ErrorMessage = "{0}的字符数必须小于{1}")]
        public string NoticeTitle { get; set; }

        /* 
          [Display(Name="正文")]
          [DataType(DataType.MultilineText)]
          public string Content { get; set; }  
         */

        [Display(Name = "作者")]
        [StringLength(20, ErrorMessage = "{0}的字符数必须小于{1}")]
        public string AuthorName { get; set; }

        [Display(Name = "发布日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PostTime { get; set; }

        [Display(Name = "是否显示")]
        public bool NoticeIsShow { get; set; }

        [Display(Name = "优先级")]
        [Range(1, 100, ErrorMessage = "{0}必须在{1}到{2}之间")]
        public int PriorOrder { get; set; }

        [Display(Name = "访问量")]
        public int ClickCount { get; set; }


    }
}