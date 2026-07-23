using RG_Project.Login;
using RG_Project.Services;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure;
using CMS.Services;
using CMS.Services.Interfaces;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RG_Project.Login
{
    /// <summary>
    /// Steel Blue – Modern, Professional &amp; Luxury Login Form.
    /// Auth รวมเข้ากับ CMS แล้ว — เรียก CMS.Services.AdAuthService + CMS.Data.Repositories.UserAccountRepository
    /// แทน RG_Project.Login.LoginManager เดิม เพื่อสร้าง CMS.Domain.Models.UserContext ตัวจริง
    /// </summary>
    public partial class LoginFrmNew : Form
    {
        private readonly string version = ConfigurationManager.AppSettings["Version"];
        private readonly IAdAuthService _adAuthService;
        private readonly IUserAccountRepository _userAccountRepo;

        /// <summary>UserContext ตัวจริงหลัง login สำเร็จ (ใช้ต่อใน Program.cs)</summary>
        public UserContext CurrentUserContext { get; private set; }
        // ── State ──────────────────────────────────────────────────────────
        private Point _formDragOffset;
        private bool  _dragging        = false;
        private bool  _passwordVisible = false;
        private bool  _hasError        = false;

        // ── Steel Blue Colour Palette ──────────────────────────────────────
        private static readonly Color ColSteelBlue    = Color.FromArgb(70,  130, 180);  // primary button/accent
        private static readonly Color ColSteelDark    = Color.FromArgb(46,   95, 138);  // dark header/hover
        private static readonly Color ColSteelDeep    = Color.FromArgb(22,   46,  68);  // form outer background
        private static readonly Color ColBorderNormal = Color.FromArgb(176, 202, 224);  // input resting border
        private static readonly Color ColBorderActive = Color.FromArgb(70,  130, 180);  // input focused border
        private static readonly Color ColBorderError  = Color.FromArgb(192,  57,  43);  // error border
        private static readonly Color ColInputBg      = Color.FromArgb(245, 249, 253);  // resting input bg
        private static readonly Color ColInputBgFocus = Color.White;

        public event Action LoginSuccess;

        // ─────────────────────────────────────────────────────────────────
        /// <summary>ใช้เมื่อไม่ได้ประกอบ dependency มาจาก composition root (เช่น เปิดผ่าน Designer) — สร้าง default ให้เอง</summary>
        public LoginFrmNew() : this(new AdAuthService(new ConfigManager()), null) { }

        public LoginFrmNew(IAdAuthService adAuthService, IUserAccountRepository userAccountRepo)
        {
            _adAuthService = adAuthService;
            _userAccountRepo = userAccountRepo ?? new CMS.Data.Repositories.UserAccountRepository(new ConfigManager());
            InitializeComponent();
            ApplyPostInitStyles();
            //WireUpDragEvents();
            WireUpKeyEvents();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtUsername.Focus();
            txtUsername.SelectAll();
        }

        // ═════════════════════════════════════════════════════════════════
        //  POST-INIT STYLING
        // ═════════════════════════════════════════════════════════════════
        private void ApplyPostInitStyles()
        {
            // Clip avatar panel to circle
            var avatarPath = new GraphicsPath();
            avatarPath.AddEllipse(0, 0, pnlAvatar.Width, pnlAvatar.Height);
            pnlAvatar.Region = new Region(avatarPath);

            // Double-buffer all painted panels to prevent flicker
            SetDoubleBuffer(pnlCard);
            SetDoubleBuffer(pnlHeader);
            SetDoubleBuffer(pnlUsernameBox);
            SetDoubleBuffer(pnlPasswordBox);

            // Paint events
            this.Paint       += Form_BackgroundPaint;
            pnlHeader.Paint  += PnlHeader_Paint;
            pnlCard.Paint    += PnlCard_Paint;

            // Input focus visuals
            txtUsername.Enter += TxtInput_Enter;
            txtUsername.Leave += TxtInput_Leave;
            txtPassword.Enter += TxtInput_Enter;
            txtPassword.Leave += TxtInput_Leave;

            // Clear error on any keystroke
            txtUsername.TextChanged += (s, e) => ClearError();
            txtPassword.TextChanged += (s, e) => ClearError();

            // Tab: username → password
            txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Tab)
                {
                    txtPassword.Focus();
                    e.SuppressKeyPress = true;
                }
            };
        }

        private static void SetDoubleBuffer(Control ctrl)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                .SetValue(ctrl, true);
        }

        // ═════════════════════════════════════════════════════════════════
        //  CUSTOM PAINTING
        // ═════════════════════════════════════════════════════════════════

        /// <summary>Form outer background — deep steel navy vignette.</summary>
        private void Form_BackgroundPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Radial gradient: slightly lighter centre, deep navy edges
            using (var brush = new PathGradientBrush(new PointF[]
            {
                new PointF(0, 0), new PointF(Width, 0),
                new PointF(Width, Height), new PointF(0, Height)
            }))
            {
                brush.CenterPoint  = new PointF(Width / 2f, Height / 2f);
                brush.CenterColor  = Color.FromArgb(35, 72, 108);
                brush.SurroundColors = new[] { Color.FromArgb(16, 36, 56) };
                g.FillRectangle(brush, ClientRectangle);
            }

            // Soft drop-shadow under card
            var shadowRect  = new Rectangle(pnlCard.Left + 5, pnlCard.Top + 5, pnlCard.Width, pnlCard.Height);
            using (var sb = new SolidBrush(Color.FromArgb(45, 0, 0, 0)))
                g.FillRectangle(sb, shadowRect);
        }

        /// <summary>Header panel — steel-blue linear gradient + 3-layer gold shimmer separator.</summary>
        private void PnlHeader_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode      = SmoothingMode.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            // ── 1. Steel-blue gradient fill ───────────────────────────────
            using (var brush = new LinearGradientBrush(
                new Point(0, 0), new Point(0, pnlHeader.Height),
                Color.FromArgb(55, 110, 160),
                Color.FromArgb(38, 80, 118)))
            {
                g.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
            }

            // ── 2. Gold shimmer separator (3 layers) ──────────────────────
            int w = pnlHeader.Width;
            int y = pnlHeader.Height - 1;   // bottom edge of header

            // Layer A — diffuse outer glow (7 px, very transparent)
            using (var glowBrush = new LinearGradientBrush(
                new Point(0, y), new Point(w, y),
                Color.Transparent, Color.Transparent))
            {
                var cb = new ColorBlend(5);
                cb.Colors    = new[] {
                    Color.FromArgb(  0, 212, 175, 55),
                    Color.FromArgb( 35, 220, 180, 60),
                    Color.FromArgb( 70, 255, 215, 90),
                    Color.FromArgb( 35, 220, 180, 60),
                    Color.FromArgb(  0, 212, 175, 55) };
                cb.Positions = new[] { 0f, 0.15f, 0.5f, 0.85f, 1f };
                glowBrush.InterpolationColors = cb;

                using (var pen = new Pen(glowBrush, 7f))
                    g.DrawLine(pen, 0, y - 3, w, y - 3);
            }

            // Layer B — mid glow (3 px, semi-transparent)
            using (var midBrush = new LinearGradientBrush(
                new Point(0, y), new Point(w, y),
                Color.Transparent, Color.Transparent))
            {
                var cb = new ColorBlend(5);
                cb.Colors    = new[] {
                    Color.FromArgb(  0, 218, 170, 45),
                    Color.FromArgb(100, 218, 170, 45),
                    Color.FromArgb(180, 255, 220, 85),
                    Color.FromArgb(100, 218, 170, 45),
                    Color.FromArgb(  0, 218, 170, 45) };
                cb.Positions = new[] { 0f, 0.2f, 0.5f, 0.8f, 1f };
                midBrush.InterpolationColors = cb;

                using (var pen = new Pen(midBrush, 3f))
                    g.DrawLine(pen, 0, y - 1, w, y - 1);
            }

            // Layer C — sharp core line (1.5 px, fully opaque at centre)
            using (var coreBrush = new LinearGradientBrush(
                new Point(0, y), new Point(w, y),
                Color.Transparent, Color.Transparent))
            {
                var cb = new ColorBlend(5);
                cb.Colors    = new[] {
                    Color.FromArgb(  0, 184, 149, 42),
                    Color.FromArgb(200, 200, 162, 50),
                    Color.FromArgb(255, 255, 228, 110),   // bright gold centre
                    Color.FromArgb(200, 200, 162, 50),
                    Color.FromArgb(  0, 184, 149, 42) };
                cb.Positions = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
                coreBrush.InterpolationColors = cb;

                using (var pen = new Pen(coreBrush, 1.5f))
                    g.DrawLine(pen, 0, y, w, y);
            }
        }

        /// <summary>Card background — subtle white gradient for depth.</summary>
        private void PnlCard_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                new Point(0, 0), new Point(0, pnlCard.Height),
                Color.White,
                Color.FromArgb(247, 251, 255)))
            {
                g.FillRectangle(brush, 0, 0, pnlCard.Width, pnlCard.Height);
            }
        }

        /// <summary>Input panel border — adapts to normal / active / error state.</summary>
        internal void InputPanel_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            var g     = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color border = _hasError           ? ColBorderError  :
                           panel.ContainsFocus ? ColBorderActive :
                           ColBorderNormal;

            float bw = panel.ContainsFocus ? 2f : 1.5f;
            using (var pen = new Pen(border, bw))
                g.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        }

        // ═════════════════════════════════════════════════════════════════
        //  FORM DRAGGING  (borderless window)
        // ═════════════════════════════════════════════════════════════════
        //private void WireUpDragEvents()
        //{
        //    foreach (Control c in new Control[] { pnlHeader, this })
        //    {
        //        c.MouseDown += Drag_MouseDown;
        //        c.MouseMove += Drag_MouseMove;
        //        c.MouseUp   += Drag_MouseUp;
        //    }
        //    foreach (Control c in new Control[] { lblTitle, lblSubtitle })
        //    {
        //        c.MouseDown += Drag_MouseDown;
        //        c.MouseMove += Drag_MouseMove;
        //        c.MouseUp   += Drag_MouseUp;
        //    }
        //}

        private void Drag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                Point screen  = ((Control)sender).PointToScreen(e.Location);
                _formDragOffset = new Point(screen.X - Left, screen.Y - Top);
            }
        }

        private void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging && e.Button == MouseButtons.Left)
            {
                Point screen = ((Control)sender).PointToScreen(e.Location);
                Location = new Point(screen.X - _formDragOffset.X,
                                     screen.Y - _formDragOffset.Y);
            }
        }

        private void Drag_MouseUp(object sender, MouseEventArgs e)
            => _dragging = false;

        // ═════════════════════════════════════════════════════════════════
        //  WINDOW CONTROLS
        // ═════════════════════════════════════════════════════════════════
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("คุณต้องการออกจากระบบใช่หรือไม่?", "ยืนยัน",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
            => WindowState = FormWindowState.Minimized;

        // ═════════════════════════════════════════════════════════════════
        //  INPUT FOCUS VISUALS
        // ═════════════════════════════════════════════════════════════════
        internal void TxtInput_Enter(object sender, EventArgs e)
        {
            var txt   = (TextBox)sender;
            var panel = (Panel)txt.Parent;
            panel.BackColor = ColInputBgFocus;
            txt.BackColor   = ColInputBgFocus;
            panel.Invalidate();
        }

        internal void TxtInput_Leave(object sender, EventArgs e)
        {
            var txt   = (TextBox)sender;
            var panel = (Panel)txt.Parent;
            panel.BackColor = ColInputBg;
            txt.BackColor   = ColInputBg;
            panel.Invalidate();
        }

        // ═════════════════════════════════════════════════════════════════
        //  PASSWORD VISIBILITY TOGGLE
        // ═════════════════════════════════════════════════════════════════
        private void btnTogglePass_Click(object sender, EventArgs e)
        {
            _passwordVisible         = !_passwordVisible;
            txtPassword.PasswordChar = _passwordVisible ? '\0' : '\u25CF';
            // E7B3 = eye open, E7B4 = eye hidden (Segoe MDL2 Assets)
            //btnTogglePass.Text       = _passwordVisible ? "\uE7B4" : "\uE7B3";
            btnTogglePass.Text = _passwordVisible ? "🙈" : "👁";
        }

        // ═════════════════════════════════════════════════════════════════
        //  KEY EVENTS  (Enter triggers login)
        // ═════════════════════════════════════════════════════════════════
        private void WireUpKeyEvents()
        {
            this.KeyDown        += Form_KeyDown;
            txtPassword.KeyDown += TxtPassword_KeyDown;
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _ = loginAsync();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _ = loginAsync();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        //  SIGN-IN LOGIC  — same concept as LoginFrm, uses LoginManager
        // ═════════════════════════════════════════════════════════════════
        private async void btnSignIn_Click(object sender, EventArgs e)
        {
            await loginAsync();
        }

        public async Task loginAsync()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("กรุณาใส่ Username");
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                ShowError("กรุณาใส่ Password");
                txtPassword.Focus();
                return;
            }

            SetBusy(true);
            Console.WriteLine("เริ่มการเข้าสู่ระบบ");

            using (var loadingForm = new Loading())
            {
                try
                {
                    loadingForm.Show();
                    loadingForm.Refresh();

                    var authResult = await _adAuthService.LoginAsync(username, password);

                    if (authResult == null || !authResult.Success)
                    {
                        SetBusy(false);
                        ShowError("Username หรือ Password ไม่ถูกต้อง");
                        Console.WriteLine("เข้าสู่ระบบล้มเหลว: " + (authResult != null ? authResult.ErrorMessage : "null result"));
                        txtPassword.Text = string.Empty;
                        txtUsername.Focus();
                        await ShakeAsync();
                        return;
                    }

                    string adUserName = !string.IsNullOrEmpty(authResult.AdUserName) ? authResult.AdUserName : username;

                    UserAccount account = _userAccountRepo.GetByADUserName(adUserName);
                    if (account == null)
                    {
                        var emp = authResult.EmployeeInfo;
                        var newAccount = new UserAccount
                        {
                            ADUserName = adUserName,
                            DisplayName = emp != null
                                ? (emp.EmployeeFirstNameTh + " " + emp.EmployeeLastNameTh).Trim()
                                : adUserName,
                            AppRole = 1, // ค่าเริ่มต้น: Officer
                            OrgUnitID = 1,
                            IsActive = true,
                            ConsentGiven = true, // โมดูล Project ไม่มีขั้นตอน PDPA consent แยกต่างหากในรอบนี้
                            CreatedDate = DateTime.Now
                        };
                        int newUserId = _userAccountRepo.Create(newAccount);
                        account = _userAccountRepo.GetById(newUserId);
                    }

                    if (!account.IsActive)
                    {
                        SetBusy(false);
                        ShowError("บัญชีของคุณถูกระงับ กรุณาติดต่อผู้ดูแลระบบ");
                        return;
                    }

                    _userAccountRepo.UpdateLastLogin(account.UserID);

                    CurrentUserContext = new UserContext
                    {
                        UserID = account.UserID,
                        ADUserName = account.ADUserName,
                        DisplayName = account.DisplayName,
                        AppRole = account.AppRole,
                        MachineName = System.Net.Dns.GetHostName(),
                        PositionName = account.PositionName,
                        DepartmentCode = account.DepartmentCode,
                        DepartmentName = account.DepartmentName,
                        DepartmentId = account.DepartmentId,
                    };

                    GlobalData.Token = authResult.AccessToken;
                    Console.WriteLine("เข้าสู่ระบบสำเร็จ: " + adUserName);
                    SetBusy(false);
                    LoginSuccess?.Invoke();
                    this.Close();
                }
                catch (Exception ex)
                {
                    SetBusy(false);
                    ShowError("เกิดข้อผิดพลาด: " + ex.Message);
                    Console.WriteLine("Exception in login: " + ex.Message);
                }
                finally
                {
                    loadingForm.Close();
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void SetBusy(bool busy)
        {
            btnSignIn.Enabled = !busy;
            btnSignIn.Text    = busy ? "Signing in…" : "SIGN IN";
            Cursor            = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void ShowError(string message)
        {
            _hasError        = true;
            lblError.Text    = message;
            lblError.Visible = true;
            pnlUsernameBox.Invalidate();
            pnlPasswordBox.Invalidate();
        }

        private void ClearError()
        {
            if (!_hasError) return;
            _hasError        = false;
            lblError.Visible = false;
            pnlUsernameBox.Invalidate();
            pnlPasswordBox.Invalidate();
        }

        /// <summary>Horizontal shake animation on auth failure.</summary>
        private async Task ShakeAsync()
        {
            int origin      = Left;
            const int dist  = 7;
            const int steps = 5;
            const int delay = 38;

            for (int i = 0; i < steps; i++)
            {
                Left = origin + dist;
                await Task.Delay(delay);
                Left = origin - dist;
                await Task.Delay(delay);
            }
            Left = origin;
        }

        // ═════════════════════════════════════════════════════════════════
        //  LINK EVENTS
        // ═════════════════════════════════════════════════════════════════
        //private void lnkForgotPass_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    MessageBox.Show(
        //        "กรุณาติดต่อผู้ดูแลระบบเพื่อรีเซ็ตรหัสผ่าน",
        //        "Forgot Password",
        //        MessageBoxButtons.OK,
        //        MessageBoxIcon.Information);
        //}

        //private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    MessageBox.Show(
        //        "กรุณาติดต่อผู้ดูแลระบบเพื่อขอสิทธิ์เข้าใช้งาน",
        //        "Create Account",
        //        MessageBoxButtons.OK,
        //        MessageBoxIcon.Information);
        //}

        private void LoginFrmNew_Load(object sender, EventArgs e)
        {
            label4.Text = version;
        }
    }
}
