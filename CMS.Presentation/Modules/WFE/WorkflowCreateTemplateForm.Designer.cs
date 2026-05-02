namespace CMS.Presentation.Modules.Workflow
{
    partial class WorkflowCreateTemplateForm
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
            this.btnSaveDraft = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.lblSection1 = new System.Windows.Forms.Label();
            this.pnlRadioFresh = new System.Windows.Forms.Panel();
            this.rdoFresh = new System.Windows.Forms.RadioButton();
            this.lblFreshTitle = new System.Windows.Forms.Label();
            this.lblFreshDesc = new System.Windows.Forms.Label();
            this.pnlRadioCopy = new System.Windows.Forms.Panel();
            this.rdoCopy = new System.Windows.Forms.RadioButton();
            this.lblCopyTitle = new System.Windows.Forms.Label();
            this.lblCopyDesc = new System.Windows.Forms.Label();
            this.pnlRadioImport = new System.Windows.Forms.Panel();
            this.rdoImport = new System.Windows.Forms.RadioButton();
            this.lblImportTitle = new System.Windows.Forms.Label();
            this.lblImportDesc = new System.Windows.Forms.Label();
            this.pnlCopyOptions = new System.Windows.Forms.Panel();
            this.lblCopyFrom = new System.Windows.Forms.Label();
            this.cmbCopyFrom = new System.Windows.Forms.ComboBox();
            this.pnlImportOptions = new System.Windows.Forms.Panel();
            this.lblImportFile = new System.Windows.Forms.Label();
            this.txtImportPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSection2 = new System.Windows.Forms.Label();
            this.lblTemplateName = new System.Windows.Forms.Label();
            this.txtTemplateName = new System.Windows.Forms.TextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblTemplateCode = new System.Windows.Forms.Label();
            this.txtTemplateCode = new System.Windows.Forms.TextBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.lblOwner = new System.Windows.Forms.Label();
            this.cmbOwner = new System.Windows.Forms.ComboBox();
            this.lblSection3 = new System.Windows.Forms.Label();
            this.lblSlaDays = new System.Windows.Forms.Label();
            this.txtSlaDays = new System.Windows.Forms.TextBox();
            this.lblPriority = new System.Windows.Forms.Label();
            this.cmbPriority = new System.Windows.Forms.ComboBox();
            this.lblInitiator = new System.Windows.Forms.Label();
            this.cmbInitiator = new System.Windows.Forms.ComboBox();
            this.lblTags = new System.Windows.Forms.Label();
            this.txtTags = new System.Windows.Forms.TextBox();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.chkFormBuilder = new System.Windows.Forms.CheckBox();
            this.chkOwnerCommittee = new System.Windows.Forms.CheckBox();
            this.chkAllowCancel = new System.Windows.Forms.CheckBox();
            this.chkAutoStoreToDms = new System.Windows.Forms.CheckBox();
            this.pnlHeader.SuspendLayout();
            this.pnlSteps.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlRadioFresh.SuspendLayout();
            this.pnlRadioCopy.SuspendLayout();
            this.pnlRadioImport.SuspendLayout();
            this.pnlCopyOptions.SuspendLayout();
            this.pnlImportOptions.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Location = new System.Drawing.Point(0, 39);
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
            this.lblTitle.Text = "สร้าง Template Workflow ใหม่";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSteps
            // 
            this.pnlSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.pnlSteps.Controls.Add(this.lblSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSteps.Location = new System.Drawing.Point(0, 0);
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
            this.pnlButtons.Controls.Add(this.btnSaveDraft);
            this.pnlButtons.Controls.Add(this.btnNext);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 695);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Padding = new System.Windows.Forms.Padding(10);
            this.pnlButtons.Size = new System.Drawing.Size(900, 55);
            this.pnlButtons.TabIndex = 2;
            // 
            // btnSaveDraft
            // 
            this.btnSaveDraft.Location = new System.Drawing.Point(570, 12);
            this.btnSaveDraft.Name = "btnSaveDraft";
            this.btnSaveDraft.Size = new System.Drawing.Size(85, 30);
            this.btnSaveDraft.TabIndex = 0;
            this.btnSaveDraft.Text = "บันทึก Draft";
            // 
            // btnNext
            // 
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Location = new System.Drawing.Point(660, 12);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 30);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "ถัดไป";
            this.btnNext.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(740, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "ยกเลิก";
            // 
            // pnlContent
            // 
            this.pnlContent.AutoScroll = true;
            this.pnlContent.BackColor = System.Drawing.Color.White;
            this.pnlContent.Controls.Add(this.lblSection1);
            this.pnlContent.Controls.Add(this.pnlRadioFresh);
            this.pnlContent.Controls.Add(this.pnlRadioCopy);
            this.pnlContent.Controls.Add(this.pnlRadioImport);
            this.pnlContent.Controls.Add(this.pnlCopyOptions);
            this.pnlContent.Controls.Add(this.pnlImportOptions);
            this.pnlContent.Controls.Add(this.lblSection2);
            this.pnlContent.Controls.Add(this.pnlHeader);
            this.pnlContent.Controls.Add(this.lblTemplateName);
            this.pnlContent.Controls.Add(this.txtTemplateName);
            this.pnlContent.Controls.Add(this.lblDescription);
            this.pnlContent.Controls.Add(this.txtDescription);
            this.pnlContent.Controls.Add(this.lblTemplateCode);
            this.pnlContent.Controls.Add(this.txtTemplateCode);
            this.pnlContent.Controls.Add(this.lblCategory);
            this.pnlContent.Controls.Add(this.cmbCategory);
            this.pnlContent.Controls.Add(this.lblOwner);
            this.pnlContent.Controls.Add(this.cmbOwner);
            this.pnlContent.Controls.Add(this.lblSection3);
            this.pnlContent.Controls.Add(this.lblSlaDays);
            this.pnlContent.Controls.Add(this.txtSlaDays);
            this.pnlContent.Controls.Add(this.lblPriority);
            this.pnlContent.Controls.Add(this.cmbPriority);
            this.pnlContent.Controls.Add(this.lblInitiator);
            this.pnlContent.Controls.Add(this.cmbInitiator);
            this.pnlContent.Controls.Add(this.lblTags);
            this.pnlContent.Controls.Add(this.txtTags);
            this.pnlContent.Controls.Add(this.pnlOptions);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(25);
            this.pnlContent.Size = new System.Drawing.Size(900, 750);
            this.pnlContent.TabIndex = 3;
            // 
            // lblSection1
            // 
            this.lblSection1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSection1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblSection1.Location = new System.Drawing.Point(25, 125);
            this.lblSection1.Name = "lblSection1";
            this.lblSection1.Size = new System.Drawing.Size(200, 25);
            this.lblSection1.TabIndex = 0;
            this.lblSection1.Text = "1. เริ่มจาก";
            // 
            // pnlRadioFresh
            // 
            this.pnlRadioFresh.BackColor = System.Drawing.Color.White;
            this.pnlRadioFresh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRadioFresh.Controls.Add(this.rdoFresh);
            this.pnlRadioFresh.Controls.Add(this.lblFreshTitle);
            this.pnlRadioFresh.Controls.Add(this.lblFreshDesc);
            this.pnlRadioFresh.Location = new System.Drawing.Point(25, 155);
            this.pnlRadioFresh.Name = "pnlRadioFresh";
            this.pnlRadioFresh.Size = new System.Drawing.Size(240, 90);
            this.pnlRadioFresh.TabIndex = 1;
            // 
            // rdoFresh
            // 
            this.rdoFresh.AutoSize = true;
            this.rdoFresh.Checked = true;
            this.rdoFresh.Location = new System.Drawing.Point(10, 10);
            this.rdoFresh.Name = "rdoFresh";
            this.rdoFresh.Size = new System.Drawing.Size(14, 13);
            this.rdoFresh.TabIndex = 0;
            this.rdoFresh.TabStop = true;
            this.rdoFresh.UseVisualStyleBackColor = true;
            // 
            // lblFreshTitle
            // 
            this.lblFreshTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFreshTitle.Location = new System.Drawing.Point(30, 10);
            this.lblFreshTitle.Name = "lblFreshTitle";
            this.lblFreshTitle.Size = new System.Drawing.Size(200, 20);
            this.lblFreshTitle.TabIndex = 1;
            this.lblFreshTitle.Text = "เริ่มจากเปล่า";
            // 
            // lblFreshDesc
            // 
            this.lblFreshDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFreshDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblFreshDesc.Location = new System.Drawing.Point(30, 35);
            this.lblFreshDesc.Name = "lblFreshDesc";
            this.lblFreshDesc.Size = new System.Drawing.Size(200, 50);
            this.lblFreshDesc.TabIndex = 2;
            this.lblFreshDesc.Text = "ออกแบบ flow เองทั้งหมด\nบน Designer";
            // 
            // pnlRadioCopy
            // 
            this.pnlRadioCopy.BackColor = System.Drawing.Color.White;
            this.pnlRadioCopy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRadioCopy.Controls.Add(this.rdoCopy);
            this.pnlRadioCopy.Controls.Add(this.lblCopyTitle);
            this.pnlRadioCopy.Controls.Add(this.lblCopyDesc);
            this.pnlRadioCopy.Location = new System.Drawing.Point(280, 155);
            this.pnlRadioCopy.Name = "pnlRadioCopy";
            this.pnlRadioCopy.Size = new System.Drawing.Size(240, 90);
            this.pnlRadioCopy.TabIndex = 2;
            // 
            // rdoCopy
            // 
            this.rdoCopy.AutoSize = true;
            this.rdoCopy.Location = new System.Drawing.Point(10, 10);
            this.rdoCopy.Name = "rdoCopy";
            this.rdoCopy.Size = new System.Drawing.Size(14, 13);
            this.rdoCopy.TabIndex = 0;
            this.rdoCopy.UseVisualStyleBackColor = true;
            // 
            // lblCopyTitle
            // 
            this.lblCopyTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCopyTitle.Location = new System.Drawing.Point(30, 10);
            this.lblCopyTitle.Name = "lblCopyTitle";
            this.lblCopyTitle.Size = new System.Drawing.Size(200, 20);
            this.lblCopyTitle.TabIndex = 1;
            this.lblCopyTitle.Text = "คัดลอกจาก Template เดิม";
            // 
            // lblCopyDesc
            // 
            this.lblCopyDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCopyDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblCopyDesc.Location = new System.Drawing.Point(30, 35);
            this.lblCopyDesc.Name = "lblCopyDesc";
            this.lblCopyDesc.Size = new System.Drawing.Size(200, 50);
            this.lblCopyDesc.TabIndex = 2;
            this.lblCopyDesc.Text = "เลือก Template ที่จะใช้\nเป็นต้นแบบ";
            // 
            // pnlRadioImport
            // 
            this.pnlRadioImport.BackColor = System.Drawing.Color.White;
            this.pnlRadioImport.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRadioImport.Controls.Add(this.rdoImport);
            this.pnlRadioImport.Controls.Add(this.lblImportTitle);
            this.pnlRadioImport.Controls.Add(this.lblImportDesc);
            this.pnlRadioImport.Location = new System.Drawing.Point(535, 155);
            this.pnlRadioImport.Name = "pnlRadioImport";
            this.pnlRadioImport.Size = new System.Drawing.Size(240, 90);
            this.pnlRadioImport.TabIndex = 3;
            // 
            // rdoImport
            // 
            this.rdoImport.AutoSize = true;
            this.rdoImport.Location = new System.Drawing.Point(10, 10);
            this.rdoImport.Name = "rdoImport";
            this.rdoImport.Size = new System.Drawing.Size(14, 13);
            this.rdoImport.TabIndex = 0;
            this.rdoImport.UseVisualStyleBackColor = true;
            // 
            // lblImportTitle
            // 
            this.lblImportTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblImportTitle.Location = new System.Drawing.Point(30, 10);
            this.lblImportTitle.Name = "lblImportTitle";
            this.lblImportTitle.Size = new System.Drawing.Size(200, 20);
            this.lblImportTitle.TabIndex = 1;
            this.lblImportTitle.Text = "นำเข้า (Import)";
            // 
            // lblImportDesc
            // 
            this.lblImportDesc.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblImportDesc.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblImportDesc.Location = new System.Drawing.Point(30, 35);
            this.lblImportDesc.Name = "lblImportDesc";
            this.lblImportDesc.Size = new System.Drawing.Size(200, 50);
            this.lblImportDesc.TabIndex = 2;
            this.lblImportDesc.Text = "จากไฟล์ .wfd หรือ\nBPMN 2.0 (.bpmn)";
            // 
            // pnlCopyOptions
            // 
            this.pnlCopyOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.pnlCopyOptions.Controls.Add(this.lblCopyFrom);
            this.pnlCopyOptions.Controls.Add(this.cmbCopyFrom);
            this.pnlCopyOptions.Location = new System.Drawing.Point(25, 260);
            this.pnlCopyOptions.Name = "pnlCopyOptions";
            this.pnlCopyOptions.Padding = new System.Windows.Forms.Padding(15);
            this.pnlCopyOptions.Size = new System.Drawing.Size(400, 70);
            this.pnlCopyOptions.TabIndex = 4;
            this.pnlCopyOptions.Visible = false;
            // 
            // lblCopyFrom
            // 
            this.lblCopyFrom.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCopyFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblCopyFrom.Location = new System.Drawing.Point(15, 10);
            this.lblCopyFrom.Name = "lblCopyFrom";
            this.lblCopyFrom.Size = new System.Drawing.Size(370, 20);
            this.lblCopyFrom.TabIndex = 0;
            this.lblCopyFrom.Text = "เลือก Template ต้นแบบ *";
            // 
            // cmbCopyFrom
            // 
            this.cmbCopyFrom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCopyFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCopyFrom.Location = new System.Drawing.Point(15, 35);
            this.cmbCopyFrom.Name = "cmbCopyFrom";
            this.cmbCopyFrom.Size = new System.Drawing.Size(370, 25);
            this.cmbCopyFrom.TabIndex = 1;
            // 
            // pnlImportOptions
            // 
            this.pnlImportOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.pnlImportOptions.Controls.Add(this.lblImportFile);
            this.pnlImportOptions.Controls.Add(this.txtImportPath);
            this.pnlImportOptions.Controls.Add(this.btnBrowse);
            this.pnlImportOptions.Location = new System.Drawing.Point(25, 260);
            this.pnlImportOptions.Name = "pnlImportOptions";
            this.pnlImportOptions.Padding = new System.Windows.Forms.Padding(15);
            this.pnlImportOptions.Size = new System.Drawing.Size(450, 70);
            this.pnlImportOptions.TabIndex = 5;
            this.pnlImportOptions.Visible = false;
            // 
            // lblImportFile
            // 
            this.lblImportFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblImportFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblImportFile.Location = new System.Drawing.Point(15, 10);
            this.lblImportFile.Name = "lblImportFile";
            this.lblImportFile.Size = new System.Drawing.Size(420, 20);
            this.lblImportFile.TabIndex = 0;
            this.lblImportFile.Text = "ไฟล์ที่จะนำเข้า *";
            // 
            // txtImportPath
            // 
            this.txtImportPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtImportPath.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtImportPath.Location = new System.Drawing.Point(15, 35);
            this.txtImportPath.Name = "txtImportPath";
            this.txtImportPath.ReadOnly = true;
            this.txtImportPath.Size = new System.Drawing.Size(335, 25);
            this.txtImportPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(355, 35);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(80, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "เลือกไฟล์";
            this.btnBrowse.UseVisualStyleBackColor = true;
            // 
            // lblSection2
            // 
            this.lblSection2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSection2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblSection2.Location = new System.Drawing.Point(25, 345);
            this.lblSection2.Name = "lblSection2";
            this.lblSection2.Size = new System.Drawing.Size(200, 25);
            this.lblSection2.TabIndex = 6;
            this.lblSection2.Text = "2. ข้อมูลพื้นฐาน";
            // 
            // lblTemplateName
            // 
            this.lblTemplateName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTemplateName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblTemplateName.Location = new System.Drawing.Point(25, 375);
            this.lblTemplateName.Name = "lblTemplateName";
            this.lblTemplateName.Size = new System.Drawing.Size(150, 25);
            this.lblTemplateName.TabIndex = 7;
            this.lblTemplateName.Text = "ชื่อ Template *";
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTemplateName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTemplateName.Location = new System.Drawing.Point(25, 405);
            this.txtTemplateName.Name = "txtTemplateName";
            this.txtTemplateName.Size = new System.Drawing.Size(400, 25);
            this.txtTemplateName.TabIndex = 8;
            // 
            // lblDescription
            // 
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblDescription.Location = new System.Drawing.Point(25, 450);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(150, 25);
            this.lblDescription.TabIndex = 9;
            this.lblDescription.Text = "คำอธิบาย";
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDescription.Location = new System.Drawing.Point(25, 480);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(400, 100);
            this.txtDescription.TabIndex = 10;
            // 
            // lblTemplateCode
            // 
            this.lblTemplateCode.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTemplateCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblTemplateCode.Location = new System.Drawing.Point(25, 375);
            this.lblTemplateCode.Name = "lblTemplateCode";
            this.lblTemplateCode.Size = new System.Drawing.Size(150, 25);
            this.lblTemplateCode.TabIndex = 11;
            this.lblTemplateCode.Text = "รหัส Template *";
            // 
            // txtTemplateCode
            // 
            this.txtTemplateCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTemplateCode.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTemplateCode.Location = new System.Drawing.Point(25, 405);
            this.txtTemplateCode.Name = "txtTemplateCode";
            this.txtTemplateCode.Size = new System.Drawing.Size(160, 25);
            this.txtTemplateCode.TabIndex = 12;
            // 
            // lblTemplateName
            // 
            this.lblTemplateName.Location = new System.Drawing.Point(195, 375);
            this.lblTemplateName.Size = new System.Drawing.Size(170, 25);
            // 
            // txtTemplateName
            // 
            this.txtTemplateName.Location = new System.Drawing.Point(195, 405);
            this.txtTemplateName.Size = new System.Drawing.Size(580, 25);
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(25, 440);
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(25, 470);
            this.txtDescription.Size = new System.Drawing.Size(750, 70);
            // 
            // lblCategory
            // 
            this.lblCategory.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCategory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblCategory.Location = new System.Drawing.Point(25, 550);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(220, 25);
            this.lblCategory.TabIndex = 13;
            this.lblCategory.Text = "หมวดหมู่ (Category) *";
            // 
            // cmbCategory
            // 
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(25, 580);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(370, 25);
            this.cmbCategory.TabIndex = 14;
            // 
            // lblOwner
            // 
            this.lblOwner.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblOwner.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblOwner.Location = new System.Drawing.Point(405, 550);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(220, 25);
            this.lblOwner.TabIndex = 15;
            this.lblOwner.Text = "เจ้าของ (Owner) *";
            // 
            // cmbOwner
            // 
            this.cmbOwner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOwner.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbOwner.FormattingEnabled = true;
            this.cmbOwner.Location = new System.Drawing.Point(405, 580);
            this.cmbOwner.Name = "cmbOwner";
            this.cmbOwner.Size = new System.Drawing.Size(370, 25);
            this.cmbOwner.TabIndex = 16;
            // 
            // lblSection3
            // 
            this.lblSection3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblSection3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblSection3.Location = new System.Drawing.Point(25, 615);
            this.lblSection3.Name = "lblSection3";
            this.lblSection3.Size = new System.Drawing.Size(240, 25);
            this.lblSection3.TabIndex = 17;
            this.lblSection3.Text = "3. การตั้งค่าเริ่มต้น";
            // 
            // lblSlaDays
            // 
            this.lblSlaDays.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSlaDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblSlaDays.Location = new System.Drawing.Point(25, 645);
            this.lblSlaDays.Name = "lblSlaDays";
            this.lblSlaDays.Size = new System.Drawing.Size(220, 25);
            this.lblSlaDays.TabIndex = 18;
            this.lblSlaDays.Text = "SLA รวมเป้าหมาย (วัน)";
            // 
            // txtSlaDays
            // 
            this.txtSlaDays.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSlaDays.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSlaDays.Location = new System.Drawing.Point(25, 675);
            this.txtSlaDays.Name = "txtSlaDays";
            this.txtSlaDays.Size = new System.Drawing.Size(250, 25);
            this.txtSlaDays.TabIndex = 19;
            // 
            // lblPriority
            // 
            this.lblPriority.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPriority.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblPriority.Location = new System.Drawing.Point(285, 645);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(170, 25);
            this.lblPriority.TabIndex = 20;
            this.lblPriority.Text = "ระดับความสำคัญเริ่มต้น";
            // 
            // cmbPriority
            // 
            this.cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPriority.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbPriority.FormattingEnabled = true;
            this.cmbPriority.Location = new System.Drawing.Point(285, 675);
            this.cmbPriority.Name = "cmbPriority";
            this.cmbPriority.Size = new System.Drawing.Size(230, 25);
            this.cmbPriority.TabIndex = 21;
            // 
            // lblInitiator
            // 
            this.lblInitiator.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblInitiator.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblInitiator.Location = new System.Drawing.Point(525, 645);
            this.lblInitiator.Name = "lblInitiator";
            this.lblInitiator.Size = new System.Drawing.Size(170, 25);
            this.lblInitiator.TabIndex = 22;
            this.lblInitiator.Text = "ผู้เริ่ม (Initiator)";
            // 
            // cmbInitiator
            // 
            this.cmbInitiator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInitiator.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbInitiator.FormattingEnabled = true;
            this.cmbInitiator.Location = new System.Drawing.Point(525, 675);
            this.cmbInitiator.Name = "cmbInitiator";
            this.cmbInitiator.Size = new System.Drawing.Size(250, 25);
            this.cmbInitiator.TabIndex = 23;
            // 
            // lblTags
            // 
            this.lblTags.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(56)))), ((int)(((byte)(100)))));
            this.lblTags.Location = new System.Drawing.Point(25, 710);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(120, 25);
            this.lblTags.TabIndex = 24;
            this.lblTags.Text = "Tags";
            // 
            // txtTags
            // 
            this.txtTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTags.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTags.Location = new System.Drawing.Point(25, 740);
            this.txtTags.Name = "txtTags";
            this.txtTags.Size = new System.Drawing.Size(750, 25);
            this.txtTags.TabIndex = 25;
            // 
            // pnlOptions
            // 
            this.pnlOptions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlOptions.Controls.Add(this.chkFormBuilder);
            this.pnlOptions.Controls.Add(this.chkOwnerCommittee);
            this.pnlOptions.Controls.Add(this.chkAllowCancel);
            this.pnlOptions.Controls.Add(this.chkAutoStoreToDms);
            this.pnlOptions.Location = new System.Drawing.Point(25, 775);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Size = new System.Drawing.Size(750, 130);
            this.pnlOptions.TabIndex = 26;
            // 
            // chkFormBuilder
            // 
            this.chkFormBuilder.AutoSize = true;
            this.chkFormBuilder.Checked = true;
            this.chkFormBuilder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFormBuilder.Location = new System.Drawing.Point(15, 15);
            this.chkFormBuilder.Name = "chkFormBuilder";
            this.chkFormBuilder.Size = new System.Drawing.Size(267, 19);
            this.chkFormBuilder.TabIndex = 0;
            this.chkFormBuilder.Text = "แบบฟอร์มกรอกข้อมูล (Form Builder ขั้นถัดไป)";
            this.chkFormBuilder.UseVisualStyleBackColor = true;
            // 
            // chkOwnerCommittee
            // 
            this.chkOwnerCommittee.AutoSize = true;
            this.chkOwnerCommittee.Checked = true;
            this.chkOwnerCommittee.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOwnerCommittee.Location = new System.Drawing.Point(15, 43);
            this.chkOwnerCommittee.Name = "chkOwnerCommittee";
            this.chkOwnerCommittee.Size = new System.Drawing.Size(300, 19);
            this.chkOwnerCommittee.TabIndex = 1;
            this.chkOwnerCommittee.Text = "ต้องผ่านการอนุมัติของ Workflow Owner Committee ก่อน Publish";
            this.chkOwnerCommittee.UseVisualStyleBackColor = true;
            // 
            // chkAllowCancel
            // 
            this.chkAllowCancel.AutoSize = true;
            this.chkAllowCancel.Location = new System.Drawing.Point(15, 71);
            this.chkAllowCancel.Name = "chkAllowCancel";
            this.chkAllowCancel.Size = new System.Drawing.Size(240, 19);
            this.chkAllowCancel.TabIndex = 2;
            this.chkAllowCancel.Text = "อนุญาตให้ผู้ส่งยกเลิกคำขอเองได้ระหว่างดำเนินการ";
            this.chkAllowCancel.UseVisualStyleBackColor = true;
            // 
            // chkAutoStoreToDms
            // 
            this.chkAutoStoreToDms.AutoSize = true;
            this.chkAutoStoreToDms.Location = new System.Drawing.Point(15, 99);
            this.chkAutoStoreToDms.Name = "chkAutoStoreToDms";
            this.chkAutoStoreToDms.Size = new System.Drawing.Size(250, 19);
            this.chkAutoStoreToDms.TabIndex = 3;
            this.chkAutoStoreToDms.Text = "เก็บไฟล์แนบของแต่ละ Step เข้า DMS อัตโนมัติ";
            this.chkAutoStoreToDms.UseVisualStyleBackColor = true;
            // 
            // WorkflowCreateTemplateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(900, 750);
            this.Controls.Add(this.pnlSteps);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WorkflowCreateTemplateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "สร้าง Template Workflow ใหม่";
            this.pnlHeader.ResumeLayout(false);
            this.pnlSteps.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlContent.PerformLayout();
            this.pnlRadioFresh.ResumeLayout(false);
            this.pnlRadioFresh.PerformLayout();
            this.pnlRadioCopy.ResumeLayout(false);
            this.pnlRadioCopy.PerformLayout();
            this.pnlRadioImport.ResumeLayout(false);
            this.pnlRadioImport.PerformLayout();
            this.pnlCopyOptions.ResumeLayout(false);
            this.pnlImportOptions.ResumeLayout(false);
            this.pnlImportOptions.PerformLayout();
            this.pnlOptions.ResumeLayout(false);
            this.pnlOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlSteps;
        private System.Windows.Forms.Label lblSteps;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnSaveDraft;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel pnlContent;

        // Section 1: Starting Mode
        private System.Windows.Forms.Label lblSection1;
        private System.Windows.Forms.Panel pnlRadioFresh;
        private System.Windows.Forms.RadioButton rdoFresh;
        private System.Windows.Forms.Label lblFreshTitle;
        private System.Windows.Forms.Label lblFreshDesc;

        private System.Windows.Forms.Panel pnlRadioCopy;
        private System.Windows.Forms.RadioButton rdoCopy;
        private System.Windows.Forms.Label lblCopyTitle;
        private System.Windows.Forms.Label lblCopyDesc;

        private System.Windows.Forms.Panel pnlRadioImport;
        private System.Windows.Forms.RadioButton rdoImport;
        private System.Windows.Forms.Label lblImportTitle;
        private System.Windows.Forms.Label lblImportDesc;

        // Conditional panels
        private System.Windows.Forms.Panel pnlCopyOptions;
        private System.Windows.Forms.Label lblCopyFrom;
        private System.Windows.Forms.ComboBox cmbCopyFrom;

        private System.Windows.Forms.Panel pnlImportOptions;
        private System.Windows.Forms.Label lblImportFile;
        private System.Windows.Forms.TextBox txtImportPath;
        private System.Windows.Forms.Button btnBrowse;

        // Section 2: Basic Info
        private System.Windows.Forms.Label lblSection2;
        private System.Windows.Forms.Label lblTemplateName;
        private System.Windows.Forms.TextBox txtTemplateName;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblTemplateCode;
        private System.Windows.Forms.TextBox txtTemplateCode;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.ComboBox cmbOwner;
        private System.Windows.Forms.Label lblSection3;
        private System.Windows.Forms.Label lblSlaDays;
        private System.Windows.Forms.TextBox txtSlaDays;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.Label lblInitiator;
        private System.Windows.Forms.ComboBox cmbInitiator;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.CheckBox chkFormBuilder;
        private System.Windows.Forms.CheckBox chkOwnerCommittee;
        private System.Windows.Forms.CheckBox chkAllowCancel;
        private System.Windows.Forms.CheckBox chkAutoStoreToDms;
    }
}
