using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
namespace TeachingMaterial.Models
{
   
    public class TeachingMaterialDbContext : IdentityDbContext<ApplicationUser>
    {
        public TeachingMaterialDbContext()
            : base("TeachingMaterialDbContext", throwIfV1Schema: false)
        {

        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Semester>  Semesters { get; set; }

        public DbSet<Major>  Majors { get; set; }

        public DbSet<Grade> Grades { get; set; }

        public DbSet<GradeMajor> GradeMajors { get; set; }

        public DbSet<SchoolClass> SchoolClasses { get; set; }

        public DbSet<Notice> Notices { get; set; }

        public DbSet<SemesterCourse> SemesterCourses { get; set; }



        public DbSet<BookType> BookTypes { get; set; }
        public DbSet<Book> Books { get; set; }
       
        public DbSet<BookSubscription> BookSubscriptions { get; set; }

        public DbSet<SchoolClassBookOrder> SchoolClassBookOrders { get; set; }

        public DbSet<TeacherBookOrder> TeacherBookOrders { get; set; }




        public static TeachingMaterialDbContext Create()
        {
            return new TeachingMaterialDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //配置人员归属部门的一对多关系，一个用户应该属于一个部门，一个部门有多个用户。
            //已解决，使用Fulent API 的Foreign Key  显式确定外键 DepartmentID.
            modelBuilder.Entity<Department>().HasMany(d => d.ApplicationUsers).WithOptional(u =>u.Department).HasForeignKey(t =>t.DepartmentID);//一个人要属于一个部门。

           //配置部门管理人员间多对多关系，一个部门的管理员可以有很多位。
            modelBuilder.Entity<Department>().HasMany(d => d.Administrators).WithMany(a => a.Departments)
            .Map(t => t.MapLeftKey("DepartmentID")
            .MapRightKey("Id")
            .ToTable("DepartmentAdministrator"));

            base.OnModelCreating(modelBuilder);
        }
        
    }
}