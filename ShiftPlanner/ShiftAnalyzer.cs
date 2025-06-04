using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace ShiftPlanner
{
    /// <summary>
    /// シフト情報の集計やグラフ化を行うクラス。
    /// </summary>
    public static class ShiftAnalyzer
    {
        /// <summary>
        /// 指定した月の総労働時間を計算します。
        /// </summary>
        public static TimeSpan CalculateMonthlyHours(IEnumerable<ShiftFrame> shifts, int year, int month)
        {
            return shifts
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .Aggregate(TimeSpan.Zero, (acc, s) => acc + (s.ShiftEnd - s.ShiftStart));
        }

        /// <summary>
        /// 指定した年の総労働時間を計算します。
        /// </summary>
        public static TimeSpan CalculateAnnualHours(IEnumerable<ShiftFrame> shifts, int year)
        {
            return shifts
                .Where(s => s.Date.Year == year)
                .Aggregate(TimeSpan.Zero, (acc, s) => acc + (s.ShiftEnd - s.ShiftStart));
        }

        /// <summary>
        /// 指定した月のシフトタイプごとの件数を取得します。
        /// </summary>
        /// <param name="shifts">対象シフト一覧</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>シフトタイプと件数の辞書</returns>
        public static Dictionary<string, int> GetShiftTypeDistribution(IEnumerable<ShiftFrame> shifts, int year, int month)
        {
            if (shifts == null)
            {
                return new Dictionary<string, int>();
            }

            return shifts
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .GroupBy(s => s.ShiftType ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// シフトタイプごとの分布を円グラフとして画像出力します。
        /// </summary>
        public static void ExportDistributionChart(IEnumerable<ShiftFrame> shifts, string path)
        {
            var counts = shifts.GroupBy(s => s.ShiftType)
                               .Select(g => new { Type = g.Key, Count = g.Count() })
                               .ToList();

            using (var chart = new Chart())
            {
                chart.Size = new System.Drawing.Size(600, 400);
                chart.ChartAreas.Add(new ChartArea());
                var series = new Series
                {
                    ChartType = SeriesChartType.Pie
                };
                foreach (var item in counts)
                {
                    series.Points.AddXY(item.Type, item.Count);
                }
                chart.Series.Add(series);
                chart.SaveImage(path, ChartImageFormat.Png);
            }
        }
    }
}
