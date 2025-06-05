using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Windows.Forms.DataVisualization.Charting;

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
        private DataGridView dtRequests;
        private Button btnAddMember;
        private Button btnRemoveMember;
        private Button btnAddRequest;
        private Button btnRemoveRequest;
        private Button btnRefreshShift;
        private DateTimePicker dtpMonth;
        // CSV、PDF出力ボタン
        private Button btnExportCsv;
        private Button btnExportPdf;
        // メニュー関連
        private MenuStrip menuStrip1;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuExportCsv;
        private ToolStripMenuItem menuExportPdf;
        // tabPage3 は上で宣言済みのため削除
        private DateTimePicker dtp分析月;
        private Label lbl総労働時間;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartシフト分布;

        // メンバー情報保存用のファイルパス
        // %APPDATA%/ShiftPlanner/members.json の形で保存する
        // データ保存ディレクトリ
        private readonly string dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner");

        // 各種データ保存用ファイルパス
        private readonly string memberFilePath;
        private readonly string frameFilePath;
        private readonly string assignmentFilePath;
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();
        private readonly string requestFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner",
            "requests.json");
        private List<ShiftRequest> shiftRequests = new List<ShiftRequest>();

        public MainForm()
        {
            // 各ファイルパスを生成
            memberFilePath = Path.Combine(dataDir, "members.json");
            frameFilePath = Path.Combine(dataDir, "shiftFrames.json");
            assignmentFilePath = Path.Combine(dataDir, "shiftAssignments.json");

            InitializeComponent(); // これだけでOK

            // データ保存用ディレクトリが無い場合は作成する
            var dir = dataDir;
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
            SetupRequestGrid();

            // グリッド編集内容を保存するイベントを設定
            if (dtMembers != null)
            {
                dtMembers.CellEndEdit += (s, e) => SaveMembers();
            }
            if (dtRequests != null)
            {
                dtRequests.CellEndEdit += (s, e) => SaveRequests();
            }
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


        private void LoadRequests()
        {
            if (File.Exists(requestFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<ShiftRequest>));
                    using (var stream = File.OpenRead(requestFilePath))
                    {
                        shiftRequests = (List<ShiftRequest>)serializer.ReadObject(stream);

                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"希望情報の読み込みに失敗しました: {ex.Message}");
                    shiftRequests = new List<ShiftRequest>();
                }
            }
        }

        private void SaveRequests()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ShiftRequest>));
                using (var stream = File.Create(requestFilePath))
                {
                    serializer.WriteObject(stream, shiftRequests);

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"希望情報の保存に失敗しました: {ex.Message}");

            }
        }

        /// <summary>
        /// シフトフレームを読み込みます。
        /// </summary>
        private void LoadFrames()
        {
            if (File.Exists(frameFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<ShiftFrame>));
                    using (var stream = File.OpenRead(frameFilePath))
                    {
                        shiftFrames = (List<ShiftFrame>)serializer.ReadObject(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"シフトフレームの読み込みに失敗しました: {ex.Message}");
                    shiftFrames = new List<ShiftFrame>();
                }
            }
        }

        /// <summary>
        /// シフトフレームを保存します。
        /// </summary>
        private void SaveFrames()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ShiftFrame>));
                using (var stream = File.Create(frameFilePath))
                {
                    serializer.WriteObject(stream, shiftFrames);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"シフトフレームの保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 割り当て結果を読み込みます。
        /// </summary>
        private void LoadAssignments()
        {
            if (File.Exists(assignmentFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<ShiftAssignment>));
                    using (var stream = File.OpenRead(assignmentFilePath))
                    {
                        assignments = (List<ShiftAssignment>)serializer.ReadObject(stream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"割り当て結果の読み込みに失敗しました: {ex.Message}");
                    assignments = new List<ShiftAssignment>();
                }
            }
        }

        /// <summary>
        /// 割り当て結果を保存します。
        /// </summary>
        private void SaveAssignments()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ShiftAssignment>));
                using (var stream = File.Create(assignmentFilePath))
                {
                    serializer.WriteObject(stream, assignments ?? new List<ShiftAssignment>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"割り当て結果の保存に失敗しました: {ex.Message}");
            }
        }

        private void InitializeData()
        {
            LoadMembers();
            LoadFrames();
            LoadRequests();
            LoadAssignments();

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

            // シフトフレームが無い場合は例としていくつか作成
            if (shiftFrames.Count == 0)
            {
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
                shiftFrames.Add(new ShiftFrame
                {
                    Date = DateTime.Today.AddDays(3),
                    ShiftType = "遅番",
                    ShiftStart = TimeSpan.FromHours(13),
                    ShiftEnd = TimeSpan.FromHours(21),
                    RequiredNumber = 2
                });
                SaveFrames();
            }

            // 割り当て結果が無い場合は自動生成
            if (assignments.Count == 0)
            {
                assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members, shiftRequests);
                SaveAssignments();
            }
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
            dtShift.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dtShift.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                var col = dtShift.Columns[header];
                if (col != null)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.Width = 40;
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

            // 自動リサイズは行わず固定幅を維持
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

        /// <summary>
        /// シフト表の「必要人数」行から値を取得してシフトフレームを更新します。
        /// </summary>
        private void ApplyRequiredNumbersFromGrid()
        {
            if (!(dtShift.DataSource is DataTable table))
            {
                return; // データテーブルが取得できない場合は何もしない
            }

            dtShift.EndEdit(); // 編集中の値を確定させる

            var requiredRow = table.AsEnumerable()
                .FirstOrDefault(r => string.Equals(r["人名"]?.ToString(), "必要人数", StringComparison.Ordinal));
            if (requiredRow == null)
            {
                return; // 行が見つからない場合も処理しない
            }

            int year = dtpMonth.Value.Year;
            int month = dtpMonth.Value.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                string header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                if (!table.Columns.Contains(header))
                {
                    continue; // 列が存在しない場合はスキップ
                }

                int number = 0;
                var val = requiredRow[header];
                if (val != null && !string.IsNullOrEmpty(val.ToString()))
                {
                    int.TryParse(val.ToString(), out number);
                }

                var frame = shiftFrames.FirstOrDefault(f => f.Date.Date == date);
                if (frame != null)
                {
                    frame.RequiredNumber = number;
                }
            }

            SaveFrames();
        }

        /// <summary>
        /// グリッドで手入力されたシフト内容を取得します。
        /// </summary>
        private Dictionary<(int memberId, DateTime date), string?> CaptureManualAssignmentsFromGrid()
        {
            var result = new Dictionary<(int, DateTime), string?>();

            if (!(dtShift.DataSource is DataTable table))
            {
                return result;
            }

            dtShift.EndEdit();

            int year = dtpMonth.Value.Year;
            int month = dtpMonth.Value.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            foreach (DataRow row in table.Rows)
            {
                var name = row["人名"]?.ToString();
                if (string.IsNullOrEmpty(name) || name == "必要人数" || name == "割当人数")
                {
                    continue;
                }

                var member = members.FirstOrDefault(m => m.Name == name);
                if (member == null)
                {
                    continue;
                }

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var header = $"{day}({GetJapaneseDayOfWeek(date.DayOfWeek)})";
                    if (!table.Columns.Contains(header))
                    {
                        continue;
                    }

                    var val = row[header]?.ToString();
                    result[(member.Id, date)] = string.IsNullOrWhiteSpace(val) ? "休" : val;
                }
            }

            return result;
        }

        /// <summary>
        /// 自動生成後の割り当てに手入力内容を反映します。
        /// </summary>
        private void ApplyManualAssignments(Dictionary<(int memberId, DateTime date), string?> manual)
        {
            if (manual == null || manual.Count == 0)
            {
                return;
            }

            foreach (var kv in manual)
            {
                var key = kv.Key;
                var shiftType = kv.Value ?? "休";
                var member = members.FirstOrDefault(m => m.Id == key.memberId);
                if (member == null)
                {
                    continue;
                }

                // その日の既存割当からメンバーを除外
                foreach (var a in assignments.Where(a => a.Date.Date == key.date).ToList())
                {
                    a.AssignedMembers.RemoveAll(m => m.Id == member.Id);
                }

                if (string.IsNullOrEmpty(shiftType) || shiftType == "休")
                {
                    // 休み指定なので割当なし
                    continue;
                }

                // シフトフレームを取得または作成
                var frame = shiftFrames.FirstOrDefault(f => f.Date.Date == key.date && f.ShiftType == shiftType);
                if (frame == null)
                {
                    frame = new ShiftFrame
                    {
                        Date = key.date,
                        ShiftType = shiftType,
                        ShiftStart = TimeSpan.FromHours(9),
                        ShiftEnd = TimeSpan.FromHours(17),
                        RequiredNumber = 1
                    };
                    shiftFrames.Add(frame);
                }

                // 割当を取得または作成
                var assign = assignments.FirstOrDefault(a => a.Date.Date == key.date && a.ShiftType == shiftType);
                if (assign == null)
                {
                    assign = new ShiftAssignment
                    {
                        Date = key.date,
                        ShiftType = shiftType,
                        RequiredNumber = frame.RequiredNumber,
                        AssignedMembers = new List<Member>()
                    };
                    assignments.Add(assign);
                }

                if (!assign.AssignedMembers.Any(m => m.Id == member.Id))
                {
                    assign.AssignedMembers.Add(member);
                }
            }

            // 不要になった割当を整理
            assignments.RemoveAll(a => a.AssignedMembers == null || a.AssignedMembers.Count == 0);

            SaveFrames();
        }

        private void SetupMemberGrid()
        {
            // データソースを一旦解除してから設定
            dtMembers.DataSource = null;
            dtMembers.DataSource = members;

            // 自動生成された列のヘッダーを日本語へ変換
            dtMembers.AutoGenerateColumns = true;
            try
            {
                foreach (DataGridViewColumn col in dtMembers.Columns)
                {
                    if (col == null || string.IsNullOrEmpty(col.Name))
                    {
                        continue; // null 安全対策
                    }

                    switch (col.Name)
                    {
                        case nameof(Member.Id):
                            col.HeaderText = "ID";
                            break;
                        case nameof(Member.Name):
                            col.HeaderText = "名前";
                            break;
                        case nameof(Member.AvailableDays):
                            col.HeaderText = "勤務可能曜日";
                            break;
                        case nameof(Member.AvailableFrom):
                            col.HeaderText = "開始時間";
                            break;
                        case nameof(Member.AvailableTo):
                            col.HeaderText = "終了時間";
                            break;
                        case nameof(Member.Skills):
                            col.HeaderText = "スキル";
                            break;
                        case nameof(Member.DesiredHolidays):
                            col.HeaderText = "希望休";
                            break;
                        case nameof(Member.Constraints):
                            col.HeaderText = "制約";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // ヘッダー変更に失敗してもアプリが落ちないよう通知のみ
                MessageBox.Show($"ヘッダー設定中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 分析タブの情報を更新します。
        /// </summary>
        private void UpdateAnalysis()
        {
            try
            {
                int 年 = dtp分析月.Value.Year;
                int 月 = dtp分析月.Value.Month;

                // 総労働時間計算
                var 時間 = ShiftAnalyzer.CalculateMonthlyHours(shiftFrames, 年, 月);
                lbl総労働時間.Text = $"総労働時間: {時間.TotalHours:F1} 時間";

                // シフトタイプ分布
                var 分布 = ShiftAnalyzer.GetShiftTypeDistribution(shiftFrames, 年, 月);
                if (chartシフト分布.Series.Count == 0)
                {
                    var series = new Series { ChartType = SeriesChartType.Pie };
                    chartシフト分布.Series.Add(series);
                }

                var s = chartシフト分布.Series[0];
                s.Points.Clear();
                foreach (var kv in 分布)
                {
                    s.Points.AddXY(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"分析情報の更新に失敗しました: {ex.Message}");
            }
        }

        private void SetupRequestGrid()
        {
            dtRequests.DataSource = null;
            dtRequests.DataSource = shiftRequests;
            dtRequests.AutoGenerateColumns = true;
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


        private void btnAddRequest_Click(object sender, EventArgs e)
        {
            using (var form = new ShiftRequestForm(members))
            {
                if (form.ShowDialog() == DialogResult.OK && form.ShiftRequest != null)
                {
                    shiftRequests.Add(form.ShiftRequest);
                    SetupRequestGrid();
                    SaveRequests();
                }
            }
        }

        private void btnRemoveRequest_Click(object sender, EventArgs e)
        {
            if (dtRequests.CurrentRow?.DataBoundItem is ShiftRequest r)
            {
                shiftRequests.Remove(r);
                SetupRequestGrid();
                SaveRequests();

            }
        }

        private void btnRefreshShift_Click(object sender, EventArgs e)
        {
            try
            {
                // 手入力されたシフト内容を取得しておく
                var manual = CaptureManualAssignmentsFromGrid();

                // 必要人数グリッドの内容をシフトフレームへ反映
                ApplyRequiredNumbersFromGrid();

                // 自動割当を生成
                assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members, shiftRequests);

                // 手入力分を優先して反映
                ApplyManualAssignments(manual);

                SetupDataGridView();
                SaveAssignments();
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

        private void dtp分析月_ValueChanged(object sender, EventArgs e)
        {
            UpdateAnalysis();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveMembers();
            SaveRequests();
            SaveFrames();
            SaveAssignments();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// CSV 出力メニューまたはボタンのクリックイベント
        /// </summary>
        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            ExportCsv();
        }

        /// <summary>
        /// PDF 出力メニューまたはボタンのクリックイベント
        /// </summary>
        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            ExportPdf();
        }

        /// <summary>
        /// 分析CSV出力メニューのクリックイベント
        /// </summary>
        private void menuExportAnalysisCsv_Click(object sender, EventArgs e)
        {
            ExportAnalysisCsv();
        }

        /// <summary>
        /// CSV を保存します。
        /// </summary>
        private void ExportCsv()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
                dialog.Title = "CSVの保存先を選択してください";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var frames = shiftFrames ?? new List<ShiftFrame>();
                        ShiftExporter.ExportToCsv(frames, dialog.FileName);
                        MessageBox.Show("CSV出力が完了しました。", "情報");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"CSV出力に失敗しました: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// PDF を保存します。
        /// </summary>
        private void ExportPdf()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "PDFファイル (*.pdf)|*.pdf|すべてのファイル (*.*)|*.*";
                dialog.Title = "PDFの保存先を選択してください";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var frames = shiftFrames ?? new List<ShiftFrame>();
                        var message = ShiftExporter.ExportToPdf(frames, dialog.FileName);
                        MessageBox.Show(message, "情報");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"PDF出力に失敗しました: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 分析結果をCSVとして保存します。
        /// </summary>
        private void ExportAnalysisCsv()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSVファイル (*.csv)|*.csv|すべてのファイル (*.*)|*.*";
                dialog.Title = "分析CSVの保存先を選択してください";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var frames = shiftFrames ?? new List<ShiftFrame>();
                        var year = dtp分析月.Value.Year;
                        var month = dtp分析月.Value.Month;
                        var message = ShiftAnalyzer.ExportDistributionToCsv(frames, year, month, dialog.FileName);
                        MessageBox.Show(message, "情報");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"分析CSV出力に失敗しました: {ex.Message}");
                    }
                }
            }
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
            this.dtp分析月 = new System.Windows.Forms.DateTimePicker();
            this.lbl総労働時間 = new System.Windows.Forms.Label();
            this.chartシフト分布 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).BeginInit();

            this.SuspendLayout();

            // menuStrip1
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
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
            this.tabPage1.Controls.Add(this.btnExportPdf);
            this.tabPage1.Controls.Add(this.btnExportCsv);
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

            // btnExportCsv
            //
            this.btnExportCsv.Location = new System.Drawing.Point(193, 6);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(75, 23);
            this.btnExportCsv.TabIndex = 3;
            this.btnExportCsv.Text = "CSV出力";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // btnExportPdf
            //
            this.btnExportPdf.Location = new System.Drawing.Point(274, 6);
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
            this.tabPage2.Size = new System.Drawing.Size(1385, 838);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "メンバー";
            this.tabPage2.UseVisualStyleBackColor = true;

            // tabPage3
            //
            this.tabPage3.Controls.Add(this.dtRequests);
            this.tabPage3.Controls.Add(this.btnRemoveRequest);
            this.tabPage3.Controls.Add(this.btnAddRequest);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1385, 838);
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
            this.dtMembers.Size = new System.Drawing.Size(1379, 800);
            this.dtMembers.TabIndex = 2;

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
            this.dtRequests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtRequests.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtRequests.Location = new System.Drawing.Point(3, 35);
            this.dtRequests.Name = "dtRequests";
            this.dtRequests.RowTemplate.Height = 21;
            this.dtRequests.Size = new System.Drawing.Size(1379, 800);
            this.dtRequests.TabIndex = 2;

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtMembers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).EndInit();

            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

