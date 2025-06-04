
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 従業員情報を表します。
    /// </summary>
    [DataContract]
    public class Member
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public List<DayOfWeek> AvailableDays { get; set; } = new List<DayOfWeek>();

        [DataMember]
        public TimeSpan AvailableFrom { get; set; }

        [DataMember]
        public TimeSpan AvailableTo { get; set; }

        [DataMember]
        public List<string> Skills { get; set; } = new List<string>();

        /// <summary>
        /// 希望休の日付一覧。
        /// </summary>
        // JSON 保存用。日付文字列の配列として保存する
        [DataMember(Name = "DesiredHolidays")]
        private List<string> DesiredHolidayStrings { get; set; } = new List<string>();

        /// <summary>
        /// 希望休の日付一覧
        /// </summary>
        [IgnoreDataMember]
        public List<DateTime> DesiredHolidays { get; set; } = new List<DateTime>();

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            DesiredHolidayStrings = new List<string>();
            if (DesiredHolidays != null)
            {
                foreach (var d in DesiredHolidays)
                {
                    DesiredHolidayStrings.Add(d.ToString("yyyy-MM-dd"));
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            DesiredHolidays = new List<DateTime>();
            if (DesiredHolidayStrings != null)
            {
                foreach (var s in DesiredHolidayStrings)
                {
                    if (DateTime.TryParse(s, out var dt))
                    {
                        DesiredHolidays.Add(dt.Date);
                    }
                }
            }
        }

        /// <summary>
        /// 勤務時間や連続勤務上限などの制約設定。
        /// </summary>
        [DataMember]
        public ShiftConstraints Constraints { get; set; } = new ShiftConstraints();
    }
}
