using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Services
{
    public class Loading : Form
    {
        private Label _lblText;

        public Loading()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(220, 80);
            BackColor       = Color.FromArgb(46, 95, 138);
            TopMost         = true;

            _lblText = new Label
            {
                Text      = "กำลังเข้าสู่ระบบ...",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11F),
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            Controls.Add(_lblText);
        }
    }
}
