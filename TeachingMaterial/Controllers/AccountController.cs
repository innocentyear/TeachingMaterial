using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TeachingMaterial.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.Owin;
using PagedList;
using System.Net;

namespace TeachingMaterial.Controllers
{
    [Authorize] 
    public class AccountController : Controller
    {
        private TeachingMaterialDbContext _dbContext;  //数据库上下文私有字段；

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager; //角色私有字段;

        public AccountController()
        {
            //_dbContext = HttpContext.GetOwinContext().Get<TeachingMaterialDbContext>(); //在默认的构造函数中 初始化数据库上下文；
           // _dbContext = context.Get<TeachingMaterialDbContext>();
        }

        public AccountController(ApplicationUserManager userManager,ApplicationRoleManager roleManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            RoleManager = roleManager; //添加角色管理器。
            SignInManager = signInManager;
           // _dbContext = HttpContext.GetOwinContext().Get<TeachingMaterialDbContext>(); //在默认的构造函数中 初始化数据库上下文；
           // _dbContext = context.Get<TeachingMaterialDbContext>();
        }

        //数据库连接字符串 属性
        public TeachingMaterialDbContext DBContext
        {
            get
            {
                return _dbContext ?? HttpContext.GetOwinContext().Get<TeachingMaterialDbContext>();
            }
            private set
            {
                _dbContext = value;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //角色管理器公有属性 liuyuanhao
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            set
            {
                _roleManager = value;
            }
        }


        //用户登录
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]  //允许匿名访问，asp.net MVc5 中，只要操作方法有[AllowAnonymous]的数据注解，不管控制器注解特性的授权限制，都可实现匿名访问此操作方法。 
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 这不会计入到为执行帐户锁定而统计的登录失败次数中
            // 若要在多次输入错误密码的情况下触发帐户锁定，请更改为 shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    //ModelState.AddModelError("", "无效的登录尝试。");
                    ModelState.AddModelError("", "用户名或密码不正确");
                    return View(model);
            }
        }

        /*
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // 要求用户已通过使用用户名/密码或外部登录名登录
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 以下代码可以防范双重身份验证代码遭到暴力破解攻击。
            // 如果用户输入错误代码的次数达到指定的次数，则会将
            // 该用户帐户锁定指定的时间。
            // 可以在 IdentityConfig 中配置帐户锁定设置
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "代码无效。");
                    return View(model);
            }
        }
        */

        [Authorize(Roles = "SuperAdministrator")]
        public async Task<ActionResult> Index(int? departmentID, string userRealName, int? pageSize, int page = 1)
        {
            ViewBag.DepartmentSelectList = new SelectList(DBContext.Departments, "DepartmentID", "DepartmentName", departmentID);

            var _users = UserManager.Users.Include(u => u.Department);
            if (departmentID != null)
            {
                _users = _users.Where(x => x.Department.DepartmentID == departmentID);
            }
            ViewBag.CurrentDepartmentID = departmentID;
            if (!string.IsNullOrEmpty(userRealName))
            {
                _users = _users.Where(u => u.RealName.Contains(userRealName.Trim()));
            }
            ViewBag.CurrentUserRealName = userRealName;
            //下面两行代码用来调试错误使用的。因为Department实体用ApplicationUser实体 没有自动创建外键关联。出现“未将对象的引用设置到对象的实例"的错误。 
            //已解决，使用Fulent API 的Foreign Key  显式确定外键 DepartmentID.
            // var varUser = UserManager.Users.FirstOrDefault();
            // var varDepartmentName = varUser.Department.DepartmentName;

            //这里要使用异步方法，不使用异步方法就报错。不知道什么原因。 //后来想到了，主要是IOrderable对象 需要转换成IQuerable对象。缺少一个隐式转换。查询语法不需要转换，方法语法需要转换。
            var usersViewModel = new List<EditUserViewModel>();
            foreach (var user in await _users.OrderBy(x => x.Id).ToListAsync()) //var user in await _users.OrderBy(u => u.UserName).ToListAsync()
            {
                var userRoles = await UserManager.GetRolesAsync(user.Id);//await UserManager.GetRolesAsync(user.Id);
                var _userViewModel = new EditUserViewModel()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Gender = user.Gender,
                    Birthday = user.Birthday,
                    DepartmentID = user.Department.DepartmentID,
                    DepartmentName = user.Department.DepartmentName,
                    // DepartmentName = DBContext.Departments.Single(d => d.DepartmentID == user.DepartmentID).DepartmentName, //如果确实出现“未将对象的引用设置到对象的实例"的错误，可直接使用数据库的Departments集。
                    RolesList = RoleManager.Roles.Where(x => userRoles.Contains(x.Name))
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id,
                        Text = x.RoleRealName
                    })


                };

                usersViewModel.Add(_userViewModel);

            }

            //分页
            int currentPageSize;
            if (pageSize == null)
            {
                if (Session["pageSize"] != null)
                {
                    currentPageSize = int.Parse(Session["pageSize"].ToString());
                }
                else
                {
                    currentPageSize = 10;
                }
            }
            else
            {
                currentPageSize = (int)pageSize;
            }
            Session["pageSize"] = currentPageSize;
            ViewBag.PageSize = currentPageSize;
            ViewBag.Page = page;

            return View(usersViewModel.ToPagedList(page, currentPageSize));
        }


        //
        // GET: /Account/Register
        [Authorize(Roles = "SuperAdministrator")]
        public async Task<ActionResult> Create()  //由原Register 方法修改成Create方法
        {
            ViewBag.RoleID = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "RoleRealName");
            ViewBag.DepartmentID = new SelectList(await DBContext.Departments.OrderBy(d => d.DepartmentID).ToListAsync(), "DepartmentID", "DepartmentName");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "SuperAdministrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model,params string[] selectedRoles)//可变数组参数。 模型绑定自动完成，给selectedRoles 可变参数数组传入参数
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email,RealName =model.RealName,Gender =model.Gender,Birthday =model.Birthday,DepartmentID =model.DepartmentID };
                var userResult = await UserManager.CreateAsync(user, model.Password);//添加用户//在数据库中创建了这个用户，那么就生成了UserID 了。
                //给用户添加角色
                if (userResult.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // 有关如何启用帐户确认和密码重置的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=320771
                    // 发送包含此链接的电子邮件
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "确认你的帐户", "请通过单击 <a href=\"" + callbackUrl + "\">這裏</a>来确认你的帐户");

                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleID = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "RoleRealName");
                            ViewBag.DepartmentID = new SelectList(await DBContext.Departments.OrderBy(d => d.DepartmentID).ToListAsync(), "DepartmentID", "DepartmentName");
                            return View(model);
                        }
                    }
                    //return RedirectToAction("Index", "Home");
                }

                else
                {
                    ModelState.AddModelError("", userResult.Errors.First());
                    ViewBag.RoleID = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "RoleRealName");
                    ViewBag.DepartmentID = new SelectList(await DBContext.Departments.OrderBy(d => d.DepartmentID).ToListAsync(), "DepartmentID", "DepartmentName");
                    return View(model);
                }

                return RedirectToAction("Index"); //如果用户添加成功，角色创建成功就返回Index.2、如果用户添加成功，但没有选中角色，也返回Index.
               // AddErrors(result);
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ViewBag.RoleID = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "RoleRealName");
            ViewBag.DepartmentID = new SelectList(await DBContext.Departments.OrderBy(d => d.DepartmentID).ToListAsync(), "DepartmentID", "DepartmentName");
            return View(model);
        }



        [Authorize(Roles = "SuperAdministrator")]
        //Get Account/EditUser 系统管理员修改其他人的信息
        public async Task<ActionResult> EditUser(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); //HttpStatusCode.BadRequest 在System.Net 命名空间下面。
            }

            var _user = await UserManager.FindByIdAsync(Id);
            if (_user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(_user.Id);

            var editUser = new EditUserViewModel
            {
                Id = _user.Id,
                UserName = _user.UserName,
                Email = _user.Email,
                RealName = _user.RealName,
                Gender = _user.Gender,
                Birthday = _user.Birthday,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()  //建立一个投影，如果角色中包含当前用户的角色，就选中此角色。
                {
                    Text = x.RoleRealName + "—" + x.Description,
                    Value = x.Name,
                    Selected = userRoles.Contains(x.Name)
                })

            };

            ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);

            return View(editUser);

        }
        /// <summary>
        /// 管理员编辑其他用户的信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="selectedRoles"></param>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdministrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser([Bind(Include = "Id,UserName,Email,RealName,Gender,Birthday,DepartmentID")]  EditUserViewModel user, params string[] selectedRoles)
        {

            if (ModelState.IsValid)
            {
                //var _user = await UserManager.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.Id);
                var _user = await UserManager.FindByIdAsync(user.Id);
                if (_user == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                _user.UserName = user.UserName;
                _user.Email = user.Email;
                _user.RealName = user.RealName;
                _user.Gender = user.Gender;
                _user.Birthday = user.Birthday;
                _user.DepartmentID = user.DepartmentID;


                var userRoles = await UserManager.GetRolesAsync(_user.Id);
                selectedRoles = selectedRoles ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles.Except(userRoles).ToArray<string>());
                if (!result.Succeeded)
                {
                    //ModelState.AddModelError("", result.Errors.First());
                    AddErrors(result);  //返回增加角色时的错误。相当于ModelState.AddModelError("",result.Errors.First) 但它能返回多个错误。
                    ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);

                    return View(user);
                }

                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRoles).ToArray<string>());
                if (!result.Succeeded)
                {
                    //ModelState.AddModelError("", result.Errors.First());
                    AddErrors(result);  //返回减少角色时的错误。相当于ModelState.AddModelError("",result.Errors.First) 但它能返回多个错误。
                    ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);
                    return View(user);
                }

                result = await UserManager.UpdateAsync(_user); //异步更新用户信息

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "绑定失败");
            ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", user.DepartmentID);
            return View(user);

        }



        //GET://Account/ChangeUserPassword   //系统管理员修改其他用户的密码
        [Authorize(Roles = "SuperAdministrator")]
        public async Task<ActionResult> ChangeUserPassword(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(Id);
            if (user == null)
            {
                return HttpNotFound();

            }

            ResetPasswordViewModel _resetPasswordViewModel = new ResetPasswordViewModel()
            {
                UserName = user.UserName
            };

            return View(_resetPasswordViewModel);
        }

        //Post:  /Account/ChangeUserPassword     //系统管理员修改其他用户的密码
        [Authorize(Roles = "SuperAdministrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeUserPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var _user = await UserManager.FindByNameAsync(resetPasswordViewModel.UserName);
                if (_user == null)
                {
                    return HttpNotFound();
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(_user.Id);

                var result = await UserManager.ResetPasswordAsync(_user.Id, code, resetPasswordViewModel.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                AddErrors(result);
                return View(resetPasswordViewModel);

            }

            ModelState.AddModelError("", "绑定失败");
            return View(resetPasswordViewModel);
        }



        //GET://  Admin/Account/Delete/5  系统管理员删除账户
        [Authorize(Roles = "SuperAdministrator")]
        public async Task<ActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var _user = await UserManager.FindByIdAsync(Id);
            if (_user == null)
            {
                return HttpNotFound();

            }

            return View(_user);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdministrator")]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirm(string Id)
        {
            var _user = await UserManager.FindByIdAsync(Id);
            if (_user == null)
            {
                return HttpNotFound();
            }

            var result = await UserManager.DeleteAsync(_user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Account");

            }

            AddErrors(result);
            return View();
        }



        //Get://Account/Edit  修改登录用户自己的信息
        public async Task<ActionResult> Edit()
        {
            var _user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (_user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var editUserSelf = new EditUserSelfViewModel
            {
                Email = _user.Email,
                RealName = _user.RealName,
                Gender = _user.Gender,
                Birthday = _user.Birthday,
            };
            ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);
            return View(editUserSelf);

        }

        //Post: /Account/Edit 修改登录用户自己的信息
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,RealName,Gender,Birthday,DepartmentID")] EditUserSelfViewModel user)
        {
            var _user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (_user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                _user.Email = user.Email;
                _user.RealName = user.RealName;
                _user.Gender = user.Gender;
                _user.Birthday = user.Birthday;
                _user.DepartmentID = user.DepartmentID;

                var result = await UserManager.UpdateAsync(_user); //异步更新用户信息
                if (!result.Succeeded)
                {
                    AddErrors(result);
                    ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);
                    return View(user);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "绑定失败");
            ViewBag.DepartmentList = new SelectList(DBContext.Departments.OrderBy(d => d.DepartmentID), "DepartmentID", "DepartmentName", _user.DepartmentID);
            return View(user);
        }


        //查看登录用户自身的信息
        public async Task<ActionResult> Details()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);//await UserManager.GetRolesAsync(user.Id);
            var _userViewModel = new EditUserViewModel
            {
                //Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RealName = user.RealName,
                Gender = user.Gender,
                Birthday = user.Birthday,
                DepartmentName = user.Department.DepartmentName,
                RolesList = RoleManager.Roles.Where(x => userRoles.Contains(x.Name))
                .Select(x => new SelectListItem
                {
                    Value = x.Id,
                    Text = x.RoleRealName
                })

            };

            return View(_userViewModel);
        }



        /// <summary>
        /// GET:// Admin/Account/ChangePassword    //登录用户修改自己的密码
        /// </summary>
        /// <returns></returns>


        public async Task<ActionResult> ChangePassword()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View();
        }

        //POST:// Admin/Account/ChangePassword    //登录用户修改自己的密码
        // [Authorize(Roles = "SuperAdmin,Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            AddErrors(result);
            return View(model);
        }










        
        //
        // GET: /Account/ConfirmEmail
       
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        //[AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
       // [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // 请不要显示该用户不存在或者未经确认
                    return View("ForgotPasswordConfirmation");
                }

                // 有关如何启用帐户确认和密码重置的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=320771
                // 发送包含此链接的电子邮件
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "重置密码", "请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来重置你的密码");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
      //  [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        //[AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // 请不要显示该用户不存在
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
       // [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
       // [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 请求重定向到外部登录提供程序
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
       // [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
       // [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 生成令牌并发送该令牌
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        //[AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // 如果用户已具有登录名，则使用此外部登录提供程序将该用户登录
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // 如果用户没有帐户，则提示该用户创建帐户
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // 从外部登录提供程序获取有关用户的信息
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region 帮助程序
        // 用于在添加外部登录名时提供 XSRF 保护
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}