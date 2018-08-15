using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using PagedList;
using TeachingMaterial.Filters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
namespace TeachingMaterial.Controllers
{
    [Authorize]
    public class VerifyGradeMajorController : Controller
    {
        private TeachingMaterialDbContext _dbContext;  //数据库上下文私有字段；
        private ApplicationUserManager _userManager;


        //数据库连接字符串 属性
        public TeachingMaterialDbContext db
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


        [AutoCaculateStudentNumberFilter]
        // GET: GradeMajor
        public ActionResult Index(string sortOrder, int? departmentID, int? majorID, int? gradeID, string majorName, int? pageSize, int page = 1)
        {
            //排序
            ViewBag.CurrentSort = sortOrder;
            ViewBag.GradeMajorIDSortParam = string.IsNullOrEmpty(sortOrder) ? "gradeMajorID_desc" : "";
            ViewBag.GradeIDSortParam = sortOrder == "gradeID" ? "gradeID_desc" : "gradeID";
            ViewBag.MajorIDSortParam = sortOrder == "majorID" ? "majorID_desc" : "majorID";

            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。


            //返回存在的部门列表
            List<Department> departments = new List<Department>();
            var departmentListQuery = from gm in db.GradeMajors.Include(x => x.Major.Department)
                                      select gm.Major.Department;
            departments.AddRange(departmentListQuery.Distinct().OrderBy(i => i.DepartmentID));
            // ViewBag.departmentSelectList = new SelectList(departments, "DepartmentID", "DepartmentName", departmentID);
            ViewBag.departmentSelectList = new SelectList(departments.Where(x => currentDepartments.Contains(x)), "DepartmentID", "DepartmentName", departmentID);

            //返回专业列表 2018.1.8
            List<Major> majors = new List<Major>();
            var majorListQuery = from sc in db.GradeMajors.Include(x => x.Major.Department).Where(x =>x.GradeMajorIsValidate ==true)
                                 select sc.Major;
            majors.AddRange(majorListQuery.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID));

            ViewBag.majorSelectList = new SelectList(majors.Where(x => currentDepartments.Contains(x.Department)), "MajorID", "MajorName", majorID);


            //返回年级列表
            List<Grade> grades = new List<Grade>();
            var gradeListQuery = from gm in db.GradeMajors.Include(x => x.Grade)
                                 select gm.Grade;
            grades.AddRange(gradeListQuery.Distinct().OrderBy(x => x.GradeID));
            ViewBag.gradeSelectList = new SelectList(grades, "GradeID", "GradeName", gradeID);

           // var _gradeMajors = db.GradeMajors.Include(x => x.Major.Department).Include(x => x.Grade).Where(x => x.GradeMajorIsValidate); //.先前考虑的是 级设为有效，才能在这里显示；.Where(x => x.Grade.GradeIsValidate ==true);但作为管理员，应该什么消息都看到。
           
            var _AllGradeMajors = db.GradeMajors.Include(x => x.Major.Department).Where(x => x.GradeMajorIsValidate).ToList();
            var _gradeMajors = _AllGradeMajors.Where(x => currentDepartments.Contains(x.Major.Department));


            //过滤操作
            if (departmentID != null)
            {
                _gradeMajors = _gradeMajors.Where(x => x.Major.DepartmentID == departmentID);
            }
            ViewBag.departmentID = departmentID;

            if (majorID != null)
            {
                _gradeMajors = _gradeMajors.Where(x => x.MajorID == majorID);
            }
            ViewBag.MajorID = majorID;

            if (gradeID != null)
            {
                _gradeMajors = _gradeMajors.Where(x => x.GradeID == gradeID);
            }
            ViewBag.gradeID = gradeID;
            if (!string.IsNullOrEmpty(majorName))
            {
                _gradeMajors = _gradeMajors.Where(x => x.Major.MajorName.Contains(majorName));
            }
            ViewBag.majorName = majorName;
            //排序
            switch (sortOrder)
            {
                case "gradeMajorID_desc":
                    _gradeMajors = _gradeMajors.OrderByDescending(x => x.GradeMajorID);
                    break;
                case "gradeID":
                    _gradeMajors = _gradeMajors.OrderBy(x => x.GradeID);
                    break;
                case "gradeID_desc":
                    _gradeMajors = _gradeMajors.OrderByDescending(x => x.GradeID);
                    break;
                case "majorID":
                    _gradeMajors = _gradeMajors.OrderBy(x => x.MajorID);
                    break;
                case "majorID_desc":
                    _gradeMajors = _gradeMajors.OrderByDescending(x => x.MajorID);
                    break;
                case "isValidate":
                    _gradeMajors = _gradeMajors.OrderBy(x => x.GradeMajorIsValidate);
                    break;
                case "isValidate_desc":
                    _gradeMajors = _gradeMajors.OrderByDescending(x => x.GradeMajorIsValidate);
                    break;
                default:
                    _gradeMajors = _gradeMajors.OrderBy(x => x.GradeMajorID);
                    break;
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
            return View(_gradeMajors.ToPagedList(page, currentPageSize));
        }



        //用于Index视图
        [HttpGet]
        public JsonResult GetMajorList(int? departmentID)
        {
            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。
            /*
            var _majorList = from m in db.SemesterCourses.Include(x => x.GradeMajor.Major)
                             select m.GradeMajor.Major;
            */

            var _majorList = db.GradeMajors.Include(x => x.Major.Department).Where(x => x.GradeMajorIsValidate).Select(x => x.Major);
            _majorList = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID);
            if (departmentID != null)
            {
                _majorList = _majorList.Where(x => x.DepartmentID == departmentID);
            }

