using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// メンバー情報を編集するフォーム
    /// </summary>
    public partial class MemberMasterForm : Form
    {
        private readonly BindingList<Member> _members;
        private readonly List<SkillGroup> _skillGroups;
        private readonly List<ShiftTime> _shiftTimes;

        public MemberMasterForm(List<Member> members, List<SkillGroup> skillGroups, List<ShiftTime> shiftTimes)
        {
            _members = new BindingList<Member>(members ?? new List<Member>());
            _skillGroups = skillGroups ?? new List<SkillGroup>();
            _shiftTimes = shiftTimes ?? new List<ShiftTime>();
            InitializeComponent();
            dtMembers.DataSource = _members;
            SetupMemberGrid();
        }

        /// <summary>
        /// メンバー一覧を取得します。
        /// </summary>
        public List<Member> Members => _members.ToList();

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var nextId = _members.Count > 0 ? _members.Max(m => m.Id) + 1 : 1;
            // 新規メンバー作成時は全曜日勤務可能とする
            var member = new Member
            {
                Id = nextId,
                Name = "新規",
                AvailableDays = Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .ToList(),
                WorksOnSaturday = true,
                WorksOnSunday = true
            };


            try
            {
                _members.Add(member);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"メンバー追加中にエラーが発生しました: {ex.Message}");
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dtMembers.CurrentRow?.DataBoundItem is Member m)
            {
                _members.Remove(m);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void SetupMemberGrid()
        {
            dtMembers.AutoGenerateColumns = true;

            // スキルグループ列をコンボボックスに置き換える
            var sgCol = dtMembers.Columns[nameof(Member.SkillGroup)];
            if (sgCol != null)
            {
                int index = sgCol.Index;
                dtMembers.Columns.Remove(sgCol);

                // スキルグループはメンバー側に存在する値も含めてリスト化
                var sgList = _skillGroups?.Select(g => g.Name).ToList() ?? new List<string>();
                var memberGroups = _members.Where(m => !string.IsNullOrEmpty(m.SkillGroup))
                                           .Select(m => m.SkillGroup);
                foreach (var g in memberGroups)
                {
                    if (!sgList.Contains(g))
                    {
                        sgList.Add(g);
                    }
                }

                var combo = new DataGridViewComboBoxColumn
                {
                    Name = nameof(Member.SkillGroup),
                    DataPropertyName = nameof(Member.SkillGroup),
                    DataSource = sgList,
                    HeaderText = "スキルグループ",
                    DisplayStyleForCurrentCellOnly = true
                };
                dtMembers.Columns.Insert(index, combo);
            }

            // 自動生成された列に日本語ヘッダーを設定
            foreach (DataGridViewColumn col in dtMembers.Columns)
            {
                if (col == null || string.IsNullOrEmpty(col.Name))
                {
                    continue;
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
                        // この列は後で曜日ごとのチェックボックス列に置き換えるため非表示
                        col.Visible = false;
                        break;
                    case nameof(Member.AvailableShiftNames):
                        // List 型列は直接編集しないため非表示
                        col.Visible = false;
                        break;
                    case nameof(Member.Skills):
                        col.HeaderText = "スキル";
                        // List 型列は直接編集しないため読み取り専用とする
                        col.ReadOnly = true;
                        break;
                    case nameof(Member.SkillGroup):
                        col.HeaderText = "スキルグループ";
                        break;
                    case nameof(Member.DesiredHolidays):
                        col.HeaderText = "希望休";
                        // List 型列は直接編集しないため読み取り専用とする
                        col.ReadOnly = true;
                        break;
                    case nameof(Member.Constraints):
                        col.Visible = false;
                        break;
                    case nameof(Member.WorksOnSaturday):
                        // 個別の曜日列を表示するため非表示
                        col.Visible = false;
                        break;
                    case nameof(Member.WorksOnSunday):
                        // 個別の曜日列を表示するため非表示
                        col.Visible = false;
                        break;
                }
            }

            // 曜日ごとの勤務可否チェック列を追加
            var days = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .OrderBy(d => (int)d == 0 ? 7 : (int)d); // 月曜始まりでソート

            foreach (DayOfWeek day in days)
            {
                var dayCol = new DataGridViewCheckBoxColumn
                {
                    Name = $"Day_{day}",
                    HeaderText = GetJapaneseDayName(day),
                    DataPropertyName = string.Empty
                };
                dtMembers.Columns.Add(dayCol);
            }

            // 出勤時間ごとの可否チェック列を追加
            foreach (var st in _shiftTimes)
            {
                // null チェックも兼ねて有効な勤務時間のみ追加
                if (st?.IsEnabled != true)
                {
                    // 無効な勤務時間は表示しない
                    continue;
                }

                var col = new DataGridViewCheckBoxColumn
                {
                    Name = $"Shift_{st.Name}",
                    HeaderText = st.Name,
                    DataPropertyName = string.Empty
                };
                try
                {
                    col.HeaderCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml(st.ColorCode);
                }
                catch
                {
                    // 無効な色コードは無視
                }
                dtMembers.Columns.Add(col);
            }

            dtMembers.CellFormatting += DtMembers_CellFormatting;
            dtMembers.CellParsing += DtMembers_CellParsing;
            dtMembers.CurrentCellDirtyStateChanged += DtMembers_CurrentCellDirtyStateChanged;
            // データエラー時に独自メッセージを表示する
            dtMembers.DataError += DtMembers_DataError;
            DataGridViewHelper.SetColumnsNotSortable(dtMembers);
            // フォームサイズに合わせて列幅を自動調整
            DataGridViewHelper.FitColumnsToGrid(dtMembers);
        }

        private void DtMembers_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dtMembers.IsCurrentCellDirty)
            {
                dtMembers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DtMembers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var column = dtMembers.Columns[e.ColumnIndex];
            if (column != null && column.Name.StartsWith("Day_"))
            {
                if (dtMembers.Rows[e.RowIndex].DataBoundItem is Member m)
                {
                    if (Enum.TryParse(column.Name.Substring(4), out DayOfWeek day))
                    {
                        e.Value = m.AvailableDays != null && m.AvailableDays.Contains(day);
                        e.FormattingApplied = true;
                    }
                }
            }
            else if (column != null && column.Name.StartsWith("Shift_"))
            {
                if (dtMembers.Rows[e.RowIndex].DataBoundItem is Member m)
                {
                    string shiftName = column.HeaderText;
                    e.Value = m.AvailableShiftNames != null && m.AvailableShiftNames.Contains(shiftName);
                    e.FormattingApplied = true;
                }
            }
        }

        private void DtMembers_CellParsing(object? sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var column = dtMembers.Columns[e.ColumnIndex];
            if (column != null && column.Name.StartsWith("Day_"))
            {
                if (dtMembers.Rows[e.RowIndex].DataBoundItem is Member m)
                {
                    bool val = false;
                    if (e.Value != null && bool.TryParse(e.Value.ToString(), out bool b))
                    {
                        val = b;
                    }

                    if (Enum.TryParse(column.Name.Substring(4), out DayOfWeek day))
                    {
                        if (val)
                        {
                            if (m.AvailableDays == null)
                            {
                                m.AvailableDays = new List<DayOfWeek>();
                            }
                            if (!m.AvailableDays.Contains(day))
                            {
                                m.AvailableDays.Add(day);
                            }
                        }
                        else
                        {
                            m.AvailableDays?.Remove(day);
                        }

                        // 土日の列は旧プロパティとも連動させる
                        if (day == DayOfWeek.Saturday)
                        {
                            m.WorksOnSaturday = val;
                        }
                        else if (day == DayOfWeek.Sunday)
                        {
                            m.WorksOnSunday = val;
                        }
                    }

                    // DataGridView に設定する値も更新しておく
                    e.Value = val;
                    e.ParsingApplied = true;
                }
            }
            else if (column != null && column.Name.StartsWith("Shift_"))
            {
                if (dtMembers.Rows[e.RowIndex].DataBoundItem is Member m)
                {
                    string shiftName = column.HeaderText;
                    bool val = false;
                    if (e.Value != null && bool.TryParse(e.Value.ToString(), out bool b))
                    {
                        val = b;
                    }

                    if (val)
                    {
                        if (m.AvailableShiftNames == null)
                        {
                            m.AvailableShiftNames = new List<string>();
                        }
                        if (!m.AvailableShiftNames.Contains(shiftName))
                        {
                            m.AvailableShiftNames.Add(shiftName);
                        }
                    }
                    else
                    {
                        m.AvailableShiftNames?.Remove(shiftName);
                    }

                    // 入力値を DataGridView に反映
                    e.Value = val;
                    e.ParsingApplied = true;
                }
            }
        }

        /// <summary>
        /// DataGridViewのデータエラーを処理します。
        /// </summary>
        private void DtMembers_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            // 既定の例外ダイアログを抑制
            e.ThrowException = false;

            // 例外メッセージが取得できない場合も考慮
            string message = e.Exception?.Message ?? "データ形式が正しくありません。";
            MessageBox.Show($"入力値に誤りがあります: {message}", "データエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// DayOfWeek を日本語一文字表記へ変換します。
        /// </summary>
        private static string GetJapaneseDayName(DayOfWeek day)
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
                default:
                    return day.ToString();
            }
        }
    }
}
