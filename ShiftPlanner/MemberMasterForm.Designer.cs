using System;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class MemberMasterForm
    {
        private System.ComponentModel.IContainer? components = null;
        private DataGridView dtMembers;
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
            this.dtMembers = new DataGridView();
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.btnOk = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // dtMembers
            this.dtMembers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.dtMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtMembers.Location = new System.Drawing.Point(12, 41);
            this.dtMembers.Name = "dtMembers";
            this.dtMembers.RowTemplate.Height = 21;
            this.dtMembers.Size = new System.Drawing.Size(560, 308);
            // 直接行を追加できないよう設定
            this.dtMembers.AllowUserToAddRows = false;

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
            this.btnOk.Location = new System.Drawing.Point(416, 355);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new EventHandler(this.BtnOk_Click);

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.Location = new System.Drawing.Point(497, 355);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Text = "キャンセル";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(584, 390);
            this.Controls.Add(this.dtMembers);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "メンバーマスター";

            this.ResumeLayout(false);
        }
    }
}
