using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using Microsoft.AspNet.Identity;
using PagedList;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity.SqlServer;
namespace TeachingMaterial.Controllers
{
    [Authorize(Roles = "DepartmentSpecialManager")]
    public class SemesterCourseForDepartmentController : Controller
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



        // GET: SemesterCourse
        public ActionResult Index(string sortOrder, int? semesterID, int? gradeID, int? departmentID, int? majorID, CourseType? courseType, string courseName, int? pageSize, int page = 1)
        {
            //排序参数
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SemesterIDSortParam = string.IsNullOrEmpty(sortOrder) ? "Semester_desc" : "";
            ViewBag.GradeIDSortParam = sortOrder == "gradeID" ? "gradeID_desc" : "gradeID";
            ViewBag.DeparmentIDSortParam = sortOrder == "deparmentID" ? "deparmentID_desc" : "deparmentID";
            ViewBag.MajorIDSortParam = sortOrder == "majorID" ? "majorID_desc" : "majorID";
            ViewBag.CourseTypeSortParam = sortOrder == "courseType" ? "courseType_desc" : "courseType";
            ViewBag.CourseNumberSortParam = sortOrder == "courseNumber" ? "courseNumber_desc" : "courseNumber";
            ViewBag.CourseNameSortParam = sortOrder == "courseName" ? "courseName_desc" : "courseName";
            ViewBag.ModifyUserSortParam = sortOrder == "modifyUser" ? "modifyUser_desc" : "modifyUser";
            ViewBag.ModifyTimeSortParam = sortOrder == "modifyTime" ? "modifyTime_desc" : "modifyTime";

            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            //返回存的学期列表
            List<Semester> semesters = new List<Semester>();
            var semesterListQuery = from sc in db.SemesterCourses.Include(x => x.Semester)
                                    select sc.Semester;
            semesters.AddRange(semesterListQuery.Distinct().OrderBy(x => x.SemesterID));
            ViewBag.semesterSelectList = new SelectList(semesters, "SemesterID", "SemesterName", semesterID);

            //返回存在的年级列表
            List<Grade> grades = new List<Grade>();
            var gradeListQuery = from sc in db.SemesterCourses.Include(x => x.GradeMajor.Grade)
                                 select sc.GradeMajor.Grade;
            grades.AddRange(gradeListQuery.Distinct().OrderBy(x => x.GradeID));
            ViewBag.gradeSelectList = new SelectList(grades, "GradeID", "GradeName", gradeID);

            //返回存在的部门列表
            List<Department> departments = new List<Department>();
            var departmentListQuery = from sc in db.SemesterCourses.Include(x => x.GradeMajor.Major.Department)
                                      select sc.GradeMajor.Major.Department;
            departments.AddRange(departmentListQuery.Distinct().OrderBy(i => i.DepartmentID).ToList());

            ViewBag.departmentSelectList = new SelectList(departments.Where(x => currentDepartments.Contains(x)), "DepartmentID", "DepartmentName", departmentID);

            //返回专业列表
            List<Major> majors = new List<Major>();
            var majorListQuery = from sc in db.SemesterCourses.Include(x => x.GradeMajor.Major)
                                 select sc.GradeMajor.Major;
            majors.AddRange(majorListQuery.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID));


            ViewBag.majorSelectList = new SelectList(majors.Where(x => currentDepartments.Contains(x.Department)), "MajorID", "MajorName", majorID);

            //返回课程类型的枚举的选项。
            var courseTypeItems = from CourseType e in Enum.GetValues(typeof(CourseType))
                                  select new SelectListItem { Value = Convert.ToInt32(e).ToString(), Text = e.ToString() };
            ViewBag.courseTypeList = new SelectList(courseTypeItems, "Value", "Text", courseType);

            var _AllsemesterCourse = db.SemesterCourses.Include(x => x.GradeMajor.Major.Department).ToList();
            var _semesterCourse = _AllsemesterCourse.Where(x => currentDepartments.Contains(x.GradeMajor.Major.Department));

