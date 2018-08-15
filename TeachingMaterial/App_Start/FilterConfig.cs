using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Filters;

namespace TeachingMaterial
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SubscriptionFilter());
          //  filters.Add(new AutoCaculateStudentNumberFilter());
        }
    }
}
