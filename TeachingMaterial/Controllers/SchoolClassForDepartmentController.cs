using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using TeachingMaterial.Filters;
using PagedList;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity.SqlServer;
namespace TeachingMaterial.Controllers
{
    //用于系部征订人员添加班级的控制器

    [Authorize(Roles = "DepartmentSpecialManager")]
    public class SchoolClassForDepartmentController : Controller
    {
        private TeachingMaterialDbContext _dbContext;  //数据库上下文私有字段；
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager; //角色私有字段;

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




        [AutoCaculateStudentNumberFilter]
        // GET: SchoolClass
        public ActionResult Index(string sortOrder, int? gradeID, int? departmentID, int? majorID, int? pageSize, int page = 1)
        {
            //排序参数
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SchoolClassIDSortParam = string.IsNullOrEmpty(sortOrder) ? "schoolClassID_desc" : "";
            ViewBag.GradeIDSortParam = sortOrder == "gradeID" ? "gradeID_desc" : "gradeID";
            ViewBag.DeparmentIDSortParam = sortOrder == "deparmentID" ? "deparmentID_desc" : "deparmentID";
            ViewBag.MajorIDSortParam = sortOrder == "majorID" ? "majorID_desc" : "majorID";

            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。

            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
             
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。
            //返回存在的年级列表
            List<Grade> grades = new List<Grade>();
            var gradeListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Grade)
                                // where currentDepartments.Contains(sc.GradeMajor.Major.Department)
                                 select sc.GradeMajor.Grade;
            
            grades.AddRange(gradeListQuery.Distinct().OrderBy(x => x.GradeID));
            ViewBag.gradeSelectList = new SelectList(grades, "GradeID", "GradeName", gradeID);

            //返回存在的部门列表
            List<Department> departments = new List<Department>();
            var departmentListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Major.Department)
                                      //where currentDepartments.Contains(sc.GradeMajor.Major.Department)
                                      select sc.GradeMajor.Major.Department;
            var queryDepartments = departmentListQuery.Distinct().OrderBy(i => i.DepartmentID).ToList();
            foreach (var departmentItem in queryDepartments)
            {
                if (currentDepartments.Contains(departmentItem))
                {
                    departments.Add(departmentItem);
                }
            }
           
            
            ViewBag.departmentSelectList = new SelectList(departments.Distinct().OrderBy(i => i.DepartmentID).ToList(), "DepartmentID", "DepartmentName", departmentID);

