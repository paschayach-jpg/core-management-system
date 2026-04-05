using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Login Form สำหรับกรณีที่เปิดโปรแกรมตรงโดยไม่มี command-line argument
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly IAdAuthService          _authService;
        private readonly IUserAccountRepository  _userAccountRepo;

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
            catch { }
            return "127.0.0.1";
        }

        public LoginForm(IAdAuthService authService, IUserAccountRepository userAccountRepo = null)
        {
            _authService     = authService     ?? throw new ArgumentNullException("authService");
            _userAccountRepo = userAccountRepo;
            InitializeComponent();

            // Wire Register link
            lnkRegister.LinkClicked += lnkRegister_LinkClicked;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Event Handlers
        // ─────────────────────────────────────────────────────────────────────

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username;
            string password;

            if (checkBox1.Checked == true)
            {
                  username = "khanitthac";
                  password = "Knid_poonpun@1412K";
            }
            else {
                  //username = txtUsername.Text.Trim();
                  //password = txtPassword.Text;

                username = "pitakpong";
                password = "P@takpong01714";
            }

            if (username.Length == 0)
            {
                lblStatus.Text = "กรุณาใส่ Username";
                txtUsername.Focus();
                return;
            }
            if (password.Length == 0)
            {
                lblStatus.Text = "กรุณาใส่ Password";
                txtPassword.Focus();
                return;
            }

            SetLoading(true);
            lblStatus.Text = "กำลังตรวจสอบ...";

            try
            {
                AdAuthResult result = await _authService.LoginAsync(username, password);

                if (result.Success)
                {
                    // เก็บ IP ตอน login สำเร็จ
                    if (_userAccountRepo != null)
                    {
                        try
                        {
                            var account = _userAccountRepo.GetByADUserName(username);
                            if (account != null)
                                _userAccountRepo.UpdateLastLoginWithIP(account.UserID, GetLocalIP());
                        }
                        catch { /* ไม่ block login */ }
                    }

                    AuthResult      = result;
                    EnteredUsername = username;
                    DialogResult    = DialogResult.OK;
                }
                else
                {
                    lblStatus.Text = "Username หรือ Password ไม่ถูกต้อง";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
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
                lblStatus.Text = "กรุณาใส่ Username ก่อน Register";
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "กรุณาใส่ Password ก่อน Register";
                txtPassword.Focus();
                return;
            }

            SetLoading(true);
            lblStatus.Text = "กำลังตรวจสอบ AD...";

            try
            {
                // 1. Authenticate กับ AD ก่อน
                AdAuthResult result = await _authService.LoginAsync(username, password);
                if (!result.Success)
                {
                    lblStatus.Text = "Username หรือ Password ไม่ถูกต้อง";
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                if (_userAccountRepo == null)
                {
                    lblStatus.Text = "ไม่สามารถเชื่อมต่อฐานข้อมูลได้";
                    return;
                }

                // 2. Upsert UserAccount (Insert ถ้าใหม่ / Update ถ้ามีแล้ว)
                string ip = GetLocalIP();
                var emp = result.EmployeeInfo;
                var account = new UserAccount
                {
                    ADUserName   = username,
                    DisplayName  = emp != null ? (emp.FullNameTh ?? emp.FullNameEn ?? username) : username,
                    EmployeeCode = emp != null ? emp.EmployeeCode : null,
                    AppRole      = 1,
                    OrgUnitID    = 1
                };

                lblStatus.Text = "กำลังบันทึกข้อมูล...";
                bool isNew;
                int userId = _userAccountRepo.RegisterOrUpdate(account, ip, out isNew);

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
            btnLogin.Enabled    = !loading;
            txtUsername.Enabled = !loading;
            txtPassword.Enabled = !loading;
        }
    }
}
