using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ShiftPlanner
{
    public partial class MainForm : Form
    {
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private DataGridView dtShift;
        private Button btnHistory;
        private BindingList<ShiftAssignment> shifts = new BindingList<ShiftAssignment>();
        private List<ChangeRecord> history = new List<ChangeRecord>();
        private List<Member> members = new List<Member>();
        private int rowIndexFromMouseDown;
        private Rectangle dragBoxFromMouseDown;

        public MainForm()
        {
            InitializeComponent(); // これだけでOK
            InitializeData();
            SetupDataGridView();
        }

        private void InitializeData()
        {
            // サンプルデータ
            shifts.Add(new ShiftAssignment { MemberName = "Alice", Date = DateTime.Today, ShiftType = "早番" });
            shifts.Add(new ShiftAssignment { MemberName = "Bob", Date = DateTime.Today, ShiftType = "遅番" });
            shifts.Add(new ShiftAssignment { MemberName = "Carol", Date = DateTime.Today.AddDays(1), ShiftType = "早番" });
        }

        private void SetupDataGridView()
        {
            dtShift.DataSource = shifts;
            dtShift.AllowDrop = true;
            dtShift.MouseDown += DtShift_MouseDown;
            dtShift.MouseMove += DtShift_MouseMove;
            dtShift.DragOver += DtShift_DragOver;
            dtShift.DragDrop += DtShift_DragDrop;
        }

        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtShift = new System.Windows.Forms.DataGridView();
            this.btnHistory = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).BeginInit();
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
            this.tabPage1.Controls.Add(this.btnHistory);
            this.tabPage1.Controls.Add(this.dtShift);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1385, 838);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "シフト";
            this.tabPage1.UseVisualStyleBackColor = true;
            //
            // dtShift
            //
            this.dtShift.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtShift.Location = new System.Drawing.Point(6, 6);
            this.dtShift.Name = "dtShift";
            this.dtShift.Size = new System.Drawing.Size(600, 300);
            this.dtShift.TabIndex = 0;
            //
            // btnHistory
            //
            this.btnHistory.Location = new System.Drawing.Point(6, 312);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(75, 23);
            this.btnHistory.TabIndex = 1;
            this.btnHistory.Text = "履歴";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 74);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1398, 889);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtShift)).EndInit();
            this.ResumeLayout(false);

        }

        private void DtShift_MouseDown(object sender, MouseEventArgs e)
        {
            rowIndexFromMouseDown = dtShift.HitTest(e.X, e.Y).RowIndex;
            if (rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void DtShift_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    dtShift.DoDragDrop(dtShift.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        private void DtShift_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void DtShift_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = dtShift.PointToClient(new Point(e.X, e.Y));
            int rowIndex = dtShift.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
            if (rowIndex < 0)
            {
                rowIndex = dtShift.Rows.Count - 1;
            }

            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                var row = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                var item = shifts[rowIndexFromMouseDown];
                if (rowIndexFromMouseDown != rowIndex)
                {
                    shifts.RemoveAt(rowIndexFromMouseDown);
                    shifts.Insert(rowIndex, item);
                    history.Add(new ChangeRecord
                    {
                        Time = DateTime.Now,
                        Description = $"{item.MemberName} の {item.ShiftType} を {rowIndexFromMouseDown} から {rowIndex} に移動"
                    });
                }
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var h in history)
            {
                sb.AppendLine(h.ToString());
            }
            MessageBox.Show(sb.ToString(), "変更履歴");
        }
    }
}