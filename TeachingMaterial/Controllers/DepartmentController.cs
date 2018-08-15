using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity; //判定 IsInrole();

namespace TeachingMaterial.Controllers
{
    [Authorize(Roles = "SuperAdministrator")]
    public class DepartmentController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        // GET: Department
        public ActionResult Index()
        {
            var _departments = db.Departments.Include(d => d.Administrators).ToList();

            return View(_departments);
        }

        // GET: Department/Details/5
        /* 由于使用了模态框，不再需要 details 方法和details 视图了，就删除了。
       public ActionResult Details(int? id)
       {
           if (id == null)
           {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           }
           Department department = db.Departments.Find(id);
           if (department == null)
           {
               return HttpNotFound();
           }
           return View(department);
       }
       */
        // GET: Department/Create
        /* 由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
       public ActionResult Create()
       {
           return View();
       }
       */

       // POST: Department/Create
       // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
       // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
       [HttpPost]
       [ValidateAntiForgeryToken]
       public ActionResult Create([Bind(Include = "DepartmentName,DepartmentLocation")] Department department)
       {
           if (ModelState.IsValid)
           {
               db.Departments.Add(department);
               db.SaveChanges();
               return RedirectToAction("Index");
           }

            return RedirectToAction("Index");
       }

       // GET: Department/Edit/5
       /*由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
       public ActionResult Edit(int? id)
       {
           if (id == null)
           {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           }
           Department department = db.Departments.Find(id);
           if (department == null)
           {
               return HttpNotFound();
           }
           return View(department);
       }
       */


       // POST: Department/Edit/5
       // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
       // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
       [HttpPost]
       [ValidateAntiForgeryToken]
       public ActionResult Edit([Bind(Include = "DepartmentID,DepartmentName,DepartmentLocation")] Department department)
       {
           if (ModelState.IsValid)
           {
               db.Entry(department).State = EntityState.Modified;
               db.SaveChanges();

                if (Request.UrlReferrer != null)
                {
                    var returnUrl = Request.UrlReferrer.ToString();
                    return new RedirectResult(returnUrl);    //由于使用的是表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态，筛选、排序、分页都保持不变。
                }

                return RedirectToAction("Index");
           }
           return View(department);
       }

       // GET: Department/Delete/5
       /*由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
       public ActionResult Delete(int? id)
       {
           if (id == null)
           {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           }
           Department department = db.Departments.Find(id);
           if (department == null)
           {
               return HttpNotFound();
           }
           return View(department);
       }
       */


       // POST: Department/Delete/5
       [HttpPost, ActionName("Delete")]
       [ValidateAntiForgeryToken]
       public ActionResult DeleteConfirmed(int id)
       {
           Department department = db.Departments.Find(id);
           db.Departments.Remove(department); //EF默认级联删除多对多关系和非空外键。如果将部门删除了，那么部门管理员 DepartmentAdministrator关联表对应部门的记录也删除了。
           db.SaveChanges();

            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态，筛选、排序、分页都保持不变。
            }

