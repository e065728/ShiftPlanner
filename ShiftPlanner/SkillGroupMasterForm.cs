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
            // 編集中の値が失われないように編集確定イベントを設定
            dtSkillGroups.CurrentCellDirtyStateChanged += DtSkillGroups_CurrentCellDirtyStateChanged;
            // 入力エラー時のメッセージ表示
            dtSkillGroups.DataError += DtSkillGroups_DataError;
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
            // 編集中のセルがあれば確定する
            dtSkillGroups.EndEdit();
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

            // 列幅をフォームのサイズに合わせて調整
            DataGridViewHelper.SetColumnsNotSortable(dtSkillGroups);
            DataGridViewHelper.FitColumnsToGrid(dtSkillGroups);
        }

        /// <summary>
        /// セル編集確定時に値をコミットします。
        /// </summary>
        private void DtSkillGroups_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dtSkillGroups.IsCurrentCellDirty)
            {
                dtSkillGroups.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        /// <summary>
        /// データエラー発生時に警告を表示します。
        /// </summary>
        private void DtSkillGroups_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            string message = e.Exception?.Message ?? "入力値が正しくありません。";
            MessageBox.Show($"データエラー: {message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
