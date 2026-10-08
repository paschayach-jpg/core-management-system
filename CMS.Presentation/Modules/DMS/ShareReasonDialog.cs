using System;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog ขอ "เหตุผล" ก่อนยกเลิก/ปฏิเสธการแชร์ — ใช้จาก ShareManagementForm.
    /// </summary>
    public partial class ShareReasonDialog : Form
    {
        public string Reason { get; private set; }

        public ShareReasonDialog(string prompt)
        {
            InitializeComponent();
            lblPrompt.Text = prompt ?? string.Empty;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var reason = (txtReason.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(reason))
            {
                MessageBox.Show("กรุณาระบุเหตุผล", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                txtReason.Focus();
                return;
            }

            Reason = reason;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
