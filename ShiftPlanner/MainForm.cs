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
        // 以下のUIコントロール定義はデザイナー部に移動しました

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
        private readonly string holidayFilePath;
        private readonly string skillGroupFilePath;
        private readonly string shiftTimeFilePath;
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();
        private readonly string requestFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner",
            "requests.json");
        private List<ShiftRequest> shiftRequests = new List<ShiftRequest>();
        private int holidayLimit = 3; // 休み希望上限
        private List<CustomHoliday> customHolidays = new List<CustomHoliday>();
        private List<SkillGroup> skillGroups = new List<SkillGroup>();
        private List<ShiftTime> shiftTimes = new List<ShiftTime>();

        public MainForm()
        {
            // 各ファイルパスを生成
            memberFilePath = Path.Combine(dataDir, "members.json");
            frameFilePath = Path.Combine(dataDir, "shiftFrames.json");
            assignmentFilePath = Path.Combine(dataDir, "shiftAssignments.json");
            holidayFilePath = Path.Combine(dataDir, "customHolidays.json");
            skillGroupFilePath = Path.Combine(dataDir, "skillGroups.json");
            shiftTimeFilePath = Path.Combine(dataDir, "shiftTimes.json");

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

            LoadHolidays();
            LoadSkillGroups();
            LoadShiftTimes();
            JapaneseHolidayHelper.SetCustomHolidays(customHolidays);

            InitializeData();
            SetupDataGridView();
            SetupRequestGrid();
            UpdateRequestSummary();

            if (cmbHolidayLimit != null)
            {
                // コンボボックスの選択値から上限を設定
                if (int.TryParse(cmbHolidayLimit.SelectedItem?.ToString(), out int v))
                {
                    holidayLimit = v;
                }
            }

            // メンバータブはメニューから表示するため削除
            if (tabControl1.TabPages.Contains(tabPage2))
            {
                tabControl1.TabPages.Remove(tabPage2);
            }

            // グリッド編集内容を保存するイベントを設定
            if (dtRequests != null)
            {
                dtRequests.CellEndEdit += DtRequests_CellEndEdit;
            }
            if (dtShift != null)
            {
                // Delete キーで選択セルをクリアするイベントを登録
                dtShift.KeyDown += dtShift_KeyDown;
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

        private void LoadHolidays()
        {
            if (File.Exists(holidayFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<CustomHoliday>));
                    using (var stream = File.OpenRead(holidayFilePath))
                    {
                        var list = serializer.ReadObject(stream) as List<CustomHoliday>;
                        if (list != null)
                        {
                            customHolidays = list;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"祝日情報の読み込みに失敗しました: {ex.Message}");
                    customHolidays = new List<CustomHoliday>();
                }
            }
        }

        private void SaveHolidays()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<CustomHoliday>));
                using (var stream = File.Create(holidayFilePath))
                {
                    serializer.WriteObject(stream, customHolidays ?? new List<CustomHoliday>());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"祝日情報の保存に失敗しました: {ex.Message}");
            }
        }

        private void LoadSkillGroups()
        {
            if (File.Exists(skillGroupFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<SkillGroup>));
                    using (var stream = File.OpenRead(skillGroupFilePath))
                    {
                        var list = serializer.ReadObject(stream) as List<SkillGroup>;
                        if (list != null)
                        {
                            skillGroups = list;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"スキルグループ情報の読み込みに失敗しました: {ex.Message}");
                    skillGroups = new List<SkillGroup>();
                }
            }
        }

        private void SaveSkillGroups()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<SkillGroup>));
                using (var stream = File.Create(skillGroupFilePath))
                {
                    serializer.WriteObject(stream, skillGroups);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"スキルグループ情報の保存に失敗しました: {ex.Message}");
            }
        }

        private void LoadShiftTimes()
        {
            if (File.Exists(shiftTimeFilePath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(List<ShiftTime>));
                    using (var stream = File.OpenRead(shiftTimeFilePath))
                    {
                        var list = serializer.ReadObject(stream) as List<ShiftTime>;
                        if (list != null)
                        {
                            shiftTimes = list;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"勤務時間情報の読み込みに失敗しました: {ex.Message}");
                    shiftTimes = new List<ShiftTime>();
                }
            }
        }

        private void SaveShiftTimes()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<ShiftTime>));
                using (var stream = File.Create(shiftTimeFilePath))
                {
                    serializer.WriteObject(stream, shiftTimes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"勤務時間情報の保存に失敗しました: {ex.Message}");
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
        /// すべてのシフトフレームの必要人数をリセットします。
        /// </summary>
        private void ResetRequiredNumbers()
        {
            if (shiftFrames == null)
            {
                return; // null 安全対策
            }

            foreach (var frame in shiftFrames)
            {
                if (frame != null)
                {
                    frame.RequiredNumber = 0;
                }
            }

            SaveFrames();
        }

        /// <summary>
        /// 勤怠時間マスターの設定をシフトフレームへ適用します。
        /// </summary>
        private void ApplyShiftTimesFromMaster()
        {
            if (shiftFrames == null || shiftTimes == null)
            {
                return; // nullチェック
            }

            foreach (var frame in shiftFrames)
            {
                if (frame == null || string.IsNullOrWhiteSpace(frame.ShiftType))
                {
                    continue;
                }

                var master = shiftTimes.FirstOrDefault(t =>
                    t != null && string.Equals(t.Name, frame.ShiftType, StringComparison.OrdinalIgnoreCase));

                if (master != null)
                {
                    frame.ShiftStart = master.Start;
                    frame.ShiftEnd = master.End;
                }
            }

            SaveFrames();
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
                    // 読み込んだメンバーを既存の参照に置き換える
                    ReconcileAssignmentMembers();
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

        /// <summary>
        /// 読み込んだ割り当てデータのメンバー参照を既存メンバーリストのものに合わせます。
        /// </summary>
        private void ReconcileAssignmentMembers()
        {
            if (assignments == null || members == null)
            {
                return;
            }

            foreach (var assign in assignments)
            {
                if (assign?.AssignedMembers == null)
                {
                    continue;
                }

                for (int i = 0; i < assign.AssignedMembers.Count; i++)
                {
                    var m = assign.AssignedMembers[i];
                    var existing = members.FirstOrDefault(x => x.Id == m.Id);
                    if (existing != null)
                    {
                        assign.AssignedMembers[i] = existing;
                    }
                }
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
                    // デフォルトの勤務時間列は廃止
                    WorksOnSaturday = false,
                    WorksOnSunday = false
                });
                members.Add(new Member
                {
                    Id = 2,
                    Name = "佐藤",
                    AvailableDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
                    // デフォルトの勤務時間列は廃止
                    WorksOnSaturday = false,
                    WorksOnSunday = false
                });
                members.Add(new Member
                {
                    Id = 3,
                    Name = "鈴木",
                    AvailableDays = new List<DayOfWeek> { DayOfWeek.Tuesday },
                    // デフォルトの勤務時間列は廃止
                    WorksOnSaturday = false,
                    WorksOnSunday = false
                });
            }

            // シフトフレームが無い場合でもサンプルデータは作成しない
            // 必要に応じて別画面からシフトフレームを登録してもらう

            // 割り当て結果が無い場合は自動生成
            // シフトフレームが存在しないときは生成しない
            if (assignments.Count == 0 && shiftFrames.Count > 0)
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
                    var assign = assignments.FirstOrDefault(a =>
                        a.Date.Date == date &&
                        a.AssignedMembers != null &&
                        a.AssignedMembers.Any(m => m.Id == member.Id));
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
                    // 土日または祝日は背景色を変更
                    if (date.DayOfWeek == DayOfWeek.Saturday ||
                        date.DayOfWeek == DayOfWeek.Sunday ||
                        JapaneseHolidayHelper.IsHoliday(date))
                    {
                        col.DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }

            // すべての列をソート不可に設定
            SetColumnsNotSortable(dtShift);

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
                            // 必要人数と割当人数が一致する場合は青、
                            // 一致しない場合は赤の背景色にする
                            if (assign.AssignedMembers.Count == assign.RequiredNumber)
                            {
                                style.BackColor = Color.LightBlue;
                            }
                            else
                            {
                                style.BackColor = Color.LightCoral;
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
        /// 指定したグリッドの列をすべてソート不可に設定します。
        /// </summary>
        /// <param name="grid">対象のDataGridView</param>
        private static void SetColumnsNotSortable(DataGridView grid)
        {
            if (grid == null || grid.Columns == null)
            {
                return; // null 安全対策
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column != null)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
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
                    var master = shiftTimes?.FirstOrDefault(t =>
                        t != null && string.Equals(t.Name, shiftType, StringComparison.OrdinalIgnoreCase));

                    frame = new ShiftFrame
                    {
                        Date = key.date,
                        ShiftType = shiftType,
                        ShiftStart = master?.Start ?? TimeSpan.Zero,
                        ShiftEnd = master?.End ?? TimeSpan.Zero
                        // RequiredNumber は初期値 0 のままとする
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
                        case nameof(Member.Skills):
                            col.HeaderText = "スキル";
                            break;
                        case nameof(Member.DesiredHolidays):
                            col.HeaderText = "希望休";
                            break;
                        case nameof(Member.Constraints):
                            // 制約列は表示しない
                            col.Visible = false;
                            break;
                        case nameof(Member.WorksOnSaturday):
                            col.HeaderText = "土曜日";
                            break;
                        case nameof(Member.WorksOnSunday):
                            col.HeaderText = "日曜日";
                            break;
                    }
                }

                // 列をソート不可に設定
                SetColumnsNotSortable(dtMembers);
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
            try
            {
                // データソースを一旦解除してから再設定
                dtRequests.DataSource = null;
                dtRequests.AutoGenerateColumns = false;
                dtRequests.Columns.Clear();

                // メンバー列は名前を表示するコンボボックスにする
                var colMember = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = nameof(ShiftRequest.MemberId),
                    HeaderText = "メンバー",
                    DataSource = members,
                    DisplayMember = nameof(Member.Name),
                    ValueMember = nameof(Member.Id)
                };

                var colDate = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = nameof(ShiftRequest.Date),
                    HeaderText = "希望日"
                };

                var colHoliday = new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = nameof(ShiftRequest.IsHolidayRequest),
                    HeaderText = "休み希望"
                };

                dtRequests.Columns.AddRange(new DataGridViewColumn[] { colMember, colDate, colHoliday });
                dtRequests.DataSource = shiftRequests ?? new List<ShiftRequest>();

                // 列をソート不可に設定
                SetColumnsNotSortable(dtRequests);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"グリッド設定中にエラーが発生しました: {ex.Message}");
            }

            UpdateRequestSummary();
        }

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            var nextId = members.Count > 0 ? members.Max(m => m.Id) + 1 : 1;
            members.Add(new Member
            {
                Id = nextId,
                Name = "新規",
                // 勤務開始・終了時間列は廃止
                WorksOnSaturday = false,
                WorksOnSunday = false
            });
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
            // 休み希望をデフォルトでチェックした状態でフォームを表示
            using (var form = new ShiftRequestForm(members, true))
            {
                if (form.ShowDialog() == DialogResult.OK && form.ShiftRequest != null)
                {
                    var req = form.ShiftRequest;
                    if (req.IsHolidayRequest)
                    {
                        int count = shiftRequests.Count(r => r.MemberId == req.MemberId && r.IsHolidayRequest);
                        if (count >= holidayLimit)
                        {
                            MessageBox.Show($"{GetMemberName(req.MemberId)} の休み希望は最大 {holidayLimit} 件までです。");
                            return;
                        }
                    }

                    shiftRequests.Add(req);
                    SetupRequestGrid();
                    SaveRequests();
                    UpdateRequestSummary();
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
                UpdateRequestSummary();

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

                // シフト時間マスターの内容をシフトフレームへ反映
                ApplyShiftTimesFromMaster();

                // 自動割当を生成
                assignments = ShiftGenerator.GenerateBaseShift(shiftFrames, members, shiftRequests);

                // 手入力分を優先して反映
                ApplyManualAssignments(manual);

                // 必要人数は保持したままにする

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
            SaveHolidays();
            SaveSkillGroups();
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
        /// 祝日マスター編集メニューのクリックイベント
        /// </summary>
        private void menuHolidayMaster_Click(object sender, EventArgs e)
        {
            using (var form = new HolidayMasterForm(customHolidays))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SaveHolidays();
                    JapaneseHolidayHelper.SetCustomHolidays(customHolidays);
                    SetupDataGridView();
                }
            }
        }

        /// <summary>
        /// メンバーマスター編集メニューのクリックイベント
        /// </summary>
        private void menuMemberMaster_Click(object sender, EventArgs e)
        {
            using (var form = new MemberMasterForm(members, skillGroups, shiftTimes))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 編集結果はそのまま反映されるため再表示のみ
                    SaveMembers();
                }
            }
        }

        /// <summary>
        /// スキルグループマスター編集メニューのクリックイベント
        /// </summary>
        private void menuSkillGroupMaster_Click(object sender, EventArgs e)
        {
            using (var form = new SkillGroupMasterForm(skillGroups))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    skillGroups = form.SkillGroups;
                    SaveSkillGroups();
                }
            }
        }

        /// <summary>
        /// 勤務時間マスター編集メニューのクリックイベント
        /// </summary>
        private void menuShiftTimeMaster_Click(object sender, EventArgs e)
        {
            using (var form = new ShiftTimeMasterForm(shiftTimes))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    shiftTimes = form.ShiftTimes;
                    SaveShiftTimes();
                }
            }
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

        /// <summary>
        /// シフト表グリッドで Delete キーが押されたとき、選択セルの値をクリアします。
        /// </summary>
        private void dtShift_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || dtShift == null)
            {
                return;
            }

            foreach (DataGridViewCell cell in dtShift.SelectedCells)
            {
                if (cell == null || cell.ReadOnly)
                {
                    continue;
                }

                // 行や列が無効な場合もスキップ
                if (cell.RowIndex < 0 || cell.ColumnIndex < 0)
                {
                    continue;
                }

                cell.Value = string.Empty;
            }

            // 既定の処理を抑制
            e.Handled = true;
        }

        private void DtRequests_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SaveRequests();
            UpdateRequestSummary();

            if (dtRequests == null || e.RowIndex < 0)
            {
                return;
            }

            if (dtRequests.Rows[e.RowIndex].DataBoundItem is ShiftRequest req && req.IsHolidayRequest)
            {
                int count = shiftRequests.Count(r => r.MemberId == req.MemberId && r.IsHolidayRequest);
                if (count > holidayLimit)
                {
                    MessageBox.Show($"{GetMemberName(req.MemberId)} の休み希望は最大 {holidayLimit} 件までです。");
                    req.IsHolidayRequest = false;
                    dtRequests.Refresh();
                    SaveRequests();
                    UpdateRequestSummary();
                }
            }
        }

        private void CmbHolidayLimit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cmbHolidayLimit?.SelectedItem?.ToString(), out int v))
            {
                holidayLimit = v;
            }
            UpdateRequestSummary();
        }

        private void UpdateRequestSummary()
        {
            if (dtRequestSummary == null)
            {
                return;
            }

            try
            {
                var list = members.Select(m => new RequestSummary
                {
                    メンバー = m.Name,
                    出勤希望数 = shiftRequests.Count(r => r.MemberId == m.Id && !r.IsHolidayRequest),
                    休希望数 = shiftRequests.Count(r => r.MemberId == m.Id && r.IsHolidayRequest)
                }).ToList();

                dtRequestSummary.DataSource = list;
                SetColumnsNotSortable(dtRequestSummary);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"サマリの更新に失敗しました: {ex.Message}");
            }
        }

        private string GetMemberName(int id)
        {
            return members.FirstOrDefault(m => m.Id == id)?.Name ?? string.Empty;
        }

        // このメソッドの内容は MainForm.Designer.cs に移動しました。
    }
}

