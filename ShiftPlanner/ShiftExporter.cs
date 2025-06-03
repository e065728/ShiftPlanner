using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            if (shifts == null || path == null)
            {
                MessageBox.Show("出力対象データまたはパスが指定されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
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
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"ファイルにアクセスできません: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"ファイルの書き込みに失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"予期しないエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (shifts == null || path == null)
            {
                MessageBox.Show("出力対象データまたはパスが指定されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // TODO: PDF出力処理を実装する
                MessageBox.Show("PDF 出力はまだ実装されていません。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"ファイルにアクセスできません: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"ファイルの書き込みに失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"予期しないエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
