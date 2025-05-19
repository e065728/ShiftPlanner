using System;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public class RequestForm : Form
    {
        public ShiftRequest Request { get; private set; }

        private TextBox txtMemberId;
        private DateTimePicker dtDate;
        private CheckBox chkHoliday;
        private Button btnOk;

        public RequestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.txtMemberId = new TextBox();
            this.dtDate = new DateTimePicker();
            this.chkHoliday = new CheckBox();
            this.btnOk = new Button();
            this.SuspendLayout();
            // txtMemberId
            this.txtMemberId.Location = new System.Drawing.Point(12, 12);
            this.txtMemberId.Name = "txtMemberId";
            this.txtMemberId.Size = new System.Drawing.Size(100, 19);
            // dtDate
            this.dtDate.Location = new System.Drawing.Point(12, 37);
            this.dtDate.Name = "dtDate";
            this.dtDate.Size = new System.Drawing.Size(200, 19);
            // chkHoliday
            this.chkHoliday.Location = new System.Drawing.Point(12, 62);
            this.chkHoliday.Name = "chkHoliday";
            this.chkHoliday.Text = "休日希望";
            // btnOk
            this.btnOk.Location = new System.Drawing.Point(12, 87);
            this.btnOk.Text = "送信";
            this.btnOk.Click += new EventHandler(this.btnOk_Click);
            // Form settings
            this.ClientSize = new System.Drawing.Size(224, 121);
            this.Controls.Add(this.txtMemberId);
            this.Controls.Add(this.dtDate);
            this.Controls.Add(this.chkHoliday);
            this.Controls.Add(this.btnOk);
            this.Name = "RequestForm";
            this.Text = "希望申請";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtMemberId.Text, out int memberId))
            {
                this.Request = new ShiftRequest
                {
                    MemberId = memberId,
                    Date = dtDate.Value.Date,
                    IsHolidayRequest = chkHoliday.Checked
                };
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("従業員IDを正しく入力してください。");
            }
        }
    }
}

