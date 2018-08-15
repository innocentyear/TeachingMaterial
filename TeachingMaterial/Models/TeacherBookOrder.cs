using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    public class TeacherBookOrder
    {
        public int TeacherBookOrderID { get; set; }

        [Display(Name = "专业ID")]
        public int? MajorID { get; set; }

        [Display(Name = "专业")]
        public virtual Major Major{ get; set; }


        [Display(Name = "征订ID")]
        public int? BookSubscriptionID { get; set; } //如果不为空将引发级联删除 和循环引用的错误。

        [Display(Name = "征订记录")]
        public virtual BookSubscription BookSubscription { get; set; }

        [Display(Name = "生成时间")]
        public DateTime GenerateTime { get; set; }
    }
}