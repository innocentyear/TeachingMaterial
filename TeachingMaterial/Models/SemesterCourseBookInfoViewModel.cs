using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeachingMaterial.Models
{
    //显示一门学期课程的教材征订信息 的视图模型，包括 该门课程的学生教材征订信息和教师用书征订信息。
    public class SemesterCourseBookInfoViewModel
    {
        public IEnumerable<BookSubscription> StudentBookSubscription { get; set; }

        public IEnumerable<BookSubscription> TeacherBookSubscription { get; set; }
    }
}