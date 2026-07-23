using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services;
using CMS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Login Form สำหรับกรณีที่เปิดโปรแกรมตรงโดยไม่มี command-line argument
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly ILoginAuthController _loginAuthController;
        private readonly ILoginAccountController _loginAccountController;
        private readonly IMenuController _menuController;

        /// <summary>ผลลัพธ์หลัง login สำเร็จ — อ่านได้หลัง DialogResult.OK</summary>
        public AdAuthResult AuthResult { get; private set; }

        /// <summary>Username ที่ผู้ใช้กรอก — ใช้เป็น ADUserName ในการค้นหา UserAccount</summary>
        public string EnteredUsername { get; private set; }

        /// <summary>IP Address ของเครื่อง client ณ เวลา login/register</summary>
        private static string GetLocalIP()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetLocalIP error: " + ex.Message);
            }
            return "127.0.0.1";
        }

        public LoginForm(ILoginAuthController loginAuthController, ILoginAccountController loginAccountController = null, IMenuController menuController = null)
        {
            _loginAuthController = loginAuthController ?? throw new ArgumentNullException("loginAuthController");
            _loginAccountController = loginAccountController;
            _menuController = menuController;
            InitializeComponent();

            // ทำให้หน้าจอมน
            this.Load += (s, e) => ApplyRoundedRegion(15);

            // ทำให้ลากหน้าจอได้โดยการคลิกที่ pnlHeader
            pnlHeader.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) ReleaseCaptureAndMove(); };

            // Wire Register link
            lnkRegister.LinkClicked += lnkRegister_LinkClicked;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Window Controls & Region
        // ─────────────────────────────────────────────────────────────────────

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void ReleaseCaptureAndMove()
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0xA1, 0x2, 0);
        }

        private void ApplyRoundedRegion(int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                this.Region = new Region(path);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────
        string username;
        string password;
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            // กำหนด username/password ตาม dev/test/prod
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            // Dev/Test block (ไม่ลบ)
            //string username = "pitakpong";
            //string password = "P@takpong01714";
            if (checkBox1.Checked)
            {
                username = "khanitthac";
                password = "KnidK@1412";
            }
            else
            {
                //username = "pitakpong";
                //password = "P@takpong0132";
                //username = "athika";
                //password = "Aple1712!";
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                using (var dlg = new LoginErrorForm(LoginErrorType.EmptyCredentials))
                    dlg.ShowDialog(this);
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                using (var dlg = new LoginErrorForm(LoginErrorType.EmptyCredentials))
                    dlg.ShowDialog(this);
                txtPassword.Focus();
                return;
            }

            SetLoading(true);
            lblStatus.Text = "กำลังตรวจสอบ...";
            try
            {
                var menuControl = new MenuControl(_menuController);
                var result = await _loginAuthController.LoginAsync(username, password);
                var menuList = await menuControl.Request(result.AccessToken);
                GlobalData.UserMenuModel = menuList;

                if (!result.Success)
                {
                    using (var dlg = new LoginErrorForm(LoginErrorType.InvalidCredentials))
                        dlg.ShowDialog(this);
                    lblStatus.Text = string.Empty;
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }
                if (menuList == null)
                {
                    using (var dlg = new AccessErrorForm(AccessErrorType.NoAccount, username))
                        dlg.ShowDialog(this);
                    lblStatus.Text = "ไม่พบ Account ในระบบ กรุณาติดต่อ HR";
                    txtPassword.Clear();
                    txtUsername.Focus();
                    return;
                }
                if (GlobalData.MenuActions == null || GlobalData.MenuActions.Count == 0)
                {
                    using (var dlg = new AccessErrorForm(AccessErrorType.NoMenuPermission, username))
                        dlg.ShowDialog(this);
                    lblStatus.Text = "ไม่มีสิทธิ์ Menu กรุณาติดต่อ IT Helpdesk";
                    txtPassword.Clear();
                    txtUsername.Focus();
                    return;
                }
                // เก็บ IP ตอน login สำเร็จ
                if (_loginAccountController != null)
                {
                    try
                    {
                        var account = _loginAccountController.GetUserByAdUserName(username);
                        if (account != null)
                            _loginAccountController.UpdateLastLoginWithIP(account.UserID, GetLocalIP());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to update login IP: " + ex.Message);
                    }
                }
                AuthResult = result;
                EnteredUsername = username;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "เชื่อมต่อ Server ไม่ได้: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("LoginForm error: " + ex.Message);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                using (var dlg = new LoginErrorForm(LoginErrorType.EmptyCredentials))
                    dlg.ShowDialog(this);
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                using (var dlg = new LoginErrorForm(LoginErrorType.EmptyCredentials))
                    dlg.ShowDialog(this);
                txtPassword.Focus();
                return;
            }

            SetLoading(true);
            lblStatus.Text = "กำลังตรวจสอบ AD...";

            try
            {
                // 1. Authenticate กับ AD ก่อน
                AdAuthResult result = await _loginAuthController.LoginAsync(username, password);
                if (!result.Success)
                {
                    using (var dlg = new LoginErrorForm(LoginErrorType.InvalidCredentials))
                        dlg.ShowDialog(this);
                    lblStatus.Text = string.Empty;
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                if (_loginAccountController == null)
                {
                    lblStatus.Text = "ไม่สามารถเชื่อมต่อฐานข้อมูลได้";
                    return;
                }

                // 2. Upsert UserAccount (Insert ถ้าใหม่ / Update ถ้ามีแล้ว)
                string ip = GetLocalIP();
                var emp = result.EmployeeInfo;
                var account = new UserAccount
                {
                    ADUserName = username,
                    DisplayName = emp != null ? (emp.FullNameTh ?? emp.FullNameEn ?? username) : username,
                    EmployeeCode = emp != null ? emp.EmployeeCode : null,
                    AppRole = 1,
                    OrgUnitID = 1
                };

                lblStatus.Text = "กำลังบันทึกข้อมูล...";
                bool isNew;
                int userId = _loginAccountController.RegisterOrUpdate(account, ip, out isNew);

                string msg = isNew
                    ? string.Format("ลงทะเบียนสำเร็จ!\n\nผู้ใช้: {0}\nIP: {1}\n\nกรุณา Login เพื่อเข้าใช้งาน", username, ip)
                    : string.Format("อัพเดทข้อมูลสำเร็จ!\n\nผู้ใช้: {0}\nIP: {1}", username, ip);

                MessageBox.Show(msg,
                    isNew ? "Register สำเร็จ" : "อัพเดทสำเร็จ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblStatus.Text = isNew ? "ลงทะเบียนสำเร็จ กรุณา Login" : "อัพเดทข้อมูลสำเร็จ";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "เกิดข้อผิดพลาด: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("Register error: " + ex.Message);
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnLogin_Click(sender, EventArgs.Empty);
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPassword.Focus();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private void SetLoading(bool loading)
        {
            btnLogin.Enabled = !loading;
            txtUsername.Enabled = !loading;
            txtPassword.Enabled = !loading;
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI Graphics (Rounded Corners & Shadows)
        // ─────────────────────────────────────────────────────────────────────

        private void pnlContent_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // วาดเส้นแบ่งระหว่างข้อมูลธุรกิจและฟอร์ม Login
            using (var pen = new Pen(Color.FromArgb(235, 240, 250), 1.5f))
            {
                e.Graphics.DrawLine(pen, 30, 66, 290, 66);
            }

            // วาดกรอบและเงาให้ TextBox Username
            Rectangle rectUser = new Rectangle(28, 95, 264, 30);
            DrawTextBoxBackground(e.Graphics, rectUser);

            // วาดกรอบและเงาให้ TextBox Password
            Rectangle rectPass = new Rectangle(28, 157, 264, 30);
            DrawTextBoxBackground(e.Graphics, rectPass);
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {
            // วาดเส้นขอบสีทองพรีเมียมที่ด้านล่างสุดของ pnlHeader
            using (var pen = new Pen(Color.FromArgb(197, 160, 89), 3f)) // สีทองพรีเมียม ความหนา 3px
            {
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            }
        }

        private void DrawTextBoxBackground(Graphics g, Rectangle rect)
        {
            int radius = 8; // ความโค้งมนของกรอบ
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                // วาดเงาให้มีมิติ (Drop Shadow บางๆ)
                using (var shadowBrush = new SolidBrush(Color.FromArgb(12, 0, 0, 0)))
                {
                    g.TranslateTransform(1, 2);
                    g.FillPath(shadowBrush, path);
                    g.TranslateTransform(-1, -2);
                }

                // เติมพื้นหลังสีขาว
                g.FillPath(Brushes.White, path);

                // วาดเส้นขอบสีเทาอมฟ้าบางๆ
                using (var pen = new Pen(Color.FromArgb(200, 205, 215), 1.5f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void btnLogin_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            int radius = 10; // ความโค้งมนของปุ่ม
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                // ใช้ Region ตัดขอบให้ปุ่มมีความมน
                btn.Region = new Region(path);
            }
        }
    }
}
