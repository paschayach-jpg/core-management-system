namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowCreateTemplateStep4Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlSteps = new System.Windows.Forms.Panel();
            this.lblSteps = new System.Windows.Forms.Label();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnComplete = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.txtSummary = new System.Windows.Forms.TextBox();
            this.grpPublish = new System.Windows.Forms.GroupBox();
            this.rdoPublish = new System.Windows.Forms.RadioButton();
            this.rdoDraft = new System.Windows.Forms.RadioButton();
            this.pnlHeader.SuspendLayout();
            this.pnlSteps.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.grpPublish.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20);
            this.pnlHeader.Size = new System.Drawing.Size(900, 70);
            this.pnlHeader.TabIndex = 0;

            // lblTitle
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(860, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "4) ตรวจสอบและเผยแพร่";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlSteps
            this.pnlSteps.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
            this.pnlSteps.Controls.Add(this.lblSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSteps.Location = new System.Drawing.Point(0, 70);
            this.pnlSteps.Name = "pnlSteps";
            this.pnlSteps.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSteps.Size = new System.Drawing.Size(900, 40);
            this.pnlSteps.TabIndex = 1;

            // lblSteps
            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSteps.Location = new System.Drawing.Point(10, 10);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(880, 20);
            this.lblSteps.TabIndex = 0;
            this.lblSteps.Text = "1) ข้อมูลพื้นฐาน  →  2) ออกแบบ Flow  →  3) ออกแบบอนุมัติ  →  4) Publish";
            this.lblSteps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlButtons
            this.pnlButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlButtons.Controls.Add(this.btnPrev);
            this.pnlButtons.Controls.Add(this.btnComplete);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 695);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10);
            this.pnlButtons.Size = new System.Drawing.Size(900, 55);
            this.pnlButtons.TabIndex = 2;

            // btnPrev
            this.btnPrev.Location = new System.Drawing.Point(660, 12);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 30);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "ย้อนกลับ";

            // btnComplete
            this.btnComplete.BackColor = System.Drawing.Color.FromArgb(34, 177, 76);
            this.btnComplete.ForeColor = System.Drawing.Color.White;
            this.btnComplete.Location = new System.Drawing.Point(740, 12);
            this.btnComplete.Name = "btnComplete";
            this.btnComplete.Size = new System.Drawing.Size(75, 30);
            this.btnComplete.TabIndex = 1;
            this.btnComplete.Text = "เสร็จสิ้น";
            this.btnComplete.UseVisualStyleBackColor = false;

            // pnlContent
            this.pnlContent.AutoScroll = true;
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.grpPublish);
            this.pnlContent.Controls.Add(this.txtSummary);
            this.pnlContent.Controls.Add(this.lblSummary);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 110);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(25);
            this.pnlContent.Size = new System.Drawing.Size(900, 585);
            this.pnlContent.TabIndex = 3;

            // lblSummary
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSummary.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblSummary.Location = new System.Drawing.Point(25, 25);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(850, 25);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "📋 สรุป Template";

            // txtSummary
            this.txtSummary.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSummary.Location = new System.Drawing.Point(25, 55);
            this.txtSummary.Multiline = true;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.ReadOnly = true;
            this.txtSummary.Size = new System.Drawing.Size(850, 120);
            this.txtSummary.TabIndex = 1;
            this.txtSummary.Text = "✓ ชื่อ Template: [Template Name]\r\n✓ Flow: [Flow Steps]\r\n✓ Approvers: [Approval Groups]\r\n\r\nกำลังจะเผยแพร่แล้ว...";

            // grpPublish
            this.grpPublish.Controls.Add(this.rdoPublish);
            this.grpPublish.Controls.Add(this.rdoDraft);
            this.grpPublish.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpPublish.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.grpPublish.Location = new System.Drawing.Point(25, 195);
            this.grpPublish.Name = "grpPublish";
            this.grpPublish.Size = new System.Drawing.Size(850, 100);
            this.grpPublish.TabIndex = 2;
            this.grpPublish.TabStop = false;
            this.grpPublish.Text = "💾 สถานะการเผยแพร่";

            // rdoDraft
            this.rdoDraft.AutoSize = true;
            this.rdoDraft.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rdoDraft.Location = new System.Drawing.Point(30, 35);
            this.rdoDraft.Name = "rdoDraft";
            this.rdoDraft.Size = new System.Drawing.Size(100, 19);
            this.rdoDraft.TabIndex = 0;
            this.rdoDraft.Text = "บันทึกเป็น Draft";
            this.rdoDraft.UseVisualStyleBackColor = true;

            // rdoPublish
            this.rdoPublish.AutoSize = true;
            this.rdoPublish.Checked = true;
            this.rdoPublish.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rdoPublish.Location = new System.Drawing.Point(30, 60);
            this.rdoPublish.Name = "rdoPublish";
            this.rdoPublish.Size = new System.Drawing.Size(120, 19);
            this.rdoPublish.TabIndex = 1;
            this.rdoPublish.TabStop = true;
            this.rdoPublish.Text = "เผยแพร่ (Publish)";
            this.rdoPublish.UseVisualStyleBackColor = true;

            // WorkflowCreateTemplateStep4Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 750);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlSteps);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ตรวจสอบและเผยแพร่ - Step 4";
            this.pnlHeader.ResumeLayout(false);
            this.pnlSteps.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.grpPublish.ResumeLayout(false);
            this.grpPublish.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSteps;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnComplete;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.TextBox txtSummary;
        private System.Windows.Forms.GroupBox grpPublish;
        private System.Windows.Forms.RadioButton rdoDraft;
        private System.Windows.Forms.RadioButton rdoPublish;
    }
}
