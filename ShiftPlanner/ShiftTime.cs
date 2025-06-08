using System;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 出勤時間マスタの1行を表します。
    /// </summary>
    [DataContract]
    public class ShiftTime
    {
        /// <summary>勤務名</summary>
        [DataMember]
        public string Name { get; set; } = string.Empty;

        /// <summary>開始時間</summary>
        [DataMember]
        public TimeSpan Start { get; set; }

        /// <summary>終了時間</summary>
        [DataMember]
        public TimeSpan End { get; set; }
    }
}
