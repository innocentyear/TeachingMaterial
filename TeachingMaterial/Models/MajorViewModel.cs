using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace TeachingMaterial.Models
{
    public class MajorViewModel
    {
        [Display(Name = "专业ID")]
        public int? MajorID { get; set; }

        [Display(Name = "专业名称")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "{0}字符数必须大于{2},小于{1}")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        public string MajorName { get; set; }  //为ViewModel添加验证，以使提交的表单能够反馈错误。提交的表单校验是以表单使用的模型为准的，而不是使用原始的数据模型为准。

        // public System.Web.Mvc.SelectList DepartmentsSelectlist { get; set; } 
        //public IEnumerable<SelectListItem> DepartmentsList { get; set; }   //用于呈现下拉框，或列表框 在System.Web.Mvc;在命名空间下面。 
        //如果将DepartmentsList 改为DepartmentID,则在视图上会显示不正确。

        public SelectList DepartmentsList { get; set; }

    }
}