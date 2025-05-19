
using System;
using System.Collections.Generic;

namespace ShiftPlanner
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DayOfWeek> AvailableDays { get; set; }
        public TimeSpan AvailableFrom { get; set; }
        public TimeSpan AvailableTo { get; set; }
        public List<string> Skills { get; set; }

        /// <summary>
        /// 希望休の日付一覧。
        /// </summary>
        public List<DateTime> DesiredHolidays { get; set; }

        /// <summary>
        /// 勤務時間や連続勤務上限などの制約設定。
        /// </summary>
        public ShiftConstraints Constraints { get; set; }
    }
}
