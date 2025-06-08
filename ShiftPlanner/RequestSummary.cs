using System;

namespace ShiftPlanner
{
    /// <summary>
    /// メンバーごとの希望件数を表示するためのサマリ行クラス。
    /// </summary>
    public class RequestSummary
    {
        public string メンバー { get; set; } = string.Empty;
        public int 出勤希望数 { get; set; }
        public int 休希望数 { get; set; }
    }
}
