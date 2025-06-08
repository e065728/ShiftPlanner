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

        public MemberMasterForm(List<Member> members)
        {
            _members = new BindingList<Member>(members ?? new List<Member>());
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
                AvailableFrom = new TimeSpan(9, 0, 0),
                AvailableTo = new TimeSpan(18, 0, 0),
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
            SetColumnsNotSortable(dtMembers);
        }

        /// <summary>
        /// 指定グリッドの列をすべてソート不可にします。
        /// </summary>
        private static void SetColumnsNotSortable(DataGridView grid)
        {
            if (grid == null || grid.Columns == null)
            {
                return;
            }
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column != null)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }
    }
}
