using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;
namespace TeachingMaterial.Models
{
    public enum Gender
    {
        男, 女
    }
    
    // 可以通过将更多属性添加到 ApplicationUser 类来为用户添加配置文件数据，请访问 https://go.microsoft.com/fwlink/?LinkID=317594 了解详细信息。
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }

        [Required]
        [StringLength(20, ErrorMessage = "{0}少于{2}个字符", MinimumLength = 2)]
        [Display(Name = "姓名")]
        public string RealName { get; set; }
        
        [Display(Name ="性别")]
        [Required]
        public Gender Gender { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}",ApplyFormatInEditMode =true)]
        [Display(Name ="出生日期")]
        public DateTime  Birthday { get; set; }

        public int? DepartmentID { get; set; }

        
        public virtual Department Department { get; set; }

        public virtual ICollection<Department> Departments { get; set; }


    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName) : base(roleName) { }

        [Display(Name = "角色名字")]
        [StringLength(10, ErrorMessage = "{0}不能超过{1}个字符")]
        [Required(AllowEmptyStrings = false)]
        public string RoleRealName { get; set; }

        [Display(Name = "角色描述")]
        [StringLength(50, ErrorMessage = "{0}不能超过{1}个字符")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

    }
   
}