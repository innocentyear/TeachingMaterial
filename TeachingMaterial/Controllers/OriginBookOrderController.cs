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
namespace TeachingMaterial.Controllers
{
    public class OriginBookOrderController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        // GET: OriginBookOrder
        public ActionResult Index(string sortOrder, int? semesterID, int? gradeID, int? departmentID, int? majorID, CourseType? courseType, SubscriptionType? subscriptionType, string courseName,string bookName, int? pageSize, int page = 1)
        {
            //排序参数
            ViewBag.CurrentSort = sortOrder;

            ViewBag.SubscriptionTimeSortParam = string.IsNullOrEmpty(sortOrder) ? "subscriptionTime" : "";
            ViewBag.SemesterIDSortParam = sortOrder == "semester" ? "semester_desc" : "semester";
            ViewBag.GradeIDSortParam = sortOrder == "gradeID" ? "gradeID_desc" : "gradeID";
            ViewBag.MajorIDSortParam = sortOrder == "majorID" ? "majorID_desc" : "majorID";
            ViewBag.CourseTypeSortParam = sortOrder == "courseType" ? "courseType_desc" : "courseType";
            ViewBag.CourseNameSortParam = sortOrder == "courseName" ? "courseName_desc" : "courseName";

            ViewBag.SubscriptionTypeSortParam = sortOrder == "subscriptionType" ? "subscriptionType_desc" : "subscriptionType";
            ViewBag.BookNameSortParam = sortOrder == "bookName" ? "bookName_desc" : "bookName";
            ViewBag.PriceSortParam = sortOrder == "price" ? "price_desc" : "price";
            ViewBag.bookSubscriptionNumberSortParam = sortOrder == "bookSubscriptionNumber" ? "bookSubscriptionNumber_desc" : "bookSubscriptionNumber";
            ViewBag.SubscriptionPriceSortParam = sortOrder == "subscriptionPrice" ? "subscriptionPrice_desc" : "subscriptionPrice";
            ViewBag.PressNameSortParam = sortOrder == "pressName" ? "pressName_desc" : "pressName";
            ViewBag.SubscriptionUserSortParam = sortOrder == "subscriptionUser" ? "subscriptionUser_desc" : "subscriptionUser";

            //返回存在的学期列表  
            List<Semester> semesters = new List<Semester>();
            var semesterListQuery = from bs in db.BookSubscriptions.Include(x =>x.SemesterCourse.Semester)
                                    select bs.SemesterCourse.Semester;
            semesters.AddRange(semesterListQuery.Distinct().OrderBy(x => x.SemesterID));
            ViewBag.semesterSelectList = new SelectList(semesters, "SemesterID", "SemesterName", semesterID);

            // ViewBag.semesterSelectList = new SelectList(db.Semesters.OrderBy(x =>x.SemesterID), "SemesterID", "SemesterName", semesterID);// 返回所有的学期列表 
            //返回存在的年级列表
            List<Grade> grades = new List<Grade>();
            var gradeListQuery = from bs in db.BookSubscriptions.Include(x =>x.SemesterCourse.GradeMajor.Grade)
                                 select bs.SemesterCourse.GradeMajor.Grade;
            grades.AddRange(gradeListQuery.Distinct().OrderBy(x => x.GradeID));
            ViewBag.gradeSelectList = new SelectList(grades, "GradeID", "GradeName", gradeID);

            // ViewBag.gradeSelectList = new SelectList(db.Grades.OrderBy(x =>x.GradeID), "GradeID", "GradeName", gradeID);// 返回所有的年级列表
            
            //返回存在的部门列表
            List<Department> departments = new List<Department>();
            var departmentListQuery = from bs in db.BookSubscriptions.Include(x => x.SemesterCourse.GradeMajor.Major.Department)
                                      select bs.SemesterCourse.GradeMajor.Major.Department;
            departments.AddRange(departmentListQuery.Distinct().OrderBy(i => i.DepartmentID).ToList());
            ViewBag.departmentSelectList = new SelectList(departments, "DepartmentID", "DepartmentName", departmentID);

            // ViewBag.departmentSelectList = new SelectList(db.Departments.OrderBy(x =>x.DepartmentID), "DepartmentID", "DepartmentName", departmentID); //返回所有的部门列表
           
            //返回存在的专业列表
            List<Major> majors = new List<Major>();
            var majorListQuery = from bs in db.BookSubscriptions.Include(x =>x.SemesterCourse.GradeMajor.Major)
                                 select bs.SemesterCourse.GradeMajor.Major;
            majors.AddRange(majorListQuery.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID));
            ViewBag.majorSelectList = new SelectList(majors, "MajorID", "MajorName", majorID);

