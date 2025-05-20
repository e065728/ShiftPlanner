using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class MainForm : Form
    {
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DataGridView dtShift;
        private DataGridView dtMembers;
        private Button btnAddMember;
        private Button btnRemoveMember;
        private Button btnRefreshShift;
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();

        public MainForm()
        {
            InitializeComponent(); // これだけでOK
            InitializeData();
            SetupDataGridView();
            SetupMemberGrid();
        }

        private void InitializeData()
        {
            // メンバー初期データ
            members.Add(new Member
            {
                Id = 1,
                Name = "山田",
                AvailableDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday },
                AvailableFrom = TimeSpan.FromHours(9),
                AvailableTo = TimeSpan.FromHours(17)
            });
            members.Add(new Member
            {
                Id = 2,
                Name = "佐藤",
                AvailableDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
                AvailableFrom = TimeSpan.FromHours(9),
                AvailableTo = TimeSpan.FromHours(17)
            });
            members.Add(new Member
            {
                Id = 3,
                Name = "鈴木",
                AvailableDays = new List<DayOfWeek> { DayOfWeek.Tuesday },
                AvailableFrom = TimeSpan.FromHours(9),
                AvailableTo = TimeSpan.FromHours(17)
            });

            // シフトフレーム例
            shiftFrames.Add(new ShiftFrame
            {
                Date = DateTime.Today.AddDays(1),
                ShiftType = "早番",
                ShiftStart = TimeSpan.FromHours(9),
                ShiftEnd = TimeSpan.FromHours(17),
                RequiredNumber = 2
            });
            shiftFrames.Add(new ShiftFrame
            {
                Date = DateTime.Today.AddDays(2),
                ShiftType = "早番",
                ShiftStart = TimeSpan.FromHours(9),
                ShiftEnd = TimeSpan.FromHours(17),
                RequiredNumber = 1
            });
            // 例として 3 日目の遅番を追加
            shiftFrames.Add(new ShiftFrame
            {
                Date = DateTime.Today.AddDays(3),
                ShiftType = "遅番",
                ShiftStart = TimeSpan.FromHours(13),
                ShiftEnd = TimeSpan.FromHours(21),
                RequiredNumber = 2
            });

            assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members);
        }

        private void SetupDataGridView()
        {
            dtShift.DataSource = null;

            if (shiftFrames.Count == 0) return;

            var first = shiftFrames.First().Date;
            int year = first.Year;
            int month = first.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var table = new DataTable();
            table.Columns.Add("人名");
            for (int day = 1; day <= daysInMonth; day++)
            {
                table.Columns.Add($"{day}日");
            }

            foreach (var member in members)
            {
                var row = table.NewRow();
                row["人名"] = member.Name;
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var assign = assignments.FirstOrDefault(a => a.Date.Date == date && a.AssignedMembers.Contains(member));
                    row[$"{day}日"] = assign != null ? assign.ShiftType : "休";
                }
                table.Rows.Add(row);
            }

            dtShift.AutoGenerateColumns = true;
            dtShift.DataSource = table;
            dtShift.AutoResizeColumns();
        }

        private void SetupMemberGrid()
        {
            dtMembers.DataSource = null;
            dtMembers.DataSource = members;
            dtMembers.AutoGenerateColumns = true;
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            var nextId = members.Count > 0 ? members.Max(m => m.Id) + 1 : 1;
            members.Add(new Member { Id = nextId, Name = "新規" });
            SetupMemberGrid();
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            if (dtMembers.CurrentRow?.DataBoundItem is Member m)
            {
                members.Remove(m);
                SetupMemberGrid();
            }
        }

        private void btnRefreshShift_Click(object sender, EventArgs e)
        {
            assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members);
            SetupDataGridView();
        }

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtShift = new System.Windows.Forms.DataGridView();
            this.dtMembers = new System.Windows.Forms.DataGridView();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnRemoveMember = new System.Windows.Forms.Button();
            this.btnRefreshShift = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1393, 864);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            //
            this.tabPage1.Controls.Add(this.dtShift);
            this.tabPage1.Controls.Add(this.btnRefreshShift);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1385, 838);
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
            this.dtShift.Size = new System.Drawing.Size(1379, 800);
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
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dtMembers);
            this.tabPage2.Controls.Add(this.btnRemoveMember);
            this.tabPage2.Controls.Add(this.btnAddMember);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1385, 838);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "メンバー";
            this.tabPage2.UseVisualStyleBackColor = true;

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
            this.dtMembers.Size = new System.Drawing.Size(1379, 800);
            this.dtMembers.TabIndex = 2;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

