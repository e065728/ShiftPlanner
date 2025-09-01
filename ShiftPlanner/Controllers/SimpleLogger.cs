using System;
using System.IO;

namespace ShiftPlanner
{
    /// <summary>
    /// アプリケーション向けの簡易ロガー。
    /// ファイルへメッセージを書き込みます。
    /// </summary>
    public static class SimpleLogger
    {
        private static readonly object _lock = new object();
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");

        /// <summary>
        /// 情報レベルのログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        public static void Info(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            WriteLog("INFO", message);
        }

        /// <summary>
        /// エラーレベルのログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="ex">例外情報</param>
        public static void Error(string message, Exception? ex = null)
        {
            if (string.IsNullOrEmpty(message) && ex == null)
            {
                return;
            }
            string msg = message;
            if (ex != null)
            {
                msg += $" - {ex}";
            }
            WriteLog("ERROR", msg);
        }

        /// <summary>
        /// ログ書き込み処理を内部的に行います。
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">メッセージ</param>
        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // ログ出力失敗時は何もしない
            }
        }
    }
}
