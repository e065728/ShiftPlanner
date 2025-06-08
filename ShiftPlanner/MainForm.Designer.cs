using System;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace ShiftPlanner
{
    public partial class MainForm
    {
        /// <summary>
        /// デザイナーで使用するコンテナ
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        // --- UI コントロール定義 ---
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private DataGridView dtShift;
        private DataGridView dtMembers;
        private DataGridView dtRequests;
        private DataGridView dtRequestSummary;
        private ComboBox cmbHolidayLimit;
        private Label lblHolidayLimit;
        private Button btnAddMember;
        private Button btnRemoveMember;
        private Button btnAddRequest;
        private Button btnRemoveRequest;
        private Button btnRefreshShift;
        private DateTimePicker dtpMonth;
        private Button btnExportCsv;
        private Button btnExportPdf;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuExportCsv;
        private ToolStripMenuItem menuExportPdf;
        private ToolStripMenuItem menuMaster;
        private ToolStripMenuItem menuHolidayMaster;
        private ToolStripMenuItem menuMemberMaster;
        private ToolStripMenuItem menuSkillGroupMaster;
        private ToolStripMenuItem menuShiftTimeMaster;
        private DateTimePicker dtp分析月;
        private Label lbl総労働時間;
        private Chart chartシフト分布;

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

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dtShift = new System.Windows.Forms.DataGridView();
            this.dtMembers = new System.Windows.Forms.DataGridView();
            this.dtRequests = new System.Windows.Forms.DataGridView();
            this.cmbHolidayLimit = new System.Windows.Forms.ComboBox();
            this.lblHolidayLimit = new System.Windows.Forms.Label();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnRemoveMember = new System.Windows.Forms.Button();
            this.btnAddRequest = new System.Windows.Forms.Button();
            this.btnRemoveRequest = new System.Windows.Forms.Button();
            this.btnRefreshShift = new System.Windows.Forms.Button();
            this.dtpMonth = new System.Windows.Forms.DateTimePicker();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExportCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExportPdf = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHolidayMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMemberMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSkillGroupMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuShiftTimeMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.dtRequestSummary = new System.Windows.Forms.DataGridView();
            this.dtp分析月 = new System.Windows.Forms.DateTimePicker();
            this.lbl総労働時間 = new System.Windows.Forms.Label();
            this.chartシフト分布 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequestSummary)).BeginInit();

            this.SuspendLayout();

            // menuStrip1
            //
            // メニュー項目を追加
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuMaster});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1398, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";

            // menuFile
            //
            ToolStripMenuItem menuExportAnalysisCsv = new ToolStripMenuItem();
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuExportCsv,
            this.menuExportPdf,
            menuExportAnalysisCsv});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(55, 20);
            this.menuFile.Text = "ファイル";

            // menuExportCsv
            //
            this.menuExportCsv.Name = "menuExportCsv";
            this.menuExportCsv.Size = new System.Drawing.Size(122, 22);
            this.menuExportCsv.Text = "CSV出力";
            this.menuExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // menuExportPdf
            //
            this.menuExportPdf.Name = "menuExportPdf";
            this.menuExportPdf.Size = new System.Drawing.Size(134, 22);
            this.menuExportPdf.Text = "PDF出力";
            this.menuExportPdf.Click += new System.EventHandler(this.btnExportPdf_Click);

            // menuExportAnalysisCsv
            //
            menuExportAnalysisCsv.Name = "menuExportAnalysisCsv";
            menuExportAnalysisCsv.Size = new System.Drawing.Size(134, 22);
            menuExportAnalysisCsv.Text = "分析CSV出力";
            menuExportAnalysisCsv.Click += new System.EventHandler(this.menuExportAnalysisCsv_Click);

            // menuMaster
            //
            // サブメニュー項目を追加
            this.menuMaster.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHolidayMaster,
            this.menuMemberMaster,
            this.menuSkillGroupMaster,
            this.menuShiftTimeMaster});
            this.menuMaster.Name = "menuMaster";
            this.menuMaster.Size = new System.Drawing.Size(61, 20);
            this.menuMaster.Text = "マスター";

            // menuHolidayMaster
            //
            this.menuHolidayMaster.Name = "menuHolidayMaster";
            this.menuHolidayMaster.Size = new System.Drawing.Size(138, 22);
            this.menuHolidayMaster.Text = "祝日マスター";
            this.menuHolidayMaster.Click += new System.EventHandler(this.menuHolidayMaster_Click);

            // menuMemberMaster
            //
            this.menuMemberMaster.Name = "menuMemberMaster";
            this.menuMemberMaster.Size = new System.Drawing.Size(138, 22);
            this.menuMemberMaster.Text = "メンバーマスター";
            this.menuMemberMaster.Click += new System.EventHandler(this.menuMemberMaster_Click);

            // menuSkillGroupMaster
            //
            this.menuSkillGroupMaster.Name = "menuSkillGroupMaster";
            this.menuSkillGroupMaster.Size = new System.Drawing.Size(138, 22);
            this.menuSkillGroupMaster.Text = "スキルグループ";
            this.menuSkillGroupMaster.Click += new System.EventHandler(this.menuSkillGroupMaster_Click);

            // menuShiftTimeMaster
            //
            this.menuShiftTimeMaster.Name = "menuShiftTimeMaster";
            this.menuShiftTimeMaster.Size = new System.Drawing.Size(138, 22);
            this.menuShiftTimeMaster.Text = "勤務時間マスター";
            this.menuShiftTimeMaster.Click += new System.EventHandler(this.menuShiftTimeMaster_Click);
            //
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            // 月選択用DateTimePickerを表示するスペースを確保するため位置を下げる
            this.tabControl1.Location = new System.Drawing.Point(2, 55);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // フォーム下端に合わせるため高さを調整
            this.tabControl1.Size = new System.Drawing.Size(1393, 834);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            //
            this.tabPage1.Controls.Add(this.dtShift);
            this.tabPage1.Controls.Add(this.btnExportPdf);
            this.tabPage1.Controls.Add(this.btnExportCsv);
            this.tabPage1.Controls.Add(this.btnRefreshShift);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            // TabControl の高さを変更したため合わせて調整
            this.tabPage1.Size = new System.Drawing.Size(1385, 808);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "シフト表";
            this.tabPage1.UseVisualStyleBackColor = true;

            // dtShift
            //
            this.dtShift.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtShift.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtShift.Location = new System.Drawing.Point(3, 35);
            this.dtShift.Name = "dtShift";
            this.dtShift.RowTemplate.Height = 21;
            this.dtShift.AllowUserToAddRows = false; // 直接行を追加させない
            // TabPage 高さ変更に合わせデータグリッドの高さを調整
            this.dtShift.Size = new System.Drawing.Size(1379, 773);
            this.dtShift.TabIndex = 1;

            // btnRefreshShift
            //
            this.btnRefreshShift.Location = new System.Drawing.Point(6, 6);
            this.btnRefreshShift.Name = "btnRefreshShift";
            this.btnRefreshShift.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshShift.TabIndex = 0;
            this.btnRefreshShift.Text = "更新";
            this.btnRefreshShift.UseVisualStyleBackColor = true;
            this.btnRefreshShift.Click += new System.EventHandler(this.btnRefreshShift_Click);

            // dtpMonth
            //
            this.dtpMonth.CustomFormat = "yyyy/MM";
            this.dtpMonth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            // タブ外に表示するため位置を調整
            this.dtpMonth.Location = new System.Drawing.Point(6, 27);
            this.dtpMonth.Name = "dtpMonth";
            this.dtpMonth.ShowUpDown = true;
            this.dtpMonth.Size = new System.Drawing.Size(100, 23);
            this.dtpMonth.TabIndex = 2;
            this.dtpMonth.Value = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            this.dtpMonth.ValueChanged += new System.EventHandler(this.dtpMonth_ValueChanged);

            // btnExportCsv
            //
            // 月選択を移動したため、左に詰める
            this.btnExportCsv.Location = new System.Drawing.Point(87, 6);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(75, 23);
            this.btnExportCsv.TabIndex = 3;
            this.btnExportCsv.Text = "CSV出力";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // btnExportPdf
            //
            this.btnExportPdf.Location = new System.Drawing.Point(168, 6);
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.Size = new System.Drawing.Size(75, 23);
            this.btnExportPdf.TabIndex = 4;
            this.btnExportPdf.Text = "PDF出力";
            this.btnExportPdf.UseVisualStyleBackColor = true;
            this.btnExportPdf.Click += new System.EventHandler(this.btnExportPdf_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dtMembers);
            this.tabPage2.Controls.Add(this.btnRemoveMember);
            this.tabPage2.Controls.Add(this.btnAddMember);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            // TabControl の高さ変更に伴い調整
            this.tabPage2.Size = new System.Drawing.Size(1385, 808);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "メンバー";
            this.tabPage2.UseVisualStyleBackColor = true;

            // tabPage3
            //
            this.tabPage3.Controls.Add(this.dtRequestSummary);
            this.tabPage3.Controls.Add(this.dtRequests);
            this.tabPage3.Controls.Add(this.btnRemoveRequest);
            this.tabPage3.Controls.Add(this.btnAddRequest);
            this.tabPage3.Controls.Add(this.cmbHolidayLimit);
            this.tabPage3.Controls.Add(this.lblHolidayLimit);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            // TabControl の高さ変更に伴い調整
            this.tabPage3.Size = new System.Drawing.Size(1385, 808);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "希望";
            this.tabPage3.UseVisualStyleBackColor = true;

            // btnAddMember
            //
            this.btnAddMember.Location = new System.Drawing.Point(6, 6);
            this.btnAddMember.Name = "btnAddMember";
            this.btnAddMember.Size = new System.Drawing.Size(75, 23);
            this.btnAddMember.TabIndex = 0;
            this.btnAddMember.Text = "追加";
            this.btnAddMember.UseVisualStyleBackColor = true;
            this.btnAddMember.Click += new System.EventHandler(this.btnAddMember_Click);

            // btnRemoveMember
            //
            this.btnRemoveMember.Location = new System.Drawing.Point(87, 6);
            this.btnRemoveMember.Name = "btnRemoveMember";
            this.btnRemoveMember.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveMember.TabIndex = 1;
            this.btnRemoveMember.Text = "削除";
            this.btnRemoveMember.UseVisualStyleBackColor = true;
            this.btnRemoveMember.Click += new System.EventHandler(this.btnRemoveMember_Click);

            // dtMembers
            //
            this.dtMembers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtMembers.Location = new System.Drawing.Point(3, 35);
            this.dtMembers.Name = "dtMembers";
            this.dtMembers.RowTemplate.Height = 21;
            this.dtMembers.AllowUserToAddRows = false; // 直接行を追加させない
            // TabPage 高さ変更に合わせデータグリッドの高さを調整
            this.dtMembers.Size = new System.Drawing.Size(1379, 773);
            this.dtMembers.TabIndex = 2;

            // lblHolidayLimit
            //
            this.lblHolidayLimit.AutoSize = true;
            this.lblHolidayLimit.Location = new System.Drawing.Point(180, 11);
            this.lblHolidayLimit.Name = "lblHolidayLimit";
            this.lblHolidayLimit.Size = new System.Drawing.Size(95, 12);
            this.lblHolidayLimit.TabIndex = 5;
            this.lblHolidayLimit.Text = "休み希望上限";

            // cmbHolidayLimit
            //
            this.cmbHolidayLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHolidayLimit.FormattingEnabled = true;
            this.cmbHolidayLimit.Items.AddRange(new object[] {"1","2","3","4","5","6","7","8","9"});
            this.cmbHolidayLimit.Location = new System.Drawing.Point(281, 8);
            this.cmbHolidayLimit.Name = "cmbHolidayLimit";
            this.cmbHolidayLimit.Size = new System.Drawing.Size(60, 20);
            this.cmbHolidayLimit.TabIndex = 6;
            this.cmbHolidayLimit.SelectedIndex = 2;
            this.cmbHolidayLimit.SelectedIndexChanged += new System.EventHandler(this.CmbHolidayLimit_SelectedIndexChanged);

            // btnAddRequest
            //
            this.btnAddRequest.Location = new System.Drawing.Point(6, 6);
            this.btnAddRequest.Name = "btnAddRequest";
            this.btnAddRequest.Size = new System.Drawing.Size(75, 23);
            this.btnAddRequest.TabIndex = 0;
            this.btnAddRequest.Text = "追加";
            this.btnAddRequest.UseVisualStyleBackColor = true;
            this.btnAddRequest.Click += new System.EventHandler(this.btnAddRequest_Click);

            // btnRemoveRequest
            //
            this.btnRemoveRequest.Location = new System.Drawing.Point(87, 6);
            this.btnRemoveRequest.Name = "btnRemoveRequest";
            this.btnRemoveRequest.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveRequest.TabIndex = 1;
            this.btnRemoveRequest.Text = "削除";
            this.btnRemoveRequest.UseVisualStyleBackColor = true;
            this.btnRemoveRequest.Click += new System.EventHandler(this.btnRemoveRequest_Click);

            // dtRequests
            //
            this.dtRequests.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.dtRequests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtRequests.Location = new System.Drawing.Point(3, 35);
            this.dtRequests.Name = "dtRequests";
            this.dtRequests.RowTemplate.Height = 21;
            this.dtRequests.AllowUserToAddRows = false; // 直接行を追加させない
            this.dtRequests.Size = new System.Drawing.Size(680, 800);
            this.dtRequests.TabIndex = 2;

            // dtRequestSummary
            //
            this.dtRequestSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtRequestSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtRequestSummary.Location = new System.Drawing.Point(689, 35);
            this.dtRequestSummary.Name = "dtRequestSummary";
            this.dtRequestSummary.RowTemplate.Height = 21;
            this.dtRequestSummary.AllowUserToAddRows = false; // 直接行を追加させない
            this.dtRequestSummary.Size = new System.Drawing.Size(693, 800);
            this.dtRequestSummary.TabIndex = 7;

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            // 月選択をタブ外に配置する
            this.Controls.Add(this.dtpMonth);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequestSummary)).EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
