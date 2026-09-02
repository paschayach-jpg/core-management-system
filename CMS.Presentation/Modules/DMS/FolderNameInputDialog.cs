using System;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog ขนาดเล็กสำหรับขอ "ชื่อโฟลเดอร์ใหม่" — ใช้จาก FrmExternalUpload
    /// เมื่อผู้ใช้กดปุ่ม "สร้างโฟลเดอร์" (ระดับความลับมาจาก dialog แม่ ไม่ต้องเลือกที่นี่).
    /// </summary>
    public partial class FolderNameInputDialog : Form
    {
        public string FolderName { get; private set; }

        public FolderNameInputDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var name = (txtName.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("กรุณาระบุชื่อโฟลเดอร์", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                txtName.Focus();
                return;
            }

            FolderName = name;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
