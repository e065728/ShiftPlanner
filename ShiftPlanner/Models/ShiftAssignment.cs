using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 各日付の割り当て結果を表します。
    /// </summary>
    [DataContract]
    public class ShiftAssignment
    {
        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string ShiftType { get; set; } = string.Empty;

        [DataMember]
        public int RequiredNumber { get; set; }

        [DataMember]
        public List<Member> AssignedMembers { get; set; } = new List<Member>();

        // 割当人数不足判定
        public bool Shortage => AssignedMembers.Count < RequiredNumber;

        // 割当人数過剰判定
        public bool Excess => AssignedMembers.Count > RequiredNumber;
    }
}