            var returnMajors = _majorList.OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            var _majorsJson = from m in returnMajors.Where(x => currentDepartments.Contains(x.Department))
                              select new { majorID = m.MajorID, majorName = m.MajorName };
            return Json(_majorsJson.ToList(), JsonRequestBehavior.AllowGet);

        }

        /*
        // GET: GradeMajor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeMajor gradeMajor = db.GradeMajors.Find(id);
            if (gradeMajor == null)
            {
                return HttpNotFound();
            }
            return View(gradeMajor);
        }
        */


        /*
        // GET: GradeMajor/Create
        public ActionResult Create()
        {
            ViewBag.GradeID = new SelectList(db.Grades, "GradeID", "GradeName");
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName");
            return View();
        }
        */


        /*
        // POST: GradeMajor/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GradeID,MajorID,GradeMajorIsValidate")] GradeMajor gradeMajor)  //因为创建视图返回回来的为空，所有不要绑定GradeMajorID
        {
            if (ModelState.IsValid)
            {
                db.GradeMajors.Add(gradeMajor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ViewBag.GradeID = new SelectList(db.Grades, "GradeID", "GradeName", gradeMajor.GradeID);
            //ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName", gradeMajor.MajorID);
            return RedirectToAction("Index");
        }

        */

        /*
        // GET: GradeMajor/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeMajor gradeMajor = db.GradeMajors.Find(id);
            if (gradeMajor == null)
            {
                return HttpNotFound();
            }
            ViewBag.GradeID = new SelectList(db.Grades, "GradeID", "GradeName", gradeMajor.GradeID);
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName", gradeMajor.MajorID);
            return View(gradeMajor);
        }
        */

        /*

        // POST: GradeMajor/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GradeMajorID,GradeID,MajorID,GradeMajorIsValidate")] GradeMajor gradeMajor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gradeMajor).State = EntityState.Modified;
                db.SaveChanges();
                if (Request.UrlReferrer != null)
                {
                    var returnUrl = Request.UrlReferrer.ToString();
                    return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        // ViewBag.GradeID = new SelectList(db.Grades, "GradeID", "GradeName", gradeMajor.GradeID);
        // ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName", gradeMajor.MajorID);
        // return View(gradeMajor);


        /*
        // GET: GradeMajor/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeMajor gradeMajor = db.GradeMajors.Find(id);
            if (gradeMajor == null)
            {
                return HttpNotFound();
            }
            return View(gradeMajor);
        }
        */

        /*

       // POST: GradeMajor/Delete/5
       [HttpPost, ActionName("Delete")]
       [ValidateAntiForgeryToken]
       public ActionResult DeleteConfirmed(int id)
       {
           GradeMajor gradeMajor = db.GradeMajors.Find(id);
           db.GradeMajors.Remove(gradeMajor);
           db.SaveChanges();
           if (Request.UrlReferrer != null)
           {
               var returnUrl = Request.UrlReferrer.ToString();
               return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
           }
           return RedirectToAction("Index");
       }

      */


        /*
         public ActionResult GetAGradeMajor(int? id)
         {
             GradeMajorViewModel _gradeMajorViewModel = new GradeMajorViewModel();

             if (id == null)
             {
                 _gradeMajorViewModel.GradeMajorIsValidate = true;
                 _gradeMajorViewModel.GradesList = new SelectList(db.Grades.Where(x => x.GradeIsValidate).OrderBy(g => g.GradeID), "GradeID", "GradeName");
                 _gradeMajorViewModel.MajorsList = new SelectList(db.Majors.OrderBy(m => m.MajorID), "MajorID", "MajorName");

                 return PartialView("_Modal.FormContent", _gradeMajorViewModel);
             }

             GradeMajor _gradeMajor = db.GradeMajors.Find(id);
             _gradeMajorViewModel.GradeMajorID = _gradeMajor.GradeMajorID;
             _gradeMajorViewModel.GradeMajorIsValidate = _gradeMajor.GradeMajorIsValidate;
             _gradeMajorViewModel.GradesList = new SelectList(db.Grades.Where(x => x.GradeIsValidate).OrderBy(g => g.GradeID), "GradeID", "GradeName", _gradeMajor.GradeID);
             _gradeMajorViewModel.MajorsList = new SelectList(db.Majors.OrderBy(g => g.MajorID), "MajorID", "MajorName", _gradeMajor.MajorID);
             return PartialView("_Modal.FormContent", _gradeMajorViewModel);
         }

     */

        /*
        //批量设置为有效或无效
        public int MakeMassValidate(bool expected = true, params int[] selectedIds) //必须为int型，负责find()会出现主键的类型不匹配
        {
            int number = 0;
            if (selectedIds != null)
            {
                foreach (var gradeMajorId in selectedIds)
                {
                    var gradeMajor = db.GradeMajors.Find(gradeMajorId);
                    if (gradeMajor != null)
                    {
                        if (expected)

                            gradeMajor.GradeMajorIsValidate = true;

                        else
                            gradeMajor.GradeMajorIsValidate = false;
                        number += db.SaveChanges();
                    }
                }
            }

            return number;

        }
        */

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