            //ViewBag.majorSelectList = new SelectList(db.Majors.OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID), "MajorID", "MajorName", majorID);  //返回所有专业列表    
            //返回课程类型的枚举的选项。
            var courseTypeItems = from CourseType e in Enum.GetValues(typeof(CourseType))   //Enum.GetValues(Typeof(type)) 返回值。 Enum.GetNames(Typeof(type)) 返回枚举字符串。
                                  select new SelectListItem { Value = Convert.ToInt32(e).ToString(), Text = e.ToString() };
            ViewBag.courseTypeList = new SelectList(courseTypeItems, "Value", "Text", courseType);

            //返回征订类型的枚举的选项。
            var subscriptionTypeItems = from SubscriptionType e in Enum.GetValues(typeof(SubscriptionType))   //Enum.GetValues(Typeof(type)) 返回值。 Enum.GetNames(Typeof(type)) 返回枚举字符串。
                                  select new SelectListItem { Value = Convert.ToInt32(e).ToString(), Text = e.ToString() };
            ViewBag.subscriptionTypeList = new SelectList(subscriptionTypeItems, "Value", "Text", subscriptionType);


            var _bookSubscription = db.BookSubscriptions.Include(x => x.SemesterCourse).Include(x =>x.Book);
            //过滤操作
            if (semesterID != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.Semester.SemesterID == semesterID);
            }
            ViewBag.SemesterID = semesterID;

            if (gradeID != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.GradeMajor.GradeID == gradeID);
            }
            ViewBag.GradeID = gradeID;

            if (departmentID != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.GradeMajor.Major.DepartmentID == departmentID);
            }
            ViewBag.DepartmentID = departmentID;

            if (majorID != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.GradeMajor.MajorID == majorID);
            }
            ViewBag.MajorID = majorID;
            if (courseType != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.CourseType == courseType);
            }
            ViewBag.CourseType = courseType;

            if (subscriptionType != null)
            {
                _bookSubscription = _bookSubscription.Where(x => x.SubscriptionType == subscriptionType);
            }
            ViewBag.SubscriptionType = subscriptionType;

            if (!string.IsNullOrEmpty(courseName))
            {
                _bookSubscription = _bookSubscription.Where(x => x.SemesterCourse.SemesterCourseName.Trim().Contains(courseName.Trim()));
            }
            ViewBag.CourseName = courseName;

            if (!string.IsNullOrEmpty(bookName))
            {
                _bookSubscription = _bookSubscription.Where(x => (x.BookName.Trim().Contains(bookName.Trim())) || (x.ISBN.Trim().Replace("-", "").Contains(bookName.Trim().Replace("-", ""))));
            }
            ViewBag.BookName = bookName;


            //排序
            switch (sortOrder)
            {
                case "subscriptionTime":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.SubscriptionTime);
                    break;
                case "semester":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.SemesterID);
                    break;
                case "semester_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.SemesterID);
                    break;

                case "gradeID":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.GradeMajor.GradeID);
                    break;
                case "gradeID_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.GradeMajor.GradeID);
                    break;
                case "majorID":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.GradeMajor.MajorID);
                    break;
                case "majorID_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.GradeMajor.MajorID);
                    break;
                case "courseType":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.CourseType);
                    break;
                case "courseType_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.CourseType);
                    break;

                case "subscriptionType":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SubscriptionType);
                    break;
                case "subscriptionType_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SubscriptionType);
                    break;

                case "courseName":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.SemesterCourseName);
                    break;
                case "courseName_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.SemesterCourseName);
                    break;
                case "subscriptionUser":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SemesterCourse.SubscriptionUserName);
                    break;
                case "subscriptionUser_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.SubscriptionUserName);
                    break;

                case "bookName":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.Book.BookName);  //教材名称
                    break;
                case "bookName_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.Book.BookName);  //教材名称
                    break;
                case "bookSubscriptionNumber":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.BookSubscriptionNumber);//征订数量
                    break;
                case "bookSubscriptionNumber_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.BookSubscriptionNumber);
                    break;
                case "pressName":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.Book.Press);//征订数量
                    break;
                case "pressName_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.Book.Press);//征订数量
                    break;
                case "price":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.Book.Price);  //单价
                    break;
                case "price_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.Book.Price); 
                    break;
                case "subscriptionPrice":
                    _bookSubscription = _bookSubscription.OrderBy(x => x.SubscriptionPrice);  //征订单价格
                    break;
                case "subscriptionPrice_desc":
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SubscriptionPrice);
                    break;
                    
                default:
                    _bookSubscription = _bookSubscription.OrderByDescending(x => x.SemesterCourse.SubscriptionTime);
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

            return View(_bookSubscription.ToPagedList(page, currentPageSize));
        }



        //用于Index视图
        [HttpGet]
        public JsonResult GetMajorList(int? gradeID,int? departmentID)
        {
            var _majorList = db.BookSubscriptions.Include(x => x.SemesterCourse.GradeMajor).Select(x => x.SemesterCourse.GradeMajor.Major);
            if (gradeID != null)
            {
                _majorList = db.BookSubscriptions.Include(x => x.SemesterCourse.GradeMajor).Where(x =>x.SemesterCourse.GradeMajor.GradeID == gradeID).Select(x => x.SemesterCourse.GradeMajor.Major);
            }

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

        // GET: OriginBookOrder/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookSubscription bookSubscription = db.BookSubscriptions.Find(id);
            if (bookSubscription == null)
            {
                return HttpNotFound();
            }
            return View(bookSubscription);
        }

        // GET: OriginBookOrder/Create
        public ActionResult Create()
        {
            ViewBag.BookID = new SelectList(db.Books, "BookID", "BookName");
            ViewBag.SemesterCourseID = new SelectList(db.SemesterCourses, "SemesterCourseID", "SemesterCourseNumber");
            return View();
        }

        // POST: OriginBookOrder/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookSubscriptionID,BookID,SemesterCourseID,BookSubscriptionNumber,SubscriptionPrice,SubscriptionType,SemesterName,GradeName,DepartmentName,MajorName,SemesterCourseNumber,SemesterCourseName,BookName,AuthorName,ISBN,Press,PublishingDate,Price,BookTypeName,SubscriptionTypeName")] BookSubscription bookSubscription)
        {
            if (ModelState.IsValid)
            {
                db.BookSubscriptions.Add(bookSubscription);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookID = new SelectList(db.Books, "BookID", "BookName", bookSubscription.BookID);
            ViewBag.SemesterCourseID = new SelectList(db.SemesterCourses, "SemesterCourseID", "SemesterCourseNumber", bookSubscription.SemesterCourseID);
            return View(bookSubscription);
        }

        // GET: OriginBookOrder/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookSubscription bookSubscription = db.BookSubscriptions.Find(id);
            if (bookSubscription == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookID = new SelectList(db.Books, "BookID", "BookName", bookSubscription.BookID);
            ViewBag.SemesterCourseID = new SelectList(db.SemesterCourses, "SemesterCourseID", "SemesterCourseNumber", bookSubscription.SemesterCourseID);
            return View(bookSubscription);
        }

        // POST: OriginBookOrder/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookSubscriptionID,BookID,SemesterCourseID,BookSubscriptionNumber,SubscriptionPrice,SubscriptionType,SemesterName,GradeName,DepartmentName,MajorName,SemesterCourseNumber,SemesterCourseName,BookName,AuthorName,ISBN,Press,PublishingDate,Price,BookTypeName,SubscriptionTypeName")] BookSubscription bookSubscription)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bookSubscription).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookID = new SelectList(db.Books, "BookID", "BookName", bookSubscription.BookID);
            ViewBag.SemesterCourseID = new SelectList(db.SemesterCourses, "SemesterCourseID", "SemesterCourseNumber", bookSubscription.SemesterCourseID);
            return View(bookSubscription);
        }

        // GET: OriginBookOrder/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookSubscription bookSubscription = db.BookSubscriptions.Find(id);
            if (bookSubscription == null)
            {
                return HttpNotFound();
            }
            return View(bookSubscription);
        }

        // POST: OriginBookOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BookSubscription bookSubscription = db.BookSubscriptions.Find(id);
            db.BookSubscriptions.Remove(bookSubscription);
            db.SaveChanges();
            return RedirectToAction("Index");
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
