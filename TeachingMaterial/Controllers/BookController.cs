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
    [Authorize(Roles = "SuperAdministrator")]
    public class BookController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        // GET: Book
        public ActionResult Index(string sortOrder,string bookName,string authorName,int? bookTypeID, int? pageSize, int page = 1)
        {
            //排序参数
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BookIDSortParam = string.IsNullOrEmpty(sortOrder) ? "bookID_desc" : "";
            ViewBag.BookNameSortParam = sortOrder == "bookName" ? "bookName_desc" : "bookName";
            ViewBag.PublishingDateSortParam = sortOrder == "publishingDate" ? "publishingDate_desc" : "publishingDate";
            ViewBag.PriceSortParam = sortOrder == "price" ? "price_desc" : "price";

            //返回所有的教材类别列表
            ViewBag.bookTypeSelectList = new SelectList(db.BookTypes.OrderBy(x =>x.BookTypeID), "BookTypeID", "BookTypeName",bookTypeID);


            var _books = db.Books.Include(b => b.BookType);

            //过滤操作
            if (!string.IsNullOrEmpty(bookName))
            {
                _books= _books.Where(x => (x.BookName.Trim().Contains(bookName.Trim()))|| ( x.ISBN.Trim().Replace("-","").Contains(bookName.Trim().Replace("-",""))  )  );
            }
            ViewBag.BookName = bookName;

            if (!string.IsNullOrEmpty(authorName))
            {
                _books= _books.Where(x => x.AuthorName.Trim().Contains(authorName.Trim()));
            }
            ViewBag.AuthorName = authorName;

            if (bookTypeID != null)
            {
                _books= _books.Where(x => x.BookTypeID == bookTypeID);
            }
            ViewBag.BookTypeID = bookTypeID;
            //排序
            switch (sortOrder)
            {
                case "bookID_desc":
                    _books = _books.OrderByDescending(x => x.BookID);
                    break;
                case "bookName":
                    _books = _books.OrderBy(x => x.BookName);
                    break;
                case "bookName_desc":
                    _books = _books.OrderByDescending(x => x.BookName);
                    break;
                case "publishingDate":
                    _books = _books.OrderBy(x => x.PublishingDate);
                    break;
                case "publishingDate_desc":
                    _books = _books.OrderByDescending(x => x.PublishingDate);
                    break;
                case "price":
                    _books = _books.OrderBy(x => x.Price);
                    break;
                case "price_desc":
                    _books = _books.OrderByDescending(x => x.Price);
                    break;

                default:
                    _books = _books.OrderBy(x => x.BookID);
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

            return View(_books.ToPagedList(page, currentPageSize));
        }

        // GET: Book/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Book/Create
        public ActionResult Create()
        {
            ViewBag.BookTypeID = new SelectList(db.BookTypes, "BookTypeID", "BookTypeName");
            return View();
        }

        // POST: Book/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookID,BookName,ISBN,AuthorName,Press,PublishingDate,Price,BookTypeID")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookTypeID = new SelectList(db.BookTypes, "BookTypeID", "BookTypeName", book.BookTypeID);
            return View(book);
        }

        // GET: Book/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookTypeID = new SelectList(db.BookTypes, "BookTypeID", "BookTypeName", book.BookTypeID);
            return View(book);
        }

        // POST: Book/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookID,BookName,ISBN,AuthorName,Press,PublishingDate,Price,BookTypeID")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookTypeID = new SelectList(db.BookTypes, "BookTypeID", "BookTypeName", book.BookTypeID);
            return View(book);
        }

        // GET: Book/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
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
