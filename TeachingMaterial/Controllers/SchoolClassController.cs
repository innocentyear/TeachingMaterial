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
namespace TeachingMaterial.Controllers
{
    //用于教务处系统管理员添加班级的控制器
    [Authorize(Roles = "SuperAdministrator")]

    public class SchoolClassController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

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

            //返回存在的年级列表
            List<Grade> grades = new List<Grade>();
            var gradeListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Grade)
                                 select sc.GradeMajor.Grade;
            grades.AddRange(gradeListQuery.Distinct().OrderBy(x => x.GradeID));
            ViewBag.gradeSelectList = new SelectList(grades, "GradeID", "GradeName", gradeID);

            //返回存在的部门列表
            List<Department> departments = new List<Department>();
            var departmentListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Major.Department)
                                      select sc.GradeMajor.Major.Department;
            departments.AddRange(departmentListQuery.Distinct().OrderBy(i => i.DepartmentID));
            ViewBag.departmentSelectList = new SelectList(departments, "DepartmentID", "DepartmentName", departmentID);

            //返回专业列表
            List<Major> majors = new List<Major>();
            var majorListQuery = from sc in db.SchoolClasses.Include(x => x.GradeMajor.Major)
                                 select sc.GradeMajor.Major;
            majors.AddRange(majorListQuery.Distinct().OrderBy(x =>x.DepartmentID).ThenBy(x => x.MajorID));
            ViewBag.majorSelectList = new SelectList(majors, "MajorID", "MajorName", majorID);
            
            var _schoolClass = db.SchoolClasses.Include(x => x.GradeMajor);
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
               // int classNameCount = SchoolClassName.Count();
                //int classStudentNumberCount = StudentNumber.Count();

                var validateLenth =SchoolClassName.Count(x => !string.IsNullOrEmpty(x));
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

            if (id == null)
            {
                var _exitGradeBlank = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                                      where grad.GradeMajorIsValidate == true
                                      select grad.Grade;
                _schoolClassViewModel.GradesList = new SelectList(_exitGradeBlank.Distinct().OrderBy(g => g.GradeID), "GradeID", "GradeName");


                var _exitDepartmentBlank = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                           select grad.Major.Department;
                _schoolClassViewModel.DepartmentList = new SelectList(_exitDepartmentBlank.Distinct().OrderBy(x => x.DepartmentID), "DepartmentID", "DepartmentName");



                var _exitMajorBlank = from maj in db.GradeMajors.Include(x =>x.Major)
                                      select maj.Major;
                _schoolClassViewModel.MajorsList = new SelectList(_exitMajorBlank.Distinct().OrderBy(x => x.MajorID), "MajorID", "MajorName");
                _schoolClassViewModel.StudentNumber = 0;

                return PartialView("_Modal.FormContent", _schoolClassViewModel);
            }

            SchoolClass _schoolClass = db.SchoolClasses.Find(id);
            _schoolClassViewModel.SchoolClassID = _schoolClass.SchoolClassID;


            var _exitGrade = from grad in db.GradeMajors //年级和专业来自gradeMajor;
                             where grad.GradeMajorIsValidate == true
                             select grad.Grade;
            _schoolClassViewModel.GradesList = new SelectList(_exitGrade.Distinct().OrderBy(g => g.GradeID), "GradeID", "GradeName", _schoolClass.GradeMajor.GradeID);

            var _exitDepartment = from grad in db.GradeMajors.Include(x => x.Major.Department)
                                  select grad.Major.Department;
            _schoolClassViewModel.DepartmentList = new SelectList(_exitDepartment.Distinct().OrderBy(x => x.DepartmentID), "DepartmentID", "DepartmentName",_schoolClass.GradeMajor.Major.DepartmentID);


            var _exitMajor = from maj in db.GradeMajors.Include(x =>x.Major)
                             where (maj.GradeMajorIsValidate == true)
                             select maj.Major;
            _schoolClassViewModel.MajorsList = new SelectList(_exitMajor.Distinct().OrderBy(x => x.MajorID), "MajorID", "MajorName", _schoolClass.GradeMajor.MajorID);
            _schoolClassViewModel.SchoolClassName = _schoolClass.SchoolClassName;
            _schoolClassViewModel.StudentNumber = _schoolClass.StudentNumber;

            return PartialView("_Modal.FormContent", _schoolClassViewModel);
        }

        //用于Index视图
        [HttpGet]
        public JsonResult GetMajorList(int? departmentID)
        {
            var _majorList = from m in db.SchoolClasses.Include(x => x.GradeMajor.Major)
                             select m.GradeMajor.Major;
             _majorList = _majorList.Distinct().OrderBy(x => x.DepartmentID).ThenBy(x => x.MajorID);
            if (departmentID !=null)
            {
                _majorList =_majorList.Where(x => x.DepartmentID == departmentID);
            }

            var _majors = from m in _majorList
                          select new { majorID = m.MajorID, majorName = m.MajorName };
            return Json(_majors.ToList(), JsonRequestBehavior.AllowGet);

        }

        //模态框取得的专业列表，一旦模态框中部门变动，返回专业
        [HttpGet]
        public JsonResult GetModalMajorList(int gradeID,int? departmentID)
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
