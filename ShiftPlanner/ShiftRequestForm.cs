using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// シフト希望を入力するための簡易フォーム。
    /// </summary>
    public class ShiftRequestForm : Form
    {
        // 以下のUIコントロール定義はデザイナー部に移動しました
        private readonly List<Member> members;

        public ShiftRequest? ShiftRequest { get; private set; }

        public ShiftRequestForm(List<Member> members)
        {
            this.members = members ?? new List<Member>();
            InitializeComponent();
            cmbMember.DataSource = this.members;
            cmbMember.DisplayMember = "Name";
            cmbMember.ValueMember = "Id";
        }

        // このメソッドの内容は ShiftRequestForm.Designer.cs に移動しました。

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var member = cmbMember.SelectedItem as Member;
            if (member == null)
            {
                MessageBox.Show("メンバーを選択してください。");
                return;
            }

            this.ShiftRequest = new ShiftRequest
            {
                MemberId = member.Id,
                Date = dtpDate.Value.Date,
                IsHolidayRequest = chkHoliday.Checked
            };
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
