using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNU.Core.Models.ContentModels {

    public class ScheduleItem {
        public string Title { get; set; }
        public string Lecturer { get; set; }
        public string Description { get; set; }
        public string CourseID { get; set; }
        public string SerialNumber { get; set; }
        public string CourceProperty { get; set; }
        public string ExamType { get; set; }
        public string Place { get; set; }
        public string Time { get; set; }
        public string WeeklyRound { get; set; }
    }

    public class ScheduleTip {
        public string WholeTitle { get; set; }
        public int? Row;
        public int? Column;
    }
}
