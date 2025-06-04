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
        private TabPage tabPage3;
        private DataGridView dtShift;
        private DataGridView dtMembers;
        private DataGridView dtHolidays;
        private Button btnAddMember;
        private Button btnRemoveMember;
        private Button btnAddHoliday;
        private Button btnRemoveHoliday;
        private Button btnRefreshShift;
        private DateTimePicker dtpMonth;
        private ComboBox cmbHolidayMember;
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
            SetupHolidayTab();
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

            // 割当人数行の不足・過剰を色分け
            try
            {
                int assignedRowIndex = table.Rows.IndexOf(assignedRow);
                if (assignedRowIndex >= 0 && assignedRowIndex < dtShift.Rows.Count)
                {
                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        var date = new DateTime(year, month, day);
                        var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                        var col = dtShift.Columns[header];
                        if (col == null)
                        {
                            continue;
                        }

                        var cell = dtShift.Rows[assignedRowIndex].Cells[col.Index];
                        var style = new DataGridViewCellStyle(cell.Style);

                        var assign = assignments.FirstOrDefault(a => a.Date.Date == date);
                        if (assign != null)
                        {
                            if (assign.Shortage)
                            {
                                style.BackColor = Color.MistyRose;      // 不足時は薄い赤
                            }
                            else if (assign.Excess)
                            {
                                style.BackColor = Color.LightBlue;      // 過剰時は薄い青
                            }
                        }

                        cell.Style = style;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"セルの着色中にエラーが発生しました: {ex.Message}");
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

        private void SetupHolidayTab()
        {
            cmbHolidayMember.DataSource = null;
            cmbHolidayMember.DataSource = members;
            cmbHolidayMember.DisplayMember = "Name";
            cmbHolidayMember.ValueMember = "Id";
            if (members.Count > 0)
            {
                cmbHolidayMember.SelectedIndex = 0;
            }
            UpdateHolidayGrid();
        }

        private void UpdateHolidayGrid()
        {
            dtHolidays.DataSource = null;
            if (cmbHolidayMember.SelectedItem is Member m && m.DesiredHolidays != null)
            {
                var table = new DataTable();
                table.Columns.Add("日付");
                foreach (var d in m.DesiredHolidays.OrderBy(d => d))
                {
                    var row = table.NewRow();
                    row[0] = d.ToString("yyyy-MM-dd");
                    table.Rows.Add(row);
                }
                dtHolidays.DataSource = table;
            }
        }

        private void cmbHolidayMember_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHolidayGrid();
        }

        private void btnAddHoliday_Click(object sender, EventArgs e)
        {
            if (!(cmbHolidayMember.SelectedItem is Member m))
            {
                return;
            }

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "日付を入力してください (YYYY-MM-DD)",
                "希望休追加",
                DateTime.Today.ToString("yyyy-MM-dd"));

            if (DateTime.TryParse(input, out var date))
            {
                date = date.Date;
                if (!m.DesiredHolidays.Contains(date))
                {
                    m.DesiredHolidays.Add(date);
                    UpdateHolidayGrid();
                    SaveMembers();
                }
            }
            else if (!string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("日付の形式が正しくありません。", "エラー");
            }
        }

        private void btnRemoveHoliday_Click(object sender, EventArgs e)
        {
            if (!(cmbHolidayMember.SelectedItem is Member m))
            {
                return;
            }

            if (dtHolidays.CurrentRow != null)
            {
                var val = dtHolidays.CurrentRow.Cells[0].Value as string;
                if (DateTime.TryParse(val, out var date))
                {
                    date = date.Date;
                    m.DesiredHolidays.Remove(date);
                    UpdateHolidayGrid();
                    SaveMembers();
                }
            }
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            var nextId = members.Count > 0 ? members.Max(m => m.Id) + 1 : 1;
            members.Add(new Member { Id = nextId, Name = "新規" });
            SetupMemberGrid();
            SetupHolidayTab();
            SaveMembers();
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            if (dtMembers.CurrentRow?.DataBoundItem is Member m)
            {
                members.Remove(m);
                SetupMemberGrid();
                SetupHolidayTab();
                SaveMembers();
            }
        }

        private void btnRefreshShift_Click(object sender, EventArgs e)
        {
            try
            {
                assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members);
                SetupDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"シフト更新中にエラーが発生しました: {ex.Message}");
            }
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
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dtShift = new System.Windows.Forms.DataGridView();
            this.dtMembers = new System.Windows.Forms.DataGridView();
            this.dtHolidays = new System.Windows.Forms.DataGridView();
            this.btnAddMember = new System.Windows.Forms.Button();
            this.btnRemoveMember = new System.Windows.Forms.Button();
            this.btnAddHoliday = new System.Windows.Forms.Button();
            this.btnRemoveHoliday = new System.Windows.Forms.Button();
            this.btnRefreshShift = new System.Windows.Forms.Button();
            this.dtpMonth = new System.Windows.Forms.DateTimePicker();
            this.cmbHolidayMember = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtHolidays)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
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

            // tabPage3
            //
            this.tabPage3.Controls.Add(this.dtHolidays);
            this.tabPage3.Controls.Add(this.cmbHolidayMember);
            this.tabPage3.Controls.Add(this.btnRemoveHoliday);
            this.tabPage3.Controls.Add(this.btnAddHoliday);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1385, 838);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "希望休";
            this.tabPage3.UseVisualStyleBackColor = true;

            // btnAddHoliday
            //
            this.btnAddHoliday.Location = new System.Drawing.Point(6, 6);
            this.btnAddHoliday.Name = "btnAddHoliday";
            this.btnAddHoliday.Size = new System.Drawing.Size(75, 23);
            this.btnAddHoliday.TabIndex = 0;
            this.btnAddHoliday.Text = "追加";
            this.btnAddHoliday.UseVisualStyleBackColor = true;
            this.btnAddHoliday.Click += new System.EventHandler(this.btnAddHoliday_Click);

            // btnRemoveHoliday
            //
            this.btnRemoveHoliday.Location = new System.Drawing.Point(87, 6);
            this.btnRemoveHoliday.Name = "btnRemoveHoliday";
            this.btnRemoveHoliday.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveHoliday.TabIndex = 1;
            this.btnRemoveHoliday.Text = "削除";
            this.btnRemoveHoliday.UseVisualStyleBackColor = true;
            this.btnRemoveHoliday.Click += new System.EventHandler(this.btnRemoveHoliday_Click);

            // cmbHolidayMember
            //
            this.cmbHolidayMember.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHolidayMember.Location = new System.Drawing.Point(168, 6);
            this.cmbHolidayMember.Name = "cmbHolidayMember";
            this.cmbHolidayMember.Size = new System.Drawing.Size(121, 23);
            this.cmbHolidayMember.TabIndex = 2;
            this.cmbHolidayMember.SelectedIndexChanged += new System.EventHandler(this.cmbHolidayMember_SelectedIndexChanged);

            // dtHolidays
            //
            this.dtHolidays.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtHolidays.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtHolidays.Location = new System.Drawing.Point(3, 35);
            this.dtHolidays.Name = "dtHolidays";
            this.dtHolidays.RowTemplate.Height = 21;
            this.dtHolidays.Size = new System.Drawing.Size(1379, 800);
            this.dtHolidays.TabIndex = 3;
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtHolidays)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

