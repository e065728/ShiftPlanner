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
        private TabPage tabPage3;
        private DataGridView dtRequests;
        private DataGridView dtRequestSummary;
        private ComboBox cmbHolidayLimit;
        private Label lblHolidayLimit;
        private ComboBox cmbDefaultRequired;
        private Label lblDefaultRequired;
        private ComboBox cmbMinHolidayCount;
        private Label lblMinHolidayCount;
        private Button btnAggregate;
        private Button btnAddRequest;
        private Button btnRemoveRequest;
        private DateTimePicker dtp分析月;
        private Label lbl総労働時間;
        private Chart chartシフト分布;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuMaster;
        private ToolStripMenuItem menuHolidayMaster;
        private ToolStripMenuItem menuMemberMaster;
        private ToolStripMenuItem menuSkillGroupMaster;
        private ToolStripMenuItem menuShiftTimeMaster;
        private ToolStripMenuItem menuExportAnalysisCsv;

        private DateTimePicker dtp対象月;
        private Button btn月更新;
        private TabPage tabShift;
        private DataGridView dtShifts;
        private Button btnShiftGenerate;

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
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dtRequestSummary = new System.Windows.Forms.DataGridView();
            this.dtRequests = new System.Windows.Forms.DataGridView();
            this.btnRemoveRequest = new System.Windows.Forms.Button();
            this.btnAddRequest = new System.Windows.Forms.Button();
            this.cmbHolidayLimit = new System.Windows.Forms.ComboBox();
            this.lblHolidayLimit = new System.Windows.Forms.Label();
            this.cmbDefaultRequired = new System.Windows.Forms.ComboBox();
            this.lblDefaultRequired = new System.Windows.Forms.Label();
            this.cmbMinHolidayCount = new System.Windows.Forms.ComboBox();
            this.lblMinHolidayCount = new System.Windows.Forms.Label();
            this.btnAggregate = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExportAnalysisCsv = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHolidayMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMemberMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSkillGroupMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.menuShiftTimeMaster = new System.Windows.Forms.ToolStripMenuItem();
            this.dtp分析月 = new System.Windows.Forms.DateTimePicker();
            this.lbl総労働時間 = new System.Windows.Forms.Label();
            this.chartシフト分布 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabShift = new System.Windows.Forms.TabPage();
            this.dtShifts = new System.Windows.Forms.DataGridView();
            this.btnShiftGenerate = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabShift.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequestSummary)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartシフト分布)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtShifts)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabShift);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(2, 55);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1393, 834);
            // フォームサイズ変更時にタブコントロールもリサイズされるようアンカーを設定
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.TabIndex = 0;
            //
            // tabShift
            //
            this.tabShift.Controls.Add(this.dtShifts);
            this.tabShift.Controls.Add(this.cmbMinHolidayCount);
            this.tabShift.Controls.Add(this.lblMinHolidayCount);
            this.tabShift.Controls.Add(this.cmbDefaultRequired);
            this.tabShift.Controls.Add(this.lblDefaultRequired);
            this.tabShift.Controls.Add(this.btnAggregate);
            this.tabShift.Controls.Add(this.btnShiftGenerate);
            this.tabShift.Location = new System.Drawing.Point(4, 22);
            this.tabShift.Name = "tabShift";
            this.tabShift.Padding = new System.Windows.Forms.Padding(3);
            this.tabShift.Size = new System.Drawing.Size(1385, 808);
            this.tabShift.TabIndex = 0;
            this.tabShift.Text = "シフト表";
            this.tabShift.UseVisualStyleBackColor = true;
            //
            // dtShifts
            //
            this.dtShifts.AllowUserToAddRows = false;
            // グリッドサイズは固定とするためアンカーをTopとLeftのみに設定
            this.dtShifts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.dtShifts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtShifts.Location = new System.Drawing.Point(6, 38);
            this.dtShifts.Name = "dtShifts";
            this.dtShifts.RowTemplate.Height = 21;
            this.dtShifts.Size = new System.Drawing.Size(1373, 764);
            // 非表示部分はスクロールして閲覧できるようにする
            this.dtShifts.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.dtShifts.TabIndex = 1;
            //
            // cmbDefaultRequired
            //
            this.cmbDefaultRequired.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDefaultRequired.FormattingEnabled = true;
            this.cmbDefaultRequired.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbDefaultRequired.Location = new System.Drawing.Point(200, 8);
            this.cmbDefaultRequired.Name = "cmbDefaultRequired";
            this.cmbDefaultRequired.Size = new System.Drawing.Size(60, 20);
            this.cmbDefaultRequired.TabIndex = 3;
            this.cmbDefaultRequired.SelectedIndexChanged += new System.EventHandler(this.CmbDefaultRequired_SelectedIndexChanged);
            //
            // cmbMinHolidayCount
            //
            this.cmbMinHolidayCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMinHolidayCount.FormattingEnabled = true;
            this.cmbMinHolidayCount.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.cmbMinHolidayCount.Location = new System.Drawing.Point(360, 8);
            this.cmbMinHolidayCount.Name = "cmbMinHolidayCount";
            this.cmbMinHolidayCount.Size = new System.Drawing.Size(60, 20);
            this.cmbMinHolidayCount.TabIndex = 5;
            this.cmbMinHolidayCount.SelectedIndexChanged += new System.EventHandler(this.CmbMinHolidayCount_SelectedIndexChanged);
            //
            // btnAggregate
            //
            this.btnAggregate.Location = new System.Drawing.Point(426, 6);
            this.btnAggregate.Name = "btnAggregate";
            this.btnAggregate.Size = new System.Drawing.Size(75, 23);
            this.btnAggregate.TabIndex = 6;
            this.btnAggregate.Text = "集計";
            this.btnAggregate.UseVisualStyleBackColor = true;
            // Anchorを指定して常に左上に表示されるようにする
            this.btnAggregate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAggregate.Click += new System.EventHandler(this.Btn集計_Click);
            //
            // lblDefaultRequired
            //
            this.lblDefaultRequired.AutoSize = true;
            this.lblDefaultRequired.Location = new System.Drawing.Point(110, 11);
            this.lblDefaultRequired.Name = "lblDefaultRequired";
            this.lblDefaultRequired.Size = new System.Drawing.Size(84, 12);
            this.lblDefaultRequired.TabIndex = 2;
            this.lblDefaultRequired.Text = "必要人数デフォルト";
            //
            // lblMinHolidayCount
            //
            this.lblMinHolidayCount.AutoSize = true;
            this.lblMinHolidayCount.Location = new System.Drawing.Point(266, 11);
            this.lblMinHolidayCount.Name = "lblMinHolidayCount";
            this.lblMinHolidayCount.Size = new System.Drawing.Size(87, 12);
            this.lblMinHolidayCount.TabIndex = 4;
            this.lblMinHolidayCount.Text = "最低休日日数";
            //
            // btnShiftGenerate
            //
            this.btnShiftGenerate.Location = new System.Drawing.Point(6, 6);
            this.btnShiftGenerate.Name = "btnShiftGenerate";
            this.btnShiftGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnShiftGenerate.TabIndex = 0;
            this.btnShiftGenerate.Text = "更新";
            this.btnShiftGenerate.UseVisualStyleBackColor = true;
            this.btnShiftGenerate.Click += new System.EventHandler(this.BtnShiftGenerate_Click);
            //
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
            this.tabPage3.Size = new System.Drawing.Size(1385, 808);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "希望";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dtRequestSummary
            // 
            this.dtRequestSummary.AllowUserToAddRows = false;
            // フォームリサイズ時に幅も変わるよう左右のアンカーを設定
            this.dtRequestSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            this.dtRequestSummary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            // 右側に表示できるよう座標を調整
            this.dtRequestSummary.Location = new System.Drawing.Point(692, 38);
            this.dtRequestSummary.Name = "dtRequestSummary";
            this.dtRequestSummary.RowTemplate.Height = 21;
            this.dtRequestSummary.Size = new System.Drawing.Size(680, 764);
            this.dtRequestSummary.TabIndex = 7;
            // 
            // dtRequests
            // 
            this.dtRequests.AllowUserToAddRows = false;
            // 左右双方のアンカーを設定し、フォームリサイズ時に幅を合わせる
            this.dtRequests.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            this.dtRequests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtRequests.Location = new System.Drawing.Point(6, 38);
            this.dtRequests.Name = "dtRequests";
            this.dtRequests.RowTemplate.Height = 21;
            this.dtRequests.Size = new System.Drawing.Size(680, 764);
            this.dtRequests.TabIndex = 2;
            // 
            // btnRemoveRequest
            // 
            this.btnRemoveRequest.Location = new System.Drawing.Point(87, 6);
            this.btnRemoveRequest.Name = "btnRemoveRequest";
            this.btnRemoveRequest.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveRequest.TabIndex = 1;
            this.btnRemoveRequest.Text = "削除";
            this.btnRemoveRequest.UseVisualStyleBackColor = true;
            this.btnRemoveRequest.Click += new System.EventHandler(this.btnRemoveRequest_Click);
            // 
            // btnAddRequest
            // 
            this.btnAddRequest.Location = new System.Drawing.Point(6, 6);
            this.btnAddRequest.Name = "btnAddRequest";
            this.btnAddRequest.Size = new System.Drawing.Size(75, 23);
            this.btnAddRequest.TabIndex = 0;
            this.btnAddRequest.Text = "追加";
            this.btnAddRequest.UseVisualStyleBackColor = true;
            this.btnAddRequest.Click += new System.EventHandler(this.btnAddRequest_Click);
            // 
            // cmbHolidayLimit
            // 
            this.cmbHolidayLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHolidayLimit.FormattingEnabled = true;
            this.cmbHolidayLimit.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.cmbHolidayLimit.Location = new System.Drawing.Point(281, 8);
            this.cmbHolidayLimit.Name = "cmbHolidayLimit";
            this.cmbHolidayLimit.Size = new System.Drawing.Size(60, 20);
            this.cmbHolidayLimit.TabIndex = 6;
            this.cmbHolidayLimit.SelectedIndexChanged += new System.EventHandler(this.CmbHolidayLimit_SelectedIndexChanged);
            // 
            // lblHolidayLimit
            // 
            this.lblHolidayLimit.AutoSize = true;
            this.lblHolidayLimit.Location = new System.Drawing.Point(183, 14);
            this.lblHolidayLimit.Name = "lblHolidayLimit";
            this.lblHolidayLimit.Size = new System.Drawing.Size(76, 12);
            this.lblHolidayLimit.TabIndex = 5;
            this.lblHolidayLimit.Text = "休み希望上限";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuMaster});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1398, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuExportAnalysisCsv});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(53, 20);
            this.menuFile.Text = "ファイル";
            // 
            // menuExportAnalysisCsv
            // 
            this.menuExportAnalysisCsv.Name = "menuExportAnalysisCsv";
            this.menuExportAnalysisCsv.Size = new System.Drawing.Size(142, 22);
            this.menuExportAnalysisCsv.Text = "分析CSV出力";
            this.menuExportAnalysisCsv.Click += new System.EventHandler(this.menuExportAnalysisCsv_Click);
            // 
            // menuMaster
            // 
            this.menuMaster.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHolidayMaster,
            this.menuMemberMaster,
            this.menuSkillGroupMaster,
            this.menuShiftTimeMaster});
            this.menuMaster.Name = "menuMaster";
            this.menuMaster.Size = new System.Drawing.Size(54, 20);
            this.menuMaster.Text = "マスター";
            // 
            // menuHolidayMaster
            // 
            this.menuHolidayMaster.Name = "menuHolidayMaster";
            this.menuHolidayMaster.Size = new System.Drawing.Size(157, 22);
            this.menuHolidayMaster.Text = "祝日マスター";
            this.menuHolidayMaster.Click += new System.EventHandler(this.menuHolidayMaster_Click);
            // 
            // menuMemberMaster
            // 
            this.menuMemberMaster.Name = "menuMemberMaster";
            this.menuMemberMaster.Size = new System.Drawing.Size(157, 22);
            this.menuMemberMaster.Text = "メンバーマスター";
            this.menuMemberMaster.Click += new System.EventHandler(this.menuMemberMaster_Click);
            // 
            // menuSkillGroupMaster
            // 
            this.menuSkillGroupMaster.Name = "menuSkillGroupMaster";
            this.menuSkillGroupMaster.Size = new System.Drawing.Size(157, 22);
            this.menuSkillGroupMaster.Text = "スキルグループ";
            this.menuSkillGroupMaster.Click += new System.EventHandler(this.menuSkillGroupMaster_Click);
            // 
            // menuShiftTimeMaster
            // 
            this.menuShiftTimeMaster.Name = "menuShiftTimeMaster";
            this.menuShiftTimeMaster.Size = new System.Drawing.Size(157, 22);
            this.menuShiftTimeMaster.Text = "勤務時間マスター";
            this.menuShiftTimeMaster.Click += new System.EventHandler(this.menuShiftTimeMaster_Click);
            //
            // dtp対象月
            //
            this.dtp対象月 = new DateTimePicker();
            this.dtp対象月.Location = new System.Drawing.Point(12, 27);
            this.dtp対象月.Size = new System.Drawing.Size(120, 19);
            this.dtp対象月.Format = DateTimePickerFormat.Custom;
            this.dtp対象月.CustomFormat = "yyyy/MM";
            this.dtp対象月.ShowUpDown = true;
            //
            // btn月更新
            //
            this.btn月更新 = new Button();
            this.btn月更新.Location = new System.Drawing.Point(138, 25);
            this.btn月更新.Size = new System.Drawing.Size(75, 23);
            this.btn月更新.Text = "月更新";
            this.btn月更新.UseVisualStyleBackColor = true;
            this.btn月更新.Click += new System.EventHandler(this.Btn月更新_Click);
            //
            // dtp分析月
            //
            this.dtp分析月.Location = new System.Drawing.Point(0, 0);
            this.dtp分析月.Name = "dtp分析月";
            this.dtp分析月.Size = new System.Drawing.Size(200, 19);
            this.dtp分析月.TabIndex = 0;
            // 
            // lbl総労働時間
            // 
            this.lbl総労働時間.Location = new System.Drawing.Point(0, 0);
            this.lbl総労働時間.Name = "lbl総労働時間";
            this.lbl総労働時間.Size = new System.Drawing.Size(100, 23);
            this.lbl総労働時間.TabIndex = 0;
            // 
            // chartシフト分布
            // 
            this.chartシフト分布.Location = new System.Drawing.Point(0, 0);
            this.chartシフト分布.Name = "chartシフト分布";
            this.chartシフト分布.Size = new System.Drawing.Size(300, 300);
            this.chartシフト分布.TabIndex = 0;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btn月更新);
            this.Controls.Add(this.dtp対象月);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            this.tabShift.ResumeLayout(false);
            this.tabShift.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequestSummary)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtShifts)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartシフト分布)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}
