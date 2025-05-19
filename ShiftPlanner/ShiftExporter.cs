using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShiftPlanner
{
    /// <summary>
    /// シフト情報を外部ファイルへ出力するクラス。
    /// </summary>
    public static class ShiftExporter
    {
        /// <summary>
        /// シフト情報をCSV形式で出力します。
        /// </summary>
        /// <param name="shifts">出力対象のシフト一覧</param>
        /// <param name="path">保存先パス</param>
        public static void ExportToCsv(IEnumerable<ShiftFrame> shifts, string path)
        {
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.WriteLine("Date,ShiftType,Start,End,RequiredNumber");
                foreach (var s in shifts)
                {
                    writer.WriteLine($"{s.Date:yyyy-MM-dd},{s.ShiftType},{s.ShiftStart},{s.ShiftEnd},{s.RequiredNumber}");
                }
            }
        }

        /// <summary>
        /// シフト情報をPDF形式で出力します。
        /// 現在は未実装です。
        /// </summary>
        /// <param name="shifts">出力対象のシフト一覧</param>
        /// <param name="path">保存先パス</param>
        public static void ExportToPdf(IEnumerable<ShiftFrame> shifts, string path)
        {
            // TODO: PDF出力処理を実装する
            throw new NotImplementedException();
        }
    }
}
