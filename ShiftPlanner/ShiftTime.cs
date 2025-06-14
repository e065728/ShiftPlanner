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

        /// <summary>表示用の色コード (HTML形式)</summary>
        [DataMember]
        public string ColorCode { get; set; } = "#FFFFFF";

        /// <summary>この勤務時間が有効かどうか</summary>
        [DataMember]
        public bool IsEnabled { get; set; } = true;
    }
}
