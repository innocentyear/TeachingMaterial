using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using System.Threading.Tasks;

namespace TeachingMaterial.Controllers
{
    [Authorize(Roles = "SuperAdministrator")]
    public class SemesterController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        // GET: Semester
        public ActionResult Index()
        {
            return View(db.Semesters.ToList());
        }



        // GET: Semester/Details/5
        /* 由于使用了模态框，不再需要 details 方法和details 视图了，就删除了。
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }
        */


        // GET: Semester/Create
        /* 由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
         public ActionResult Create()
         {
             return View();
         }
         */
       
        // POST: Semester/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SemesterName,StartDateOfSubscription,OverDateOfSubscription,SwitchOfSubscription,IsCurrentSemester")] Semester semester)
        {
            if (ModelState.IsValid)
            {
                if (semester.StartDateOfSubscription < semester.OverDateOfSubscription)
                {
                    ModelState.AddModelError("", "开始日期不能小于结束日期");
                }
                db.Semesters.Add(semester);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //PartialView("_AddSemester.Modal.Preview", semester);
            return RedirectToAction("Index");
        }

        // GET: Semester/Edit/5
        /*由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }
        */


        // POST: Semester/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SemesterID,SemesterName,StartDateOfSubscription,OverDateOfSubscription,SwitchOfSubscription,IsCurrentSemester")] Semester semester)
        {
          
            if (ModelState.IsValid)
            {
                db.Entry(semester).State = EntityState.Modified;
                db.SaveChanges();

                if (Request.UrlReferrer != null)
                {
                    var returnUrl = Request.UrlReferrer.ToString();
                    return new RedirectResult(returnUrl);    //由于使用的是表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态，筛选、排序、分页都保持不变。
                }

                return RedirectToAction("Index");
           
            }

            return View(semester);
        }

        // GET: Semester/Delete/5
        /* 由于使用了模态框，采用的是jquery ajax 方式返回分部视图直接提交，因此，get 形式的方法和原视图就不需要了。
       public ActionResult Delete(int? id)
       {
           if (id == null)
           {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           }
           Semester semester = db.Semesters.Find(id);
           if (semester == null)
           {
               return HttpNotFound();
           }
           return View(semester);
       }
        */

        // POST: Semester/Delete/5
        [HttpPost, ActionName("Delete")]
       [ValidateAntiForgeryToken]
       public ActionResult DeleteConfirmed(int id)
       {
           Semester semester = db.Semesters.Find(id);
           db.Semesters.Remove(semester);
           db.SaveChanges();
            if (Request.UrlReferrer != null)
            {
                var returnUrl = Request.UrlReferrer.ToString();
                return new RedirectResult(returnUrl);    //由于使用的是表单提交而非Ajax无刷新异步提交。所以使用jquery将表单提交到控制器后，返回Request.UrlReferrer返回到上一个页面将是数据库更新后的状态，筛选、排序、分页都保持不变。
            }


            return RedirectToAction("Index");
       }


       [HttpGet]
       public ActionResult GetEmptySemester()
       {
           return PartialView("_AddSemester.Modal.Preview");
       }

       [HttpPost]
       public ActionResult GetEditSemester(int? id)
       {
           if (id == null)
           {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           }
           Semester semester = db.Semesters.Find(id);
           if (semester == null)
           {
               return HttpNotFound();
           }
           return PartialView("_EditSemester.Modal.Preview", semester);

       }

        [HttpPost]
        public ActionResult GetDeleteSemester(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Semester semester = db.Semesters.Find(id);
            if (semester == null)
            {
                return HttpNotFound();
            }

            return PartialView("_DeleteSemester.Modal.Preview", semester);

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
