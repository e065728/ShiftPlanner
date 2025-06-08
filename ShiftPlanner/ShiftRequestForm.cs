using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ShiftPlanner
{
    /// <summary>
    /// シフト希望を入力するための簡易フォーム。
    /// </summary>
    // Designer で定義されているクラスと結合するため partial とする
    public partial class ShiftRequestForm : Form
    {
        // 以下のUIコントロール定義はデザイナー部に移動しました
        private readonly List<Member> members;

        public ShiftRequest? ShiftRequest { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="members">メンバー一覧</param>
        /// <param name="holidayChecked">休み希望チェックの初期値</param>
        public ShiftRequestForm(List<Member> members, bool holidayChecked = false)
        {
            this.members = members ?? new List<Member>();
            InitializeComponent();

            try
            {
                // メンバー情報をコンボボックスへ設定
                cmbMember.DataSource = this.members;
                cmbMember.DisplayMember = "Name";
                cmbMember.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                // 例外が発生してもアプリが終了しないよう通知のみ
                MessageBox.Show($"メンバー情報の読込に失敗しました: {ex.Message}");
            }

            // 休み希望チェックボックスの初期状態を設定
            chkHoliday.Checked = holidayChecked;
        }

        // このメソッドの内容は ShiftRequestForm.Designer.cs に移動しました。

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var member = cmbMember.SelectedItem as Member;
            if (member == null)
            {
                MessageBox.Show("メンバーを選択してください。");
                return;
            }

            this.ShiftRequest = new ShiftRequest
            {
                MemberId = member.Id,
                Date = dtpDate.Value.Date,
                IsHolidayRequest = chkHoliday.Checked
            };
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
