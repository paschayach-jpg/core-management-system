using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Login
{
    partial class LoginFrmNew
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.pnlAvatar = new System.Windows.Forms.Panel();
            this.lblAvatarIcon = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUsernameLabel = new System.Windows.Forms.Label();
            this.pnlUsernameBox = new System.Windows.Forms.Panel();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPasswordLabel = new System.Windows.Forms.Label();
            this.pnlPasswordBox = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnTogglePass = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            this.btnSignIn = new System.Windows.Forms.Button();
            this.pnlDivLeft = new System.Windows.Forms.Panel();
            this.pnlDivRight = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlCard.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlAvatar.SuspendLayout();
            this.pnlUsernameBox.SuspendLayout();
            this.pnlPasswordBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCard
            // 
            this.pnlCard.BackColor = System.Drawing.Color.White;
            this.pnlCard.Controls.Add(this.panel1);
            this.pnlCard.Controls.Add(this.label4);
            this.pnlCard.Controls.Add(this.pnlHeader);
            this.pnlCard.Controls.Add(this.lblUsernameLabel);
            this.pnlCard.Controls.Add(this.pnlUsernameBox);
            this.pnlCard.Controls.Add(this.lblPasswordLabel);
            this.pnlCard.Controls.Add(this.pnlPasswordBox);
            this.pnlCard.Controls.Add(this.lblError);
            this.pnlCard.Controls.Add(this.btnSignIn);
            this.pnlCard.Controls.Add(this.pnlDivLeft);
            this.pnlCard.Controls.Add(this.pnlDivRight);
            this.pnlCard.Location = new System.Drawing.Point(-1, -1);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(326, 442);
            this.pnlCard.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(95)))), ((int)(((byte)(138)))));
            this.pnlHeader.Controls.Add(this.label1);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Controls.Add(this.btnMinimize);
            this.pnlHeader.Controls.Add(this.pnlAvatar);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(326, 160);
            this.pnlHeader.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(299, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(22, 30);
            this.btnClose.TabIndex = 1;
            this.btnClose.TabStop = false;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(110)))), ((int)(((byte)(160)))));
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(273, 4);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(22, 30);
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.TabStop = false;
            this.btnMinimize.Text = "─";
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // pnlAvatar
            // 
            this.pnlAvatar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(155)))), ((int)(((byte)(205)))));
            this.pnlAvatar.Controls.Add(this.lblAvatarIcon);
            this.pnlAvatar.Location = new System.Drawing.Point(135, 17);
            this.pnlAvatar.Name = "pnlAvatar";
            this.pnlAvatar.Size = new System.Drawing.Size(57, 57);
            this.pnlAvatar.TabIndex = 3;
            // 
            // lblAvatarIcon
            // 
            this.lblAvatarIcon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvatarIcon.Font = new System.Drawing.Font("Segoe MDL2 Assets", 22F);
            this.lblAvatarIcon.ForeColor = System.Drawing.Color.White;
            this.lblAvatarIcon.Location = new System.Drawing.Point(0, 0);
            this.lblAvatarIcon.Name = "lblAvatarIcon";
            this.lblAvatarIcon.Size = new System.Drawing.Size(57, 57);
            this.lblAvatarIcon.TabIndex = 0;
            this.lblAvatarIcon.Text = "";
            this.lblAvatarIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(0, 80);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(326, 37);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "ทะเบียน RG";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblUsernameLabel
            // 
            this.lblUsernameLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblUsernameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblUsernameLabel.Location = new System.Drawing.Point(34, 183);
            this.lblUsernameLabel.Name = "lblUsernameLabel";
            this.lblUsernameLabel.Size = new System.Drawing.Size(103, 16);
            this.lblUsernameLabel.TabIndex = 6;
            this.lblUsernameLabel.Text = "USERNAME";
            // 
            // pnlUsernameBox
            // 
            this.pnlUsernameBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(253)))));
            this.pnlUsernameBox.Controls.Add(this.txtUsername);
            this.pnlUsernameBox.Location = new System.Drawing.Point(34, 202);
            this.pnlUsernameBox.Name = "pnlUsernameBox";
            this.pnlUsernameBox.Size = new System.Drawing.Size(257, 36);
            this.pnlUsernameBox.TabIndex = 7;
            this.pnlUsernameBox.Paint += new System.Windows.Forms.PaintEventHandler(this.InputPanel_Paint);
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(253)))));
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.txtUsername.Location = new System.Drawing.Point(10, 9);
            this.txtUsername.MaxLength = 100;
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(237, 18);
            this.txtUsername.TabIndex = 0;
            // 
            // lblPasswordLabel
            // 
            this.lblPasswordLabel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPasswordLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.lblPasswordLabel.Location = new System.Drawing.Point(34, 252);
            this.lblPasswordLabel.Name = "lblPasswordLabel";
            this.lblPasswordLabel.Size = new System.Drawing.Size(103, 16);
            this.lblPasswordLabel.TabIndex = 8;
            this.lblPasswordLabel.Text = "PASSWORD";
            // 
            // pnlPasswordBox
            // 
            this.pnlPasswordBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(253)))));
            this.pnlPasswordBox.Controls.Add(this.txtPassword);
            this.pnlPasswordBox.Controls.Add(this.btnTogglePass);
            this.pnlPasswordBox.Location = new System.Drawing.Point(34, 271);
            this.pnlPasswordBox.Name = "pnlPasswordBox";
            this.pnlPasswordBox.Size = new System.Drawing.Size(257, 36);
            this.pnlPasswordBox.TabIndex = 9;
            this.pnlPasswordBox.Paint += new System.Windows.Forms.PaintEventHandler(this.InputPanel_Paint);
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(249)))), ((int)(((byte)(253)))));
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.txtPassword.Location = new System.Drawing.Point(10, 9);
            this.txtPassword.MaxLength = 100;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(208, 18);
            this.txtPassword.TabIndex = 0;
            // 
            // btnTogglePass
            // 
            this.btnTogglePass.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePass.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTogglePass.FlatAppearance.BorderSize = 0;
            this.btnTogglePass.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(235)))), ((int)(((byte)(248)))));
            this.btnTogglePass.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTogglePass.Font = new System.Drawing.Font("Segoe MDL2 Assets", 11F);
            this.btnTogglePass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(150)))), ((int)(((byte)(190)))));
            this.btnTogglePass.Location = new System.Drawing.Point(227, 6);
            this.btnTogglePass.Name = "btnTogglePass";
            this.btnTogglePass.Size = new System.Drawing.Size(24, 24);
            this.btnTogglePass.TabIndex = 1;
            this.btnTogglePass.TabStop = false;
            this.btnTogglePass.Text = "";
            this.btnTogglePass.UseVisualStyleBackColor = false;
            this.btnTogglePass.Click += new System.EventHandler(this.btnTogglePass_Click);
            // 
            // lblError
            // 
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.lblError.Location = new System.Drawing.Point(34, 309);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(257, 14);
            this.lblError.TabIndex = 12;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblError.Visible = false;
            // 
            // btnSignIn
            // 
            this.btnSignIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
            this.btnSignIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSignIn.FlatAppearance.BorderSize = 0;
            this.btnSignIn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(95)))), ((int)(((byte)(138)))));
            this.btnSignIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSignIn.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnSignIn.ForeColor = System.Drawing.Color.White;
            this.btnSignIn.Location = new System.Drawing.Point(34, 328);
            this.btnSignIn.Name = "btnSignIn";
            this.btnSignIn.Size = new System.Drawing.Size(257, 42);
            this.btnSignIn.TabIndex = 13;
            this.btnSignIn.Text = "SIGN IN";
            this.btnSignIn.UseVisualStyleBackColor = false;
            this.btnSignIn.Click += new System.EventHandler(this.btnSignIn_Click);
            // 
            // pnlDivLeft
            // 
            this.pnlDivLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(218)))), ((int)(((byte)(234)))));
            this.pnlDivLeft.Location = new System.Drawing.Point(49, 391);
            this.pnlDivLeft.Name = "pnlDivLeft";
            this.pnlDivLeft.Size = new System.Drawing.Size(81, 1);
            this.pnlDivLeft.TabIndex = 14;
            // 
            // pnlDivRight
            // 
            this.pnlDivRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(218)))), ((int)(((byte)(234)))));
            this.pnlDivRight.Location = new System.Drawing.Point(195, 391);
            this.pnlDivRight.Name = "pnlDivRight";
            this.pnlDivRight.Size = new System.Drawing.Size(81, 1);
            this.pnlDivRight.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(222)));
            this.label4.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label4.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label4.Location = new System.Drawing.Point(211, 417);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 20);
            this.label4.TabIndex = 51;
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(-1, 117);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(326, 34);
            this.label1.TabIndex = 5;
            this.label1.Text = "บริษัท บางกอกสหประกันภัย จำกัด (มหาชน)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(218)))), ((int)(((byte)(234)))));
            this.panel1.Location = new System.Drawing.Point(114, 391);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(81, 1);
            this.panel1.TabIndex = 17;
            // 
            // LoginFrmNew
            // 
            this.AcceptButton = this.btnSignIn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(46)))), ((int)(((byte)(68)))));
            this.ClientSize = new System.Drawing.Size(320, 439);
            this.Controls.Add(this.pnlCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "LoginFrmNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login — RG Claim";
            this.Load += new System.EventHandler(this.LoginFrmNew_Load);
            this.pnlCard.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlAvatar.ResumeLayout(false);
            this.pnlUsernameBox.ResumeLayout(false);
            this.pnlUsernameBox.PerformLayout();
            this.pnlPasswordBox.ResumeLayout(false);
            this.pnlPasswordBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        // ── Field declarations ────────────────────────────────────────────
        private System.Windows.Forms.Panel     pnlCard;
        private System.Windows.Forms.Panel     pnlHeader;
        private System.Windows.Forms.Button    btnClose;
        private System.Windows.Forms.Button    btnMinimize;
        private System.Windows.Forms.Panel     pnlAvatar;
        private System.Windows.Forms.Label     lblAvatarIcon;
        private System.Windows.Forms.Label     lblTitle;
        private System.Windows.Forms.Label     lblUsernameLabel;
        private System.Windows.Forms.Panel     pnlUsernameBox;
        private System.Windows.Forms.TextBox   txtUsername;
        private System.Windows.Forms.Label     lblPasswordLabel;
        private System.Windows.Forms.Panel     pnlPasswordBox;
        private System.Windows.Forms.TextBox   txtPassword;
        private System.Windows.Forms.Button    btnTogglePass;
        private System.Windows.Forms.Label     lblError;
        private System.Windows.Forms.Button    btnSignIn;
        private System.Windows.Forms.Panel     pnlDivLeft;
        private System.Windows.Forms.Panel     pnlDivRight;
        private Label label4;
        private Label label1;
        private Panel panel1;
    }
}
