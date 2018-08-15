using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TeachingMaterial.Models
{
    //配置应用程序初始化器
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<TeachingMaterialDbContext>  //如果是采用数据库初始化器 DropCreateDatabaseIfModelChanges，不需要判断数据库中是否存在这些数据才添加，因为数据库上下文或者模型类一变，就自动卸载数据库重新创建创建数据库。
    {
        protected override void Seed(TeachingMaterialDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context)); //HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); //取得userManager ,或者使用new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context)); //HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();

            //创建部门列表
            var departments = new List<Department>
            {
                new Department{ DepartmentName="五粮液技术学院",DepartmentLocation="昌明校区"},
                new Department{ DepartmentName="现代制造工程系",DepartmentLocation="办公楼301"},
                new Department{ DepartmentName="电子信息与控制工程系",DepartmentLocation="办公楼307"},
                new Department{ DepartmentName="建筑工程系",DepartmentLocation="办公楼510"},
                new Department{ DepartmentName="生物与化学工程系",DepartmentLocation="办公楼201"},
                new Department{ DepartmentName="经济管理贸易系",DepartmentLocation="B1区经管楼"},
                new Department{ DepartmentName="人文与社会科学系",DepartmentLocation="办公楼513"},
                new Department{ DepartmentName="思想政治教学科研室",DepartmentLocation="办公楼512"},
                new Department{ DepartmentName="五年制大专教学部",DepartmentLocation="B1区101"},
                new Department{ DepartmentName="教务处",DepartmentLocation="办公楼309"},
                new Department{ DepartmentName="学生处",DepartmentLocation="B1区艺术楼"}
            };

            context.Departments.AddRange(departments); //可以一起加入;   // departments.ForEach(p =>context.Departments.Add(p)); 也可以一个一个的加入。
            context.SaveChanges();

            //创建角色列表
            var roles = new List<ApplicationRole>()
            {
                new ApplicationRole{ Name="SuperAdministrator",RoleRealName="系统管理员",Description="负责用户、角色管理，系部、专业、班级的管理，学期课程的导入，教材库导入，生成订单"},
                new ApplicationRole{ Name="DepartmentAdministrator",RoleRealName="院系征订负责人",Description="核实本部门的班级人数、学期课程，征订专业课教材，查看教材库，查看订单"},
                new ApplicationRole{ Name="PublicCourseAdministrator",RoleRealName="公共课程征订负责人",Description="征订公共课程的教材，一般由人文系的管理员担任"},
                new ApplicationRole{ Name="PoliticCourseAdministrator",RoleRealName="思政征订负责人",Description="征订思政课程的教材，一般由思政室的管理员担任"},
                new ApplicationRole{ Name="DepartmentSpecialManager",RoleRealName="院系特殊权限",Description="核实和修改本人所管理的系部班级和课程开设信息"}
            };

            roles.ForEach(r => roleManager.Create(r));


            //创建用户列表

            //1、创建管理人员
            var userAdministrator = new ApplicationUser { Email = "jwc@ybzy.cn", UserName = "administrator", RealName = "教务处", Gender = Gender.男, Birthday = DateTime.Parse("2002-07-01"), DepartmentID = departments.Single(d => d.DepartmentName == "教务处").DepartmentID, Departments = new List<Department>() }; //因为执行了Context.SaveChanges,将departments保存在数据库中，会生成主键值departmentID.同时，也将departments 变量同步到了内存中，所有此时，可以此时使用departments变量的departmentID。
            var userLiuyuanhao = new ApplicationUser { Email = "yuanhaoliu@vip.qq.com", UserName = "liuyuanhao", RealName = "刘元浩", Gender = Gender.男, Birthday = DateTime.Parse("1982-05-21"), DepartmentID = departments.Single(d => d.DepartmentName == "教务处").DepartmentID, Departments = new List<Department>() };

            userManager.Create(userAdministrator, "$$Ifkmjb9f");
            userManager.Create(userLiuyuanhao, "$$Ifkmjb9f");

            userManager.SetLockoutEnabled(userAdministrator.Id, false);
            userManager.SetLockoutEnabled(userLiuyuanhao.Id, false);

            //2、批量创建 征订人列表
            var users = new List<ApplicationUser>
            {
                new ApplicationUser{ Email="luling@qq.com",UserName="lulin",RealName="卢琳",Gender= Gender.女,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="五粮液技术学院").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="zhangdehong@qq.com",UserName="zhangdehong",RealName="张德红",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="现代制造工程系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="liaojianwen@qq.com",UserName="liaojianwen",RealName="廖建文",Gender= Gender.女,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="电子信息与控制工程系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="fengxiang@qq.com",UserName="fengxiang",RealName="冯翔",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="建筑工程系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="xianyuanhua@qq.com",UserName="xianyuanhua",RealName="先员华",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="生物与化学工程系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="molijg@qq.com",UserName="molijg",RealName="莫莉",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="经济管理贸易系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="xuqilin@qq.com",UserName="xuqilin",RealName="徐奇霖",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="人文与社会科学系").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="liweiping@qq.com",UserName="liweiping",RealName="李卫平",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="思想政治教学科研室").DepartmentID,Departments =new List<Department>()},
                new ApplicationUser{ Email="zhangjie@qq.com",UserName="zhangjie",RealName="张洁",Gender= Gender.男,Birthday=DateTime.Parse("1982-05-21"), DepartmentID=departments.Single(d =>d.DepartmentName=="五年制大专教学部").DepartmentID,Departments =new List<Department>()},

            };

            ///创建用户并设置不锁定不用户

            users.ForEach(u => userManager.Create(u, "$Ifkmjb9f"));
            users.ForEach(u => userManager.SetLockoutEnabled(u.Id, false));


            //给用户添加角色
            foreach (var user in users)
            {
                userManager.AddToRoles(user.Id, new string[] { "DepartmentAdministrator", "PublicCourseAdministrator", "PoliticCourseAdministrator" });
                user.Departments = new List<Department> { user.Department };
            }

            userManager.AddToRoles(userAdministrator.Id, new string[] { "SuperAdministrator", "DepartmentAdministrator", "PublicCourseAdministrator", "PoliticCourseAdministrator", "DepartmentSpecialManager" });
            userManager.AddToRoles(userLiuyuanhao.Id, new string[] { "SuperAdministrator", "DepartmentAdministrator", "PublicCourseAdministrator", "PoliticCourseAdministrator", "DepartmentSpecialManager" });

            context.SaveChanges();

            //设置学期

            DateTime someDate = DateTime.Parse("2017-12-30");
            var semesters = new List<Semester>
            {
                new Semester {  SemesterName ="2016-2017学年第一期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =false, SwitchOfSubscription=true},
                new Semester {  SemesterName ="2016-2017学年第二期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =false, SwitchOfSubscription=true},
                new Semester {  SemesterName ="2017-2018学年第一期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =false, SwitchOfSubscription=true},
                new Semester {  SemesterName ="2017-2018学年第二期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =true, SwitchOfSubscription=true},
                new Semester {  SemesterName ="2018-2019学年第一期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =true, SwitchOfSubscription=true},
                new Semester {  SemesterName ="2018-2019学年第二期", StartDateOfSubscription =someDate,OverDateOfSubscription = someDate.AddDays(20), IsCurrentSemester =false, SwitchOfSubscription=true},
            };

            context.Semesters.AddRange(semesters);
            context.SaveChanges();


            var majors = new List<Major>
            {
                new Major {  MajorName ="机电一体化技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="工业机器人技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="食品生物技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="酿酒技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="物流管理",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="电子商务",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },
                new Major {  MajorName ="物流工程技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五粮液技术学院").DepartmentID },

                new Major {  MajorName ="数控技术",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="精密机械技术",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="机械制造与自动化",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="模具设计与制造",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="汽车运用与维修技术",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="新能源汽车技术",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },
                new Major {  MajorName ="材料工程技术",DepartmentID =departments.Single(d =>d.DepartmentName =="现代制造工程系").DepartmentID },

                new Major {  MajorName ="电子信息工程技术",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="电气自动化技术",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="计算机网络技术",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="通信技术",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="发电厂及电力系统",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="铁道机车",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="铁道供用电",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },
                new Major {  MajorName ="数字媒体艺术设计",DepartmentID =departments.Single(d =>d.DepartmentName =="电子信息与控制工程系").DepartmentID },


                new Major {  MajorName ="建筑工程技术",DepartmentID =departments.Single(d =>d.DepartmentName =="建筑工程系").DepartmentID },
                new Major {  MajorName ="建筑装饰工程技术",DepartmentID =departments.Single(d =>d.DepartmentName =="建筑工程系").DepartmentID },


                new Major {  MajorName ="五年制数控技术",DepartmentID =departments.Single(d =>d.DepartmentName =="五年制大专教学部").DepartmentID },

            };

            context.Majors.AddRange(majors);
            context.SaveChanges();

            var grades = new List<Grade>
            {
                new Grade{  GradeName="2015级", GradeIsValidate=true },
                new Grade{  GradeName="2016级", GradeIsValidate=true },
                new Grade{  GradeName="2017级", GradeIsValidate=true },
                new Grade{  GradeName="2018级", GradeIsValidate=true }
            };

            context.Grades.AddRange(grades);
            context.SaveChanges();


            var gradeMajors = new List<GradeMajor>();

            //求年级和专业的笛卡尔积
            foreach (var gradeItem in grades)
                foreach (var majorItem in majors)
                {
                    gradeMajors.Add(new GradeMajor { GradeID = gradeItem.GradeID, MajorID = majorItem.MajorID, GradeMajorIsValidate = true });
                }
            context.GradeMajors.AddRange(gradeMajors);
            context.SaveChanges();

            var schoolClasses = new List<SchoolClass>()
            {
                new SchoolClass {  GradeMajorID =1, SchoolClassName="机电11501", StudentNumber=50 },
                new SchoolClass {  GradeMajorID = 1, SchoolClassName="机电11502",StudentNumber =42 },
                new SchoolClass {  GradeMajorID = 1, SchoolClassName="机电11503",StudentNumber =43 },
                new SchoolClass {  GradeMajorID = 2, SchoolClassName="机器人11502",StudentNumber =45 },
                new SchoolClass {  GradeMajorID = 2, SchoolClassName="机器人11503",StudentNumber =38 },
                new SchoolClass {  GradeMajorID = 24, SchoolClassName="建装11501",StudentNumber =42 },
                new SchoolClass {  GradeMajorID = 24, SchoolClassName="建装11502",StudentNumber =40 },
                new SchoolClass {  GradeMajorID = 24, SchoolClassName="建装11503",StudentNumber =42 },
                new SchoolClass {  GradeMajorID = 24, SchoolClassName="建装11504",StudentNumber =45 }
            };
            context.SchoolClasses.AddRange(schoolClasses);
            context.SaveChanges();

            var bookTypes = new List<BookType>()
            {
                new BookType {  BookTypeName="教育部规划教材"},
                new BookType {  BookTypeName="教育部精品教材"},
                new BookType {  BookTypeName="行业部委统编教材"},
                new BookType {  BookTypeName="校企合作开发教材"},
                new BookType {  BookTypeName="自编教材"},
                new BookType {  BookTypeName="讲义"},
                new BookType {  BookTypeName="其他"}

            };
            context.BookTypes.AddRange(bookTypes);
            context.SaveChanges();

            var books = new List<Book>
            {
                new Book {  BookName ="模拟电子技术", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="教育部规划教材").BookTypeID, AuthorName="张园,于宝明",ISBN ="978-7-04-047663-7" , Press="高等教育出版社", PublishingDate =DateTime.Parse("2017-9-1"), Price=38.50M },
                new Book {  BookName ="工业机器人技术基础", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="教育部规划教材").BookTypeID, AuthorName="许文稼,张飞",ISBN ="978-7-04-047675-0" , Press="高等教育出版社" ,PublishingDate =DateTime.Parse("2017-9-1"), Price=35.00M },
                new Book {  BookName ="工业机器人应用系统三维建模", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="教育部规划教材").BookTypeID, AuthorName="文清平，李勇兵",ISBN ="978-7-04-047677-4" , Press="高等教育出版社" , PublishingDate =DateTime.Parse("2017-9-1"), Price=38.00M },
                new Book {  BookName ="电工电子技术", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="教育部规划教材").BookTypeID, AuthorName="董昌春,袁冬琴",ISBN ="978-7-04-047843-3" , Press="高等教育出版社" , PublishingDate =DateTime.Parse("2017-9-1"), Price=36.00M },
                new Book {  BookName ="工厂供配电技术", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="行业部委统编教材").BookTypeID, AuthorName="李高建",ISBN ="978-7-04-048149-5" , Press="高等教育出版社" , PublishingDate =DateTime.Parse("2017-9-1"), Price=36.00M },
                new Book {  BookName ="PLC应用与实践", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="教育部规划教材").BookTypeID, AuthorName="温贻芳,李洪群,王月芹",ISBN ="978-7-04-048350-5" , Press="高等教育出版社" ,PublishingDate =DateTime.Parse("2017-9-1"), Price=38.00M },
                new Book {  BookName ="数控机床装配与调整", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="校企合作开发教材").BookTypeID, AuthorName="龚仲华",ISBN ="978-7-04-047754-2" , Press="高等教育出版社" ,PublishingDate =DateTime.Parse("2017-9-1"), Price=34.80M },
                new Book {  BookName ="施工图识读", BookTypeID =bookTypes.SingleOrDefault(x =>x.BookTypeName =="讲义").BookTypeID, AuthorName="夏玲涛，邬京虹",ISBN ="978-7-04-046783-3" , Press="高等教育出版社" , PublishingDate =DateTime.Parse("2017-9-1"), Price=39.80M }
            };
            context.Books.AddRange(books);
            context.SaveChanges();

            base.Seed(context);
        }
    }
}