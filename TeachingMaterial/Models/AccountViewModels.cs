using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace TeachingMaterial.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "代码")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "记住此浏览器?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }

    //登录视图模型
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "用户")]
        [StringLength(20,MinimumLength =4,ErrorMessage ="{0}至少包含{2}个字符")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我?")]
        public bool RememberMe { get; set; }
    }

    //创建新用户视图模型
    public class RegisterViewModel
    {
        //可以通过构造方法添加默认值。 最好的方式是在控制器里新建一个对象，给予默认值。
        /*  public RegisterViewModel()
        {
            this.Gender = Gender.女;
            this.ProfessionalTitle = ProfessionalTitle.讲师;
            this.Birthday = DateTime.Now;
        }
      */

        [Required]
        [StringLength(20,MinimumLength =4, ErrorMessage = "{0}至少包含{2}个字符")]
        [Display(Name ="用户名")]
        public string UserName { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Required]
        [StringLength(20,MinimumLength =2, ErrorMessage = "{0}至少包含{2}个字符")]
        [Display(Name ="姓名")]
        public string RealName { get; set; }

        [Display(Name ="性别")]
        [Required]
        public Gender Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "出生日期")]
        public DateTime Birthday { get; set; }

        [Display(Name ="所属部门")]
        public int? DepartmentID { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]        
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    //既作为EditUser视图模型，也作为Index视图模型。
    public class EditUserViewModel
    {
        [Display(Name ="用户ID")]
        public string Id { get; set; }

        [Display(Name ="用户名")]
        [Required]
        [StringLength(20,MinimumLength =4,ErrorMessage ="{0}至少包含{2}个字符")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }


        [Required]
        [StringLength(20, ErrorMessage = "{0}少于{2}个字符", MinimumLength = 2)]
        [Display(Name = "姓名")]
        public string RealName { get; set; }

        [Display(Name = "性别")]
        [Required]
        public Gender Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "出生日期")]
        public DateTime Birthday { get; set; }

        [Display(Name = "年龄")]
        public int Age
        {
            get
            {
                if ((DateTime.Now.Month > Birthday.Month) || ((DateTime.Now.Month == Birthday.Month) && (DateTime.Now.Day > Birthday.Day)))
                    return (DateTime.Now.Year - Birthday.Year);
                else
                    return (DateTime.Now.Year - Birthday.Year - 1);
            }
        }


        [Display(Name = "所属部门")]
        public int?  DepartmentID { get; set; }

        [Display(Name = "部门名字")]
        public string DepartmentName { get; set; }

        [Display(Name = "该用户所拥有的角色")]
        public IEnumerable<System.Web.Mvc.SelectListItem> RolesList { get; set; }  //注意命名空间，这里使用ICollection<System.Web.Mvc.SelectListItem> 主要是为了在编辑视图中呈现 全部角色数据，但也可表示角色是否选中的状态。

    }


    /// <summary>
    /// 用户更改自身账户视图模型
    /// </summary>
    public class EditUserSelfViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "{0}少于{2}个字符", MinimumLength = 2)]
        [Display(Name = "姓名")]
        public string RealName { get; set; }

        [Display(Name = "性别")]
        [Required]
        public Gender Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "出生日期")]
        public DateTime Birthday { get; set; }

        [Display(Name = "所属部门")]
        public int? DepartmentID { get; set; }

    }
    public class ResetPasswordViewModel
    {
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码")]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "电子邮件")]
        public string Email { get; set; }
    }
}
