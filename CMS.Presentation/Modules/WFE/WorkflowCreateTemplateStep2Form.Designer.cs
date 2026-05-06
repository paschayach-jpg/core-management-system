namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowCreateTemplateStep2Form
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
            this.btnNext = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblGuide = new System.Windows.Forms.Label();
            this.pnlStepActions = new System.Windows.Forms.Panel();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnDeleteStep = new System.Windows.Forms.Button();
            this.btnAddStep = new System.Windows.Forms.Button();
            this.dgvSteps = new System.Windows.Forms.DataGridView();
            this.colSeq = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNodeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStepName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colNextStep = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAutoPath = new System.Windows.Forms.Button();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblPreviewTitle = new System.Windows.Forms.Label();
            this.rtbPreview = new System.Windows.Forms.RichTextBox();
            this.pnlHeader.SuspendLayout();
            this.pnlSteps.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlStepActions.SuspendLayout();
            this.pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20);
            this.pnlHeader.Size = new System.Drawing.Size(900, 70);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(860, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "2) ออกแบบ Flow";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSteps
            // 
            this.pnlSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.pnlSteps.Controls.Add(this.lblSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSteps.Location = new System.Drawing.Point(0, 70);
            this.pnlSteps.Name = "pnlSteps";
            this.pnlSteps.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSteps.Size = new System.Drawing.Size(900, 40);
            this.pnlSteps.TabIndex = 1;
            // 
            // lblSteps
            // 
            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSteps.Location = new System.Drawing.Point(10, 10);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(880, 20);
            this.lblSteps.TabIndex = 0;
            this.lblSteps.Text = "1) ข้อมูลพื้นฐาน  →  2) ออกแบบ Flow  →  3) ออกแบบอนุมัติ  →  4) Publish";
            this.lblSteps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            this.pnlButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlButtons.Controls.Add(this.btnPrev);
            this.pnlButtons.Controls.Add(this.btnNext);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 695);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10);
            this.pnlButtons.Size = new System.Drawing.Size(900, 55);
            this.pnlButtons.TabIndex = 2;
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(660, 12);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 30);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "ย้อนกลับ";
            this.btnPrev.UseVisualStyleBackColor = true;
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Location = new System.Drawing.Point(740, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 30);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "ถัดไป";
            this.btnNext.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.lblGuide);
            this.pnlContent.Controls.Add(this.pnlStepActions);
            this.pnlContent.Controls.Add(this.dgvSteps);
            this.pnlContent.Controls.Add(this.pnlPreview);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 110);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(25);
            this.pnlContent.Size = new System.Drawing.Size(900, 585);
            this.pnlContent.TabIndex = 3;
            // 
            // lblGuide
            // 
            this.lblGuide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.lblGuide.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblGuide.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGuide.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblGuide.Location = new System.Drawing.Point(25, 432);
            this.lblGuide.Name = "lblGuide";
            this.lblGuide.Padding = new System.Windows.Forms.Padding(10);
            this.lblGuide.Size = new System.Drawing.Size(850, 118);
            this.lblGuide.TabIndex = 2;
            this.lblGuide.Text = "โหมดระยะสั้น (ไม่ลากวาง): ออกแบบ Workflow แบบแถวต่อแถว\r\n\r\n- ใช้ปุ่ม + เพิ่มขั้นตอน เพื่อเพิ่มแถว\r\n- กำหนดลำดับด้วย ย้ายขึ้น/ย้ายลง\r\n- ชื่อขั้นตอนต้องไม่ซ้ำ\r\n- คอลัมน์ ขั้นตอนถัดไป ให้ใส่ชื่อ Step ที่จะไปต่อ";
            // 
            // pnlStepActions
            // 
            this.pnlStepActions.Controls.Add(this.btnMoveDown);
            this.pnlStepActions.Controls.Add(this.btnMoveUp);
            this.pnlStepActions.Controls.Add(this.btnDeleteStep);
            this.pnlStepActions.Controls.Add(this.btnAddStep);
            this.pnlStepActions.Controls.Add(this.btnAutoPath);
            this.pnlStepActions.Location = new System.Drawing.Point(25, 25);
            this.pnlStepActions.Name = "pnlStepActions";
            this.pnlStepActions.Size = new System.Drawing.Size(850, 45);
            this.pnlStepActions.TabIndex = 0;
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Location = new System.Drawing.Point(325, 8);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(95, 28);
            this.btnMoveDown.TabIndex = 3;
            this.btnMoveDown.Text = "ย้ายลง";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            // 
            // btnAutoPath
            // 
            this.btnAutoPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnAutoPath.ForeColor = System.Drawing.Color.White;
            this.btnAutoPath.Location = new System.Drawing.Point(430, 8);
            this.btnAutoPath.Name = "btnAutoPath";
            this.btnAutoPath.Size = new System.Drawing.Size(165, 28);
            this.btnAutoPath.TabIndex = 4;
            this.btnAutoPath.Text = "Generate เส้นทางอัตโนมัติ";
            this.btnAutoPath.UseVisualStyleBackColor = false;
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Location = new System.Drawing.Point(224, 8);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(95, 28);
            this.btnMoveUp.TabIndex = 2;
            this.btnMoveUp.Text = "ย้ายขึ้น";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            // 
            // btnDeleteStep
            // 
            this.btnDeleteStep.Location = new System.Drawing.Point(118, 8);
            this.btnDeleteStep.Name = "btnDeleteStep";
            this.btnDeleteStep.Size = new System.Drawing.Size(100, 28);
            this.btnDeleteStep.TabIndex = 1;
            this.btnDeleteStep.Text = "ลบขั้นตอน";
            this.btnDeleteStep.UseVisualStyleBackColor = true;
            // 
            // btnAddStep
            // 
            this.btnAddStep.Location = new System.Drawing.Point(12, 8);
            this.btnAddStep.Name = "btnAddStep";
            this.btnAddStep.Size = new System.Drawing.Size(100, 28);
            this.btnAddStep.TabIndex = 0;
            this.btnAddStep.Text = "+ เพิ่มขั้นตอน";
            this.btnAddStep.UseVisualStyleBackColor = true;
            // 
            // dgvSteps
            // 
            this.dgvSteps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSteps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSeq,
            this.colNodeType,
            this.colStepName,
            this.colAction,
            this.colNextStep});
            this.dgvSteps.Location = new System.Drawing.Point(25, 76);
            this.dgvSteps.Name = "dgvSteps";
            this.dgvSteps.Size = new System.Drawing.Size(850, 250);
            this.dgvSteps.TabIndex = 1;
            // 
            // colSeq
            // 
            this.colSeq.HeaderText = "ลำดับ";
            this.colSeq.Name = "colSeq";
            this.colSeq.ReadOnly = true;
            this.colSeq.Width = 60;
            // 
            // colNodeType
            // 
            this.colNodeType.HeaderText = "สถานะ";
            this.colNodeType.Name = "colNodeType";
            this.colNodeType.ReadOnly = true;
            this.colNodeType.Width = 90;
            // 
            // colStepName
            // 
            this.colStepName.HeaderText = "ชื่อขั้นตอน";
            this.colStepName.Name = "colStepName";
            this.colStepName.Width = 220;
            // 
            // colAction
            // 
            this.colAction.HeaderText = "ประเภทกิจกรรม";
            this.colAction.Name = "colAction";
            this.colAction.Width = 170;
            // 
            // colNextStep
            // 
            this.colNextStep.HeaderText = "ขั้นตอนถัดไป (ชื่อ Step)";
            this.colNextStep.Name = "colNextStep";
            this.colNextStep.Width = 260;
            // 
            // pnlPreview
            // 
            this.pnlPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
            this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreview.Controls.Add(this.rtbPreview);
            this.pnlPreview.Controls.Add(this.lblPreviewTitle);
            this.pnlPreview.Location = new System.Drawing.Point(25, 332);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(850, 94);
            this.pnlPreview.TabIndex = 4;
            // 
            // lblPreviewTitle
            // 
            this.lblPreviewTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(150)))), ((int)(((byte)(105)))));
            this.lblPreviewTitle.Location = new System.Drawing.Point(8, 6);
            this.lblPreviewTitle.Name = "lblPreviewTitle";
            this.lblPreviewTitle.Size = new System.Drawing.Size(140, 16);
            this.lblPreviewTitle.TabIndex = 0;
            this.lblPreviewTitle.Text = "Preview เส้นทาง:";
            // 
            // rtbPreview
            // 
            this.rtbPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(253)))), ((int)(((byte)(244)))));
            this.rtbPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbPreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rtbPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(120)))), ((int)(((byte)(87)))));
            this.rtbPreview.Location = new System.Drawing.Point(8, 26);
            this.rtbPreview.Name = "rtbPreview";
            this.rtbPreview.ReadOnly = true;
            this.rtbPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbPreview.Size = new System.Drawing.Size(834, 60);
            this.rtbPreview.TabIndex = 1;
            this.rtbPreview.TabStop = false;
            this.rtbPreview.Text = "(ยังไม่มีขั้นตอน)";
            // 
            // WorkflowCreateTemplateStep2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
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
            this.Name = "WorkflowCreateTemplateStep2Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ออกแบบ Flow - Step 2";
            this.pnlHeader.ResumeLayout(false);
            this.pnlSteps.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlStepActions.ResumeLayout(false);
            this.pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSteps)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSteps;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label lblGuide;
        private System.Windows.Forms.Panel pnlStepActions;
        private System.Windows.Forms.Button btnAddStep;
        private System.Windows.Forms.Button btnDeleteStep;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.DataGridView dgvSteps;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeq;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStepName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNextStep;
        private System.Windows.Forms.Button btnAutoPath;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPreviewTitle;
        private System.Windows.Forms.RichTextBox rtbPreview;
    }
}
