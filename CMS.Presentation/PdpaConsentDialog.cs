using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// แสดงนโยบายข้อมูลส่วนบุคคล (PDPA) และรับการยืนยันจาก user
    /// ก่อนเข้าใช้งานระบบ
    /// </summary>
    public partial class PdpaConsentDialog : Form
    {
        public PdpaConsentDialog()
        {
            InitializeComponent();
            SetPolicyText();
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void ChkConsent_CheckedChanged(object sender, EventArgs e)
        {
            btnConfirm.Enabled = chkConsent.Checked;
        }

        private void PnlBottom_Resize(object sender, EventArgs e)
        {
            btnExit.Left    = pnlBottom.ClientSize.Width - btnExit.Width - 8;
            btnConfirm.Left = btnExit.Left - btnConfirm.Width - 8;
        }

        private void BtnReadFull_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "นโยบายความเป็นส่วนตัวฉบับเต็มสามารถดูได้ที่\n" +
                "Intranet: http://intranet/pdpa-policy\n\n" +
                "หรือติดต่อ IT Department / DPO เพื่อขอสำเนา",
                "นโยบายฉบับเต็ม",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (!chkConsent.Checked)
            {
                MessageBox.Show("กรุณาทำเครื่องหมายยืนยันก่อนดำเนินการต่อ",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ── Policy content ────────────────────────────────────────────────────

        private void SetPolicyText()
        {
            rtbPolicy.Clear();

            AppendLine("ระบบ Core Management System (CMS) จำเป็นต้องเก็บข้อมูลส่วนบุคคลดังต่อไปนี้เพื่อการทำงาน:\n",
                       Color.Black, 9.5f, false);

            AppendLine("• ชื่อ-นามสกุล:", Color.Black, 9.5f, true);
            AppendLine("  เพื่อแสดงผลและ Audit Trail\n", Color.Black, 9.5f, false);

            AppendLine("• Email:", Color.Black, 9.5f, true);
            AppendLine("  เพื่อรับการแจ้งเตือน Approval Workflow\n", Color.Black, 9.5f, false);

            AppendLine("• ประวัติการใช้งาน:", Color.Black, 9.5f, true);
            AppendLine("  เพื่อความปลอดภัยและการปฏิบัติตามกฎระเบียบ OIC Circular 2566\n\n",
                       Color.Black, 9.5f, false);

            AppendLine("ข้อมูลจะถูกเก็บไว้ตลอดระยะเวลาการจ้างงานและอีก 3–5 ปีตามกฎหมายที่เกี่ยวข้อง\n\n",
                       Color.FromArgb(80, 80, 80), 9f, false);

            AppendLine("คุณมีสิทธิ์ขอตรวจสอบ แก้ไข หรือลบข้อมูล โดยติดต่อ IT Department หรือ DPO",
                       Color.FromArgb(80, 80, 80), 9f, false);
        }

        private void AppendLine(string text, Color color, float size, bool bold)
        {
            rtbPolicy.SelectionStart  = rtbPolicy.TextLength;
            rtbPolicy.SelectionLength = 0;
            rtbPolicy.SelectionColor  = color;
            rtbPolicy.SelectionFont   = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            rtbPolicy.AppendText(text);
        }
    }
}
