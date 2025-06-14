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
using System.Threading;
using System.Threading.Tasks;

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
        private readonly string settingsFilePath;
        private readonly string shiftTableFilePath;
        private AppSettings settings = new AppSettings();
        private List<Member> members = new List<Member>();
        private List<ShiftFrame> shiftFrames = new List<ShiftFrame>();
        private List<ShiftAssignment> assignments = new List<ShiftAssignment>();
        private readonly string requestFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShiftPlanner",
            "requests.json");
        private List<ShiftRequest> shiftRequests = new List<ShiftRequest>();
        private int holidayLimit = 3; // 休み希望上限
        private int minHolidayCount = 4; // 最低休日日数
        private List<CustomHoliday> customHolidays = new List<CustomHoliday>();
        private List<SkillGroup> skillGroups = new List<SkillGroup>();
        private List<ShiftTime> shiftTimes = new List<ShiftTime>();
        /// <summary>
        /// 有効な勤務時間のみを保持するリスト
        /// </summary>
        private List<ShiftTime> enabledShiftTimes = new List<ShiftTime>();

        // 読み込み失敗を検知するためのフラグ
        private bool loadFailed = false;

        // シフト表用のテーブル
        private DataTable shiftTable = new DataTable();

        // 保存処理の排他制御に使用するロックオブジェクト
        private readonly object saveLock = new object();

        // 乱数生成用の共有インスタンス
        private static readonly Random _rand = new Random();

        // 日付列が開始するインデックス
        private int dateColumnStartIndex = 1;

        public MainForm()
        {
            // 各ファイルパスを生成
            memberFilePath = Path.Combine(dataDir, "members.json");
            frameFilePath = Path.Combine(dataDir, "shiftFrames.json");
            assignmentFilePath = Path.Combine(dataDir, "shiftAssignments.json");
            holidayFilePath = Path.Combine(dataDir, "customHolidays.json");
            skillGroupFilePath = Path.Combine(dataDir, "skillGroups.json");
            shiftTimeFilePath = Path.Combine(dataDir, "shiftTimes.json");
            settingsFilePath = Path.Combine(dataDir, "settings.json");
            shiftTableFilePath = Path.Combine(dataDir, "shiftTable.json");

            LoadSettings();

            InitializeComponent(); // これだけでOK

            // 初期表示月を当月に設定
            if (dtp対象月 != null)
            {
                dtp対象月.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

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
            NormalizeMemberShiftNames();
            SetupRequestGrid();
            UpdateRequestSummary();
            SetupShiftGrid();

            // いずれかの読み込みに失敗した場合はデータを初期化
            if (loadFailed)
            {
                MessageBox.Show("設定の読み込みに失敗したため、データを初期化します。");
                ResetAllData();

                // 再度データを読み込み直す
                loadFailed = false;
                LoadHolidays();
                LoadSkillGroups();
                LoadShiftTimes();
                InitializeData();
                SetupRequestGrid();
                UpdateRequestSummary();
                SetupShiftGrid();
            }

            if (cmbHolidayLimit != null)
            {
                cmbHolidayLimit.SelectedItem = settings.HolidayLimit.ToString();
                holidayLimit = settings.HolidayLimit;
            }

            if (cmbDefaultRequired != null)
            {
                cmbDefaultRequired.SelectedItem = settings.DefaultRequired.ToString();
            }

            if (cmbMinHolidayCount != null)
            {
                cmbMinHolidayCount.SelectedItem = settings.MinHolidayCount.ToString();
                minHolidayCount = settings.MinHolidayCount;
            }

          
            // グリッド編集内容を保存するイベントを設定
            if (dtRequests != null)
            {
                dtRequests.CellEndEdit += DtRequests_CellEndEdit;
            }
        }

        private void LoadMembers()
        {
            if (!File.Exists(memberFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
            }
        }

        private void SaveMembers()
        {
            // 保存前に土日勤務可否フラグと曜日リストを同期
            NormalizeMemberAvailability();
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

        /// <summary>
        /// メンバーの制約設定を補正します。
        /// 読み込み後に必ず呼び出してください。
        /// </summary>
        private void NormalizeMemberConstraints()
        {
            if (members == null)
            {
                return; // null安全対策
            }

            try
            {
                foreach (var メンバー in members)
                {
                    if (メンバー == null)
                    {
                        continue;
                    }

                    // 制約オブジェクトが無い場合は生成
                    メンバー.Constraints ??= new ShiftConstraints();

                    // 連続勤務上限が0以下なら既定値5日を設定
                    if (メンバー.Constraints.MaxConsecutiveDays <= 0)
                    {
                        メンバー.Constraints.MaxConsecutiveDays = 5;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"メンバー制約の正規化中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// メンバーの曜日勤務可否フラグと AvailableDays リストを整合させます。
        /// </summary>
        private void NormalizeMemberAvailability()
        {
            if (members == null)
            {
                return; // null安全対策
            }

            try
            {
                foreach (var m in members)
                {
                    if (m == null)
                    {
                        continue;
                    }

                    m.AvailableDays ??= new List<DayOfWeek>();

                    // AvailableDays に基づきフラグを更新
                    m.WorksOnSaturday = m.AvailableDays.Contains(DayOfWeek.Saturday) || m.WorksOnSaturday;
                    m.WorksOnSunday = m.AvailableDays.Contains(DayOfWeek.Sunday) || m.WorksOnSunday;

                    // フラグ側を優先してリストを更新
                    if (m.WorksOnSaturday)
                    {
                        if (!m.AvailableDays.Contains(DayOfWeek.Saturday))
                        {
                            m.AvailableDays.Add(DayOfWeek.Saturday);
                        }
                    }
                    else
                    {
                        m.AvailableDays.Remove(DayOfWeek.Saturday);
                    }

                    if (m.WorksOnSunday)
                    {
                        if (!m.AvailableDays.Contains(DayOfWeek.Sunday))
                        {
                            m.AvailableDays.Add(DayOfWeek.Sunday);
                        }
                    }
                    else
                    {
                        m.AvailableDays.Remove(DayOfWeek.Sunday);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"メンバー勤務可否の正規化中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// メンバーが対応可能な勤務時間名を勤務時間マスターと同期します。
        /// </summary>
        private void NormalizeMemberShiftNames()
        {
            if (members == null || shiftTimes == null)
            {
                return; // null安全対策
            }

            try
            {
                // 有効な勤務時間名だけを取得
                var validNames = shiftTimes
                    .Where(s => s != null && s.IsEnabled)
                    .Select(s => s.Name)
                    .ToHashSet();

                foreach (var m in members)
                {
                    if (m == null)
                    {
                        continue;
                    }

                    m.AvailableShiftNames ??= new List<string>();

                    // 無効な勤務時間名を除去
                    m.AvailableShiftNames = m.AvailableShiftNames
                        .Where(n => validNames.Contains(n))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"勤務時間設定の正規化中にエラーが発生しました: {ex.Message}");
            }
        }

        private void LoadHolidays()
        {
            if (!File.Exists(holidayFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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
            if (!File.Exists(skillGroupFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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
            if (!File.Exists(shiftTimeFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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

        private void LoadSettings()
        {
            if (!File.Exists(settingsFilePath))
            {
                loadFailed = true;
                return;
            }

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(AppSettings));
                using (var stream = File.OpenRead(settingsFilePath))
                {
                    var obj = serializer.ReadObject(stream) as AppSettings;
                    if (obj != null)
                    {
                        settings = obj;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の読み込みに失敗しました: {ex.Message}");
                settings = new AppSettings();
                loadFailed = true;
            }
        }

        private void SaveSettings()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(AppSettings));
                using (var stream = File.Create(settingsFilePath))
                {
                    serializer.WriteObject(stream, settings ?? new AppSettings());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の保存に失敗しました: {ex.Message}");
            }
        }


        private void LoadRequests()
        {
            if (!File.Exists(requestFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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
            if (!File.Exists(frameFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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
        /// 割り当て結果を読み込みます。
        /// </summary>
        private void LoadAssignments()
        {
            if (!File.Exists(assignmentFilePath))
            {
                loadFailed = true;
                return;
            }

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
                loadFailed = true;
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
            // フラグを初期化
            loadFailed = false;

            LoadMembers();
            NormalizeMemberConstraints();
            NormalizeMemberAvailability();
            LoadFrames();
            LoadRequests();
            LoadAssignments();

            if (loadFailed)
            {
                return;
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
                DataGridViewHelper.SetColumnsNotSortable(dtRequests);
                DataGridViewHelper.FitColumnsToGrid(dtRequests);
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

            // 新規メンバー作成時は全曜日かつ全勤務時間帯を選択可能とする
            var member = new Member
            {
                Id = nextId,
                Name = "新規",
                AvailableDays = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .ToList(),
                WorksOnSaturday = true,
                WorksOnSunday = true,
                AvailableShiftNames = shiftTimes?
                    .Where(st => st != null && st.IsEnabled)
                    .Select(st => st.Name)
                    .ToList() ?? new List<string>()
            };

            // 連続勤務上限が未設定であれば 5 日を設定
            if (member.Constraints == null)
            {
                member.Constraints = new ShiftConstraints();
            }
            if (member.Constraints.MaxConsecutiveDays <= 0)
            {
                member.Constraints.MaxConsecutiveDays = 5;
            }

            try
            {
                members.Add(member);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"メンバー追加中にエラーが発生しました: {ex.Message}");
                return;
            }

            SaveMembers();
            // 新規メンバーをシフト表と希望一覧に反映
            SetupShiftGrid();
            SetupRequestGrid();
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
            SaveShiftTable();
            SaveSettings();
            base.OnFormClosing(e);
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
                    // メンバー変更をシフト表と希望一覧に反映
                    SetupShiftGrid();
                    SetupRequestGrid();
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
                    NormalizeMemberShiftNames();
                    SaveShiftTimes();
                    SaveMembers();
                    // 勤務時間マスター変更時はシフト表の内容を一旦クリア
                    SetupShiftGrid(false);
                    SaveShiftTable();
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
                settings.HolidayLimit = v;
                SaveSettings();
            }
            UpdateRequestSummary();
        }

        private void CmbDefaultRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(cmbDefaultRequired?.SelectedItem?.ToString(), out int v))
            {
                return;
            }

            settings.DefaultRequired = v;
            SaveSettings();

            int reqStart = members.Count;
            int reqEnd = shiftTable.Rows.Count - 1; // 出勤人数行以外
            for (int row = reqStart; row < reqEnd; row++)
            {
                for (int col = 1; col < shiftTable.Columns.Count; col++)
                {
                    shiftTable.Rows[row][col] = v;
                }
            }
            UpdateAttendanceCounts();
            // 保存処理はバックグラウンドで実行し、UI の応答性を確保
            SaveShiftTableAsync();
        }

        private void CmbMinHolidayCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(cmbMinHolidayCount?.SelectedItem?.ToString(), out int v))
            {
                return;
            }

            minHolidayCount = v;
            settings.MinHolidayCount = v;
            SaveSettings();
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
                DataGridViewHelper.SetColumnsNotSortable(dtRequestSummary);
                DataGridViewHelper.FitColumnsToGrid(dtRequestSummary);
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

        /// <summary>
        /// 月を変更してシフト表を再構築します。
        /// </summary>
        private void Btn月更新_Click(object? sender, EventArgs e)
        {
            SetupShiftGrid();
            SetupRequestGrid();
            if (dtp対象月 != null && dtp分析月 != null)
            {
                dtp分析月.Value = dtp対象月.Value;
            }
        }

        /// <summary>
        /// シフト自動生成ボタンのハンドラ
        /// </summary>
        private void BtnShiftGenerate_Click(object? sender, EventArgs e)
        {
            // メンバー数が変更されている可能性があるため
            // 事前にグリッドを再構築して反映させる
            SetupShiftGrid();
            GenerateRandomShifts();
            UpdateAttendanceCounts();
        }

        /// <summary>
        /// シフト表のグリッドを作成します。
        /// </summary>
        /// <summary>
        /// シフト表のグリッドを作成します。
        /// </summary>
        /// <param name="loadSavedData">保存済みシフト表を読み込むかどうか</param>
        private void SetupShiftGrid(bool loadSavedData = true)
        {
            if (dtShifts == null || dtp対象月 == null)
            {
                return;
            }

            // 有効な勤務時間を抽出して保持
            enabledShiftTimes = shiftTimes
                .Where(s => s != null && s.IsEnabled)
                .ToList();

            shiftTable = new DataTable();
            shiftTable.Columns.Add("メンバー名", typeof(string));

            // 勤怠時間ごとの集計列を作成
            foreach (var st in enabledShiftTimes)
            {
                shiftTable.Columns.Add(st.Name, typeof(int));
            }

            // 休み集計列
            shiftTable.Columns.Add("休", typeof(int));

            // 曜日ごとの出勤日数列
            string[] daysOfWeek = { "月", "火", "水", "木", "金", "土", "日" };
            foreach (var d in daysOfWeek)
            {
                shiftTable.Columns.Add(d, typeof(int));
            }

            // 日付列開始位置を記憶
            dateColumnStartIndex = shiftTable.Columns.Count;

            var baseDate = new DateTime(dtp対象月.Value.Year, dtp対象月.Value.Month, 1);
            var days = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
            for (int i = 0; i < days; i++)
            {
                var d = baseDate.AddDays(i);
                shiftTable.Columns.Add($"{d.Day}({GetJapaneseDayOfWeek(d.DayOfWeek)})", typeof(string));
            }

            foreach (var m in members)
            {
                var row = shiftTable.NewRow();
                row[0] = m.Name;
                shiftTable.Rows.Add(row);
            }

            // スキルごとの必要人数行を追加
            foreach (var sg in skillGroups)
            {
                var row = shiftTable.NewRow();
                row[0] = sg.Name;
                for (int i = 1; i < shiftTable.Columns.Count; i++)
                {
                    row[i] = settings.DefaultRequired;
                }
                shiftTable.Rows.Add(row);
            }

            // 勤怠時間ごとの必要人数行を追加
            foreach (var st in enabledShiftTimes)
            {
                var row = shiftTable.NewRow();
                row[0] = st.Name;
                for (int i = 1; i < shiftTable.Columns.Count; i++)
                {
                    row[i] = settings.DefaultRequired;
                }
                shiftTable.Rows.Add(row);
            }

            // 出勤人数行
            var countRow = shiftTable.NewRow();
            countRow[0] = "出勤人数";
            for (int i = 1; i < shiftTable.Columns.Count; i++)
            {
                countRow[i] = 0;
            }
            shiftTable.Rows.Add(countRow);

            dtShifts.DataSource = shiftTable;
            DataGridViewHelper.SetColumnsNotSortable(dtShifts);
            // 列幅をヘッダー表示に合わせて調整
            DataGridViewHelper.AdjustColumnWidthToHeader(dtShifts);
            DataGridViewHelper.FitColumnsToGrid(dtShifts);
            // メンバー名列の幅を固定
            DataGridViewHelper.SetColumnWidth(dtShifts, 0, 120);

            dtShifts.CellFormatting -= DtShifts_CellFormatting;
            dtShifts.CellFormatting += DtShifts_CellFormatting;
            dtShifts.CellBeginEdit -= DtShifts_CellBeginEdit;
            dtShifts.CellBeginEdit += DtShifts_CellBeginEdit;
            dtShifts.CellEndEdit -= DtShifts_CellEndEdit;
            dtShifts.CellEndEdit += DtShifts_CellEndEdit;
            dtShifts.EditingControlShowing -= DtShifts_EditingControlShowing;
            dtShifts.EditingControlShowing += DtShifts_EditingControlShowing;

            if (loadSavedData)
            {
                LoadShiftTable();
            }
            UpdateAttendanceCounts();
        }

        /// <summary>
        /// シフト表セルの色付け処理
        /// </summary>
        private void DtShifts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dtShifts == null || dtp対象月 == null || e.ColumnIndex < dateColumnStartIndex || e.RowIndex < 0)
            {
                return;
            }

            var baseDate = new DateTime(dtp対象月.Value.Year, dtp対象月.Value.Month, 1);
            var date = baseDate.AddDays(e.ColumnIndex - dateColumnStartIndex);

            if (JapaneseHolidayHelper.IsHoliday(date) || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                e.CellStyle.BackColor = Color.LightYellow;
            }

            // 出勤人数行の色付け
            if (shiftTable.Rows[e.RowIndex][0].ToString() == "出勤人数")
            {
                int required = 0;
                for (int r = members.Count; r < shiftTable.Rows.Count - 1; r++)
                {
                    int.TryParse(shiftTable.Rows[r][e.ColumnIndex]?.ToString(), out int v);
                    required += v;
                }

                int.TryParse(shiftTable.Rows[e.RowIndex][e.ColumnIndex]?.ToString(), out int actual);
                if (actual == required)
                {
                    e.CellStyle.BackColor = Color.LightBlue;
                }
                else if (actual < required)
                {
                    e.CellStyle.BackColor = Color.Red;
                }
                else
                {
                    e.CellStyle.BackColor = Color.LightGreen;
                }
            }
            else if (e.RowIndex < members.Count)
            {
                // メンバー行の色付け
                var val = shiftTable.Rows[e.RowIndex][e.ColumnIndex]?.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    string name = val.StartsWith("希") ? val.Substring(1) : val;
                    if (val == "希休")
                    {
                        // 希望休は赤字で表示
                        e.CellStyle.ForeColor = Color.Red;
                        e.CellStyle.BackColor = Color.LightGray;
                    }
                    else if (!string.IsNullOrEmpty(name) && name != "休")
                    {
                        var st = shiftTimes.FirstOrDefault(s => s.Name == name);
                        if (st != null)
                        {
                            try
                            {
                                e.CellStyle.BackColor = ColorTranslator.FromHtml(st.ColorCode);
                            }
                            catch
                            {
                                // 無効な色コードの場合は既定色
                            }
                        }
                    }
                    else if (name == "休")
                    {
                        e.CellStyle.BackColor = Color.LightGray;
                    }
                }
            }
        }

        /// <summary>
        /// シフトをランダム生成します。
        /// </summary>
        private void GenerateRandomShifts()
        {
            if (dtp対象月 == null)
            {
                return;
            }

            var baseDate = new DateTime(dtp対象月.Value.Year, dtp対象月.Value.Month, 1);
            var days = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);

            // シフト表から必要人数を読み取る
            var skillNeeds = new Dictionary<DateTime, Dictionary<string, int>>();
            var shiftNeeds = new Dictionary<DateTime, Dictionary<string, int>>();

            for (int col = dateColumnStartIndex; col < dateColumnStartIndex + days; col++)
            {
                var date = baseDate.AddDays(col - dateColumnStartIndex);

                var sreq = new Dictionary<string, int>();
                for (int r = 0; r < skillGroups.Count; r++)
                {
                    int rowIndex = members.Count + r;
                    int.TryParse(shiftTable.Rows[rowIndex][col]?.ToString(), out int v);
                    sreq[skillGroups[r].Name] = v;
                }
                skillNeeds[date] = sreq;

                var shreq = new Dictionary<string, int>();
                for (int r = 0; r < enabledShiftTimes.Count; r++)
                {
                    int rowIndex = members.Count + skillGroups.Count + r;
                    int.TryParse(shiftTable.Rows[rowIndex][col]?.ToString(), out int v);
                    shreq[enabledShiftTimes[r].Name] = v;
                }
                shiftNeeds[date] = shreq;
            }

            var assignmentsMap = ShiftGeneratorGreedy.Generate(
                members,
                baseDate,
                days,
                skillNeeds,
                shiftNeeds,
                shiftRequests,
                skillGroups,
                enabledShiftTimes,
                minHolidayCount);

            // 結果をテーブルへ反映
            for (int i = 0; i < members.Count; i++)
            {
                var m = members[i];
                if (!assignmentsMap.TryGetValue(m.Id, out var dayMap))
                {
                    continue;
                }

                for (int d = 0; d < days; d++)
                {
                    var date = baseDate.AddDays(d);
                    if (dayMap.TryGetValue(date, out var value))
                    {
                        shiftTable.Rows[i][dateColumnStartIndex + d] = value;
                    }
                }
            }

            UpdateAttendanceCounts();
            dtShifts.Refresh();
            SaveShiftTable();
        }

        /// <summary>
        /// 出勤人数行を更新します。
        /// </summary>
        private void UpdateAttendanceCounts()
        {
            if (shiftTable.Rows.Count <= members.Count)
            {
                return;
            }

            var countRow = shiftTable.Rows[shiftTable.Rows.Count - 1];

            for (int col = dateColumnStartIndex; col < shiftTable.Columns.Count; col++)
            {
                int count = 0;
                for (int row = 0; row < members.Count; row++)
                {
                    var v = shiftTable.Rows[row][col]?.ToString();
                    if (!string.IsNullOrEmpty(v) && v != "休" && v != "希休")
                    {
                        count++;
                    }
                }
                countRow[col] = count;

                // カラー更新は CellFormatting で処理
            }

            UpdateMemberSummaryColumns();
            dtShifts.Refresh();
        }

        /// <summary>
        /// メンバーごとの集計列を更新します。
        /// </summary>
        private void UpdateMemberSummaryColumns()
        {
            if (dtp対象月 == null)
            {
                return;
            }

            var baseDate = new DateTime(dtp対象月.Value.Year, dtp対象月.Value.Month, 1);
            var days = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
            // 勤怠時間列の後ろに休列が入るため +1 する
            int weekdayStart = 1 + enabledShiftTimes.Count + 1; // 月曜日列の開始位置

            for (int row = 0; row < members.Count; row++)
            {
                int[] shiftCount = new int[enabledShiftTimes.Count];
                int restCount = 0;
                int[] weekdayCount = new int[7];

                for (int col = dateColumnStartIndex; col < dateColumnStartIndex + days; col++)
                {
                    string val = shiftTable.Rows[row][col]?.ToString() ?? string.Empty;
                    string name = val.StartsWith("希") ? val.Substring(1) : val;

                    var date = baseDate.AddDays(col - dateColumnStartIndex);

                    if (name == "休" || name == "希休")
                    {
                        restCount++;
                    }
                    else if (!string.IsNullOrEmpty(name))
                    {
                        int idx = enabledShiftTimes.FindIndex(s => s.Name == name);
                        if (idx >= 0)
                        {
                            shiftCount[idx]++;
                        }
                        weekdayCount[(int)date.DayOfWeek]++;
                    }
                }

                // 勤怠時間列
                for (int i = 0; i < enabledShiftTimes.Count; i++)
                {
                    shiftTable.Rows[row][1 + i] = shiftCount[i];
                }

                // 休列
                shiftTable.Rows[row][1 + enabledShiftTimes.Count] = restCount;

                // 曜日列 (月〜日)
                shiftTable.Rows[row][weekdayStart + 0] = weekdayCount[(int)DayOfWeek.Monday];
                shiftTable.Rows[row][weekdayStart + 1] = weekdayCount[(int)DayOfWeek.Tuesday];
                shiftTable.Rows[row][weekdayStart + 2] = weekdayCount[(int)DayOfWeek.Wednesday];
                shiftTable.Rows[row][weekdayStart + 3] = weekdayCount[(int)DayOfWeek.Thursday];
                shiftTable.Rows[row][weekdayStart + 4] = weekdayCount[(int)DayOfWeek.Friday];
                shiftTable.Rows[row][weekdayStart + 5] = weekdayCount[(int)DayOfWeek.Saturday];
                shiftTable.Rows[row][weekdayStart + 6] = weekdayCount[(int)DayOfWeek.Sunday];
            }
        }

        /// <summary>
        /// セル編集完了時に出勤人数を更新します。
        /// </summary>
        private void DtShifts_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (dtShifts == null)
            {
                return;
            }

            object? value = dtShifts[e.ColumnIndex, e.RowIndex].Value;

            try
            {
                var cell = dtShifts[e.ColumnIndex, e.RowIndex];
                if (cell is DataGridViewComboBoxCell)
                {
                    // コンボボックスセルは編集終了時にテキストボックスに戻す
                    dtShifts[e.ColumnIndex, e.RowIndex] = new DataGridViewTextBoxCell { Value = value };
                }

                // 選択されている複数セルに同じ値を設定する
                if (dtShifts.SelectedCells.Count > 1)
                {
                    foreach (DataGridViewCell c in dtShifts.SelectedCells)
                    {
                        if (c.RowIndex == e.RowIndex && c.ColumnIndex == e.ColumnIndex)
                        {
                            continue; // 編集対象セルは既に処理済み
                        }

                        if (c.ColumnIndex < dateColumnStartIndex || c.ReadOnly)
                        {
                            continue; // 日付列以外や読み取り専用セルは除外
                        }

                        c.Value = value;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"セル値の反映中にエラーが発生しました: {ex.Message}");
            }

            UpdateAttendanceCounts();
            // 保存処理はバックグラウンドで実行し、UI の応答性を確保
            SaveShiftTableAsync();
        }

        /// <summary>
        /// 編集開始時にセルの種類を差し替えます。
        /// メンバー行の日付列のみコンボボックスを表示します。
        /// </summary>
        private void DtShifts_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (dtShifts == null)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < dateColumnStartIndex)
            {
                return;
            }

            // メンバー行のみ対象
            if (e.RowIndex >= members.Count)
            {
                return;
            }

            var cell = dtShifts[e.ColumnIndex, e.RowIndex];
            if (cell is DataGridViewComboBoxCell)
            {
                return;
            }

            var combo = new DataGridViewComboBoxCell();
            // 有効な勤務時間のみ候補として追加
            foreach (var st in enabledShiftTimes)
            {
                combo.Items.Add(st.Name);
            }
            combo.Items.Add("休");
            combo.Value = cell.Value;
            dtShifts[e.ColumnIndex, e.RowIndex] = combo;
        }

        /// <summary>
        /// 編集コントロール表示時に入力制限を設定します。
        /// 必要人数を入力する行では数値のみ入力可能にします。
        /// </summary>
        private void DtShifts_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dtShifts?.CurrentCell == null)
            {
                return;
            }

            int rowIndex = dtShifts.CurrentCell.RowIndex;
            int colIndex = dtShifts.CurrentCell.ColumnIndex;
            if (rowIndex < 0 || colIndex < 0)
            {
                return;
            }

            bool isRequirementRow = rowIndex >= members.Count && rowIndex < shiftTable.Rows.Count - 1;
            if (isRequirementRow && e.Control is TextBox tb)
            {
                tb.KeyPress -= NumericTextBox_KeyPress;
                tb.KeyPress += NumericTextBox_KeyPress;
            }
            else if (e.Control is TextBox tb2)
            {
                tb2.KeyPress -= NumericTextBox_KeyPress;
            }
        }

        /// <summary>
        /// 数値以外の入力を抑制します。
        /// </summary>
        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void LoadShiftTable()
        {
            if (!File.Exists(shiftTableFilePath))
            {
                loadFailed = true;
                return;
            }

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(List<List<string>>));
                using (var stream = File.OpenRead(shiftTableFilePath))
                {
                    var data = serializer.ReadObject(stream) as List<List<string>>;
                    if (data == null)
                    {
                        return;
                    }

                    // 1列目の値をキーとした辞書に変換
                    var rowMap = new Dictionary<string, List<string>>();
                    foreach (var row in data)
                    {
                        if (row == null || row.Count == 0)
                        {
                            continue;
                        }

                        var key = row[0] ?? string.Empty;
                        if (!rowMap.ContainsKey(key))
                        {
                            rowMap[key] = row;
                        }
                    }

                    // 現在のテーブルの行名を基にデータを上書き
                    foreach (DataRow row in shiftTable.Rows)
                    {
                        var name = row[0]?.ToString() ?? string.Empty;
                        if (!rowMap.TryGetValue(name, out var savedRow))
                        {
                            continue; // 保存データに該当行がなければスキップ
                        }

                        int cols = Math.Min(savedRow.Count, shiftTable.Columns.Count);
                        for (int c = 1; c < cols; c++)
                        {
                            row[c] = savedRow[c];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"シフト表の読み込みに失敗しました: {ex.Message}");
                loadFailed = true;
            }
        }

        private void SaveShiftTable()
        {
            SaveShiftTableInternal(shiftTable);
        }

        /// <summary>
        /// 非同期でシフト表を保存します。UI スレッドをブロックしないようにします。
        /// </summary>
        private void SaveShiftTableAsync()
        {
            // 編集中のテーブルをコピーしてバックグラウンドで保存
            var copy = shiftTable.Copy();
            Task.Run(() => SaveShiftTableInternal(copy));
        }

        /// <summary>
        /// 実際の保存処理本体。引数で渡されたテーブルをファイルに書き込みます。
        /// </summary>
        /// <param name="table">保存対象のテーブル</param>
        private void SaveShiftTableInternal(DataTable table)
        {
            try
            {
                var data = new List<List<string>>();
                foreach (DataRow row in table.Rows)
                {
                    var list = new List<string>();
                    foreach (var item in row.ItemArray)
                    {
                        list.Add(item?.ToString() ?? string.Empty);
                    }
                    data.Add(list);
                }

                var serializer = new DataContractJsonSerializer(typeof(List<List<string>>));

                lock (saveLock)
                {
                    using (var stream = File.Create(shiftTableFilePath))
                    {
                        serializer.WriteObject(stream, data);
                    }
                }
            }
            catch (Exception ex)
            {
                // バックグラウンドスレッドから呼び出される可能性があるため、Invoke を使用
                if (InvokeRequired)
                {
                    Invoke(new Action(() => MessageBox.Show($"シフト表の保存に失敗しました: {ex.Message}")));
                }
                else
                {
                    MessageBox.Show($"シフト表の保存に失敗しました: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 保存データをすべて削除して初期状態に戻します。
        /// </summary>
        private void ResetAllData()
        {
            try
            {
                if (Directory.Exists(dataDir))
                {
                    Directory.Delete(dataDir, true);
                }
                Directory.CreateDirectory(dataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データディレクトリのリセットに失敗しました: {ex.Message}");
            }

            // 空のデータを生成
            members = new List<Member>();
            shiftFrames = new List<ShiftFrame>();
            assignments = new List<ShiftAssignment>();
            shiftRequests = new List<ShiftRequest>();
            customHolidays = new List<CustomHoliday>();
            skillGroups = new List<SkillGroup>();
            shiftTimes = new List<ShiftTime>();
            settings = new AppSettings();
            shiftTable = new DataTable();

            // 空データを保存しておく
            SaveMembers();
            SaveFrames();
            SaveAssignments();
            SaveRequests();
            SaveHolidays();
            SaveSkillGroups();
            SaveShiftTimes();
            SaveSettings();
            SaveShiftTable();
        }

        // このメソッドの内容は MainForm.Designer.cs に移動しました。
    }
}

