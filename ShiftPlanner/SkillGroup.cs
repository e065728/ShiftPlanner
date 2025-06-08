using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// スキルグループ情報を表します。
    /// </summary>
    [DataContract]
    public class SkillGroup
    {
        /// <summary>識別用 ID</summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>グループ名</summary>
        [DataMember]
        public string Name { get; set; } = string.Empty;
    }
}