            //过滤操作
            if (semesterID != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.Semester.SemesterID == semesterID);
            }
            ViewBag.SemesterID = semesterID;

            if (gradeID != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.GradeMajor.GradeID == gradeID);
            }
            ViewBag.GradeID = gradeID;

            if (departmentID != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.GradeMajor.Major.DepartmentID == departmentID);
            }
            ViewBag.DepartmentID = departmentID;

            if (majorID != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.GradeMajor.MajorID == majorID);
            }
            ViewBag.MajorID = majorID;
            if (courseType != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.CourseType == courseType);
            }
            ViewBag.CourseType = courseType;
            if (!string.IsNullOrEmpty(courseName))
            {
                _semesterCourse = _semesterCourse.Where(x => x.SemesterCourseName.Contains(courseName));
            }
            ViewBag.CourseName = courseName;

            //排序
            switch (sortOrder)
            {
                case "Semester_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SemesterID);
                    break;
                case "gradeID":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.GradeMajor.GradeID);
                    break;
                case "gradeID_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.GradeMajor.GradeID);
                    break;
                case "deparmentID":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.GradeMajor.Major.DepartmentID);
                    break;
                case "deparmentID_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.GradeMajor.Major.DepartmentID);
                    break;
                case "majorID":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.GradeMajor.MajorID);
                    break;
                case "majorID_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.GradeMajor.MajorID);
                    break;
                case "courseType":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.CourseType);
                    break;
                case "courseType_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.CourseType);
                    break;
                case "courseNumber":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SemesterCourseNumber);
                    break;
                case "courseNumber_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SemesterCourseNumber);
                    break;
                case "courseName":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SemesterCourseName);
                    break;
                case "courseName_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SemesterCourseName);
                    break;

                case "ModifyUser":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.AuthorName);
                    break;
                case "ModifyUser_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.AuthorName);
                    break;
                case "modifyTime":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.PostTime);
                    break;
                case "modifyTime_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.PostTime);
                    break;
                default:
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SemesterID);
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
                    currentPageSize = 15;
                }
            }
            else
            {
                currentPageSize = (int)pageSize;
            }
            Session["pageSize"] = currentPageSize;
            ViewBag.PageSize = currentPageSize;
            ViewBag.Page = page;

            return View(_semesterCourse.ToPagedList(page, currentPageSize));
        }
        /*

        // GET: SemesterCourse/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SemesterCourse semesterCourse = db.SemesterCourses.Find(id);
            if (semesterCourse == null)
            {
                return HttpNotFound();
            }
            return View(semesterCourse);
        }
        */

        /*
        // GET: SemesterCourse/Create
        public ActionResult Create()
        {
            ViewBag.GradeMajorID = new SelectList(db.GradeMajors, "GradeMajorID", "GradeMajorID");
            ViewBag.SemesterID = new SelectList(db.Semesters, "SemesterID", "SemesterName");
            return View();
        }
        */

        // POST: SemesterCourse/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int SemesterID, int GradeID, int MajorID, string[] SemesterCourseNumber, string[] SemesterCourseName, CourseType[] CourseType)
        {

            if (ModelState.IsValid)
            {
                int semesterCourseNumberCount = SemesterCourseNumber.Count(x => !string.IsNullOrEmpty(x));
                int semesterCourseNameCount = SemesterCourseName.Count(x => !string.IsNullOrEmpty(x));
                int courseTypeCount = CourseType.Count();

                var validateLenth = Math.Min(semesterCourseNumberCount, semesterCourseNameCount);
                var _gradMajor = db.GradeMajors.SingleOrDefault(x => x.GradeID == GradeID && x.MajorID == MajorID);
                if (_gradMajor == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    for (int i = 0; i < validateLenth; i++)
                    {
                        var semesterCourseUpdate = new SemesterCourse();
                        semesterCourseUpdate.SemesterID = SemesterID;
                        semesterCourseUpdate.GradeMajorID = _gradMajor.GradeMajorID;
                        semesterCourseUpdate.SemesterCourseNumber = SemesterCourseNumber[i];
                        semesterCourseUpdate.SemesterCourseName = SemesterCourseName[i];
                        semesterCourseUpdate.CourseType = CourseType[i];
                        semesterCourseUpdate.AuthorName = User.Identity.GetUserName();
                        semesterCourseUpdate.PostTime = DateTime.Now;
                        db.SemesterCourses.Add(semesterCourseUpdate);
                        db.SaveChanges();

                    }
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        /*
        // GET: SemesterCourse/Edit/5
        public ActionResult Edit([Bind(Include = "SemesterCourseID,SemesterCourseNumber,SemesterCourseName,CourseType")] SemesterCourse semesterCourse,int SemesterID, int GradeID, int MajorID)
        {
            if (ModelState.IsValid)
            {
                var gradeMajor = db.GradeMajors.SingleOrDefault(x => x.GradeID == GradeID && x.MajorID == MajorID);
                if (gradeMajor == null )
                {
                    return HttpNotFound();
                }
                semesterCourse.GradeMajorID = gradeMajor.GradeMajorID;
                semesterCourse.AuthorName = User.Identity.GetUserName();
                semesterCourse.PostTime = DateTime.Now;
                db.Entry(semesterCourse).State = EntityState.Modified;
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
        */

        // POST: SemesterCourse/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SemesterCourseID,SemesterID,SemesterCourseNumber,SemesterCourseName,CourseType")] SemesterCourse semesterCourse, int GradeID, int MajorID)
        {
            if (ModelState.IsValid)
            {
                var gradeMajor = db.GradeMajors.SingleOrDefault(x => x.GradeID == GradeID && x.MajorID == MajorID);
                if (gradeMajor == null || gradeMajor.GradeMajorIsValidate == false)
                {
                    return HttpNotFound();
                }
                semesterCourse.GradeMajorID = gradeMajor.GradeMajorID;
                semesterCourse.AuthorName = User.Identity.GetUserName();
                semesterCourse.PostTime = DateTime.Now;
                db.Entry(semesterCourse).State = EntityState.Modified;
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
        // GET: SemesterCourse/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SemesterCourse semesterCourse = db.SemesterCourses.Find(id);
            if (semesterCourse == null)
            {
                return HttpNotFound();
            }
            return View(semesterCourse);
        }

      */
        // POST: SemesterCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SemesterCourse semesterCourse = db.SemesterCourses.Find(id);
            db.SemesterCourses.Remove(semesterCourse);
            db.SaveChanges();
            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public PartialViewResult GetASemesterCourse(int? id)
        {
            SemesterCourseViewModel _semesterCourseViewModel = new SemesterCourseViewModel();
            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            if (id == null)
            {
                var _exitSemesterBlank = from sem in db.Semesters //来自学期表
                                         where sem.IsCurrentSemester == true
                                         select sem;
                _semesterCourseViewModel.SemesterList = new SelectList(_exitSemesterBlank.Distinct().OrderBy(x => x.SemesterID), "SemesterID", "SemesterName");

                var _exitGradeBlank = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                                      where grad.GradeMajorIsValidate == true
                                      select grad.Grade;
                _semesterCourseViewModel.GradesList = new SelectList(_exitGradeBlank.Distinct().OrderBy(g => g.GradeID), "GradeID", "GradeName");


                var _exitDepartmentBlank = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                           select grad.Major.Department;
                var _returnDepartmentBlank = _exitDepartmentBlank.Distinct().OrderBy(x => x.DepartmentID).ToList();
                _semesterCourseViewModel.DepartmentList = new SelectList(_returnDepartmentBlank.Where(x =>currentDepartments.Contains(x)), "DepartmentID", "DepartmentName");

                var _exitMajorBlank = from maj in db.GradeMajors.Include(x => x.Major)
                                      select maj.Major;
                var _returnMajorBlank= _exitMajorBlank.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
                _semesterCourseViewModel.MajorsList = new SelectList(_returnMajorBlank.Where(x =>currentDepartments.Contains(x.Department)), "MajorID", "MajorName");


                return PartialView("_Modal.FormContent", _semesterCourseViewModel);
            }

            SemesterCourse _semesterCourse = db.SemesterCourses.Find(id);
            _semesterCourseViewModel.SemesterCourseID = _semesterCourse.SemesterCourseID;

            var _exitSemester = from sem in db.Semesters //来自学期表
                                where sem.IsCurrentSemester == true
                                select sem;
            _semesterCourseViewModel.SemesterList = new SelectList(_exitSemester.Distinct().OrderBy(x => x.SemesterID), "SemesterID", "SemesterName", _semesterCourse.SemesterID);

            var _exitGrade = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                             where grad.GradeMajorIsValidate == true
                             select grad.Grade;
            _semesterCourseViewModel.GradesList = new SelectList(_exitGrade.Distinct().OrderBy(g => g.GradeID), "GradeID", "GradeName", _semesterCourse.GradeMajor.GradeID);

            var _exitDepartment = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                  select grad.Major.Department;
            var _returnDepartment = _exitDepartment.Distinct().OrderBy(x => x.DepartmentID).ToList();
            _semesterCourseViewModel.DepartmentList = new SelectList(_returnDepartment.Where(x => currentDepartments.Contains(x)), "DepartmentID", "DepartmentName", _semesterCourse.GradeMajor.Major.DepartmentID);
            
            var _exitMajor = from maj in db.GradeMajors.Include(x => x.Major)
                             where (maj.GradeMajorIsValidate == true)
                             select maj.Major;
            var _returnMajor = _exitMajor.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            _semesterCourseViewModel.MajorsList = new SelectList(_returnMajor.Where(x => currentDepartments.Contains(x.Department)), "MajorID", "MajorName", _semesterCourse.GradeMajor.MajorID);

            _semesterCourseViewModel.SemesterCourseNumber = _semesterCourse.SemesterCourseNumber;
            _semesterCourseViewModel.SemesterCourseName = _semesterCourse.SemesterCourseName;
            _semesterCourseViewModel.CourseType = _semesterCourse.CourseType;

            return PartialView("_Modal.FormContent", _semesterCourseViewModel);
        }


        //用于Index视图
        [HttpGet]
        public JsonResult GetMajorList(int? departmentID)
        {
            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            var _majorList = from m in db.SemesterCourses.Include(x => x.GradeMajor.Major)
                             select m.GradeMajor.Major;
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

            var returnMajors = _majorList.OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID).ToList();
            var _majorsJson = from m in returnMajors.Where(x => currentDepartments.Contains(x.Department))
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
