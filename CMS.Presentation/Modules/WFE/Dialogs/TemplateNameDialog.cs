using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.Workflow.Dialogs
{
    public class TemplateNameDialog : Form
    {
        private Label lblTitle;
        private Label lblName;
        private Label lblDesc;
        private TextBox txtName;
        private TextBox txtDesc;
        private Button btnOK;
        private Button btnCancel;

        public string TemplateName => txtName.Text.Trim();
        public string TemplateDescription => txtDesc.Text.Trim();

        public TemplateNameDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblName = new Label();
            this.lblDesc = new Label();
            this.txtName = new TextBox();
            this.txtDesc = new TextBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();

            this.SuspendLayout();

            // Form
            this.Text = "บันทึก Template";
            this.ClientSize = new Size(400, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Segoe UI", 9F);
            this.BackColor = Color.White;

            // Title
            this.lblTitle.Text = "ตั้งชื่อและรายละเอียด Workflow Template";
            this.lblTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(22, 36, 62);
            this.lblTitle.Location = new Point(16, 16);
            this.lblTitle.Size = new Size(368, 24);

            // Name label & box
            this.lblName.Text = "ชื่อ Template:";
            this.lblName.Location = new Point(16, 56);
            this.lblName.Size = new Size(100, 20);

            this.txtName.Location = new Point(16, 76);
            this.txtName.Size = new Size(368, 24);

            // Desc label & box
            this.lblDesc.Text = "รายละเอียด:";
            this.lblDesc.Location = new Point(16, 110);
            this.lblDesc.Size = new Size(100, 20);

            this.txtDesc.Location = new Point(16, 130);
            this.txtDesc.Size = new Size(368, 54);
            this.txtDesc.Multiline = true;

            // Buttons
            this.btnOK.Text = "บันทึก";
            this.btnOK.Location = new Point(220, 196);
            this.btnOK.Size = new Size(80, 28);
            this.btnOK.FlatStyle = FlatStyle.Flat;
            this.btnOK.BackColor = Color.FromArgb(37, 99, 235);
            this.btnOK.ForeColor = Color.White;
            this.btnOK.Cursor = Cursors.Hand;
            this.btnOK.Click += BtnOK_Click;

            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.Location = new Point(304, 196);
            this.btnCancel.Size = new Size(80, 28);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDesc);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TemplateName))
            {
                MessageBox.Show("กรุณากรอกชื่อ Template", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
