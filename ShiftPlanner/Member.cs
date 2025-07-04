
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
        public List<string> Skills { get; set; } = new List<string>();

        /// <summary>
        /// スキルグループ名。
        /// </summary>
        [DataMember]
        public string SkillGroup { get; set; } = string.Empty;

        /// <summary>
        /// 土曜日に勤務可能かどうか。
        /// </summary>
        [DataMember]
        public bool WorksOnSaturday { get; set; }

        /// <summary>
        /// 日曜日に勤務可能かどうか。
        /// </summary>
        [DataMember]
        public bool WorksOnSunday { get; set; }

        /// <summary>
        /// 出勤時間マスタで定義された勤務名のうち、このメンバーが対応可能なもの。
        /// </summary>
        [DataMember]
        public List<string> AvailableShiftNames { get; set; } = new List<string>();

        /// <summary>
        /// 希望休の日付一覧。
        /// </summary>
        [DataMember]
        public List<DateTime> DesiredHolidays { get; set; } = new List<DateTime>();

        /// <summary>
        /// 勤務時間や連続勤務上限などの制約設定。
        /// </summary>
        [DataMember]
        public ShiftConstraints Constraints { get; set; } = new ShiftConstraints();
    }
}
