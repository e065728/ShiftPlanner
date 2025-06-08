using System;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class HolidayMasterForm
    {
        private System.ComponentModel.IContainer? components = null;
        private DataGridView dtHolidays;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnOk;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dtHolidays = new DataGridView();
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.btnOk = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // dtHolidays
            this.dtHolidays.Anchor = ((AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right));
            this.dtHolidays.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtHolidays.Location = new System.Drawing.Point(12, 41);
            this.dtHolidays.Name = "dtHolidays";
            this.dtHolidays.RowTemplate.Height = 21;
            this.dtHolidays.Size = new System.Drawing.Size(360, 208);

            // btnAdd
            this.btnAdd.Location = new System.Drawing.Point(12, 12);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.Text = "追加";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new EventHandler(this.BtnAdd_Click);

            // btnRemove
            this.btnRemove.Location = new System.Drawing.Point(93, 12);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.Text = "削除";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new EventHandler(this.BtnRemove_Click);

            // btnOk
            this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnOk.Location = new System.Drawing.Point(216, 255);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new EventHandler(this.BtnOk_Click);

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.Location = new System.Drawing.Point(297, 255);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "キャンセル";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(384, 290);
            this.Controls.Add(this.dtHolidays);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "祝日マスター";

            this.ResumeLayout(false);
        }
    }
}
