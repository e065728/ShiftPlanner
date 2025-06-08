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
                End = new TimeSpan(18, 0, 0)
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
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SetupGrid()
        {
            dtShiftTimes.AutoGenerateColumns = true;
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
                }
            }
        }
    }
}
