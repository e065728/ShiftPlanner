using System;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class ShiftRequestForm
    {
        /// <summary>
        /// デザイナーで使用するコンテナ
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        // --- UI コントロール定義 ---
        private ComboBox cmbMember;
        private DateTimePicker dtpDate;
        private ComboBox cmb種別;
        private Button btnOk;
        private Button btnCancel;

        /// <summary>
        /// 使用中のリソースをすべて解放します。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// フォームのコントロールを初期化します。
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbMember = new ComboBox();
            this.dtpDate = new DateTimePicker();
            this.cmb種別 = new ComboBox();
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

            // cmb種別
            this.cmb種別.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmb種別.Location = new System.Drawing.Point(12, 70);
            this.cmb種別.Size = new System.Drawing.Size(200, 23);

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
            this.Controls.Add(this.cmb種別);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "希望登録";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
