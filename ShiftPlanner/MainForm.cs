using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class MainForm : Form
    {
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DataGridView dtShift;
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();

        public MainForm()
        {
            InitializeComponent(); // これだけでOK
            InitializeData();
            SetupDataGridView();
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

            assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members);
        }

        private void SetupDataGridView()
        {
            dtShift.DataSource = assignments;
            dtShift.AutoGenerateColumns = true;

            foreach (DataGridViewRow row in dtShift.Rows)
            {
                var assignment = row.DataBoundItem as ShiftAssignment;
                if (assignment == null) continue;

                if (assignment.Shortage)
                {
                    row.DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (assignment.Excess)
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtShift = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
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
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1385, 838);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "シフト表";
            this.tabPage1.UseVisualStyleBackColor = true;

            // dtShift
            //
            this.dtShift.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtShift.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtShift.Location = new System.Drawing.Point(3, 3);
            this.dtShift.Name = "dtShift";
            this.dtShift.RowTemplate.Height = 21;
            this.dtShift.Size = new System.Drawing.Size(1379, 832);
            this.dtShift.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 74);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

