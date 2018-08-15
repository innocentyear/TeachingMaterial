using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
namespace TeachingMaterial.Filters
{
    public class SubscriptionFilter:ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var context = new TeachingMaterialDbContext();

            StringBuilder str = new StringBuilder();
            var allSemesters = context.Semesters.ToList();
            //获取征订当前学期
            foreach (var seme in allSemesters)
            {
                if (seme.IsCurrentSemester)
                {
                    str.Append(seme.SemesterName + "征订日期" + seme.StartToOverDate + " ! ");
                }
                

            }
            //获取当前是否存在征订正在进行的学期。
            bool _subsState = allSemesters.Any(x => x.SubscriptionState == SubscriptionState.正在进行);

            filterContext.Controller.ViewBag.SubscriptionInfo = str;
            filterContext.Controller.ViewBag.SubscriptionState = _subsState;
            base.OnActionExecuted(filterContext);
        }
       
    }
}