            return RedirectToAction("Index");
       }

        [HttpGet]
        public ActionResult GetEmptyDepartment()
        {
            return PartialView("_AddDepartment.Modal.Preview");
        }

        [HttpPost]
        public ActionResult GetEditDepartment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return PartialView("_EditDepartment.Modal.Preview", department);

        }

        [HttpPost]
        public ActionResult GetDeleteDepartment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            return PartialView("_DeleteDepartment.Modal.Preview", department);

        }

        //返回部门名称，实现二级关联菜单
        [HttpGet]
        public JsonResult GetDepartmentList()
        {
            var departmentList = from d in db.Departments
                                 orderby d.DepartmentID
                                 select new DepartmentViewModel
                                 {
                                     DepartmentID = d.DepartmentID,
                                     DepartmentName = d.DepartmentName
                                 };
            return Json(departmentList.ToList(), JsonRequestBehavior.AllowGet);
        }


        //返回模态框中供选择的的征订人员
        [HttpGet]
        public JsonResult GetAdministratorList(int? departmentID,int manageDepartmentID)
        {
            var _administratorList = new List<AdministratorViewModel>();
            var manageAdministrators = db.Departments.Find(manageDepartmentID).Administrators;
            

            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            foreach (var user in userManager.Users.OrderBy(u =>u.Id).ToList())
            {
                var _administrator = user;
                if (userManager.IsInRole(user.Id, "DepartmentAdministrator"))
                {
                    if (!manageAdministrators.Contains(_administrator)) 

                    _administratorList.Add(new AdministratorViewModel { Id = _administrator.Id, RealName = _administrator.RealName, DepartmentID = _administrator.DepartmentID });
                }
            }            
            IEnumerable<AdministratorViewModel> returnAdministrators = _administratorList as IEnumerable<AdministratorViewModel>;
            if (departmentID !=null)
            {
                returnAdministrators = returnAdministrators.Where(e => e.DepartmentID == departmentID);
            }
            return Json(returnAdministrators.ToList(), JsonRequestBehavior.AllowGet);


        }


        //获取已存在的征订人员
        [HttpGet]
        public JsonResult GetExistingAdministratorList(int? departmentID, int manageDepartmentID)
        {
            var _administratorList = new List<AdministratorViewModel>();
            var manageAdministrators = db.Departments.Single(d =>d.DepartmentID ==manageDepartmentID).Administrators;


            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            foreach (var user in userManager.Users.OrderBy(u => u.Id).ToList())
            {
                var _administrator = user;
                if (userManager.IsInRole(user.Id, "DepartmentAdministrator"))
                {
                    if (manageAdministrators.Contains(_administrator))

                        _administratorList.Add(new AdministratorViewModel { Id = _administrator.Id, RealName = _administrator.RealName, DepartmentID = _administrator.DepartmentID });
                }
            }            
            IEnumerable<AdministratorViewModel> returnAdministrators = _administratorList as IEnumerable<AdministratorViewModel>;
            if (departmentID != null)
            {
                returnAdministrators = returnAdministrators.Where(e => e.DepartmentID == departmentID);
            }
            return Json(returnAdministrators.ToList(), JsonRequestBehavior.AllowGet);

        }

        //指定管理人员
        [HttpPost]
        public int SpecifyAministrators(int departmentID,params string[] chooseAdministratorsID)
        {
            int count = 0;
            var _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            var _department = db.Departments.Include(d =>d.Administrators).Single(d => d.DepartmentID == departmentID);

            if (chooseAdministratorsID == null) //如果没有选择一个征订人员，则赋值一个空的列表，清空多对多关系。
            {
                _department.Administrators = new List<ApplicationUser>(); //赋值一个空的列表，清空多对多关系。

                //解决并非所有代码路径都有返回值。
                count = db.SaveChanges();
                return count;
                

            }
            else
            {
                var selectedExpertsHS = new HashSet<string>(chooseAdministratorsID); //哈希集合 没有重复值，如果加进去，都会自动删除掉。
                var departmentAdministratorsHS = new HashSet<string>(_department.Administrators.Select(a => a.Id));

                //  var needToAdd = selectedExpertsHS.Except(departmentAdministratorsHS);
                // var needToDelete = departmentAdministratorsHS.Except(selectedExpertsHS);

                foreach (var user in _userManager.Users.ToList())
                {
                    if (selectedExpertsHS.Contains(user.Id))
                    {
                        if (!departmentAdministratorsHS.Contains(user.Id))
                        {
                            _department.Administrators.Add(user);
                        }
                    }

                    else
                    {
                        if (departmentAdministratorsHS.Contains(user.Id))
                        {
                            _department.Administrators.Remove(user);
                        }
                    }
                }


               count= db.SaveChanges();

                return count;
            }
            



        }

        protected override void Dispose(bool disposing)
       {
           if (disposing)
           {
               db.Dispose();
           }
           base.Dispose(disposing);
       }
   }
}
