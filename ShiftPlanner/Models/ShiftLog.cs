using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ShiftPlanner
{
    /// <summary>
    /// JSON Lines 形式でログを出力するクラス。
    /// </summary>
    public static class ShiftLog
    {
        private static readonly object _lock = new object();
        private static readonly string logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner");
        private static readonly string logFilePath = Path.Combine(logDir, "shift.log");

        static ShiftLog()
        {
            try
            {
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
            }
            catch
            {
                // ディレクトリ作成に失敗しても例外は無視
            }
        }

        /// <summary>
        /// 情報レベルのメッセージを出力します。
        /// </summary>
        /// <param name="message">出力する内容</param>
        public static void WriteInfo(string message)
        {
            Write("INFO", message);
        }

        /// <summary>
        /// エラーレベルのメッセージを出力します。
        /// </summary>
        /// <param name="message">出力する内容</param>
        public static void WriteError(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            try
            {
                var entry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };

                var serializer = new DataContractJsonSerializer(typeof(LogEntry));
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, entry);
                    string line = Encoding.UTF8.GetString(ms.ToArray());
                    lock (_lock)
                    {
                        File.AppendAllText(logFilePath, line + Environment.NewLine, Encoding.UTF8);
                    }
                }
            }
            catch
            {
                // ログ出力失敗時は何もしない
            }
        }

        [DataContract]
        private class LogEntry
        {
            [DataMember(Name = "time")]
            public DateTime Timestamp { get; set; }

            [DataMember(Name = "level")]
            public string Level { get; set; } = string.Empty;

            [DataMember(Name = "message")]
            public string Message { get; set; } = string.Empty;
        }
    }
}
