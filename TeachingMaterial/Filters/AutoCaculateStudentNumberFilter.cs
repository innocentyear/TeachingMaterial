using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeachingMaterial.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Net;
namespace TeachingMaterial.Filters
{
    /// <summary>
    /// 没有使用这个过滤器。
    /// </summary>
    public class AutoCaculateStudentNumberFilter:ActionFilterAttribute
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // TeachingMaterialDbContext db=filterContext.Controller

           // TeachingMaterialDbContext db = filterContext.HttpContext.GetOwinContext().Get<TeachingMaterialDbContext>();


            foreach (var gradeItem in db.Grades.Include(x =>x.GradeMajors).ToList())
            {
                int _gradeStudentNumber = 0;
                if (gradeItem.GradeMajors != null)
                {
                    foreach (var gradeMajorItem in gradeItem.GradeMajors)
                    {
                        _gradeStudentNumber += gradeMajorItem.GradeMajorStudentNumber;

                        /* if (gradeMajorItem.SchoolClasses != null)
                        {
                           foreach (var classItem in gradeMajorItem.SchoolClasses)
                            {
                                _gradeStudentNumber += classItem.StudentNumber;
                            }
                           
                        } */
                    }
                    gradeItem.GradeStudentNumber = _gradeStudentNumber;
                    db.SaveChanges();
                }
                
            }


            foreach (var majorItem in db.Majors.Include(m => m.GradeMajors).ToList())
            {
                int _majorStudentNumber = 0;
                if (majorItem.GradeMajors != null)
                {
                    foreach (var gradeMajorItem in majorItem.GradeMajors)
                    {
                        _majorStudentNumber += gradeMajorItem.GradeMajorStudentNumber;

                        /* if (gradeMajorItem.SchoolClasses != null)
                        {
                            foreach (var classItem in gradeMajorItem.SchoolClasses)
                            {
                                _gradeStudentNumber += classItem.StudentNumber;
                            }
                           
                        } */
                    }
                    majorItem.MajorStudentNumber = _majorStudentNumber;
                    db.SaveChanges();
                }

            }


            base.OnActionExecuted(filterContext);
        }
    }
}