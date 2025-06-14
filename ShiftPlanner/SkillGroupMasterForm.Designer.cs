using System;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class SkillGroupMasterForm
    {
        private System.ComponentModel.IContainer? components = null;
        private DataGridView dtSkillGroups = null!;
        private Button btnAdd = null!;
        private Button btnRemove = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

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
            dtSkillGroups = new DataGridView();
            btnAdd = new Button();
            btnRemove = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();

            // dtSkillGroups
            dtSkillGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dtSkillGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtSkillGroups.Location = new System.Drawing.Point(12, 41);
            dtSkillGroups.Name = "dtSkillGroups";
            dtSkillGroups.RowTemplate.Height = 21;
            dtSkillGroups.Size = new System.Drawing.Size(360, 208);
            // 直接行を追加できないよう設定
            dtSkillGroups.AllowUserToAddRows = false;

            // btnAdd
            btnAdd.Location = new System.Drawing.Point(12, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(75, 23);
            btnAdd.Text = "追加";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += new EventHandler(BtnAdd_Click);

            // btnRemove
            btnRemove.Location = new System.Drawing.Point(93, 12);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(75, 23);
            btnRemove.Text = "削除";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += new EventHandler(BtnRemove_Click);

            // btnOk
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Location = new System.Drawing.Point(216, 255);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(75, 23);
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += new EventHandler(BtnOk_Click);

            // btnCancel
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(297, 255);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += new EventHandler(BtnCancel_Click);

            // Form
            ClientSize = new System.Drawing.Size(384, 290);
            Controls.Add(dtSkillGroups);
            Controls.Add(btnAdd);
            Controls.Add(btnRemove);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            Text = "スキルグループマスター";

            ResumeLayout(false);
        }
    }
}
