using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// 出勤時間マスタ編集用フォーム
    /// </summary>
    public partial class ShiftTimeMasterForm : Form
    {
        private readonly BindingList<ShiftTime> _shiftTimes;

        public ShiftTimeMasterForm(List<ShiftTime> shiftTimes)
        {
            _shiftTimes = new BindingList<ShiftTime>(shiftTimes ?? new List<ShiftTime>());
            InitializeComponent();
            dtShiftTimes.DataSource = _shiftTimes;
            SetupGrid();
            dtShiftTimes.CellFormatting += DtShiftTimes_CellFormatting;
            dtShiftTimes.CellClick += DtShiftTimes_CellClick;
            // 編集確定とエラー表示を追加
            dtShiftTimes.CurrentCellDirtyStateChanged += DtShiftTimes_CurrentCellDirtyStateChanged;
            dtShiftTimes.DataError += DtShiftTimes_DataError;
        }

        /// <summary>
        /// 編集後の出勤時間一覧を取得します。
        /// </summary>
        public List<ShiftTime> ShiftTimes => _shiftTimes.ToList();

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            _shiftTimes.Add(new ShiftTime
            {
                Name = "新規勤務",
                Start = new TimeSpan(9, 0, 0),
                End = new TimeSpan(18, 0, 0),
                ColorCode = "#FFFFFF",
                IsEnabled = true
            });
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (dtShiftTimes.CurrentRow?.DataBoundItem is ShiftTime st)
            {
                _shiftTimes.Remove(st);
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // 編集中のセルがあれば確定する
            dtShiftTimes.EndEdit();
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SetupGrid()
        {
            dtShiftTimes.AutoGenerateColumns = true;

            // 色選択用のボタン列を追加
            var colorColumn = new DataGridViewButtonColumn
            {
                Name = nameof(ShiftTime.ColorCode),
                HeaderText = "色",
                DataPropertyName = nameof(ShiftTime.ColorCode),
                Width = 60,
                UseColumnTextForButtonValue = false
            };
            if (!dtShiftTimes.Columns.Contains(nameof(ShiftTime.ColorCode)))
            {
                dtShiftTimes.Columns.Add(colorColumn);
            }

            foreach (DataGridViewColumn col in dtShiftTimes.Columns)
            {
                if (col == null || string.IsNullOrEmpty(col.Name))
                {
                    continue;
                }

                switch (col.Name)
                {
                    case nameof(ShiftTime.Name):
                        col.HeaderText = "勤務名";
                        break;
                    case nameof(ShiftTime.Start):
                        col.HeaderText = "開始時間";
                        break;
                    case nameof(ShiftTime.End):
                        col.HeaderText = "終了時間";
                        break;
                    case nameof(ShiftTime.ColorCode):
                        col.HeaderText = "色";
                        break;
                    case nameof(ShiftTime.IsEnabled):
                        col.HeaderText = "有効";
                        break;
                }
            }

            // 列幅をフォームのサイズに合わせて調整
            DataGridViewHelper.SetColumnsNotSortable(dtShiftTimes);
            DataGridViewHelper.FitColumnsToGrid(dtShiftTimes);
        }

        private void DtShiftTimes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dtShiftTimes.Columns[e.ColumnIndex].Name == nameof(ShiftTime.ColorCode))
            {
                if (e.Value != null)
                {
                    try
                    {
                        var color = System.Drawing.ColorTranslator.FromHtml(e.Value.ToString());
                        e.CellStyle.BackColor = color;
                        e.Value = string.Empty;
                        e.FormattingApplied = true;
                    }
                    catch
                    {
                        // カラー変換失敗時は何もしない
                    }
                }
            }
        }

        private void DtShiftTimes_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dtShiftTimes.Columns[e.ColumnIndex].Name == nameof(ShiftTime.ColorCode))
            {
                if (dtShiftTimes.Rows[e.RowIndex].DataBoundItem is ShiftTime st)
                {
                    using (var dlg = new ColorDialog())
                    {
                        try
                        {
                            dlg.Color = System.Drawing.ColorTranslator.FromHtml(st.ColorCode);
                        }
                        catch
                        {
                            // 無効なカラーコードの場合は既定値
                        }

                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            st.ColorCode = System.Drawing.ColorTranslator.ToHtml(dlg.Color);
                            dtShiftTimes.InvalidateCell(e.ColumnIndex, e.RowIndex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// セル編集確定時に値をコミットします。
        /// </summary>
        private void DtShiftTimes_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dtShiftTimes.IsCurrentCellDirty)
            {
                dtShiftTimes.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// データエラー発生時に警告を表示します。
        /// </summary>
        private void DtShiftTimes_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            string message = e.Exception?.Message ?? "入力値が正しくありません。";
            MessageBox.Show($"データエラー: {message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
