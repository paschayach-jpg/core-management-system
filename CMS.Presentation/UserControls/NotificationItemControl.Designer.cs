// =============================================
// CMS Notification System
// UserControls/NotificationItemControl.Designer.cs
// =============================================

namespace CMS.Presentation.UserControls
{
    partial class NotificationItemControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblIcon = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblReference = new System.Windows.Forms.Label();
            this.pnlColorBar = new System.Windows.Forms.Panel();
            this.pnlUnreadIndicator = new System.Windows.Forms.Panel();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnMarkRead = new System.Windows.Forms.Button();
            this.btnDismiss = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlMain.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.White;
            this.pnlMain.Controls.Add(this.pnlContent);
            this.pnlMain.Controls.Add(this.pnlColorBar);
            this.pnlMain.Controls.Add(this.pnlUnreadIndicator);
            this.pnlMain.Controls.Add(this.pnlActions);
            this.pnlMain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(400, 105);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.lblIcon);
            this.pnlContent.Controls.Add(this.lblType);
            this.pnlContent.Controls.Add(this.lblPriority);
            this.pnlContent.Controls.Add(this.lblTitle);
            this.pnlContent.Controls.Add(this.lblMessage);
            this.pnlContent.Controls.Add(this.lblTime);
            this.pnlContent.Controls.Add(this.lblReference);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(4, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.pnlContent.Size = new System.Drawing.Size(346, 105);
            this.pnlContent.TabIndex = 0;
            // 
            // lblIcon
            // 
            this.lblIcon.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblIcon.Location = new System.Drawing.Point(8, 18);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(32, 32);
            this.lblIcon.TabIndex = 0;
            this.lblIcon.Text = "🔔";
            this.lblIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.lblType.Location = new System.Drawing.Point(45, 12);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(60, 13);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "การแจ้งเตือน";
            // 
            // lblPriority
            // 
            this.lblPriority.AutoSize = true;
            this.lblPriority.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.lblPriority.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold);
            this.lblPriority.ForeColor = System.Drawing.Color.White;
            this.lblPriority.Location = new System.Drawing.Point(115, 12);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblPriority.Size = new System.Drawing.Size(40, 16);
            this.lblPriority.TabIndex = 2;
            this.lblPriority.Text = "ด่วน";
            this.lblPriority.Visible = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.AutoEllipsis = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(33)))), ((int)(((byte)(33)))));
            this.lblTitle.Location = new System.Drawing.Point(45, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(295, 20);
            this.lblTitle.TabIndex = 3;
            this.lblTitle.Text = "หัวข้อการแจ้งเตือน";
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.AutoEllipsis = true;
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(97)))), ((int)(((byte)(97)))));
            this.lblMessage.Location = new System.Drawing.Point(45, 52);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(295, 32);
            this.lblMessage.TabIndex = 4;
            this.lblMessage.Text = "รายละเอียดการแจ้งเตือน";
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTime.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.lblTime.Location = new System.Drawing.Point(255, 87);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(85, 15);
            this.lblTime.TabIndex = 5;
            this.lblTime.Text = "เมื่อสักครู่";
            this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblReference
            // 
            this.lblReference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblReference.AutoSize = true;
            this.lblReference.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblReference.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(158)))), ((int)(((byte)(158)))));
            this.lblReference.Location = new System.Drawing.Point(45, 87);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(0, 12);
            this.lblReference.TabIndex = 6;
            this.lblReference.Visible = false;
            // 
            // pnlColorBar
            // 
            this.pnlColorBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.pnlColorBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlColorBar.Location = new System.Drawing.Point(0, 0);
            this.pnlColorBar.Name = "pnlColorBar";
            this.pnlColorBar.Size = new System.Drawing.Size(4, 105);
            this.pnlColorBar.TabIndex = 1;
            // 
            // pnlUnreadIndicator
            // 
            this.pnlUnreadIndicator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.pnlUnreadIndicator.Location = new System.Drawing.Point(12, 48);
            this.pnlUnreadIndicator.Name = "pnlUnreadIndicator";
            this.pnlUnreadIndicator.Size = new System.Drawing.Size(8, 8);
            this.pnlUnreadIndicator.TabIndex = 2;
            // 
            // pnlActions
            // 
            this.pnlActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.pnlActions.Controls.Add(this.btnMarkRead);
            this.pnlActions.Controls.Add(this.btnDismiss);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlActions.Location = new System.Drawing.Point(350, 0);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(50, 105);
            this.pnlActions.TabIndex = 3;
            this.pnlActions.Visible = false;
            // 
            // btnMarkRead
            // 
            this.btnMarkRead.FlatAppearance.BorderSize = 0;
            this.btnMarkRead.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(245)))), ((int)(((byte)(233)))));
            this.btnMarkRead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarkRead.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnMarkRead.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnMarkRead.Location = new System.Drawing.Point(5, 20);
            this.btnMarkRead.Name = "btnMarkRead";
            this.btnMarkRead.Size = new System.Drawing.Size(40, 28);
            this.btnMarkRead.TabIndex = 0;
            this.btnMarkRead.Text = "✓";
            this.toolTip.SetToolTip(this.btnMarkRead, "ทำเครื่องหมายว่าอ่านแล้ว");
            this.btnMarkRead.UseVisualStyleBackColor = true;
            // 
            // btnDismiss
            // 
            this.btnDismiss.FlatAppearance.BorderSize = 0;
            this.btnDismiss.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(235)))), ((int)(((byte)(238)))));
            this.btnDismiss.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDismiss.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnDismiss.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(67)))), ((int)(((byte)(54)))));
            this.btnDismiss.Location = new System.Drawing.Point(5, 52);
            this.btnDismiss.Name = "btnDismiss";
            this.btnDismiss.Size = new System.Drawing.Size(40, 28);
            this.btnDismiss.TabIndex = 1;
            this.btnDismiss.Text = "✕";
            this.toolTip.SetToolTip(this.btnDismiss, "ปิดการแจ้งเตือน");
            this.btnDismiss.UseVisualStyleBackColor = true;
            // 
            // NotificationItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "NotificationItemControl";
            this.Size = new System.Drawing.Size(400, 105);
            this.pnlMain.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlColorBar;
        private System.Windows.Forms.Panel pnlUnreadIndicator;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblReference;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnMarkRead;
        private System.Windows.Forms.Button btnDismiss;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
