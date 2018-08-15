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
using Microsoft.AspNet.Identity;
namespace TeachingMaterial.Controllers
{
    [Authorize]
    public class NoticeController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();

        [Authorize(Roles = "SuperAdministrator")]
        // GET: Notice
        public ActionResult Index(string searchTitle, int? pageSize, int page = 1)
        {
            ViewBag.searchTitle = searchTitle;

            var _notices = from n in db.Notices
                           select n;

            if (!string.IsNullOrEmpty(searchTitle))
            {
                _notices = _notices.Where(n => n.NoticeTitle.Contains(searchTitle.Trim()));
            }

            //定义一个用于显示公告列表的视图模型对象，主要是用于返回不用生成 公告内容的字段，这样会提高系统访问速度；
            List<NoticeIndexViewModel> noticesIndexViewModels = new List<NoticeIndexViewModel>();
            foreach (var item in _notices)
            {
                var indexViewModel = new NoticeIndexViewModel(); //直接初始化还要报错，干脆先建立一个对象及引用，再来赋值。

                indexViewModel.NoticeID = item.NoticeID;
                indexViewModel.NoticeTitle = item.NoticeTitle;
                indexViewModel.AuthorName = item.AuthorName;
                indexViewModel.PostTime = item.PostTime;
                indexViewModel.NoticeIsShow = item.NoticeIsShow;
                indexViewModel.ClickCount = item.ClickCount;
                indexViewModel.PriorOrder = item.PriorOrder;

                noticesIndexViewModels.Add(indexViewModel);

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

            return View(noticesIndexViewModels.OrderBy(n => n.PriorOrder).OrderByDescending(n => n.PostTime).ToPagedList(page, currentPageSize));
        }

        [Authorize(Roles = "SuperAdministrator")]
        // GET: Notice/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notice notice = db.Notices.Find(id);
            if (notice == null)
            {
                return HttpNotFound();
            }
            return View(notice);
        }


        [Authorize(Roles = "SuperAdministrator")]
        // GET: Notice/Create
        public ActionResult Create() //要使用 User.Identity.GetUserName()方法获取登录名，必须使用Microsoft.aspnet.Identity命名空间；
        {
            Notice notice = new Notice { AuthorName = User.Identity.GetUserName(), PostTime = DateTime.Now, NoticeIsShow = true, PriorOrder = 10, ClickCount = 0 };
            return View(notice);
        }

        // POST: Notice/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [Authorize(Roles = "SuperAdministrator")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NoticeTitle,Content,AuthorName,PostTime,NoticeIsShow,PriorOrder,ClickCount")] Notice notice)
        {
            if (ModelState.IsValid)
            {
                db.Notices.Add(notice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(notice);
        }


        // GET: Notice/Edit/5
        [Authorize(Roles = "SuperAdministrator")]
        public ActionResult Edit(int?  id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notice notice = db.Notices.Find(id);
            if (notice == null)
            {
                return HttpNotFound();
            }
            return View(notice);
        }

        // POST: Notice/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [Authorize(Roles = "SuperAdministrator")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "NoticeID,NoticeTitle,Content,AuthorName,PostTime,NoticeIsShow,PriorOrder,ClickCount")] Notice notice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(notice);
        }

        [Authorize(Roles = "SuperAdministrator")]
        // GET: Notice/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notice notice = db.Notices.Find(id);
            if (notice == null)
            {
                return HttpNotFound();
            }
            return View(notice);
        }

        // POST: Notice/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "SuperAdministrator")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Notice notice = db.Notices.Find(id);
            db.Notices.Remove(notice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        //用户端显示通知公告列表，只显示标题，作者和发布日期。
        public ActionResult NoticeList(string searchTitle, int? pageSize, int page = 1)
        {
            ViewBag.searchTitle = searchTitle;

            var _notices = from n in db.Notices.Where(n =>n.NoticeIsShow ==true)
                           select n;

            if (!string.IsNullOrEmpty(searchTitle))
            {
                _notices = _notices.Where(n => n.NoticeTitle.Contains(searchTitle.Trim()));
            }

            //定义一个用于显示公告列表的视图模型对象，主要是用于返回不用生成 公告内容的字段，这样会提高系统访问速度；
            List<NoticeIndexViewModel> noticesIndexViewModels = new List<NoticeIndexViewModel>();
            foreach (var item in _notices)
            {
                var indexViewModel = new NoticeIndexViewModel(); //直接初始化还要报错，干脆先建立一个对象及引用，再来赋值。

                indexViewModel.NoticeID = item.NoticeID;
                indexViewModel.NoticeTitle = item.NoticeTitle;
                indexViewModel.AuthorName = item.AuthorName;
                indexViewModel.PostTime = item.PostTime;
                indexViewModel.ClickCount = item.ClickCount;
                noticesIndexViewModels.Add(indexViewModel);

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

            return View(noticesIndexViewModels.OrderBy(n => n.PriorOrder).OrderByDescending(n => n.PostTime).ToPagedList(page, currentPageSize));
        }



        //用户端显示具体一个通知公告的内容；
        public ActionResult ShowNotice(int? noticeID)
        {
            if (noticeID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notice notice = db.Notices.Find(noticeID);
            if (notice == null)
            {
                return HttpNotFound();
            }

            notice.ClickCount++;
            db.SaveChanges();

            return View(notice);

        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult LoginNoticeList() //在登录页面上显示一个分部视图，只能查看标题，不能访问具体内容。
        {
            IQueryable<Notice> _notices = db.Notices.Where(x => x.NoticeIsShow == true).OrderBy(x => x.PriorOrder).OrderBy(x => x.PostTime).Take(6);
            //定义一个用于显示公告列表的视图模型对象，主要是用于返回不用生成 公告内容的字段，这样会提高系统访问速度；
            List<NoticeIndexViewModel> noticesIndexViewModels = new List<NoticeIndexViewModel>();
            foreach (var item in _notices)
            {
                var indexViewModel = new NoticeIndexViewModel
                {
                    NoticeID = item.NoticeID,
                    NoticeTitle = item.NoticeTitle,
                    PostTime = item.PostTime
                };

                noticesIndexViewModels.Add(indexViewModel);

            }
            return PartialView("_LoginNoticeList", noticesIndexViewModels);
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
