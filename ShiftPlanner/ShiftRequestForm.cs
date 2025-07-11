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
        /// <param name="初期種別">初期表示する種別</param>
        public ShiftRequestForm(List<Member> members, 申請種別 初期種別 = 申請種別.希望休)
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

            // 種別コンボボックスの初期値を設定
            cmb種別.DataSource = Enum.GetValues(typeof(申請種別));
            cmb種別.SelectedItem = 初期種別;
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

            if (cmb種別.SelectedItem == null)
            {
                MessageBox.Show("種別を選択してください。");
                return;
            }

            this.ShiftRequest = new ShiftRequest
            {
                MemberId = member.Id,
                Date = dtpDate.Value.Date,
                種別 = (申請種別)cmb種別.SelectedItem
            };
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
