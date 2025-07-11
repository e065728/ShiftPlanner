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
        public int 希望休数 { get; set; }
        public int 有休数 { get; set; }
        public int 健康診断数 { get; set; }
    }
}
