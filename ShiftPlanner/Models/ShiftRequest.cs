
using System;
using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 従業員のシフト希望を表すクラス。
    /// </summary>
    [DataContract]
    public class ShiftRequest
    {
        /// <summary>対象メンバーID</summary>
        [DataMember]
        public int MemberId { get; set; }

        /// <summary>希望日</summary>
        [DataMember]
        public DateTime Date { get; set; }

        /// <summary>個別日程調整の種別</summary>
        [DataMember]
        public 申請種別 種別 { get; set; } = 申請種別.勤務希望;
    }
}
