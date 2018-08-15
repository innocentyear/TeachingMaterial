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
namespace TeachingMaterial.Controllers
{
    [Authorize(Roles = "SuperAdministrator")]
    public class MajorController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        [AutoCaculateStudentNumberFilter]
        // GET: Major
        public ActionResult Index(string sortOrder,int? departmentID,int? pageSize,int page=1)
        {
            //排序
            ViewBag.CurrentSort = sortOrder;
            ViewBag.MajorIDSortParam = string.IsNullOrEmpty(sortOrder) ? "majorID_desc" : "";
            ViewBag.MajorNameSortParam = sortOrder == "majorName" ? "majorName_desc" : "majorName";
            ViewBag.DepartmentIDSortParam = sortOrder == "departmentID" ? "departmentID_desc" : "departmentID";


            //获取部门下拉框的值
            List <Department> departments = new List<Department>();
            var departmentListQuery = from major in db.Majors.Include(m => m.Department)
                                      select major.Department;
            departments.AddRange(departmentListQuery.Distinct().OrderBy(d =>d.DepartmentID));
            ViewBag.departmentSelectList = new SelectList(departments, "DepartmentID", "DepartmentName",departmentID);

            var _majors = db.Majors.Include(m => m.Department);

            //过滤departmentID;
            if (departmentID != null)
            {
                _majors = _majors.Where(m => m.DepartmentID == departmentID);
            }
            ViewBag.DepartmentID = departmentID;

            switch (sortOrder)
            {
                case "majorID_desc":
                    _majors = _majors.OrderByDescending(m => m.MajorID);
                    break;
                case "majorName":
                    _majors = _majors.OrderBy(m => m.MajorName);
                    break;
                case "majorName_desc":
                    _majors = _majors.OrderByDescending(m => m.MajorName);
                    break;
                case "departmentID":
                    _majors = _majors.OrderBy(m => m.Department.DepartmentID);
                    break;
                case "departmentID_desc":
                    _majors = _majors.OrderByDescending(m => m.Department.DepartmentID);
                    break;
                default:
                    _majors = _majors.OrderBy(m => m.MajorID);
                    break;
            }

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

            return View(_majors.ToPagedList(page, currentPageSize));
        }

        /*
        // GET: Major/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Major major = db.Majors.Find(id);
            if (major == null)
            {
                return HttpNotFound();
            }
            return View(major);
        }
        */


        //使用了模态框，就不使用 Get方式的方法了。
        // GET: Major/Create
      /*  public ActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "DepartmentName");
            return View();
        }

     */
        // POST: Major/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MajorName,DepartmentID")] Major major)  //以前在Create 方法中包含了MajorID,而Create方法中的majorID值为空，因此，始终模型绑定不成功。有了它还导致了绑定失败；
        {
            if (ModelState.IsValid)
            {
                db.Majors.Add(major);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "DepartmentName", major.DepartmentID);
            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
            }


            return RedirectToAction("Index");
        }

        ////使用了模态框，就不使用 Get方式的方法了。
        // GET: Major/Edit/5
      /*  public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Major major = db.Majors.Find(id);
            if (major == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "DepartmentName", major.DepartmentID);
            return View(major);
        }
        */

        // POST: Major/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MajorID,MajorName,DepartmentID")] Major major)
        {
            if (ModelState.IsValid)
            {
                db.Entry(major).State = EntityState.Modified;
                db.SaveChanges();

                // ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "DepartmentName", major.DepartmentID);
                //return View(major);
                if (Request.UrlReferrer != null)
                {
                    var returnUrl = Request.UrlReferrer.ToString();
                    return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        ////使用了模态框，就不使用 Get方式的方法了。
        // GET: Major/Delete/5
      /*  public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Major major = db.Majors.Find(id);
            if (major == null)
            {
                return HttpNotFound();
            }
            return View(major);
        }
        */

        // POST: Major/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int MajorID)
        {
            Major major = db.Majors.Find(MajorID);
            db.Majors.Remove(major);
            db.SaveChanges();
            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是隐藏表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态。
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult GetAMajor(int? id)
        {
            MajorViewModel _majorViewModel = new MajorViewModel();

            if (id == null)
            {
               _majorViewModel.DepartmentsList = new SelectList(db.Departments, "DepartmentID", "DepartmentName");

                return PartialView("_Modal.FormContent", _majorViewModel);
            }
           
           Major major = db.Majors.Find(id);
            _majorViewModel.MajorID = major.MajorID;
            _majorViewModel.MajorName = major.MajorName;   
           _majorViewModel.DepartmentsList = new SelectList(db.Departments, "DepartmentID", "DepartmentName", major.DepartmentID);
           return PartialView("_Modal.FormContent", _majorViewModel);
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
