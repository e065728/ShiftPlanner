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
    }
}
