using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TeachingMaterial.Models
{
    //学期，要设置是否是当前征订学期
    public class Semester
    {
        [Display(Name ="学期ID")]
        public int SemesterID { get; set; }

        [Display(Name = "学期")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0}不能全为空字符")]
        [StringLength(20, MinimumLength = 5, ErrorMessage ="{0}的字符数要大于{2},小于{1}")]
        public string SemesterName { get; set; }


        [Display(Name ="征订开始日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:yyyy-MM-dd}",ApplyFormatInEditMode =true)]
        public DateTime StartDateOfSubscription { get; set; }


        [Display(Name = "征订结束日期")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]

        public DateTime OverDateOfSubscription { get; set; }

        [Display(Name ="征订时段")]
        public string StartToOverDate
        {
            get
            {
                return StartDateOfSubscription.ToShortDateString() + "—" + OverDateOfSubscription.ToShortDateString();
            }
        }

        //征订开关字段
        [Display(Name = "征订开关")]
        private bool _SwitchOfSubscription;

        //征订开关属性
        [Display(Name ="征订开关")]
        public bool SwitchOfSubscription
        {
            get
            {
                return _SwitchOfSubscription;
            }
            set
            {
                if (DateTime.Now < StartDateOfSubscription || DateTime.Now > OverDateOfSubscription.AddDays(1)) //如果当前时间不在指定的范围内，强制关闭开关。如果在指定的范围内，可以打开开关，也可以关闭开关。
                    _SwitchOfSubscription = false;
                else
                    _SwitchOfSubscription = value;

            }
        }
        
        
        [Display(Name = "是否当前征订学期")]
        public bool IsCurrentSemester { get; set; }



        [Display(Name ="征订状态")]
        public SubscriptionState SubscriptionState
        {
            get
            {
                if(!IsCurrentSemester)
                {
                    return SubscriptionState.非当前征订学期;
                }
                if (DateTime.Now < StartDateOfSubscription)
                {
                    return SubscriptionState.尚未开始;
                }
                if (DateTime.Now > OverDateOfSubscription.AddDays(1))
                {
                    return SubscriptionState.已经结束;
                }
                if (! _SwitchOfSubscription)
                {
                    return SubscriptionState.管理员关闭;
                }

                //只有当 当前项目有效，并且当前时间在指定的范围内，开关打开时，状态才显示正在进行。
                return SubscriptionState.正在进行;
            }
        }



        [Display(Name = "开设的学期课程")]
        public virtual ICollection<SemesterCourse> SemesterCourses { get; set; }



    }


    //申报或评审状态
    public enum SubscriptionState
    {
        非当前征订学期, 尚未开始, 正在进行, 已经结束, 管理员关闭,

    }




}