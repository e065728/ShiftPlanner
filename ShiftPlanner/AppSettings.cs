using System;
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

        /// <summary>
        /// メンバーごとの最低休日日数
        /// </summary>
        [DataMember]
        public int MinHolidayCount { get; set; } = 4;

        /// <summary>
        /// Excel入出力ダイアログの直近フォルダ
        /// </summary>
        [DataMember]
        public string? LastExcelFolder { get; set; } = 
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }
}
