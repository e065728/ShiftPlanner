using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShiftPlanner
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // グローバル例外ハンドラの登録
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// UI スレッドで処理されなかった例外を扱います。
        /// </summary>
        private static void OnThreadException(object? sender, ThreadExceptionEventArgs e)
        {
            if (e?.Exception != null)
            {
                HandleException(e.Exception);
            }
        }

        /// <summary>
        /// 非 UI スレッドで発生した未処理例外を扱います。
        /// </summary>
        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            var ex = e?.ExceptionObject as Exception;
            if (ex != null)
            {
                HandleException(ex);
            }
            else
            {
                SimpleLogger.Error("Unhandled exception without Exception instance.");
            }
        }

        /// <summary>
        /// 例外をログへ記録し、ユーザーへ通知します。
        /// </summary>
        private static void HandleException(Exception ex)
        {
            SimpleLogger.Error("Unhandled exception", ex);
            var result = MessageBox.Show(
                $"予期しないエラーが発生しました:\n{ex.Message}\nアプリケーションを終了しますか?",
                "エラー",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
