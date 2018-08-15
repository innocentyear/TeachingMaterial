using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;

namespace TeachingMaterial.Controllers
{
    [Authorize(Roles = "SuperAdministrator")]
    public class BookTypeController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        // GET: BookType
        public ActionResult Index()
        {
            return View(db.BookTypes.ToList());
        }
        /*

        // GET: BookType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookType bookType = db.BookTypes.Find(id);
            if (bookType == null)
            {
                return HttpNotFound();
            }
            return View(bookType);
        }
        */

        /*
        // GET: BookType/Create
        public ActionResult Create()
        {
            return View();
        }

    */
        // POST: BookType/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookTypeName")] BookType bookType)
        {
            if (ModelState.IsValid)
            {
                db.BookTypes.Add(bookType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bookType);
        }

        /*

        // GET: BookType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookType bookType = db.BookTypes.Find(id);
            if (bookType == null)
            {
                return HttpNotFound();
            }
            return View(bookType);
        }
        */

        // POST: BookType/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookTypeID,BookTypeName")] BookType bookType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bookType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bookType);
        }

        /*
        // GET: BookType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookType bookType = db.BookTypes.Find(id);
            if (bookType == null)
            {
                return HttpNotFound();
            }
            return View(bookType);
        }
        */

        // POST: BookType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BookType bookType = db.BookTypes.Find(id);
            db.BookTypes.Remove(bookType);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetABookType(int? id)
        {
            if(id ==null)
                return PartialView("_Modal.FormContent");
            BookType bookType = db.BookTypes.Find(id);
            if (bookType == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Modal.FormContent",bookType);

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
