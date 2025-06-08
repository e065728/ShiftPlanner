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
        // 全祝日リスト
        private readonly List<CustomHoliday> holidaySource;
        // 表示用リスト
        private readonly BindingList<CustomHoliday> filteredHolidays;

        public HolidayMasterForm(List<CustomHoliday> holidays)
        {
            // 元のリストを保持
            this.holidaySource = holidays ?? new List<CustomHoliday>();
            // 表示年でフィルタしたリストを作成
            int year = DateTime.Today.Year;
            this.filteredHolidays = new BindingList<CustomHoliday>(
                new List<CustomHoliday>(holidaySource.FindAll(h => h.Date.Year == year)));
            InitializeComponent();
            // 年の初期値を設定
            nudYear.Value = year;
            dtHolidays.DataSource = this.filteredHolidays;
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
            int year = (int)nudYear.Value;
            var newHoliday = new CustomHoliday { Date = new DateTime(year, 1, 1), Name = "新しい祝日" };
            holidaySource.Add(newHoliday);
            filteredHolidays.Add(newHoliday);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dtHolidays.CurrentRow?.DataBoundItem is CustomHoliday h)
            {
                holidaySource.Remove(h);
                filteredHolidays.Remove(h);
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

        private void NudYear_ValueChanged(object sender, EventArgs e)
        {
            UpdateFilteredHolidays();
        }

        /// <summary>
        /// 表示年を変更した際にリストを更新します。
        /// </summary>
        private void UpdateFilteredHolidays()
        {
            int year = (int)nudYear.Value;
            filteredHolidays.Clear();
            foreach (var h in holidaySource)
            {
                if (h.Date.Year == year)
                {
                    filteredHolidays.Add(h);
                }
            }
        }
    }
}
