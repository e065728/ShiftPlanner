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
    }
}
