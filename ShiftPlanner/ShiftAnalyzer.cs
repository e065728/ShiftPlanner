using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        /// シフトタイプごとの分布をCSV形式で出力します。
        /// </summary>
        /// <param name="shifts">対象シフト一覧</param>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="path">保存先パス</param>
        /// <returns>処理結果メッセージ</returns>
        public static string ExportDistributionToCsv(IEnumerable<ShiftFrame> shifts, int year, int month, string path)
        {
            if (shifts == null)
            {
                return "シフト情報がありません。";
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                return "出力先パスが指定されていません。";
            }

            try
            {
                var counts = GetShiftTypeDistribution(shifts, year, month);
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    writer.WriteLine("ShiftType,Count");
                    foreach (var kv in counts)
                    {
                        writer.WriteLine($"{kv.Key},{kv.Value}");
                    }
                }

                return "CSV出力が完了しました。";
            }
            catch (Exception ex)
            {
                return $"CSV出力に失敗しました: {ex.Message}";
            }
        }
    }
}
