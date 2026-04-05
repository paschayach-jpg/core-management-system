using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Form for external recipients to access shared files via token links.
    /// Satisfies Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 7.2, 7.4, 7.5
    /// </summary>
    public partial class AccessShareForm : Form
    {
        private readonly IShareAccessService _shareAccessService;
        private readonly Guid _shareToken;
        private bool _accessGranted;

        private TextBox _txtPassword;
        private Button _btnDownload;
        private Label _lblStatus;
        private Label _lblFileInfo;
        private Dictionary<string, object> _controls;

        public AccessShareForm(IShareAccessService shareAccessService, Guid shareToken)
        {
            InitializeComponent();
            _shareAccessService = shareAccessService;
            _shareToken = shareToken;
            _accessGranted = false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "Access Shared File";
            this.Width = 500;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // File info label
            _lblFileInfo = new Label { Text = "File Information:", Left = 10, Top = 10, Width = 470, Height = 60, AutoSize = false };

            // Password textbox (initially hidden)
            var lblPassword = new Label { Text = "Password:", Left = 10, Top = 80, Width = 100 };
            _txtPassword = new TextBox { Left = 120, Top = 80, Width = 360, PasswordChar = '*', Visible = false };

            var btnValidate = new Button { Text = "Validate", Left = 120, Top = 110, Width = 80, Visible = false };
            btnValidate.Click += (s, e) => ValidatePassword();

            // Download button (initially disabled)
            _btnDownload = new Button { Text = "Download", Left = 120, Top = 150, Width = 80, Enabled = false };
            _btnDownload.Click += (s, e) => DownloadFile();

            // Status label
            _lblStatus = new Label { Text = "", Left = 10, Top = 190, Width = 470, Height = 80, AutoSize = false };

            var btnClose = new Button { Text = "Close", Left = 400, Top = 250, Width = 80, DialogResult = DialogResult.Cancel };

            this.Controls.AddRange(new Control[]
            {
                _lblFileInfo,
                lblPassword, _txtPassword,
                btnValidate,
                _btnDownload,
                _lblStatus,
                btnClose
            });

            // Store references
            _controls = new Dictionary<string, object>
            {
                { "btnValidate", btnValidate },
                { "lblPassword", lblPassword }
            };

            this.ResumeLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ValidateTokenAndCheckPassword();
        }

        private void ValidateTokenAndCheckPassword()
        {
            try
            {
                // Create a UserContext for anonymous access
                var anonymousUser = new UserContext
                {
                    UserID = 0,
                    ADUserName = "Anonymous"
                };

                // Validate the token without password first
                var result = _shareAccessService.ValidateAndGrantAccess(_shareToken, null, anonymousUser);

                if (!result.IsGranted)
                {
                    if (result.DenyReason != null && result.DenyReason.ToLowerInvariant().Contains("password"))
                    {
                        // Password required
                        var lblPassword = (Label)_controls["lblPassword"];
                        var btnValidate = (Button)_controls["btnValidate"];

                        _txtPassword.Visible = true;
                        lblPassword.Visible = true;
                        btnValidate.Visible = true;

                        _lblStatus.Text = "This share is password-protected. Please enter the password.";
                    }
                    else
                    {
                        _lblStatus.Text = $"Access Denied: {result.DenyReason}";
                    }
                }
                else
                {
                    // Access granted without password
                    _accessGranted = true;
                    DisplayFileInfo(result);
                    _btnDownload.Enabled = true;
                    _lblStatus.Text = "Access granted. You can now download the file.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void ValidatePassword()
        {
            try
            {
                var anonymousUser = new UserContext
                {
                    UserID = 0,
                    ADUserName = "Anonymous"
                };

                var result = _shareAccessService.ValidateAndGrantAccess(_shareToken, _txtPassword.Text, anonymousUser);

                if (result.IsGranted)
                {
                    _accessGranted = true;
                    DisplayFileInfo(result);
                    _btnDownload.Enabled = true;
                    _lblStatus.Text = "Password validated. You can now download the file.";

                    var lblPassword = (Label)_controls["lblPassword"];
                    var btnValidate = (Button)_controls["btnValidate"];

                    _txtPassword.Visible = false;
                    lblPassword.Visible = false;
                    btnValidate.Visible = false;
                }
                else
                {
                    _lblStatus.Text = $"Access Denied: {result.DenyReason}";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        private void DisplayFileInfo(ShareAccessResult result)
        {
            var info = new System.Text.StringBuilder();
            if (result.File != null)
            {
                info.AppendLine(string.Format("File: {0}", result.File.OriginalFileName));
                info.AppendLine($"Size: {result.File.FileSizeBytes} bytes");
            }
            else if (result.Folder != null)
            {
                info.AppendLine($"Folder: {result.Folder.FolderName}");
            }

            if (result.RemainingDownloads > 0)
            {
                info.AppendLine($"Remaining Downloads: {result.RemainingDownloads}");
            }

            _lblFileInfo.Text = info.ToString();
        }

        private void DownloadFile()
        {
            if (!_accessGranted)
            {
                MessageBox.Show("Access not granted.", "Error");
                return;
            }

            try
            {
                // In a real implementation, this would trigger the file download
                MessageBox.Show("File download would be initiated here.", "Info");
                _shareAccessService.IncrementDownloadCount(0); // Would use actual share ID
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
