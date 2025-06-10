using System.Runtime.Serialization;

namespace ShiftPlanner
{
    /// <summary>
    /// アプリ全体の設定を保持するクラス。
    /// </summary>
    [DataContract]
    public class AppSettings
    {
        [DataMember]
        public int HolidayLimit { get; set; } = 3;

        [DataMember]
        public int DefaultRequired { get; set; } = 1;
    }
}
