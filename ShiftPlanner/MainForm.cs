using System.Collections.Generic;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class MainForm : Form
    {
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DataGridView dtMember;
        private DataGridView dtRequests;
        private Button btnAddRequest;
        private List<Member> members = new List<Member>();
        private List<ShiftRequest> requests = new List<ShiftRequest>();

        public MainForm()
        {
            InitializeComponent(); // これだけでOK
            InitializeData();
            SetupDataGridView();
        }

        private void InitializeData()
        {
            // メンバー初期データ
            members.Add(new Member { Id = 1, Name = "田中" });
            members.Add(new Member { Id = 2, Name = "佐藤" });
        }

        private void SetupDataGridView()
        {
            dtMember.AutoGenerateColumns = true;
            dtRequests.AutoGenerateColumns = true;
            dtMember.DataSource = members; // デザイナで作ったdtMemberにデータをセットするだけ
            dtRequests.DataSource = requests;
        }

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtMember = new System.Windows.Forms.DataGridView();
            this.dtRequests = new System.Windows.Forms.DataGridView();
            this.btnAddRequest = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtMember)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1393, 864);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1385, 838);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "メンバー";
            this.tabPage1.UseVisualStyleBackColor = true;
            // dtMember
            this.dtMember.Location = new System.Drawing.Point(6, 6);
            this.dtMember.Size = new System.Drawing.Size(600, 400);
            this.dtMember.Name = "dtMember";
            this.tabPage1.Controls.Add(this.dtMember);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1385, 838);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "希望";
            this.tabPage2.UseVisualStyleBackColor = true;
            // dtRequests
            this.dtRequests.Location = new System.Drawing.Point(6, 6);
            this.dtRequests.Size = new System.Drawing.Size(600, 400);
            this.dtRequests.Name = "dtRequests";
            this.tabPage2.Controls.Add(this.dtRequests);
            // btnAddRequest
            this.btnAddRequest.Location = new System.Drawing.Point(6, 412);
            this.btnAddRequest.Text = "希望追加";
            this.btnAddRequest.Name = "btnAddRequest";
            this.btnAddRequest.Click += new System.EventHandler(this.btnAddRequest_Click);
            this.tabPage2.Controls.Add(this.btnAddRequest);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtMember)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtRequests)).EndInit();
            this.ResumeLayout(false);

        }

        private void btnAddRequest_Click(object sender, System.EventArgs e)
        {
            using (var form = new RequestForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    requests.Add(form.Request);
                    dtRequests.DataSource = null;
                    dtRequests.DataSource = requests;
                }
            }
        }
    }
}
