using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

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
        private DateTimePicker dtpMonth;
        // メンバー情報保存用のファイルパス
        // %APPDATA%/ShiftPlanner/members.json の形で保存する
        private readonly string memberFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner",
            "members.json");
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();

        public MainForm()
        {
            InitializeComponent(); // これだけでOK

            // データ保存用ディレクトリが無い場合は作成する
            var dir = Path.GetDirectoryName(memberFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"データ保存用ディレクトリの作成に失敗しました: {ex.Message}");
                }
            }

            InitializeData();
            SetupDataGridView();
            SetupMemberGrid();
        }

        private void LoadMembers()
        {
            if (File.Exists(memberFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<Member>));
                    using (var stream = File.OpenRead(memberFilePath))
                    {
                        members = (List<Member>)serializer.ReadObject(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"メンバー情報の読み込みに失敗しました: {ex.Message}");
                    members = new List<Member>();
                }
            }
        }

        private void SaveMembers()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<Member>));
                using (var stream = File.Create(memberFilePath))
                {
                    serializer.WriteObject(stream, members);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"メンバー情報の保存に失敗しました: {ex.Message}");
            }
        }

        private void InitializeData()
        {
            LoadMembers();

            if (members.Count == 0)
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
            }

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

            int year = dtpMonth.Value.Year;
            int month = dtpMonth.Value.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var table = new DataTable();
            table.Columns.Add("人名");
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                table.Columns.Add(header);
            }

            foreach (var member in members)
            {
                var row = table.NewRow();
                row["人名"] = member.Name;
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                    var assign = assignments.FirstOrDefault(a => a.Date.Date == date && a.AssignedMembers.Contains(member));
                    row[header] = assign != null ? assign.ShiftType : "休";
                }
                table.Rows.Add(row);
            }

            var requiredRow = table.NewRow();
            requiredRow["人名"] = "必要人数";
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                var frame = shiftFrames.FirstOrDefault(f => f.Date.Date == date);
                requiredRow[header] = frame?.RequiredNumber ?? 0;
            }
            table.Rows.Add(requiredRow);

            var assignedRow = table.NewRow();
            assignedRow["人名"] = "割当人数";
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                var count = assignments.Where(a => a.Date.Date == date).Sum(a => a.AssignedMembers.Count);
                assignedRow[header] = count;
            }
            table.Rows.Add(assignedRow);

            dtShift.AutoGenerateColumns = true;
            dtShift.DataSource = table;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                var col = dtShift.Columns[header];
                if (col != null)
                {
                    col.Width = 45;
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        col.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }

            dtShift.AutoResizeColumns();
        }

        /// <summary>
        /// 指定した曜日を日本語表記で返します。
        /// </summary>
        /// <param name="day">曜日</param>
        /// <returns>日・月・火などの文字列。該当しない場合は空文字列。</returns>
        private static string GetJapaneseDayOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return "日";
                case DayOfWeek.Monday:
                    return "月";
                case DayOfWeek.Tuesday:
                    return "火";
                case DayOfWeek.Wednesday:
                    return "水";
                case DayOfWeek.Thursday:
                    return "木";
                case DayOfWeek.Friday:
                    return "金";
                case DayOfWeek.Saturday:
                    return "土";
            }

            return string.Empty;
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
            SaveMembers();
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            if (dtMembers.CurrentRow?.DataBoundItem is Member m)
            {
                members.Remove(m);
                SetupMemberGrid();
                SaveMembers();
            }
        }

        private void btnRefreshShift_Click(object sender, EventArgs e)
        {
            assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members);
            SetupDataGridView();
        }

        private void dtpMonth_ValueChanged(object sender, EventArgs e)
        {
            SetupDataGridView();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveMembers();
            base.OnFormClosing(e);
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
            this.dtpMonth = new System.Windows.Forms.DateTimePicker();
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
            this.tabPage1.Controls.Add(this.dtpMonth);
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

            // dtpMonth
            //
            this.dtpMonth.CustomFormat = "yyyy/MM";
            this.dtpMonth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpMonth.Location = new System.Drawing.Point(87, 6);
            this.dtpMonth.Name = "dtpMonth";
            this.dtpMonth.ShowUpDown = true;
            this.dtpMonth.Size = new System.Drawing.Size(100, 23);
            this.dtpMonth.TabIndex = 2;
            this.dtpMonth.Value = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            this.dtpMonth.ValueChanged += new System.EventHandler(this.dtpMonth_ValueChanged);
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

