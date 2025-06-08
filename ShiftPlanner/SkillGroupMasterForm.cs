using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// スキルグループマスター編集用フォーム
    /// </summary>
    public partial class SkillGroupMasterForm : Form
    {
        private readonly BindingList<SkillGroup> _skillGroups;

        public SkillGroupMasterForm(List<SkillGroup> groups)
        {
            _skillGroups = new BindingList<SkillGroup>(groups ?? new List<SkillGroup>());
            InitializeComponent();
            dtSkillGroups.DataSource = _skillGroups;
            SetupGrid();
        }

        /// <summary>
        /// 編集後のスキルグループ一覧を取得します。
        /// </summary>
        public List<SkillGroup> SkillGroups => _skillGroups.ToList();

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var nextId = _skillGroups.Count > 0 ? _skillGroups.Max(g => g.Id) + 1 : 1;
            _skillGroups.Add(new SkillGroup { Id = nextId, Name = "新規グループ" });
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (dtSkillGroups.CurrentRow?.DataBoundItem is SkillGroup g)
            {
                _skillGroups.Remove(g);
            }
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SetupGrid()
        {
            dtSkillGroups.AutoGenerateColumns = true;
            foreach (DataGridViewColumn col in dtSkillGroups.Columns)
            {
                if (col == null || string.IsNullOrEmpty(col.Name))
                {
                    continue;
                }

                switch (col.Name)
                {
                    case nameof(SkillGroup.Id):
                        col.HeaderText = "ID";
                        break;
                    case nameof(SkillGroup.Name):
                        col.HeaderText = "名称";
                        break;
                }
            }
        }
    }
}
