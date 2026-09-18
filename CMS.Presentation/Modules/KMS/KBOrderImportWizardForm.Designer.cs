namespace CMS.Presentation.Modules.KMS
{
    partial class KBOrderImportWizardForm
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
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.btnBackToMain = new System.Windows.Forms.Button();
            this.pnlStepProgress = new System.Windows.Forms.Panel();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.lblStep2 = new System.Windows.Forms.Label();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.pnlBody = new System.Windows.Forms.Panel();
            this.pnlStep3 = new System.Windows.Forms.Panel();
            this.dgvFailed = new System.Windows.Forms.DataGridView();
            this.pnlCompleteTop = new System.Windows.Forms.Panel();
            this.lblCompleteTitle = new System.Windows.Forms.Label();
            this.lblCompleteSub = new System.Windows.Forms.Label();
            this.pnlActStep3 = new System.Windows.Forms.Panel();
            this.btnImportNew = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlStep2 = new System.Windows.Forms.Panel();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            this.pnlSections = new System.Windows.Forms.Panel();
            this.dgvSections = new System.Windows.Forms.DataGridView();
            this.lblSectionsHdr = new System.Windows.Forms.Label();
            this.pnlPills = new System.Windows.Forms.Panel();
            this.lblPillTotal = new System.Windows.Forms.Label();
            this.lblPillValid = new System.Windows.Forms.Label();
            this.lblPillError = new System.Windows.Forms.Label();
            this.pnlActStep2 = new System.Windows.Forms.Panel();
            this.chkSkipDownloadFail = new System.Windows.Forms.CheckBox();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.pnlStep1 = new System.Windows.Forms.Panel();
            this.pnlSheetRow = new System.Windows.Forms.Panel();
            this.cboSheet = new System.Windows.Forms.ComboBox();
            this.lblSheet = new System.Windows.Forms.Label();
            this.pnlUploadedFile = new System.Windows.Forms.Panel();
            this.btnRemoveFile = new System.Windows.Forms.Button();
            this.lblUploadedFileName = new System.Windows.Forms.Label();
            this.pnlUploadZone = new System.Windows.Forms.Panel();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.lblUploadText = new System.Windows.Forms.Label();
            this.lblUploadIcon = new System.Windows.Forms.Label();
            this.pnlActStep1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.pnlStatusBar = new System.Windows.Forms.Panel();
            this.lblStatusMessage = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlStepProgress.SuspendLayout();
            this.pnlBody.SuspendLayout();
            this.pnlStep3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFailed)).BeginInit();
            this.pnlCompleteTop.SuspendLayout();
            this.pnlActStep3.SuspendLayout();
            this.pnlStep2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.pnlSections.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSections)).BeginInit();
            this.pnlPills.SuspendLayout();
            this.pnlActStep2.SuspendLayout();
            this.pnlStep1.SuspendLayout();
            this.pnlSheetRow.SuspendLayout();
            this.pnlUploadedFile.SuspendLayout();
            this.pnlUploadZone.SuspendLayout();
            this.pnlActStep1.SuspendLayout();
            this.pnlStatusBar.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlHeader.Controls.Add(this.btnBackToMain);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1200, 46);
            this.pnlHeader.TabIndex = 0;
            //
            // lblHeaderTitle
            //
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(120, 0);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(1080, 46);
            this.lblHeaderTitle.TabIndex = 1;
            this.lblHeaderTitle.Text = "📥  นำเข้า ประกาศ/คำสั่ง จาก Excel";
            this.lblHeaderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnBackToMain
            //
            this.btnBackToMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnBackToMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBackToMain.FlatAppearance.BorderSize = 0;
            this.btnBackToMain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBackToMain.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnBackToMain.ForeColor = System.Drawing.Color.White;
            this.btnBackToMain.Location = new System.Drawing.Point(0, 0);
            this.btnBackToMain.Name = "btnBackToMain";
            this.btnBackToMain.Size = new System.Drawing.Size(120, 46);
            this.btnBackToMain.TabIndex = 0;
            this.btnBackToMain.Text = "◄  กลับ";
            this.btnBackToMain.UseVisualStyleBackColor = false;
            this.btnBackToMain.Click += new System.EventHandler(this.BtnBackToMain_Click);
            //
            // pnlStepProgress
            //
            this.pnlStepProgress.BackColor = System.Drawing.Color.White;
            this.pnlStepProgress.Controls.Add(this.lblStep3);
            this.pnlStepProgress.Controls.Add(this.lblStep2);
            this.pnlStepProgress.Controls.Add(this.lblStep1);
            this.pnlStepProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStepProgress.Location = new System.Drawing.Point(0, 46);
            this.pnlStepProgress.Name = "pnlStepProgress";
            this.pnlStepProgress.Padding = new System.Windows.Forms.Padding(24, 12, 24, 12);
            this.pnlStepProgress.Size = new System.Drawing.Size(1200, 56);
            this.pnlStepProgress.TabIndex = 1;
            //
            // lblStep1
            //
            this.lblStep1.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStep1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStep1.Location = new System.Drawing.Point(24, 12);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(260, 32);
            this.lblStep1.TabIndex = 0;
            this.lblStep1.Text = "①  อัปโหลดไฟล์";
            this.lblStep1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblStep2
            //
            this.lblStep2.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStep2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStep2.Location = new System.Drawing.Point(284, 12);
            this.lblStep2.Name = "lblStep2";
            this.lblStep2.Size = new System.Drawing.Size(260, 32);
            this.lblStep2.TabIndex = 1;
            this.lblStep2.Text = "②  ตรวจสอบข้อมูล";
            this.lblStep2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblStep3
            //
            this.lblStep3.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblStep3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStep3.Location = new System.Drawing.Point(544, 12);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(260, 32);
            this.lblStep3.TabIndex = 2;
            this.lblStep3.Text = "③  นำเข้าเสร็จสิ้น";
            this.lblStep3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlBody
            //
            this.pnlBody.Controls.Add(this.pnlStep3);
            this.pnlBody.Controls.Add(this.pnlStep2);
            this.pnlBody.Controls.Add(this.pnlStep1);
            this.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBody.Location = new System.Drawing.Point(0, 102);
            this.pnlBody.Name = "pnlBody";
            this.pnlBody.Size = new System.Drawing.Size(1200, 672);
            this.pnlBody.TabIndex = 2;
            //
            // pnlStep3
            //
            this.pnlStep3.Controls.Add(this.dgvFailed);
            this.pnlStep3.Controls.Add(this.pnlCompleteTop);
            this.pnlStep3.Controls.Add(this.pnlActStep3);
            this.pnlStep3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep3.Location = new System.Drawing.Point(0, 0);
            this.pnlStep3.Name = "pnlStep3";
            this.pnlStep3.Padding = new System.Windows.Forms.Padding(24, 16, 24, 0);
            this.pnlStep3.Size = new System.Drawing.Size(1200, 672);
            this.pnlStep3.TabIndex = 2;
            this.pnlStep3.Visible = false;
            //
            // dgvFailed
            //
            this.dgvFailed.AllowUserToAddRows = false;
            this.dgvFailed.AllowUserToDeleteRows = false;
            this.dgvFailed.BackgroundColor = System.Drawing.Color.White;
            this.dgvFailed.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFailed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvFailed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFailed.Location = new System.Drawing.Point(24, 96);
            this.dgvFailed.Name = "dgvFailed";
            this.dgvFailed.ReadOnly = true;
            this.dgvFailed.RowHeadersVisible = false;
            this.dgvFailed.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFailed.Size = new System.Drawing.Size(1152, 512);
            this.dgvFailed.TabIndex = 1;
            //
            // pnlCompleteTop
            //
            this.pnlCompleteTop.Controls.Add(this.lblCompleteSub);
            this.pnlCompleteTop.Controls.Add(this.lblCompleteTitle);
            this.pnlCompleteTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCompleteTop.Location = new System.Drawing.Point(24, 16);
            this.pnlCompleteTop.Name = "pnlCompleteTop";
            this.pnlCompleteTop.Size = new System.Drawing.Size(1152, 80);
            this.pnlCompleteTop.TabIndex = 0;
            //
            // lblCompleteTitle
            //
            this.lblCompleteTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCompleteTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCompleteTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblCompleteTitle.Location = new System.Drawing.Point(0, 0);
            this.lblCompleteTitle.Name = "lblCompleteTitle";
            this.lblCompleteTitle.Size = new System.Drawing.Size(1152, 40);
            this.lblCompleteTitle.TabIndex = 0;
            this.lblCompleteTitle.Text = "✅  นำเข้าข้อมูลเรียบร้อย";
            this.lblCompleteTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblCompleteSub
            //
            this.lblCompleteSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCompleteSub.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblCompleteSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblCompleteSub.Location = new System.Drawing.Point(0, 40);
            this.lblCompleteSub.Name = "lblCompleteSub";
            this.lblCompleteSub.Size = new System.Drawing.Size(1152, 40);
            this.lblCompleteSub.TabIndex = 1;
            this.lblCompleteSub.Text = "-";
            this.lblCompleteSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlActStep3
            //
            this.pnlActStep3.Controls.Add(this.btnClose);
            this.pnlActStep3.Controls.Add(this.btnImportNew);
            this.pnlActStep3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActStep3.Location = new System.Drawing.Point(24, 608);
            this.pnlActStep3.Name = "pnlActStep3";
            this.pnlActStep3.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.pnlActStep3.Size = new System.Drawing.Size(1152, 64);
            this.pnlActStep3.TabIndex = 2;
            //
            // btnImportNew
            //
            this.btnImportNew.BackColor = System.Drawing.Color.White;
            this.btnImportNew.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnImportNew.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnImportNew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImportNew.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnImportNew.Location = new System.Drawing.Point(0, 10);
            this.btnImportNew.Name = "btnImportNew";
            this.btnImportNew.Size = new System.Drawing.Size(170, 44);
            this.btnImportNew.TabIndex = 0;
            this.btnImportNew.Text = "📥  นำเข้าไฟล์ใหม่";
            this.btnImportNew.UseVisualStyleBackColor = false;
            this.btnImportNew.Click += new System.EventHandler(this.BtnImportNew_Click);
            //
            // btnClose
            //
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1002, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 44);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "เสร็จสิ้น";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // pnlStep2
            //
            this.pnlStep2.Controls.Add(this.dgvPreview);
            this.pnlStep2.Controls.Add(this.pnlSections);
            this.pnlStep2.Controls.Add(this.pnlPills);
            this.pnlStep2.Controls.Add(this.pnlActStep2);
            this.pnlStep2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep2.Location = new System.Drawing.Point(0, 0);
            this.pnlStep2.Name = "pnlStep2";
            this.pnlStep2.Padding = new System.Windows.Forms.Padding(24, 12, 24, 0);
            this.pnlStep2.Size = new System.Drawing.Size(1200, 672);
            this.pnlStep2.TabIndex = 1;
            this.pnlStep2.Visible = false;
            //
            // dgvPreview
            //
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.AllowUserToDeleteRows = false;
            this.dgvPreview.BackgroundColor = System.Drawing.Color.White;
            this.dgvPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPreview.Location = new System.Drawing.Point(24, 250);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.ReadOnly = true;
            this.dgvPreview.RowHeadersVisible = false;
            this.dgvPreview.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPreview.Size = new System.Drawing.Size(1152, 356);
            this.dgvPreview.TabIndex = 3;
            //
            // pnlSections
            //
            this.pnlSections.Controls.Add(this.dgvSections);
            this.pnlSections.Controls.Add(this.lblSectionsHdr);
            this.pnlSections.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSections.Location = new System.Drawing.Point(24, 82);
            this.pnlSections.Name = "pnlSections";
            this.pnlSections.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlSections.Size = new System.Drawing.Size(1152, 168);
            this.pnlSections.TabIndex = 2;
            //
            // dgvSections
            //
            this.dgvSections.AllowUserToAddRows = false;
            this.dgvSections.AllowUserToDeleteRows = false;
            this.dgvSections.BackgroundColor = System.Drawing.Color.White;
            this.dgvSections.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvSections.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSections.Location = new System.Drawing.Point(0, 28);
            this.dgvSections.Name = "dgvSections";
            this.dgvSections.RowHeadersVisible = false;
            this.dgvSections.Size = new System.Drawing.Size(1152, 128);
            this.dgvSections.TabIndex = 1;
            //
            // lblSectionsHdr
            //
            this.lblSectionsHdr.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSectionsHdr.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSectionsHdr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblSectionsHdr.Location = new System.Drawing.Point(0, 0);
            this.lblSectionsHdr.Name = "lblSectionsHdr";
            this.lblSectionsHdr.Size = new System.Drawing.Size(1152, 28);
            this.lblSectionsHdr.TabIndex = 0;
            this.lblSectionsHdr.Text = "🔗  มาตราที่อ้างถึง — เลือกปีให้ครบก่อนนำเข้า";
            this.lblSectionsHdr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pnlPills
            //
            this.pnlPills.Controls.Add(this.lblPillError);
            this.pnlPills.Controls.Add(this.lblPillValid);
            this.pnlPills.Controls.Add(this.lblPillTotal);
            this.pnlPills.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPills.Location = new System.Drawing.Point(24, 12);
            this.pnlPills.Name = "pnlPills";
            this.pnlPills.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.pnlPills.Size = new System.Drawing.Size(1152, 70);
            this.pnlPills.TabIndex = 1;
            //
            // lblPillTotal
            //
            this.lblPillTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.lblPillTotal.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPillTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPillTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblPillTotal.Location = new System.Drawing.Point(0, 0);
            this.lblPillTotal.Margin = new System.Windows.Forms.Padding(0);
            this.lblPillTotal.Name = "lblPillTotal";
            this.lblPillTotal.Size = new System.Drawing.Size(220, 58);
            this.lblPillTotal.TabIndex = 0;
            this.lblPillTotal.Text = "ทั้งหมด 0";
            this.lblPillTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPillValid
            //
            this.lblPillValid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(255)))), ((int)(((byte)(244)))));
            this.lblPillValid.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPillValid.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPillValid.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblPillValid.Location = new System.Drawing.Point(220, 0);
            this.lblPillValid.Name = "lblPillValid";
            this.lblPillValid.Size = new System.Drawing.Size(220, 58);
            this.lblPillValid.TabIndex = 1;
            this.lblPillValid.Text = "พร้อมนำเข้า 0";
            this.lblPillValid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblPillError
            //
            this.lblPillError.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(237)))), ((int)(((byte)(236)))));
            this.lblPillError.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblPillError.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPillError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.lblPillError.Location = new System.Drawing.Point(440, 0);
            this.lblPillError.Name = "lblPillError";
            this.lblPillError.Size = new System.Drawing.Size(220, 58);
            this.lblPillError.TabIndex = 2;
            this.lblPillError.Text = "ข้อผิดพลาด 0";
            this.lblPillError.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // pnlActStep2
            //
            this.pnlActStep2.Controls.Add(this.lblProgress);
            this.pnlActStep2.Controls.Add(this.progressBar);
            this.pnlActStep2.Controls.Add(this.chkSkipDownloadFail);
            this.pnlActStep2.Controls.Add(this.btnImport);
            this.pnlActStep2.Controls.Add(this.btnBack);
            this.pnlActStep2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActStep2.Location = new System.Drawing.Point(24, 606);
            this.pnlActStep2.Name = "pnlActStep2";
            this.pnlActStep2.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.pnlActStep2.Size = new System.Drawing.Size(1152, 66);
            this.pnlActStep2.TabIndex = 0;
            //
            // chkSkipDownloadFail
            //
            this.chkSkipDownloadFail.AutoSize = true;
            this.chkSkipDownloadFail.Checked = true;
            this.chkSkipDownloadFail.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSkipDownloadFail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.chkSkipDownloadFail.Location = new System.Drawing.Point(300, 24);
            this.chkSkipDownloadFail.Name = "chkSkipDownloadFail";
            this.chkSkipDownloadFail.Size = new System.Drawing.Size(280, 21);
            this.chkSkipDownloadFail.TabIndex = 2;
            this.chkSkipDownloadFail.Text = "ข้ามแถวที่ดาวน์โหลดไฟล์แนบไม่สำเร็จ";
            this.chkSkipDownloadFail.UseVisualStyleBackColor = true;
            //
            // btnBack
            //
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnBack.Location = new System.Drawing.Point(0, 10);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(140, 46);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "◄  ย้อนกลับ";
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            //
            // btnImport
            //
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnImport.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(962, 10);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(190, 46);
            this.btnImport.TabIndex = 1;
            this.btnImport.Text = "📥  เริ่มนำเข้าข้อมูล";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.BtnImport_Click);
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(620, 22);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 20);
            this.progressBar.TabIndex = 3;
            this.progressBar.Visible = false;
            //
            // lblProgress
            //
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblProgress.Location = new System.Drawing.Point(826, 22);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(130, 20);
            this.lblProgress.TabIndex = 4;
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblProgress.Visible = false;
            //
            // pnlStep1
            //
            this.pnlStep1.Controls.Add(this.pnlSheetRow);
            this.pnlStep1.Controls.Add(this.pnlUploadedFile);
            this.pnlStep1.Controls.Add(this.pnlUploadZone);
            this.pnlStep1.Controls.Add(this.pnlActStep1);
            this.pnlStep1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlStep1.Location = new System.Drawing.Point(0, 0);
            this.pnlStep1.Name = "pnlStep1";
            this.pnlStep1.Padding = new System.Windows.Forms.Padding(24, 16, 24, 0);
            this.pnlStep1.Size = new System.Drawing.Size(1200, 672);
            this.pnlStep1.TabIndex = 0;
            //
            // pnlUploadZone
            //
            this.pnlUploadZone.AllowDrop = true;
            this.pnlUploadZone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlUploadZone.Controls.Add(this.btnSelectFile);
            this.pnlUploadZone.Controls.Add(this.lblUploadText);
            this.pnlUploadZone.Controls.Add(this.lblUploadIcon);
            this.pnlUploadZone.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUploadZone.Location = new System.Drawing.Point(24, 16);
            this.pnlUploadZone.Name = "pnlUploadZone";
            this.pnlUploadZone.Size = new System.Drawing.Size(1152, 200);
            this.pnlUploadZone.TabIndex = 0;
            //
            // lblUploadIcon
            //
            this.lblUploadIcon.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUploadIcon.Font = new System.Drawing.Font("Segoe UI", 28F);
            this.lblUploadIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.lblUploadIcon.Location = new System.Drawing.Point(0, 0);
            this.lblUploadIcon.Name = "lblUploadIcon";
            this.lblUploadIcon.Size = new System.Drawing.Size(1152, 62);
            this.lblUploadIcon.TabIndex = 0;
            this.lblUploadIcon.Text = "📊";
            this.lblUploadIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblUploadText
            //
            this.lblUploadText.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUploadText.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblUploadText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblUploadText.Location = new System.Drawing.Point(0, 62);
            this.lblUploadText.Name = "lblUploadText";
            this.lblUploadText.Size = new System.Drawing.Size(1152, 40);
            this.lblUploadText.TabIndex = 1;
            this.lblUploadText.Text = "ลากไฟล์ Excel มาวางที่นี่  หรือ";
            this.lblUploadText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnSelectFile
            //
            this.btnSelectFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnSelectFile.FlatAppearance.BorderSize = 0;
            this.btnSelectFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectFile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSelectFile.ForeColor = System.Drawing.Color.White;
            this.btnSelectFile.Location = new System.Drawing.Point(486, 112);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(180, 46);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "📂  เลือกไฟล์";
            this.btnSelectFile.UseVisualStyleBackColor = false;
            this.btnSelectFile.Click += new System.EventHandler(this.BtnSelectFile_Click);
            //
            // pnlUploadedFile
            //
            this.pnlUploadedFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(255)))), ((int)(((byte)(244)))));
            this.pnlUploadedFile.Controls.Add(this.lblUploadedFileName);
            this.pnlUploadedFile.Controls.Add(this.btnRemoveFile);
            this.pnlUploadedFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUploadedFile.Location = new System.Drawing.Point(24, 216);
            this.pnlUploadedFile.Name = "pnlUploadedFile";
            this.pnlUploadedFile.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.pnlUploadedFile.Size = new System.Drawing.Size(1152, 56);
            this.pnlUploadedFile.TabIndex = 1;
            this.pnlUploadedFile.Visible = false;
            //
            // lblUploadedFileName
            //
            this.lblUploadedFileName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUploadedFileName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUploadedFileName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblUploadedFileName.Location = new System.Drawing.Point(12, 0);
            this.lblUploadedFileName.Name = "lblUploadedFileName";
            this.lblUploadedFileName.Size = new System.Drawing.Size(1080, 56);
            this.lblUploadedFileName.TabIndex = 0;
            this.lblUploadedFileName.Text = "-";
            this.lblUploadedFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // btnRemoveFile
            //
            this.btnRemoveFile.BackColor = System.Drawing.Color.White;
            this.btnRemoveFile.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRemoveFile.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnRemoveFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveFile.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnRemoveFile.Location = new System.Drawing.Point(1092, 0);
            this.btnRemoveFile.Name = "btnRemoveFile";
            this.btnRemoveFile.Size = new System.Drawing.Size(60, 56);
            this.btnRemoveFile.TabIndex = 1;
            this.btnRemoveFile.Text = "✕";
            this.btnRemoveFile.UseVisualStyleBackColor = false;
            this.btnRemoveFile.Click += new System.EventHandler(this.BtnRemoveFile_Click);
            //
            // pnlSheetRow
            //
            this.pnlSheetRow.Controls.Add(this.cboSheet);
            this.pnlSheetRow.Controls.Add(this.lblSheet);
            this.pnlSheetRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSheetRow.Location = new System.Drawing.Point(24, 272);
            this.pnlSheetRow.Name = "pnlSheetRow";
            this.pnlSheetRow.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
            this.pnlSheetRow.Size = new System.Drawing.Size(1152, 64);
            this.pnlSheetRow.TabIndex = 2;
            this.pnlSheetRow.Visible = false;
            //
            // lblSheet
            //
            this.lblSheet.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblSheet.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSheet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(58)))), ((int)(((byte)(95)))));
            this.lblSheet.Location = new System.Drawing.Point(0, 16);
            this.lblSheet.Name = "lblSheet";
            this.lblSheet.Size = new System.Drawing.Size(130, 48);
            this.lblSheet.TabIndex = 0;
            this.lblSheet.Text = "เลือก Sheet";
            this.lblSheet.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cboSheet
            //
            this.cboSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheet.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboSheet.FormattingEnabled = true;
            this.cboSheet.Location = new System.Drawing.Point(136, 26);
            this.cboSheet.Name = "cboSheet";
            this.cboSheet.Size = new System.Drawing.Size(380, 25);
            this.cboSheet.TabIndex = 1;
            //
            // pnlActStep1
            //
            this.pnlActStep1.Controls.Add(this.btnNext);
            this.pnlActStep1.Controls.Add(this.btnCancel);
            this.pnlActStep1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActStep1.Location = new System.Drawing.Point(24, 608);
            this.pnlActStep1.Name = "pnlActStep1";
            this.pnlActStep1.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.pnlActStep1.Size = new System.Drawing.Size(1152, 64);
            this.pnlActStep1.TabIndex = 3;
            //
            // btnCancel
            //
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(222)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.Location = new System.Drawing.Point(0, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(140, 44);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // btnNext
            //
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(116)))), ((int)(((byte)(181)))));
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNext.Enabled = false;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNext.ForeColor = System.Drawing.Color.White;
            this.btnNext.Location = new System.Drawing.Point(992, 10);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(160, 44);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = "ถัดไป  ►";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            //
            // pnlStatusBar
            //
            this.pnlStatusBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(249)))));
            this.pnlStatusBar.Controls.Add(this.lblStatusMessage);
            this.pnlStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatusBar.Location = new System.Drawing.Point(0, 774);
            this.pnlStatusBar.Name = "pnlStatusBar";
            this.pnlStatusBar.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.pnlStatusBar.Size = new System.Drawing.Size(1200, 26);
            this.pnlStatusBar.TabIndex = 3;
            //
            // lblStatusMessage
            //
            this.lblStatusMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusMessage.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblStatusMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblStatusMessage.Location = new System.Drawing.Point(16, 0);
            this.lblStatusMessage.Name = "lblStatusMessage";
            this.lblStatusMessage.Size = new System.Drawing.Size(1168, 26);
            this.lblStatusMessage.TabIndex = 0;
            this.lblStatusMessage.Text = "พร้อมใช้งาน";
            this.lblStatusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // KBOrderImportWizardForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.pnlBody);
            this.Controls.Add(this.pnlStatusBar);
            this.Controls.Add(this.pnlStepProgress);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "KBOrderImportWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "นำเข้า ประกาศ/คำสั่ง";
            this.Load += new System.EventHandler(this.KBOrderImportWizardForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlStepProgress.ResumeLayout(false);
            this.pnlBody.ResumeLayout(false);
            this.pnlStep3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFailed)).EndInit();
            this.pnlCompleteTop.ResumeLayout(false);
            this.pnlActStep3.ResumeLayout(false);
            this.pnlStep2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.pnlSections.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSections)).EndInit();
            this.pnlPills.ResumeLayout(false);
            this.pnlActStep2.ResumeLayout(false);
            this.pnlActStep2.PerformLayout();
            this.pnlStep1.ResumeLayout(false);
            this.pnlSheetRow.ResumeLayout(false);
            this.pnlUploadedFile.ResumeLayout(false);
            this.pnlUploadZone.ResumeLayout(false);
            this.pnlActStep1.ResumeLayout(false);
            this.pnlStatusBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Button btnBackToMain;
        private System.Windows.Forms.Panel pnlStepProgress;
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.Label lblStep2;
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.Panel pnlStep1;
        private System.Windows.Forms.Panel pnlUploadZone;
        private System.Windows.Forms.Label lblUploadIcon;
        private System.Windows.Forms.Label lblUploadText;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Panel pnlUploadedFile;
        private System.Windows.Forms.Label lblUploadedFileName;
        private System.Windows.Forms.Button btnRemoveFile;
        private System.Windows.Forms.Panel pnlSheetRow;
        private System.Windows.Forms.Label lblSheet;
        private System.Windows.Forms.ComboBox cboSheet;
        private System.Windows.Forms.Panel pnlActStep1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Panel pnlStep2;
        private System.Windows.Forms.Panel pnlPills;
        private System.Windows.Forms.Label lblPillTotal;
        private System.Windows.Forms.Label lblPillValid;
        private System.Windows.Forms.Label lblPillError;
        private System.Windows.Forms.Panel pnlSections;
        private System.Windows.Forms.Label lblSectionsHdr;
        private System.Windows.Forms.DataGridView dgvSections;
        private System.Windows.Forms.DataGridView dgvPreview;
        private System.Windows.Forms.Panel pnlActStep2;
        private System.Windows.Forms.CheckBox chkSkipDownloadFail;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Panel pnlStep3;
        private System.Windows.Forms.Panel pnlCompleteTop;
        private System.Windows.Forms.Label lblCompleteTitle;
        private System.Windows.Forms.Label lblCompleteSub;
        private System.Windows.Forms.DataGridView dgvFailed;
        private System.Windows.Forms.Panel pnlActStep3;
        private System.Windows.Forms.Button btnImportNew;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlStatusBar;
        private System.Windows.Forms.Label lblStatusMessage;
    }
}
