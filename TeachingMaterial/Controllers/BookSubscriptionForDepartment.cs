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
    [Authorize(Roles = "DepartmentAdministrator, PublicCourseAdministrator, PoliticCourseAdministrator")]
    public class BookSubscriptionForDepartmentController : Controller
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
        public ActionResult Index(string sortOrder, int? semesterID, int? gradeID, int? departmentID, int? majorID, CourseType? courseType, string courseName, bool? isSubmmit, int? pageSize, int page = 1)
        {
            //排序参数
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SemesterIDSortParam = string.IsNullOrEmpty(sortOrder) ? "Semester_desc" : "";
            ViewBag.GradeIDSortParam = sortOrder == "gradeID" ? "gradeID_desc" : "gradeID";
            ViewBag.MajorIDSortParam = sortOrder == "majorID" ? "majorID_desc" : "majorID";
            ViewBag.CourseTypeSortParam = sortOrder == "courseType" ? "courseType_desc" : "courseType";
            ViewBag.CourseNumberSortParam = sortOrder == "courseNumber" ? "courseNumber_desc" : "courseNumber";
            ViewBag.CourseNameSortParam = sortOrder == "courseName" ? "courseName_desc" : "courseName";
            ViewBag.SubscriptionUserSortParam = sortOrder == "subscriptionUser" ? "subscriptionUser_desc" : "subscriptionUser";
            ViewBag.SubscriptionTimeSortParam = sortOrder == "subscriptionTime" ? "subscriptionTime_desc" : "subscriptionTime";
            ViewBag.IsSubmmitSortParm = sortOrder == "isSubmmit" ? "isSubmmit_desc" : "isSubmmit";


            //获取当前用户所负责征订的部门；2017.12.30
            var exitUsers = UserManager.Users.Include(x => x.Departments).ToList(); //获取系统所有用户 及管理的部门。
            var currentUser = exitUsers.SingleOrDefault(x => x.Id == User.Identity.GetUserId()); //获取登录用户。
            var currentDepartments = currentUser.Departments.ToList(); //获取登录用户所管理的部门。

            //返回存在的学期列表
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
            departments.AddRange(departmentListQuery.Distinct().OrderBy(i => i.DepartmentID));

            //ViewBag.departmentSelectList = new SelectList(departments, "DepartmentID", "DepartmentName", departmentID);
            ViewBag.departmentSelectList = new SelectList(departments.Where(x => currentDepartments.Contains(x)), "DepartmentID", "DepartmentName", departmentID);

            //返回专业列表
            List<Major> majors = new List<Major>();
            var majorListQuery = from sc in db.SemesterCourses.Include(x => x.GradeMajor.Major)
                                 select sc.GradeMajor.Major;
            majors.AddRange(majorListQuery.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID));

            //ViewBag.majorSelectList = new SelectList(majors, "MajorID", "MajorName", majorID);
            ViewBag.majorSelectList = new SelectList(majors.Where(x => currentDepartments.Contains(x.Department)), "MajorID", "MajorName", majorID);

            //返回课程类型的枚举的选项。
            var courseTypeItems = from CourseType e in Enum.GetValues(typeof(CourseType))   //Enum.GetValues(Typeof(type)) 返回值。 Enum.GetNames(Typeof(type)) 返回枚举字符串。
                                  select new SelectListItem { Value = Convert.ToInt32(e).ToString(), Text = e.ToString() };
            ViewBag.courseTypeList = new SelectList(courseTypeItems, "Value", "Text", courseType);


            //返回是否提交的bool型列表，定义一个selectListItem的列表

            List<SelectListItem> isSubmmitItems = new List<SelectListItem>()
            {
                new SelectListItem(){ Value =bool.TrueString,Text="提交"},
                new SelectListItem(){ Value=bool.FalseString,Text="未提交"}

            };
            ViewBag.isSubmmitSelectList = new SelectList(isSubmmitItems, "Value", "Text", isSubmmit);


            var _AllsemesterCourse = db.SemesterCourses.Include(x => x.GradeMajor.Major.Department).Include(x =>x.BookSubscriptions).ToList(); //首先查找全部课程。 //必须转换为ToLIist() ，否则发生 必须为基元类型的错误。
            var NotManageDepartmentMajor = _AllsemesterCourse.Where(x => (!currentDepartments.Contains(x.GradeMajor.Major.Department))).Where(x =>x.CourseType ==CourseType.专业课).ToList(); //找到非本部门的专业课。

            var _semesterCourse = _AllsemesterCourse.Except(NotManageDepartmentMajor); //除去 非本部门的专业课。

            if (!User.IsInRole("PublicCourseAdministrator"))
            {
                _semesterCourse = _semesterCourse.Where(x => x.CourseType != CourseType.公共课);
            }

            if (!User.IsInRole("PoliticCourseAdministrator"))
            {
                _semesterCourse = _semesterCourse.Where(x => x.CourseType != CourseType.思政课);
            }
            

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
                _semesterCourse = _semesterCourse.Where(x => x.SemesterCourseName.Trim().Contains(courseName.Trim()));
            }
            ViewBag.CourseName = courseName;

            if (isSubmmit != null)
            {
                _semesterCourse = _semesterCourse.Where(x => x.SubmmitState == isSubmmit);
            }
            ViewBag.IsSubmmit = isSubmmit;

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

                case "subscriptionUser":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SubscriptionUserName);
                    break;
                case "subscriptionUser_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SubscriptionUserName);
                    break;
                case "subscriptionTime":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SubscriptionTime);
                    break;
                case "subscriptionTime_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SubscriptionTime);
                    break;
                case "isSubmmit":
                    _semesterCourse = _semesterCourse.OrderBy(x => x.SubmmitState);
                    break;
                case "isSubmmit_desc":
                    _semesterCourse = _semesterCourse.OrderByDescending(x => x.SubmmitState);
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

        /*
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
        */

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

        /*

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

       */

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
        /*
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

       */
        /*
           [HttpGet]
           public PartialViewResult GetASemesterCourse(int? id)
           {
               SemesterCourseViewModel _semesterCourseViewModel = new SemesterCourseViewModel();

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
                   _semesterCourseViewModel.DepartmentList = new SelectList(_exitDepartmentBlank.Distinct().OrderBy(x => x.DepartmentID), "DepartmentID", "DepartmentName");



                   var _exitMajorBlank = from maj in db.GradeMajors.Include(x => x.Major)
                                         select maj.Major;
                   _semesterCourseViewModel.MajorsList = new SelectList(_exitMajorBlank.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID), "MajorID", "MajorName");


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
               _semesterCourseViewModel.DepartmentList = new SelectList(_exitDepartment.Distinct().OrderBy(x => x.DepartmentID), "DepartmentID", "DepartmentName", _semesterCourse.GradeMajor.Major.DepartmentID);

               var _exitMajor = from maj in db.GradeMajors.Include(x => x.Major)
                                where (maj.GradeMajorIsValidate == true)
                                select maj.Major;
               _semesterCourseViewModel.MajorsList = new SelectList(_exitMajor.Distinct().OrderBy(x => x.MajorID), "MajorID", "MajorName", _semesterCourse.GradeMajor.MajorID);

               _semesterCourseViewModel.SemesterCourseNumber = _semesterCourse.SemesterCourseNumber;
               _semesterCourseViewModel.SemesterCourseName = _semesterCourse.SemesterCourseName;
               _semesterCourseViewModel.CourseType = _semesterCourse.CourseType;

               return PartialView("_Modal.FormContent", _semesterCourseViewModel);
           }

          */
      
         //用于Index视图
         [HttpGet]
         public JsonResult GetMajorList(int? departmentID)
         {
             var _majorList = from m in db.SemesterCourses.Include(x => x.GradeMajor.Major)
                              select m.GradeMajor.Major;
             _majorList = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID);
             if (departmentID != null)
             {
                 _majorList = _majorList.Where(x => x.DepartmentID == departmentID);
             }

             var _majors = from m in _majorList
                           select new { majorID = m.MajorID, majorName = m.MajorName };
             return Json(_majors.ToList(), JsonRequestBehavior.AllowGet);

         }

        /*
         //模态框取得的专业列表，一旦模态框中部门变动，返回专业
         [HttpGet]
         public JsonResult GetModalMajorList(int gradeID, int? departmentID)
         {
             var _majorList = from m in db.GradeMajors.Include(x => x.Major)
                              where m.GradeID == gradeID && m.GradeMajorIsValidate == true
                              select m.Major;

             _majorList = _majorList.Distinct().OrderBy(x => x.MajorID);

             if (departmentID != null)
             {
                 _majorList = _majorList.Where(x => x.DepartmentID == departmentID);
             }

             var _majors = from m in _majorList
                           select new { majorID = m.MajorID, majorName = m.MajorName };
             return Json(_majors.ToList(), JsonRequestBehavior.AllowGet);

         }
         */

        //批量设置需要或不需要征订学生教材。适用于批准设置需要 教材 和批量设置不需要教材 两种选择。
        public int MassSetNeedStudentBook(bool expected = true, params int[] selectedIds) //必须为int型，负责find()会出现主键的类型不匹配
        {
            int number = 0;
            if (selectedIds != null)
            {
                foreach (var semesterCourseId in selectedIds)
                {
                    var semesterCourse = db.SemesterCourses.Find(semesterCourseId);
                    if (semesterCourse != null)
                    {
                        if (semesterCourse.SubmmitState != true)
                        {
                            if (expected)
                            {
                                if (semesterCourse.IsNeedStudentBook != true)
                                {
                                    semesterCourse.IsNeedStudentBook = true;
                                    number++;
                                }
                            }
                            else
                            {
                                if (semesterCourse.IsNeedStudentBook != false)
                                {
                                    semesterCourse.IsNeedStudentBook = false;
                                    number++;
                                    var alreadyBookSubscription = semesterCourse.BookSubscriptions.Where(x => x.SubscriptionType == SubscriptionType.学生教材);
                                    if (alreadyBookSubscription != null)//如果存在与该 课程关联的订单。
                                    {
                                        db.BookSubscriptions.RemoveRange(alreadyBookSubscription); //删除该课程对应的所有订单。
                                    }
                                }
                            }
                            db.SaveChanges();

                        }
                       
                    }
                }
            }
            return number;
        }


        //批量设置需要或不需要征订教师用书。适用于批准设置需要 教师用书 和批量设置不需要教师用书 两种选择。
        public int MassSetNeedTeacherBook(bool expected = true, params int[] selectedIds) //必须为int型，负责find()会出现主键的类型不匹配
        {
            int number = 0;
            if (selectedIds != null)
            {
                foreach (var semesterCourseId in selectedIds)
                {
                    var semesterCourse = db.SemesterCourses.Find(semesterCourseId);
                    if (semesterCourse != null)
                    {
                        if (semesterCourse.SubmmitState != true)
                        {
                            if (expected)
                            {
                                if (semesterCourse.IsNeedTeacherBook != true)
                                {
                                    semesterCourse.IsNeedTeacherBook = true;
                                    number++;
                                }
                            }
                            else
                            {
                                if (semesterCourse.IsNeedTeacherBook != false)
                                {
                                    semesterCourse.IsNeedTeacherBook = false;
                                    number++;
                                    var alreadyBookSubscription = semesterCourse.BookSubscriptions.Where(x => x.SubscriptionType == SubscriptionType.教师用书);
                                    if (alreadyBookSubscription != null)//如果存在与该 课程关联的订单。
                                    {
                                        db.BookSubscriptions.RemoveRange(alreadyBookSubscription); //删除该课程对应的所有订单。
                                    }
                                }
                            }

                            db.SaveChanges();
                        }

                      
                    }
                }
            }
            return number;
        }



        //批量设置提交或教材征订信息。院系征订用户只能提交，而不能取消提交。
        public int MassSetSubmmit(params int[] selectedIds) //必须为int型，负责find()会出现主键的类型不匹配
        {
            int number = 0;
            
            if (selectedIds != null)
            {
                foreach (var semesterCourseId in selectedIds)
                {
                    var semesterCourse = db.SemesterCourses.Find(semesterCourseId);
                    if (semesterCourse != null)
                    {
                        if (semesterCourse.SubmmitState != true)
                        {
                            semesterCourse.SubmmitState = true;
                            semesterCourse.SubscriptionUserName = User.Identity.GetUserName();
                            semesterCourse.SubscriptionTime = DateTime.Now;
                            number += db.SaveChanges();
                        }
                           
                    }
                }
               

            }

            return number;
        }




        //返回书目给左边的列表框，如果为空，就返回所有的图书 。
        [HttpGet]
        public JsonResult GetBookList(string searchBookString, int semesterCourseID, string bookUseage)
        {
            // var booksList= new List<ChooseBookViewModel>();
            var _currentSemesterCourse = db.SemesterCourses.Include(x => x.BookSubscriptions).SingleOrDefault(x => x.SemesterCourseID == semesterCourseID); //先找出学期课程
            IEnumerable<Book> choosedBooks;
            if (bookUseage == "studentBook")
            {
                choosedBooks = from sc in _currentSemesterCourse.BookSubscriptions
                               where sc.SubscriptionType == SubscriptionType.学生教材
                               select sc.Book;
            }
            else
            {
                choosedBooks = from sc in _currentSemesterCourse.BookSubscriptions
                               where sc.SubscriptionType == SubscriptionType.教师用书
                               select sc.Book;
            }
            //查找所有教材库
            var _bookQuery = from b in db.Books.Include(x => x.BookType)
                             select b;

            if (!string.IsNullOrEmpty(searchBookString))
            {
                _bookQuery = _bookQuery.Where(x => (x.BookName.Trim().Contains(searchBookString.Trim())) || (x.ISBN.Replace("-", "").Contains(searchBookString.Replace("-", ""))) || (x.AuthorName.Trim().Contains(searchBookString.Trim())));
            }

            var returnBooks = _bookQuery.ToList().Except(choosedBooks.ToList()); //求差集运算符。但需要把它转换成ToList对象。
                                                                                 // var returnBooks = _bookQuery.Except(choosedBooks).ToList();

            var returnJson = from cb in returnBooks.OrderBy(x => x.BookName)
                             select new ChooseBookViewModel() { BookID = cb.BookID, BookInfo = cb.BookName + "/" + cb.AuthorName + "/" + cb.ISBN + "/" + cb.Press + "/" + cb.BookType.BookTypeName + "/" + cb.Price + "元" };//返回强类型的视图

            return Json(returnJson.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetExitBookList(int semesterCourseID, string bookUseage)   //找出该门课程已经有的教材
        {
            var _currentSemesterCourse = db.SemesterCourses.Include(x => x.BookSubscriptions).SingleOrDefault(x => x.SemesterCourseID == semesterCourseID); //先找出学期课程
            IEnumerable<Book> choosedBooks;
            if (bookUseage == "studentBook")
            {
                choosedBooks = from sc in _currentSemesterCourse.BookSubscriptions
                               where sc.SubscriptionType == SubscriptionType.学生教材
                               select sc.Book;
            }
            else
            {
                choosedBooks = from sc in _currentSemesterCourse.BookSubscriptions
                               where sc.SubscriptionType == SubscriptionType.教师用书
                               select sc.Book;
            }
            var returnJson = from cb in choosedBooks.OrderBy(x => x.BookName)
                             select new ChooseBookViewModel() { BookID = cb.BookID, BookInfo = cb.BookName + "/" + cb.AuthorName + "/" + cb.ISBN + "/" + cb.Press + "/" + cb.BookType.BookTypeName + "/" + cb.Price + "元" };//返回强类型的视图

            return Json(returnJson.ToList(), JsonRequestBehavior.AllowGet);
        }

        //征订数量
        [HttpGet]
        public int GetSubscriptionStudentNumber(int semesterCourseID, string bookUseage)
        {
            int _subscriptonNumber = 0;
            if (bookUseage == "studentBook")
            {
                //学生用书每一种的个数。应该等于年级专业学生个数；

                _subscriptonNumber = (int)db.SemesterCourses.Find(semesterCourseID).GradeMajor.GradeMajorStudentNumber;

            }
            else
            {
                //教师用书每一种的个数。如果1门学期课程有多门教材，则必须相同。
                var teacherBookSubscription = db.SemesterCourses.Find(semesterCourseID).BookSubscriptions.FirstOrDefault(x => x.SubscriptionType == SubscriptionType.教师用书);
                if (teacherBookSubscription != null)
                    _subscriptonNumber = (int)teacherBookSubscription.BookSubscriptionNumber;
            }

            return _subscriptonNumber;

        }




        //指定征订教材
        [HttpPost]
        public int BookSubscript(int semesterCourseID, string bookUse, int bookSubscriptionNumber, params int[] chooseBooksIDs)
        {
            int count = 0;
            if (bookSubscriptionNumber == 0)
                return 0;
            var _currentSemesterCourse = db.SemesterCourses.Find(semesterCourseID); //先找出学期课程
            if (_currentSemesterCourse != null)
            {
                if (_currentSemesterCourse.SubmmitState != true)
                {
                    if (bookUse == "studentBook")
                    {
                        count = StudentBookSubscript(semesterCourseID, bookSubscriptionNumber, chooseBooksIDs);
                    }
                    else
                    {
                        count = TeacherBookSubscript(semesterCourseID, bookSubscriptionNumber, chooseBooksIDs);
                    }

                }
            }
            return count;
        }


        //学生教材征订
        private int StudentBookSubscript(int semesterCourseID, int bookSubscriptionNumber, params int[] chooseBooksIDs)
        {
            int count = 0;
            var _currentSemesterCourse = db.SemesterCourses.Include(x => x.BookSubscriptions).SingleOrDefault(x => x.SemesterCourseID == semesterCourseID); //先找出学期课程

            //该课程已有的学生教材订单
            var choosedBooksID = from sc in _currentSemesterCourse.BookSubscriptions
                                 where sc.SubscriptionType == SubscriptionType.学生教材
                                 select sc.Book.BookID;
            //现在选择的教材ID;
            if (chooseBooksIDs == null)
            {
                var alreadyBookSubscription = _currentSemesterCourse.BookSubscriptions.Where(x => x.SubscriptionType == SubscriptionType.学生教材);
                count = alreadyBookSubscription.Count();
                //_currentSemesterCourse.BookSubscriptions = new List<BookSubscription>(); //通过外键属性删除 会出错。只有通过删除 原始的征订单，就自动级联删除了关联的 学期课程。这里与多对多关系不一样。
                //解决并非所有代码路径都有返回值。

                // thisBookSubscription.ForEach(x => _currentSemesterCourse.BookSubscriptions.Remove(x));

                db.BookSubscriptions.RemoveRange(alreadyBookSubscription); //删除该课程对应的所有订单。
                _currentSemesterCourse.SubscriptionUserName = User.Identity.GetUserName();
                _currentSemesterCourse.SubscriptionTime = DateTime.Now;
                db.SaveChanges();
                return count;
            }
            else
            {
                var selectedBooksHS = new HashSet<int>(chooseBooksIDs); //选择的教材ID;
                var semesterCourseBooksHS = new HashSet<int>(choosedBooksID); //已经有的ID;
                var UnionBookIdsHS = selectedBooksHS.Union(semesterCourseBooksHS); //求并集。

                foreach (var bookIDItem in UnionBookIdsHS)
                {
                    var _BookItem = db.Books.Find(bookIDItem);
                    if (selectedBooksHS.Contains(bookIDItem))
                    {
                        if (!semesterCourseBooksHS.Contains(bookIDItem))
                        {
                            _currentSemesterCourse.BookSubscriptions.Add(new BookSubscription()
                            {
                                BookID = bookIDItem,
                                SemesterCourseID = _currentSemesterCourse.SemesterCourseID,
                                BookSubscriptionNumber = _currentSemesterCourse.GradeMajor.GradeMajorStudentNumber,
                                SubscriptionPrice = _BookItem.Price * _currentSemesterCourse.GradeMajor.GradeMajorStudentNumber,
                                SubscriptionType = SubscriptionType.学生教材,

                                //为方便汇总统计，后补加的订单中的14项信息,
                                SemesterName = _currentSemesterCourse.Semester.SemesterName,
                                GradeName = _currentSemesterCourse.GradeMajor.Grade.GradeName,
                                DepartmentName = _currentSemesterCourse.GradeMajor.Major.Department.DepartmentName,
                                MajorName = _currentSemesterCourse.GradeMajor.Major.MajorName,
                                SemesterCourseNumber = _currentSemesterCourse.SemesterCourseNumber,
                                SemesterCourseName = _currentSemesterCourse.SemesterCourseName,
                                BookName = _BookItem.BookName,
                                AuthorName = _BookItem.AuthorName,
                                ISBN = _BookItem.ISBN,
                                Press = _BookItem.Press,
                                PublishingDate = _BookItem.PublishingDate,
                                Price = _BookItem.Price,
                                BookTypeName = _BookItem.BookType.BookTypeName, // 书的类别，是规划教材还是讲义。 这里不是枚举类型。 BookTypeName = _BookItem.BookType.ToString()
                                SubscriptionTypeName = SubscriptionType.学生教材.ToString() //SubscriptionTypeName = _BookItem.BookType.ToString() //征订的类别，枚举类型通过.ToString()转换为枚举项，而不是整形值。是学生教材还是教师用书。

                            });
                            _currentSemesterCourse.SubscriptionUserName = User.Identity.GetUserName();
                            _currentSemesterCourse.SubscriptionTime = DateTime.Now;
                            db.SaveChanges();
                            count++;
                        }
                    }
                    else
                    {
                        if (semesterCourseBooksHS.Contains(bookIDItem))
                        {
                            var thisBookSubscription = db.BookSubscriptions.Where(x => x.BookID == bookIDItem).ToList();
                            // thisBookSubscription.ForEach(x => _currentSemesterCourse.BookSubscriptions.Remove(x));

                            thisBookSubscription.ForEach(x => db.BookSubscriptions.Remove(x));
                            _currentSemesterCourse.SubscriptionUserName = User.Identity.GetUserName();
                            _currentSemesterCourse.SubscriptionTime = DateTime.Now;
                            db.SaveChanges();
                            count++;
                        }
                    }
                }



            }

            return count;
        }




        //教师用书征订
        private int TeacherBookSubscript(int semesterCourseID, int bookSubscriptionNumber, params int[] chooseBooksIDs)
        {
            int count = 0;
            var _currentSemesterCourse = db.SemesterCourses.Include(x => x.BookSubscriptions).SingleOrDefault(x => x.SemesterCourseID == semesterCourseID); //先找出学期课程

            var alreadyBookSubscription = _currentSemesterCourse.BookSubscriptions.Where(x => x.SubscriptionType == SubscriptionType.教师用书); //找到的订单
            var alreadyBooksID = (from sc in alreadyBookSubscription  //找出所有的书
                                  select sc.BookID).ToList();

            db.BookSubscriptions.RemoveRange(alreadyBookSubscription); //删除该课程对应的所有订单。
            _currentSemesterCourse.SubscriptionUserName = User.Identity.GetUserName();
            _currentSemesterCourse.SubscriptionTime = DateTime.Now;
            count = db.SaveChanges() - 1; //执行db.SaveChanges()返回的记录数 是等于 删除的 订单 + 修改 的课程记录1项。因此，要减1.
            if (chooseBooksIDs == null)
            {
                return count;
            }

            var selectedBooksHS = new HashSet<int>(chooseBooksIDs); //选择的教材ID;
            var semesterCourseBooksHS = new HashSet<int>(alreadyBooksID); //已经有的ID;

            //要求的是对称差集，可以直接求，也可以使用补集减去差集。
            //var dd = selectedBooksHS.SymmetricExceptWith(semesterCourseBooksHS)
            var UnionBookIdsHSCount = semesterCourseBooksHS.Union(selectedBooksHS).Count();
            var IntersectBookIdsHSCount = semesterCourseBooksHS.Intersect(selectedBooksHS).Count(); //求交集。 Except 求差集 

            foreach (var bookIDItem in selectedBooksHS)
            {
                var _BookItem = db.Books.Find(bookIDItem);

                _currentSemesterCourse.BookSubscriptions.Add(new BookSubscription()
                {
                    BookID = bookIDItem,
                    SemesterCourseID = _currentSemesterCourse.SemesterCourseID,
                    BookSubscriptionNumber = bookSubscriptionNumber,
                    SubscriptionPrice = _BookItem.Price * bookSubscriptionNumber, //教师用书征订 是手工输入数字。
                    SubscriptionType = SubscriptionType.教师用书,

                    //为方便汇总统计，后补加的订单中的14项信息,
                    SemesterName = _currentSemesterCourse.Semester.SemesterName,
                    GradeName = _currentSemesterCourse.GradeMajor.Grade.GradeName,
                    DepartmentName = _currentSemesterCourse.GradeMajor.Major.Department.DepartmentName,
                    MajorName = _currentSemesterCourse.GradeMajor.Major.MajorName,
                    SemesterCourseNumber = _currentSemesterCourse.SemesterCourseNumber,
                    SemesterCourseName = _currentSemesterCourse.SemesterCourseName,
                    BookName = _BookItem.BookName,
                    AuthorName = _BookItem.AuthorName,
                    ISBN = _BookItem.ISBN,
                    Press = _BookItem.Press,
                    PublishingDate = _BookItem.PublishingDate,
                    Price = _BookItem.Price,
                    BookTypeName = _BookItem.BookType.BookTypeName, // 书的类别，是规划教材还是讲义。 这里不是枚举类型。 BookTypeName = _BookItem.BookType.ToString()
                    SubscriptionTypeName = SubscriptionType.教师用书.ToString() //征订的类别，枚举类型通过.ToString()转换为枚举项，而不是整形值。是学生教材还是教师用书。


                });
            }

            _currentSemesterCourse.SubscriptionUserName = User.Identity.GetUserName();
            _currentSemesterCourse.SubscriptionTime = DateTime.Now;
            db.SaveChanges();

            return UnionBookIdsHSCount - IntersectBookIdsHSCount;
        }


        public PartialViewResult GetSemesterCourseBookInfo(int semesterCourseID)
        {
            var _currentSemesterCourse = db.SemesterCourses.Include(x => x.BookSubscriptions).FirstOrDefault(x => x.SemesterCourseID == semesterCourseID); //找出课程及订单。
            var _currentSubscription = _currentSemesterCourse.BookSubscriptions.OrderBy(x => x.SubscriptionType).OrderBy(x => x.SemesterCourseName).ToList();
            var semesterCourseBookInfoViewModel = new SemesterCourseBookInfoViewModel()
            {
                StudentBookSubscription = _currentSubscription.Where(x => x.SubscriptionType == SubscriptionType.学生教材),
                TeacherBookSubscription = _currentSubscription.Where(x => x.SubscriptionType == SubscriptionType.教师用书),
            };
            return PartialView("_SemesterCourseBookInfo.Preview", semesterCourseBookInfoViewModel);

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
