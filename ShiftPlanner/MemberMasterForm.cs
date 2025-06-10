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
            _members.Add(new Member
            {
                Id = nextId,
                Name = "新規",
                WorksOnSaturday = false,
                WorksOnSunday = false
            });
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
                var combo = new DataGridViewComboBoxColumn
                {
                    Name = nameof(Member.SkillGroup),
                    DataPropertyName = nameof(Member.SkillGroup),
                    DataSource = _skillGroups?.Select(g => g.Name).ToList() ?? new List<string>(),
                    HeaderText = "スキルグループ"
                };
                dtMembers.Columns.Insert(index, combo);
            }

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
                        col.HeaderText = "勤務可能曜日";
                        break;
                    case nameof(Member.Skills):
                        col.HeaderText = "スキル";
                        break;
                    case nameof(Member.SkillGroup):
                        col.HeaderText = "スキルグループ";
                        break;
                    case nameof(Member.DesiredHolidays):
                        col.HeaderText = "希望休";
                        break;
                    case nameof(Member.Constraints):
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

            // 出勤時間ごとの可否チェック列を追加
            foreach (var st in _shiftTimes)
            {
                if (st == null)
                {
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
            DataGridViewHelper.SetColumnsNotSortable(dtMembers);
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
            if (column != null && column.Name.StartsWith("Shift_"))
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
            if (column != null && column.Name.StartsWith("Shift_"))
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

                    e.ParsingApplied = true;
                }
            }
        }

        /// <summary>
        /// 指定グリッドの列をすべてソート不可にします。
    }
}
