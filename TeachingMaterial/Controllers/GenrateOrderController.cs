using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using TeachingMaterial.Models;
using System.Data.Entity;
namespace TeachingMaterial.Controllers
{
    public class GenrateOrderController : Controller
    {
        private TeachingMaterialDbContext db = new TeachingMaterialDbContext();
        // GET: GenrateOrder
        public ActionResult Index()
        {
            return View();
        }

        public int GenerateSchoolClassBookOrder(int? semesterIDParam)
        {
            int count = 0;
            if (semesterIDParam == null)
            {
                return 0;
            }
            var currentSemester = db.Semesters.Single(x => x.SemesterID == semesterIDParam);
            if (currentSemester == null)
            {
                return 0;
            }
            if (!currentSemester.IsCurrentSemester)
            {
                return 0;
            }

            var exitSchoolClassBookOrder = db.SchoolClassBookOrders.Where(x => x.BookSubscription.SemesterCourse.SemesterID == semesterIDParam); //找出已经存在的班级订单，全部删除；
            db.SchoolClassBookOrders.RemoveRange(exitSchoolClassBookOrder);

            var exitMajorTeacherBookOrder = db.TeacherBookOrders.Where(x => x.BookSubscription.SemesterCourse.SemesterID == semesterIDParam);
            db.TeacherBookOrders.RemoveRange(exitMajorTeacherBookOrder);

            var subscritpion = db.BookSubscriptions.Include(x => x.SemesterCourse).Include(x => x.Book).Where(x => x.SemesterCourse.SemesterID == semesterIDParam); //找出所有的原始订单。

            foreach (var subItem in subscritpion)
            {
                var gradeMajorItem = db.GradeMajors.Include(x =>x.SchoolClasses).Single(x => x.GradeMajorID == subItem.SemesterCourse.GradeMajorID);
                if (gradeMajorItem != null)
                {
                    if (subItem.SubscriptionType == SubscriptionType.学生教材)
                    {
                        foreach (var classItem in gradeMajorItem.SchoolClasses)
                        {
                            db.SchoolClassBookOrders.Add(
                                new SchoolClassBookOrder
                                {
                                    SchoolClassID = classItem.SchoolClassID,
                                    BookSubscriptionID = subItem.BookSubscriptionID,
                                    GenerateTime = DateTime.Now,

                                    //为了使电子表格导出数据，添加了冗余列。
                                   SemesterName = subItem.SemesterName,
                                   GradeName =subItem.GradeName,
                                   DepartmentName =subItem.DepartmentName,
                                   MajorName =subItem.MajorName,
                                   SchoolClassName =classItem.SchoolClassName, //班级名称
                                   SemesterCourseNumber = subItem.SemesterCourseNumber,
                                   SemesterCourseName =subItem.SemesterCourseName,
                                   BookName = subItem.BookName,
                                   AuthorName =subItem.AuthorName,
                                   ISBN =subItem.ISBN,
                                   Press =subItem.Press,
                                   PublishingDate =subItem.PublishingDate,
                                   Price =subItem.Price,
                                   BookSubscriptionNumber =classItem.StudentNumber, //订单数量等于班级人数
                                   BookTypeName = subItem.BookTypeName,
                                   SubscriptionTypeName =subItem.BookTypeName,
                                   SubscriptionPrice  =subItem.SubscriptionPrice

    });
                        }

                    }
                    else if (subItem.SubscriptionType == SubscriptionType.教师用书)
                    {
                        db.TeacherBookOrders.Add(new TeacherBookOrder()
                        {
                           MajorID = gradeMajorItem.MajorID, BookSubscriptionID =subItem.BookSubscriptionID,
                           GenerateTime =DateTime.Now

                        });

                    } 
                   count += db.SaveChanges();
                }
            }

            return count;
        }
        

    }
}