
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 1つのシフト枠を表します。
    /// </summary>
    [DataContract]
    public class ShiftFrame
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string ShiftType { get; set; } = string.Empty;

        [DataMember]
        public TimeSpan ShiftStart { get; set; }

        [DataMember]
        public TimeSpan ShiftEnd { get; set; }

        [DataMember]
        public int RequiredNumber { get; set; }

        /// <summary>
        /// このシフトに必要なスキル一覧。
        /// 空の場合はスキル条件無し。
        /// </summary>
        [DataMember]
        public List<string> RequiredSkills { get; set; } = new List<string>();
    }
}
