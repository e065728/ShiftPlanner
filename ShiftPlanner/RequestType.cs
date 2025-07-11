using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// 個別日程調整の種別を表す列挙。
    /// </summary>
    [DataContract]
    public enum 申請種別
    {
        [EnumMember]
        勤務希望 = 0,
        [EnumMember]
        希望休 = 1,
        [EnumMember]
        有休 = 2,
        [EnumMember]
        健康診断 = 3
    }
}
