namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowCreateTemplateStep3Form
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
            this.btnSaveDraft = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();

            // Palette (Left)
            this.pnlPalette = new System.Windows.Forms.Panel();
            this.lblPaletteHeader = new System.Windows.Forms.Label();

            this.lblPaletteSection1 = new System.Windows.Forms.Label();
            this.flowPaletteBasic = new System.Windows.Forms.FlowLayoutPanel();

            this.lblPaletteSection2 = new System.Windows.Forms.Label();
            this.flowPaletteSelect = new System.Windows.Forms.FlowLayoutPanel();

            this.lblPaletteSection3 = new System.Windows.Forms.Label();
            this.flowPaletteAdvanced = new System.Windows.Forms.FlowLayoutPanel();

            this.lblPaletteSection4 = new System.Windows.Forms.Label();
            this.flowPaletteLayout = new System.Windows.Forms.FlowLayoutPanel();

            // Canvas (Center)
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.txtFormTitle = new System.Windows.Forms.TextBox();
            this.pnlSectionsContainer = new System.Windows.Forms.Panel();

            // Properties (Right)
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.lblPropertiesTitle = new System.Windows.Forms.Label();
            this.lblNoSelection = new System.Windows.Forms.Label();

            this.pnlFieldProps = new System.Windows.Forms.Panel();
            this.lblPropLabel = new System.Windows.Forms.Label();
            this.txtPropLabel = new System.Windows.Forms.TextBox();
            this.lblPropFieldName = new System.Windows.Forms.Label();
            this.txtPropFieldName = new System.Windows.Forms.TextBox();
            this.lblPropHelpText = new System.Windows.Forms.Label();
            this.txtPropHelpText = new System.Windows.Forms.TextBox();
            this.chkRequired = new System.Windows.Forms.CheckBox();
            this.chkVisible = new System.Windows.Forms.CheckBox();
            this.lblPropSource = new System.Windows.Forms.Label();
            this.cmbPropSource = new System.Windows.Forms.ComboBox();
            this.lblPropDefault = new System.Windows.Forms.Label();
            this.txtPropDefault = new System.Windows.Forms.TextBox();
            this.lblPropConditional = new System.Windows.Forms.Label();
            this.txtPropConditional = new System.Windows.Forms.TextBox();
            this.lblPropMapContext = new System.Windows.Forms.Label();
            this.txtPropMapContext = new System.Windows.Forms.TextBox();
            this.btnApplyProps = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlSteps.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlPalette.SuspendLayout();
            this.flowPaletteBasic.SuspendLayout();
            this.flowPaletteSelect.SuspendLayout();
            this.flowPaletteAdvanced.SuspendLayout();
            this.flowPaletteLayout.SuspendLayout();
            this.pnlCanvas.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.pnlFieldProps.SuspendLayout();
            this.SuspendLayout();

            // ==================== Header ====================
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(20);
            this.pnlHeader.Size = new System.Drawing.Size(1200, 70);
            this.pnlHeader.TabIndex = 0;

            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1160, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "3) ออกแบบฟอร์ม";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ==================== Steps ====================
            this.pnlSteps.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
            this.pnlSteps.Controls.Add(this.lblSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSteps.Location = new System.Drawing.Point(0, 70);
            this.pnlSteps.Name = "pnlSteps";
            this.pnlSteps.Padding = new System.Windows.Forms.Padding(10);
            this.pnlSteps.Size = new System.Drawing.Size(1200, 40);
            this.pnlSteps.TabIndex = 1;

            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSteps.Location = new System.Drawing.Point(10, 10);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Size = new System.Drawing.Size(1180, 20);
            this.lblSteps.TabIndex = 0;
            this.lblSteps.Text = "1) ข้อมูลพื้นฐาน  →  2) ออกแบบ Flow  →  3) ออกแบบฟอร์ม  →  4) ส่งอนุมัติ/Publish";
            this.lblSteps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ==================== Buttons ====================
            this.pnlButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlButtons.Controls.Add(this.btnPrev);
            this.pnlButtons.Controls.Add(this.btnSaveDraft);
            this.pnlButtons.Controls.Add(this.btnNext);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 1130);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10);
            this.pnlButtons.Size = new System.Drawing.Size(1200, 50);
            this.pnlButtons.TabIndex = 2;

            this.btnPrev.Location = new System.Drawing.Point(980, 12);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 28);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "ย้อนกลับ";

            this.btnSaveDraft.Location = new System.Drawing.Point(1060, 12);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(75, 28);
            this.btnSaveDraft.TabIndex = 1;
            this.btnSaveDraft.Text = "บันทึก Draft";

            this.btnNext.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Location = new System.Drawing.Point(1115, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 28);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "ถัดไป";
            this.btnNext.UseVisualStyleBackColor = false;

            // ==================== Main Container (3 panels) ====================
            this.pnlMain.BackColor = System.Drawing.Color.White;
            this.pnlMain.Controls.Add(this.pnlProperties);
            this.pnlMain.Controls.Add(this.pnlCanvas);
            this.pnlMain.Controls.Add(this.pnlPalette);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 110);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1200, 1020);
            this.pnlMain.TabIndex = 3;

            // ==================== PALETTE (LEFT) ====================
            this.pnlPalette.BackColor = System.Drawing.Color.FromArgb(250, 251, 252);
            this.pnlPalette.Controls.Add(this.lblPaletteHeader);
            this.pnlPalette.Controls.Add(this.flowPaletteLayout);
            this.pnlPalette.Controls.Add(this.lblPaletteSection4);
            this.pnlPalette.Controls.Add(this.flowPaletteAdvanced);
            this.pnlPalette.Controls.Add(this.lblPaletteSection3);
            this.pnlPalette.Controls.Add(this.flowPaletteSelect);
            this.pnlPalette.Controls.Add(this.lblPaletteSection2);
            this.pnlPalette.Controls.Add(this.flowPaletteBasic);
            this.pnlPalette.Controls.Add(this.lblPaletteSection1);
            this.pnlPalette.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPalette.Location = new System.Drawing.Point(0, 0);
            this.pnlPalette.Name = "pnlPalette";
            this.pnlPalette.Padding = new System.Windows.Forms.Padding(10);
            this.pnlPalette.Size = new System.Drawing.Size(180, 1020);
            this.pnlPalette.TabIndex = 0;
            this.pnlPalette.AutoScroll = true;

            this.lblPaletteHeader.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblPaletteHeader.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblPaletteHeader.Location = new System.Drawing.Point(10, 10);
            this.lblPaletteHeader.Name = "lblPaletteHeader";
            this.lblPaletteHeader.Size = new System.Drawing.Size(160, 25);
            this.lblPaletteHeader.TabIndex = 0;
            this.lblPaletteHeader.Text = "📋 Field Types";

            this.lblPaletteSection1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection1.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection1.Location = new System.Drawing.Point(10, 40);
            this.lblPaletteSection1.Name = "lblPaletteSection1";
            this.lblPaletteSection1.Size = new System.Drawing.Size(160, 20);
            this.lblPaletteSection1.TabIndex = 1;
            this.lblPaletteSection1.Text = "พื้นฐาน";

            this.flowPaletteBasic.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteBasic.Location = new System.Drawing.Point(10, 60);
            this.flowPaletteBasic.Name = "flowPaletteBasic";
            this.flowPaletteBasic.Size = new System.Drawing.Size(160, 100);
            this.flowPaletteBasic.TabIndex = 2;
            this.flowPaletteBasic.WrapContents = false;
            this.flowPaletteBasic.AutoScroll = true;

            this.lblPaletteSection2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection2.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection2.Location = new System.Drawing.Point(10, 170);
            this.lblPaletteSection2.Name = "lblPaletteSection2";
            this.lblPaletteSection2.Size = new System.Drawing.Size(160, 20);
            this.lblPaletteSection2.TabIndex = 3;
            this.lblPaletteSection2.Text = "เลือก";

            this.flowPaletteSelect.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteSelect.Location = new System.Drawing.Point(10, 190);
            this.flowPaletteSelect.Name = "flowPaletteSelect";
            this.flowPaletteSelect.Size = new System.Drawing.Size(160, 100);
            this.flowPaletteSelect.TabIndex = 4;
            this.flowPaletteSelect.WrapContents = false;
            this.flowPaletteSelect.AutoScroll = true;

            this.lblPaletteSection3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection3.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection3.Location = new System.Drawing.Point(10, 300);
            this.lblPaletteSection3.Name = "lblPaletteSection3";
            this.lblPaletteSection3.Size = new System.Drawing.Size(160, 20);
            this.lblPaletteSection3.TabIndex = 5;
            this.lblPaletteSection3.Text = "ขั้นสูง";

            this.flowPaletteAdvanced.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteAdvanced.Location = new System.Drawing.Point(10, 320);
            this.flowPaletteAdvanced.Name = "flowPaletteAdvanced";
            this.flowPaletteAdvanced.Size = new System.Drawing.Size(160, 100);
            this.flowPaletteAdvanced.TabIndex = 6;
            this.flowPaletteAdvanced.WrapContents = false;
            this.flowPaletteAdvanced.AutoScroll = true;

            this.lblPaletteSection4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection4.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection4.Location = new System.Drawing.Point(10, 430);
            this.lblPaletteSection4.Name = "lblPaletteSection4";
            this.lblPaletteSection4.Size = new System.Drawing.Size(160, 20);
            this.lblPaletteSection4.TabIndex = 7;
            this.lblPaletteSection4.Text = "เค้าโครง";

            this.flowPaletteLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteLayout.Location = new System.Drawing.Point(10, 450);
            this.flowPaletteLayout.Name = "flowPaletteLayout";
            this.flowPaletteLayout.Size = new System.Drawing.Size(160, 70);
            this.flowPaletteLayout.TabIndex = 8;
            this.flowPaletteLayout.WrapContents = false;
            this.flowPaletteLayout.AutoScroll = true;

            // ==================== CANVAS (CENTER) ====================
            this.pnlCanvas.BackColor = System.Drawing.Color.White;
            this.pnlCanvas.Controls.Add(this.pnlSectionsContainer);
            this.pnlCanvas.Controls.Add(this.txtFormTitle);
            this.pnlCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.Location = new System.Drawing.Point(180, 0);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Padding = new System.Windows.Forms.Padding(20);
            this.pnlCanvas.Size = new System.Drawing.Size(740, 1020);
            this.pnlCanvas.TabIndex = 1;
            this.pnlCanvas.AutoScroll = true;

            this.txtFormTitle.BackColor = System.Drawing.Color.White;
            this.txtFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFormTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.txtFormTitle.ForeColor = System.Drawing.Color.FromArgb(30, 58, 138);
            this.txtFormTitle.Location = new System.Drawing.Point(15, 15);
            this.txtFormTitle.Name = "txtFormTitle";
            this.txtFormTitle.Size = new System.Drawing.Size(680, 38);
            this.txtFormTitle.TabIndex = 0;
            this.txtFormTitle.Text = "ชื่อฟอร์ม";
            this.txtFormTitle.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);

            this.pnlSectionsContainer.Location = new System.Drawing.Point(15, 70);
            this.pnlSectionsContainer.Name = "pnlSectionsContainer";
            this.pnlSectionsContainer.Size = new System.Drawing.Size(690, 900);
            this.pnlSectionsContainer.TabIndex = 1;
            this.pnlSectionsContainer.AutoScroll = true;
            this.pnlSectionsContainer.BackColor = System.Drawing.Color.White;

            // ==================== PROPERTIES (RIGHT) ====================
            this.pnlProperties.BackColor = System.Drawing.Color.FromArgb(240, 245, 250);
            this.pnlProperties.Controls.Add(this.pnlFieldProps);
            this.pnlProperties.Controls.Add(this.lblNoSelection);
            this.pnlProperties.Controls.Add(this.lblPropertiesTitle);
            this.pnlProperties.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Location = new System.Drawing.Point(920, 0);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Padding = new System.Windows.Forms.Padding(15);
            this.pnlProperties.Size = new System.Drawing.Size(280, 1020);
            this.pnlProperties.TabIndex = 2;
            this.pnlProperties.AutoScroll = true;

            this.lblPropertiesTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblPropertiesTitle.ForeColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.lblPropertiesTitle.Location = new System.Drawing.Point(15, 15);
            this.lblPropertiesTitle.Name = "lblPropertiesTitle";
            this.lblPropertiesTitle.Size = new System.Drawing.Size(250, 25);
            this.lblPropertiesTitle.TabIndex = 0;
            this.lblPropertiesTitle.Text = "⚙️ Field Properties";

            this.lblNoSelection.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNoSelection.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblNoSelection.Location = new System.Drawing.Point(15, 50);
            this.lblNoSelection.Name = "lblNoSelection";
            this.lblNoSelection.Size = new System.Drawing.Size(250, 50);
            this.lblNoSelection.TabIndex = 1;
            this.lblNoSelection.Text = "คลิกที่ field ในฟอร์มเพื่อแก้ไขคุณสมบัติ";

            this.pnlFieldProps.BackColor = System.Drawing.Color.White;
            this.pnlFieldProps.Controls.Add(this.btnApplyProps);
            this.pnlFieldProps.Controls.Add(this.txtPropMapContext);
            this.pnlFieldProps.Controls.Add(this.lblPropMapContext);
            this.pnlFieldProps.Controls.Add(this.txtPropConditional);
            this.pnlFieldProps.Controls.Add(this.lblPropConditional);
            this.pnlFieldProps.Controls.Add(this.txtPropDefault);
            this.pnlFieldProps.Controls.Add(this.lblPropDefault);
            this.pnlFieldProps.Controls.Add(this.cmbPropSource);
            this.pnlFieldProps.Controls.Add(this.lblPropSource);
            this.pnlFieldProps.Controls.Add(this.chkVisible);
            this.pnlFieldProps.Controls.Add(this.chkRequired);
            this.pnlFieldProps.Controls.Add(this.txtPropHelpText);
            this.pnlFieldProps.Controls.Add(this.lblPropHelpText);
            this.pnlFieldProps.Controls.Add(this.txtPropFieldName);
            this.pnlFieldProps.Controls.Add(this.lblPropFieldName);
            this.pnlFieldProps.Controls.Add(this.txtPropLabel);
            this.pnlFieldProps.Controls.Add(this.lblPropLabel);
            this.pnlFieldProps.Location = new System.Drawing.Point(15, 50);
            this.pnlFieldProps.Name = "pnlFieldProps";
            this.pnlFieldProps.Padding = new System.Windows.Forms.Padding(10);
            this.pnlFieldProps.Size = new System.Drawing.Size(250, 950);
            this.pnlFieldProps.TabIndex = 2;
            this.pnlFieldProps.Visible = false;
            this.pnlFieldProps.AutoScroll = true;

            this.lblPropLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropLabel.Location = new System.Drawing.Point(10, 10);
            this.lblPropLabel.Name = "lblPropLabel";
            this.lblPropLabel.Size = new System.Drawing.Size(230, 18);
            this.lblPropLabel.TabIndex = 0;
            this.lblPropLabel.Text = "Label";

            this.txtPropLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropLabel.Location = new System.Drawing.Point(10, 30);
            this.txtPropLabel.Name = "txtPropLabel";
            this.txtPropLabel.Size = new System.Drawing.Size(230, 23);
            this.txtPropLabel.TabIndex = 1;

            this.lblPropFieldName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropFieldName.Location = new System.Drawing.Point(10, 60);
            this.lblPropFieldName.Name = "lblPropFieldName";
            this.lblPropFieldName.Size = new System.Drawing.Size(230, 18);
            this.lblPropFieldName.TabIndex = 2;
            this.lblPropFieldName.Text = "Field Name (ID)";

            this.txtPropFieldName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropFieldName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropFieldName.Location = new System.Drawing.Point(10, 80);
            this.txtPropFieldName.Name = "txtPropFieldName";
            this.txtPropFieldName.Size = new System.Drawing.Size(230, 23);
            this.txtPropFieldName.TabIndex = 3;

            this.lblPropHelpText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropHelpText.Location = new System.Drawing.Point(10, 110);
            this.lblPropHelpText.Name = "lblPropHelpText";
            this.lblPropHelpText.Size = new System.Drawing.Size(230, 18);
            this.lblPropHelpText.TabIndex = 4;
            this.lblPropHelpText.Text = "Help Text";

            this.txtPropHelpText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropHelpText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropHelpText.Location = new System.Drawing.Point(10, 130);
            this.txtPropHelpText.Multiline = true;
            this.txtPropHelpText.Name = "txtPropHelpText";
            this.txtPropHelpText.Size = new System.Drawing.Size(230, 60);
            this.txtPropHelpText.TabIndex = 5;

            this.chkRequired.AutoSize = true;
            this.chkRequired.Location = new System.Drawing.Point(10, 200);
            this.chkRequired.Name = "chkRequired";
            this.chkRequired.Size = new System.Drawing.Size(70, 19);
            this.chkRequired.TabIndex = 6;
            this.chkRequired.Text = "Required";

            this.chkVisible.AutoSize = true;
            this.chkVisible.Checked = true;
            this.chkVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVisible.Location = new System.Drawing.Point(10, 225);
            this.chkVisible.Name = "chkVisible";
            this.chkVisible.Size = new System.Drawing.Size(63, 19);
            this.chkVisible.TabIndex = 7;
            this.chkVisible.Text = "Visible";

            this.lblPropSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropSource.Location = new System.Drawing.Point(10, 250);
            this.lblPropSource.Name = "lblPropSource";
            this.lblPropSource.Size = new System.Drawing.Size(230, 18);
            this.lblPropSource.TabIndex = 8;
            this.lblPropSource.Text = "Source";

            this.cmbPropSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPropSource.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbPropSource.Location = new System.Drawing.Point(10, 270);
            this.cmbPropSource.Name = "cmbPropSource";
            this.cmbPropSource.Size = new System.Drawing.Size(230, 25);
            this.cmbPropSource.TabIndex = 9;

            this.lblPropDefault.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropDefault.Location = new System.Drawing.Point(10, 300);
            this.lblPropDefault.Name = "lblPropDefault";
            this.lblPropDefault.Size = new System.Drawing.Size(230, 18);
            this.lblPropDefault.TabIndex = 10;
            this.lblPropDefault.Text = "Default Value";

            this.txtPropDefault.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropDefault.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropDefault.Location = new System.Drawing.Point(10, 320);
            this.txtPropDefault.Name = "txtPropDefault";
            this.txtPropDefault.Size = new System.Drawing.Size(230, 23);
            this.txtPropDefault.TabIndex = 11;

            this.lblPropConditional.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropConditional.Location = new System.Drawing.Point(10, 350);
            this.lblPropConditional.Name = "lblPropConditional";
            this.lblPropConditional.Size = new System.Drawing.Size(230, 18);
            this.lblPropConditional.TabIndex = 12;
            this.lblPropConditional.Text = "Conditional Logic";

            this.txtPropConditional.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropConditional.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtPropConditional.Location = new System.Drawing.Point(10, 370);
            this.txtPropConditional.Multiline = true;
            this.txtPropConditional.Name = "txtPropConditional";
            this.txtPropConditional.Size = new System.Drawing.Size(230, 80);
            this.txtPropConditional.TabIndex = 13;

            this.lblPropMapContext.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPropMapContext.Location = new System.Drawing.Point(10, 460);
            this.lblPropMapContext.Name = "lblPropMapContext";
            this.lblPropMapContext.Size = new System.Drawing.Size(230, 18);
            this.lblPropMapContext.TabIndex = 14;
            this.lblPropMapContext.Text = "Map to Context";

            this.txtPropMapContext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropMapContext.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropMapContext.Location = new System.Drawing.Point(10, 480);
            this.txtPropMapContext.Name = "txtPropMapContext";
            this.txtPropMapContext.Size = new System.Drawing.Size(230, 23);
            this.txtPropMapContext.TabIndex = 15;

            this.btnApplyProps.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnApplyProps.ForeColor = System.Drawing.Color.White;
            this.btnApplyProps.Location = new System.Drawing.Point(10, 520);
            this.btnApplyProps.Name = "btnApplyProps";
            this.btnApplyProps.Size = new System.Drawing.Size(230, 30);
            this.btnApplyProps.TabIndex = 16;
            this.btnApplyProps.Text = "Apply Changes";
            this.btnApplyProps.UseVisualStyleBackColor = false;

            // ==================== Main Form ====================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 1180);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlSteps);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkflowCreateTemplateStep3Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "3) ออกแบบฟอร์ม - Step 3";
            this.pnlHeader.ResumeLayout(false);
            this.pnlSteps.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlPalette.ResumeLayout(false);
            this.flowPaletteBasic.ResumeLayout(false);
            this.flowPaletteSelect.ResumeLayout(false);
            this.flowPaletteAdvanced.ResumeLayout(false);
            this.flowPaletteLayout.ResumeLayout(false);
            this.pnlCanvas.ResumeLayout(false);
            this.pnlCanvas.PerformLayout();
            this.pnlProperties.ResumeLayout(false);
            this.pnlFieldProps.ResumeLayout(false);
            this.pnlFieldProps.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSteps;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnSaveDraft;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Panel pnlMain;

        // Palette
        private System.Windows.Forms.Panel pnlPalette;
        private System.Windows.Forms.Label lblPaletteHeader;
        private System.Windows.Forms.Label lblPaletteSection1;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteBasic;
        private System.Windows.Forms.Label lblPaletteSection2;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteSelect;
        private System.Windows.Forms.Label lblPaletteSection3;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteAdvanced;
        private System.Windows.Forms.Label lblPaletteSection4;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteLayout;

        // Canvas
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.TextBox txtFormTitle;
        private System.Windows.Forms.Panel pnlSectionsContainer;

        // Properties
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.Label lblPropertiesTitle;
        private System.Windows.Forms.Label lblNoSelection;
        private System.Windows.Forms.Panel pnlFieldProps;
        private System.Windows.Forms.Label lblPropLabel;
        private System.Windows.Forms.TextBox txtPropLabel;
        private System.Windows.Forms.Label lblPropFieldName;
        private System.Windows.Forms.TextBox txtPropFieldName;
        private System.Windows.Forms.Label lblPropHelpText;
        private System.Windows.Forms.TextBox txtPropHelpText;
        private System.Windows.Forms.CheckBox chkRequired;
        private System.Windows.Forms.CheckBox chkVisible;
        private System.Windows.Forms.Label lblPropSource;
        private System.Windows.Forms.ComboBox cmbPropSource;
        private System.Windows.Forms.Label lblPropDefault;
        private System.Windows.Forms.TextBox txtPropDefault;
        private System.Windows.Forms.Label lblPropConditional;
        private System.Windows.Forms.TextBox txtPropConditional;
        private System.Windows.Forms.Label lblPropMapContext;
        private System.Windows.Forms.TextBox txtPropMapContext;
        private System.Windows.Forms.Button btnApplyProps;
    }
}
