using System;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.DMS
{
    public partial class ShareExpiryPickerDialog : Form
    {
        public DateTime? SelectedExpiryDate { get; private set; }

        public ShareExpiryPickerDialog(DateTime? initialDate)
        {
            InitializeComponent();

            if (initialDate.HasValue)
            {
                dtpExpiry.Value = initialDate.Value;
                chkNoExpiry.Checked = false;
                dtpExpiry.Enabled = true;
            }
            else
            {
                dtpExpiry.Value = DateTime.Today.AddMonths(1);
                chkNoExpiry.Checked = true;
                dtpExpiry.Enabled = false;
            }

            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            btnOK.Click += (s, e) =>
            {
                if (chkNoExpiry.Checked)
                {
                    SelectedExpiryDate = null;
                }
                else
                {
                    if (dtpExpiry.Value.Date < DateTime.Today)
                    {
                        MessageBox.Show("ไม่สามารถกำหนดวันหมดอายุย้อนหลังได้", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                    if (dtpExpiry.Value.Date > DateTime.Today.AddYears(1))
                    {
                        MessageBox.Show("ไม่สามารถกำหนดอายุเกิน 1 ปีได้", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                    SelectedExpiryDate = dtpExpiry.Value.Date;
                }
                this.DialogResult = DialogResult.OK;
            };
        }
    }
}
