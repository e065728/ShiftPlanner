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
        private ComboBox cmbMember;
        private DateTimePicker dtpDate;
        private CheckBox chkHoliday;
        private Button btnOk;
        private Button btnCancel;
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

        private void InitializeComponent()
        {
            this.cmbMember = new ComboBox();
            this.dtpDate = new DateTimePicker();
            this.chkHoliday = new CheckBox();
            this.btnOk = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            // cmbMember
            this.cmbMember.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbMember.Location = new System.Drawing.Point(12, 12);
            this.cmbMember.Size = new System.Drawing.Size(200, 23);

            // dtpDate
            this.dtpDate.Location = new System.Drawing.Point(12, 41);
            this.dtpDate.Size = new System.Drawing.Size(200, 23);

            // chkHoliday
            this.chkHoliday.Location = new System.Drawing.Point(12, 70);
            this.chkHoliday.Text = "休み希望";

            // btnOk
            this.btnOk.Location = new System.Drawing.Point(12, 100);
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.Text = "OK";
            this.btnOk.Click += new EventHandler(this.BtnOk_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(137, 100);
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "キャンセル";
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(224, 135);
            this.Controls.Add(this.cmbMember);
            this.Controls.Add(this.dtpDate);
            this.Controls.Add(this.chkHoliday);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "希望登録";

            this.ResumeLayout(false);
        }

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