            //返回专业列表
            List<Major> majors = new List<Major>();
            var majorListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Major)
                                 //where currentDepartments.Contains(sc.GradeMajor.Major.Department)
                                 select sc.GradeMajor.Major;
            var queryMajors = majorListQuery.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x =>x.MajorID).ToList();
            foreach(var majorItem in queryMajors)
            {
                if(currentDepartments.Contains(majorItem.Department))
                {
                    majors.Add(majorItem);
                }
                
            }
          
            ViewBag.majorSelectList = new SelectList(majors.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList(), "MajorID", "MajorName", majorID);


            var _AllSchoolClass = db.SchoolClasses.Include(x => x.GradeMajor.Major.Department).ToList();
            var _schoolClass = _AllSchoolClass.Where(x => currentDepartments.Contains(x.GradeMajor.Major.Department));

            //过滤操作
            if (gradeID != null)
            {
                _schoolClass = _schoolClass.Where(x => x.GradeMajor.GradeID == gradeID);
            }
            ViewBag.GradeID = gradeID;

            if (departmentID != null)
            {
                _schoolClass = _schoolClass.Where(x => x.GradeMajor.Major.DepartmentID == departmentID);
            }
            ViewBag.DepartmentID = departmentID;

            if (majorID != null)
            {
                _schoolClass = _schoolClass.Where(x => x.GradeMajor.MajorID == majorID);
            }
            ViewBag.MajorID = majorID;


            //排序
            switch (sortOrder)
            {
                case "schoolClassID_desc":
                    _schoolClass = _schoolClass.OrderByDescending(x => x.SchoolClassID);
                    break;
                case "gradeID":
                    _schoolClass = _schoolClass.OrderBy(x => x.GradeMajor.GradeID);
                    break;
                case "gradeID_desc":
                    _schoolClass = _schoolClass.OrderByDescending(x => x.GradeMajor.GradeID);
                    break;
                case "deparmentID":
                    _schoolClass = _schoolClass.OrderBy(x => x.GradeMajor.Major.DepartmentID);
                    break;
                case "deparmentID_desc":
                    _schoolClass = _schoolClass.OrderByDescending(x => x.GradeMajor.Major.DepartmentID);
                    break;
                case "majorID":
                    _schoolClass = _schoolClass.OrderBy(x => x.GradeMajor.MajorID);
                    break;
                case "majorID_desc":
                    _schoolClass = _schoolClass.OrderByDescending(x => x.GradeMajor.MajorID);
                    break;
                default:
                    _schoolClass = _schoolClass.OrderBy(x => x.SchoolClassID);
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

            return View(_schoolClass.ToPagedList(page, currentPageSize));
        }

        /*
        // GET: SchoolClass/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolClass schoolClass = db.SchoolClasses.Find(id);
            if (schoolClass == null)
            {
                return HttpNotFound();
            }
            return View(schoolClass);
        }
        */

        /*
        // GET: SchoolClass/Create
        public ActionResult Create()
        {
            ViewBag.GradeMajorID = new SelectList(db.GradeMajors, "GradeMajorID", "GradeMajorID");
            return View();
        }
        */


        // POST: SchoolClass/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [AutoCaculateStudentNumberFilter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int GradeID, int MajorID, string[] SchoolClassName, int[] StudentNumber)
        {

            if (ModelState.IsValid)
            {
                int classNameCount = SchoolClassName.Count();
                int classStudentNumberCount = StudentNumber.Count();

                var validateLenth = SchoolClassName.Count(x => !string.IsNullOrEmpty(x));
                var _gradMajor = db.GradeMajors.SingleOrDefault(x => x.GradeID == GradeID && x.MajorID == MajorID);
                if (_gradMajor == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    for (int i = 0; i < validateLenth; i++)
                    {
                        var schoolClassUpdate = new SchoolClass();

                        schoolClassUpdate.GradeMajorID = _gradMajor.GradeMajorID;
                        schoolClassUpdate.SchoolClassName = SchoolClassName[i];
                        schoolClassUpdate.StudentNumber = StudentNumber[i];

                        db.SchoolClasses.Add(schoolClassUpdate);
                        db.SaveChanges();

                    }
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        /*
        // GET: SchoolClass/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolClass schoolClass = db.SchoolClasses.Find(id);
            if (schoolClass == null)
            {
                return HttpNotFound();
            }
            ViewBag.GradeMajorID = new SelectList(db.GradeMajors, "GradeMajorID", "GradeMajorID", schoolClass.GradeMajorID);
            return View(schoolClass);
        }
        */

        // POST: SchoolClass/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [AutoCaculateStudentNumberFilter]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SchoolClassID,SchoolClassName,StudentNumber")] SchoolClass schoolClass, int GradeID, int MajorID)
        {
            if (ModelState.IsValid)
            {
                var gradeMajor = db.GradeMajors.SingleOrDefault(x => x.GradeID == GradeID && x.MajorID == MajorID);
                if (gradeMajor == null)
                {
                    return HttpNotFound();
                }
                schoolClass.GradeMajorID = gradeMajor.GradeMajorID;

                db.Entry(schoolClass).State = EntityState.Modified;
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

        /*
        // GET: SchoolClass/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SchoolClass schoolClass = db.SchoolClasses.Find(id);
            if (schoolClass == null)
            {
                return HttpNotFound();
            }
            return View(schoolClass);
        }
        */


        // POST: SchoolClass/Delete/5
        [AutoCaculateStudentNumberFilter]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SchoolClass schoolClass = db.SchoolClasses.Find(id);
            db.SchoolClasses.Remove(schoolClass);
            db.SaveChanges();
            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public PartialViewResult GetASchoolClass(int? id)
        {
            SchoolClassViewModel _schoolClassViewModel = new SchoolClassViewModel();


            //获取当前用户所负责征订的部门；2017.12.30
            //var currentDepartments = UserManager.Users.Include(x => x.Departments).SingleOrDefault(x => x.Id == User.Identity.GetUserId()).Departments.ToList();

            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。

            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。

            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。


            if (id == null)
            {
                var _exitGradeBlank = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                                      where grad.GradeMajorIsValidate == true
                                      select grad.Grade;

                _schoolClassViewModel.GradesList = new SelectList(_exitGradeBlank.Distinct().OrderBy(g => g.GradeID), "GradeID", "GradeName");


                var _exitDepartmentBlank = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                           select grad.Major.Department;
                var queryDepartmentsBlank = _exitDepartmentBlank.Distinct().OrderBy(x => x.DepartmentID).ToList();

                var returnDepartmentsBlank = new List<Department>();
                foreach (var departmentItem in queryDepartmentsBlank)
                {
                    if (currentDepartments.Contains(departmentItem))
                    {
                        returnDepartmentsBlank.Add(departmentItem);
                    }
                }

                _schoolClassViewModel.DepartmentList = new SelectList(returnDepartmentsBlank.Distinct().OrderBy(x => x.DepartmentID).ToList(), "DepartmentID", "DepartmentName");



                var _exitMajorBlank = from maj in db.GradeMajors.Include(x => x.Major)
                                      select maj.Major;

                var queryMajorBlank =_exitMajorBlank.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
                var returnMajorsBlank = new List<Major>();
                foreach (var majorItem in queryMajorBlank)
                {
                    if (currentDepartments.Contains(majorItem.Department))
                    {
                        returnMajorsBlank.Add(majorItem);
                    }
                }

                _schoolClassViewModel.MajorsList = new SelectList(returnMajorsBlank.Distinct().OrderBy(x => x.MajorID).ToList(), "MajorID", "MajorName");
                _schoolClassViewModel.StudentNumber = 0;

                return PartialView("_Modal.FormContent", _schoolClassViewModel);
            }

            SchoolClass _schoolClass = db.SchoolClasses.Find(id);
            _schoolClassViewModel.SchoolClassID = _schoolClass.SchoolClassID;


            var _exitGrade = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                             where grad.GradeMajorIsValidate == true
                             select grad.Grade;
            _schoolClassViewModel.GradesList = new SelectList(_exitGrade.Distinct().OrderBy(g => g.GradeID).ToList(), "GradeID", "GradeName", _schoolClass.GradeMajor.GradeID);

            var _exitDepartment = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                  select grad.Major.Department;
            var queryDepartments = _exitDepartment.Distinct().OrderBy(x => x.DepartmentID).ToList();

            var returnDepartments = new List<Department>();
            foreach (var departmentItem in queryDepartments)
            {
                if (currentDepartments.Contains(departmentItem))
                {
                    returnDepartments.Add(departmentItem);
                }
            }
            _schoolClassViewModel.DepartmentList = new SelectList(returnDepartments.Distinct().OrderBy(x => x.DepartmentID).ToList(), "DepartmentID", "DepartmentName", _schoolClass.GradeMajor.Major.DepartmentID);


            var _exitMajor = from maj in db.GradeMajors.Include(x => x.Major)
                             where (maj.GradeMajorIsValidate == true)
                             select maj.Major;
            var queryMajor = _exitMajor.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            var returnMajors = new List<Major>();
            foreach (var majorItem in queryMajor)
            {
                if (currentDepartments.Contains(majorItem.Department))
                {
                    returnMajors.Add(majorItem);
                }
            }

            _schoolClassViewModel.MajorsList = new SelectList(returnMajors.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList(), "MajorID", "MajorName", _schoolClass.GradeMajor.MajorID);
            _schoolClassViewModel.SchoolClassName = _schoolClass.SchoolClassName;
            _schoolClassViewModel.StudentNumber = _schoolClass.StudentNumber;

            return PartialView("_Modal.FormContent", _schoolClassViewModel);
        }


        //用于Index视图
        [HttpGet]
        public JsonResult GetMajorList(int? departmentID)
        {
             //获取当前用户所负责征订的部门；2017.12.30
            //var currentDepartments = UserManager.Users.Include(x => x.Departments).SingleOrDefault(x => x.Id == User.Identity.GetUserId()).Departments.ToList();

            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            var _majorList = from m in db.SchoolClasses.Include(x => x.GradeMajor.Major)
                             select m.GradeMajor.Major;


            _majorList = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x =>x.MajorID);
            if (departmentID != null)
            {
                _majorList = _majorList.Where(x => x.DepartmentID == departmentID);
            }

            var queryMajor = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            var returnMajor = new List<Major>();
            foreach (var majorItem in queryMajor)
            {
                if (currentDepartments.Contains(majorItem.Department))
                {
                    returnMajor.Add(majorItem);
                }
            }
                var _majorsJson = from m in returnMajor
                          select new { majorID = m.MajorID, majorName = m.MajorName };
            return Json(_majorsJson.ToList(), JsonRequestBehavior.AllowGet);

        }

        //模态框取得的专业列表，一旦模态框中部门变动，返回专业
        [HttpGet]
        public JsonResult GetModalMajorList(int gradeID, int? departmentID)
        {
            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            var _majorList = from m in db.GradeMajors.Include(x => x.Major)
                             where m.GradeID == gradeID && m.GradeMajorIsValidate == true
                             select m.Major;

            _majorList = _majorList.Distinct().OrderBy(x => x.MajorID);

            if (departmentID != null)
            {
                _majorList = _majorList.Where(x => x.DepartmentID == departmentID);
            }

            var queryMajor = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            var returnMajor = new List<Major>();
            foreach (var majorItem in queryMajor)
            {
                if (currentDepartments.Contains(majorItem.Department))
                {
                    returnMajor.Add(majorItem);
                }
            }

            var _majorsJson = from m in returnMajor
                          select new { majorID = m.MajorID, majorName = m.MajorName };
            return Json(_majorsJson.ToList(), JsonRequestBehavior.AllowGet);

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
