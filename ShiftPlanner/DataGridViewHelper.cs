using System; // 例外およびコンソール利用のため
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// DataGridView に関する補助機能を提供するクラス
    /// </summary>
    public static class DataGridViewHelper
    {
        /// <summary>
        /// すべての列をソート不可に設定します。
        /// </summary>
        /// <param name="grid">対象の DataGridView</param>
        public static void SetColumnsNotSortable(DataGridView? grid)
        {
            if (grid?.Columns == null)
            {
                return;
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column != null)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }

        /// <summary>
        /// 列幅をヘッダー文字列が収まる程度に自動調整します。
        /// </summary>
        /// <param name="grid">対象の DataGridView</param>
        public static void AdjustColumnWidthToHeader(DataGridView? grid)
        {
            if (grid == null)
            {
                return;
            }

            // 列ごとにヘッダーを基準に自動サイズ設定
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
        }

        /// <summary>
        /// フォームの幅に合わせて列幅を自動調整します。
        /// </summary>
        /// <param name="grid">対象の DataGridView</param>
        public static void FitColumnsToGrid(DataGridView? grid)
        {
            if (grid == null)
            {
                return;
            }

            // 利用可能な領域で列幅を均等に調整
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>
        /// 指定した列幅を設定します。
        /// </summary>
        /// <param name="grid">対象の DataGridView</param>
        /// <param name="columnIndex">列インデックス</param>
        /// <param name="width">幅</param>
        public static void SetColumnWidth(DataGridView? grid, int columnIndex, int width)
        {
            if (grid?.Columns == null)
            {
                return;
            }

            if (columnIndex < 0 || columnIndex >= grid.Columns.Count)
            {
                return;
            }

            var column = grid.Columns[columnIndex];
            if (column == null)
            {
                return;
            }

            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            column.Width = width;
        }

        /// <summary>
        /// セル編集を確定させた上で DataGridView のフォーカスを外します。
        /// </summary>
        /// <param name="grid">対象の DataGridView</param>
        /// <param name="nextFocus">フォーカスを移す先のコントロール。null の場合は親コントロールにフォーカスします。</param>
        public static void セル確定してフォーカス解除(DataGridView? grid, Control? nextFocus)
        {
            if (grid == null)
            {
                return;
            }

            try
            {
                grid.EndEdit();
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);

                if (nextFocus != null)
                {
                    nextFocus.Focus();
                }
                else if (grid.Parent != null)
                {
                    grid.Parent.Focus();
                }
            }
            catch (Exception ex)
            {
                // フォーカス解除処理中の例外は致命的でないためログに出力のみ行う
                Console.WriteLine($"セル確定中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}
