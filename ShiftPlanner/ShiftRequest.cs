
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

        /// <summary>true の場合は休み希望、false の場合は勤務希望</summary>
        [DataMember]
        public bool IsHolidayRequest { get; set; }
    }
}
