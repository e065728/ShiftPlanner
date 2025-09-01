using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
        /// <returns>処理結果メッセージ</returns>
        public static string ExportToCsv(IEnumerable<ShiftFrame> shifts, string path)
        {
            // 入力チェック
            if (shifts == null || !shifts.Any())
            {
                SimpleLogger.Info("[CsvExport] シフト情報がないため処理を中止しました。");
                return "シフト情報がありません。";
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                SimpleLogger.Error("[CsvExport] 出力先パスが指定されていません。");
                return "出力先パスが指定されていません。";
            }

            try
            {
                SimpleLogger.Info($"[CsvExport] 出力開始: {path}");

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var writer = new StreamWriter(path, false, Encoding.UTF8))
                {
                    // CSVヘッダ
                    writer.WriteLine("Date,ShiftType,Start,End,RequiredNumber");
                    foreach (var s in shifts)
                    {
                        writer.WriteLine($"{s.Date:yyyy-MM-dd},{s.ShiftType},{s.ShiftStart},{s.ShiftEnd},{s.RequiredNumber}");
                    }
                }

                SimpleLogger.Info("[CsvExport] 出力完了");
                return "CSV出力が完了しました。";
            }
            catch (Exception ex)
            {
                SimpleLogger.Error("[CsvExport] 出力中にエラーが発生しました", ex);
                return $"CSV出力に失敗しました: {ex.Message}";
            }
        }

        /// <summary>
        /// シフト情報をPDF形式で出力します。
        /// </summary>
        /// <param name="shifts">出力対象のシフト一覧</param>
        /// <param name="path">保存先パス</param>
        /// <returns>処理結果メッセージ</returns>
        public static string ExportToPdf(IEnumerable<ShiftFrame> shifts, string path)
        {
            if (shifts == null) return "シフト情報がありません。";
            if (string.IsNullOrWhiteSpace(path)) return "出力先パスが指定されていません。";

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var document = new Document(PageSize.A4))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    // ヘッダ行を含む5列のテーブルを作成
                    var table = new PdfPTable(5);
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    table.AddCell(new Phrase("Date", headerFont));
                    table.AddCell(new Phrase("ShiftType", headerFont));
                    table.AddCell(new Phrase("Start", headerFont));
                    table.AddCell(new Phrase("End", headerFont));
                    table.AddCell(new Phrase("RequiredNumber", headerFont));

                    foreach (var s in shifts)
                    {
                        table.AddCell(new Phrase(s.Date.ToString("yyyy-MM-dd"), cellFont));
                        table.AddCell(new Phrase(s.ShiftType ?? string.Empty, cellFont));
                        table.AddCell(new Phrase(s.ShiftStart.ToString(), cellFont));
                        table.AddCell(new Phrase(s.ShiftEnd.ToString(), cellFont));
                        table.AddCell(new Phrase(s.RequiredNumber.ToString(), cellFont));
                    }

                    document.Add(table);
                    document.Close();
                }

                return "PDF出力が完了しました。";
            }
            catch (Exception ex)
            {
                return $"PDF出力に失敗しました: {ex.Message}";
            }
        }
    }
}
