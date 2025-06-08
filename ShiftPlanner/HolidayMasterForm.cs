using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// 祝日マスター編集用フォーム
    /// </summary>
    public partial class HolidayMasterForm : Form
    {
        private readonly BindingList<CustomHoliday> holidays;

        public HolidayMasterForm(List<CustomHoliday> holidays)
        {
            this.holidays = new BindingList<CustomHoliday>(holidays ?? new List<CustomHoliday>());
            InitializeComponent();
            dtHolidays.DataSource = this.holidays;
            dtHolidays.AutoGenerateColumns = true;

            // ヘッダー文字列を日本語化
            foreach (DataGridViewColumn col in dtHolidays.Columns)
            {
                if (col.Name == nameof(CustomHoliday.Date))
                {
                    col.HeaderText = "日付";
                }
                else if (col.Name == nameof(CustomHoliday.Name))
                {
                    col.HeaderText = "名称";
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            holidays.Add(new CustomHoliday { Date = DateTime.Today, Name = "新しい祝日" });
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dtHolidays.CurrentRow?.DataBoundItem is CustomHoliday h)
            {
                holidays.Remove(h);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
