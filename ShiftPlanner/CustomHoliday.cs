using System;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// ユーザーが追加する祝日情報を表します。
    /// </summary>
    [DataContract]
    public class CustomHoliday
    {
        /// <summary>祝日の日付</summary>
        [DataMember]
        public DateTime Date { get; set; }

        /// <summary>祝日の名称</summary>
        [DataMember]
        public string Name { get; set; } = string.Empty;
    }
}
