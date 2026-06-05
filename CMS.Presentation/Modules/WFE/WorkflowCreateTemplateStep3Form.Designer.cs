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
            this.pnlHeader   = new System.Windows.Forms.Panel();
            this.lblTitle    = new System.Windows.Forms.Label();
            this.pnlBreadcrumb = new System.Windows.Forms.Panel();
            this.lblBreadcrumb = new System.Windows.Forms.Label();

            // Bottom button bar
            this.pnlButtons    = new System.Windows.Forms.Panel();
            this.lblFieldCount = new System.Windows.Forms.Label();
            this.btnPrev       = new System.Windows.Forms.Button();
            this.btnSaveDraft  = new System.Windows.Forms.Button();
            this.btnNext       = new System.Windows.Forms.Button();

            // 3-column main area
            this.pnlMain = new System.Windows.Forms.Panel();

            // LEFT — palette
            this.pnlPalette       = new System.Windows.Forms.Panel();
            this.lblPaletteHeader = new System.Windows.Forms.Label();
            this.lblPaletteHint   = new System.Windows.Forms.Label();
            this.pnlPaletteScroll = new System.Windows.Forms.Panel();

            this.lblPaletteSection1   = new System.Windows.Forms.Label();
            this.flowPaletteBasic     = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPaletteSection2   = new System.Windows.Forms.Label();
            this.flowPaletteSelect    = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPaletteSection3   = new System.Windows.Forms.Label();
            this.flowPaletteAdvanced  = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPaletteSection4   = new System.Windows.Forms.Label();
            this.flowPaletteLayout    = new System.Windows.Forms.FlowLayoutPanel();

            // CENTER — canvas
            this.pnlCanvas           = new System.Windows.Forms.Panel();
            this.pnlCanvasInner      = new System.Windows.Forms.Panel();
            this.lblCanvasHint       = new System.Windows.Forms.Label();
            this.txtFormTitle        = new System.Windows.Forms.TextBox();
            this.pnlSectionsContainer = new System.Windows.Forms.Panel();

            // RIGHT — properties
            this.pnlProperties      = new System.Windows.Forms.Panel();
            this.lblPropertiesTitle = new System.Windows.Forms.Label();
            this.lblPropertiesSub   = new System.Windows.Forms.Label();
            this.lblNoSelection     = new System.Windows.Forms.Label();
            this.pnlFieldProps      = new System.Windows.Forms.Panel();

            this.lblPropLabel       = new System.Windows.Forms.Label();
            this.txtPropLabel       = new System.Windows.Forms.TextBox();
            this.lblPropFieldName   = new System.Windows.Forms.Label();
            this.txtPropFieldName   = new System.Windows.Forms.TextBox();
            this.lblPropHelpText    = new System.Windows.Forms.Label();
            this.txtPropHelpText    = new System.Windows.Forms.TextBox();
            this.chkRequired        = new System.Windows.Forms.CheckBox();
            this.chkVisible         = new System.Windows.Forms.CheckBox();
            this.pnlDivider1        = new System.Windows.Forms.Panel();
            this.lblPropSource      = new System.Windows.Forms.Label();
            this.cmbPropSource      = new System.Windows.Forms.ComboBox();
            this.lblPropDefault     = new System.Windows.Forms.Label();
            this.txtPropDefault     = new System.Windows.Forms.TextBox();
            this.lblPropConditional = new System.Windows.Forms.Label();
            this.txtPropConditional = new System.Windows.Forms.TextBox();
            this.lblPropMapContext  = new System.Windows.Forms.Label();
            this.txtPropMapContext  = new System.Windows.Forms.TextBox();
            this.btnApplyProps      = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // ═══════════════════════════════════════════════════════════
            //  HEADER  (56 px, Dock=Top)
            // ═══════════════════════════════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 56, 100);
            this.pnlHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height    = 56;
            this.pnlHeader.Padding   = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.pnlHeader.Controls.Add(this.lblTitle);

            this.lblTitle.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font      = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Text      = "3) ออกแบบฟอร์ม";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ═══════════════════════════════════════════════════════════
            //  BREADCRUMB  (36 px, Dock=Top)
            // ═══════════════════════════════════════════════════════════
            this.pnlBreadcrumb.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Top;
            this.pnlBreadcrumb.Height    = 34;
            this.pnlBreadcrumb.Padding   = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.pnlBreadcrumb.Controls.Add(this.lblBreadcrumb);

            this.lblBreadcrumb.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblBreadcrumb.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblBreadcrumb.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblBreadcrumb.Text      = "1) ข้อมูลพื้นฐาน   →   2) ออกแบบ Flow   →   ● 3) ออกแบบฟอร์ม   →   4) ส่งอนุมัติ/Publish";
            this.lblBreadcrumb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // ═══════════════════════════════════════════════════════════
            //  BOTTOM BUTTON BAR  (52 px, Dock=Bottom)  ← ต้อง Add ก่อน Fill
            // ═══════════════════════════════════════════════════════════
            this.pnlButtons.BackColor = System.Drawing.Color.FromArgb(250, 251, 252);
            this.pnlButtons.Dock      = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Height    = 52;
            this.pnlButtons.Padding   = new System.Windows.Forms.Padding(16, 0, 16, 0);

            // field count label (left side)
            this.lblFieldCount.AutoSize  = false;
            this.lblFieldCount.Dock      = System.Windows.Forms.DockStyle.Left;
            this.lblFieldCount.Width     = 280;
            this.lblFieldCount.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblFieldCount.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblFieldCount.Text      = "คลิก field ในแพลเลตซ้ายมือเพื่อเพิ่ม";
            this.lblFieldCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFieldCount.Name      = "lblFieldCount";

            // ← ย้อนกลับ
            this.btnPrev.Text      = "← ย้อนกลับ";
            this.btnPrev.Size      = new System.Drawing.Size(100, 32);
            this.btnPrev.Anchor    = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnPrev.Location  = new System.Drawing.Point(960, 10);
            this.btnPrev.BackColor = System.Drawing.Color.White;
            this.btnPrev.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnPrev.FlatAppearance.BorderSize  = 1;
            this.btnPrev.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPrev.Name      = "btnPrev";

            // บันทึก Draft
            this.btnSaveDraft.Text      = "บันทึก Draft";
            this.btnSaveDraft.Size      = new System.Drawing.Size(100, 32);
            this.btnSaveDraft.Anchor    = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnSaveDraft.Location  = new System.Drawing.Point(1068, 10);
            this.btnSaveDraft.BackColor = System.Drawing.Color.White;
            this.btnSaveDraft.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnSaveDraft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveDraft.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnSaveDraft.FlatAppearance.BorderSize  = 1;
            this.btnSaveDraft.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSaveDraft.Name      = "btnSaveDraft";

            // ถัดไป →  (Primary, สีน้ำเงิน)
            this.btnNext.Text             = "ถัดไป →";
            this.btnNext.Size             = new System.Drawing.Size(110, 32);
            this.btnNext.Anchor           = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.btnNext.Location         = new System.Drawing.Point(1176, 10);
            this.btnNext.BackColor        = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnNext.ForeColor        = System.Drawing.Color.White;
            this.btnNext.FlatStyle        = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.Font             = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNext.Name             = "btnNext";
            this.btnNext.UseVisualStyleBackColor = false;

            this.pnlButtons.Controls.Add(this.lblFieldCount);
            this.pnlButtons.Controls.Add(this.btnPrev);
            this.pnlButtons.Controls.Add(this.btnSaveDraft);
            this.pnlButtons.Controls.Add(this.btnNext);

            // ═══════════════════════════════════════════════════════════
            //  MAIN (Dock=Fill) — contains 3 columns
            // ═══════════════════════════════════════════════════════════
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.pnlMain.Dock      = System.Windows.Forms.DockStyle.Fill;

            // ── LEFT: Palette ──
            this.pnlPalette.BackColor  = System.Drawing.Color.White;
            this.pnlPalette.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlPalette.Width      = 190;
            this.pnlPalette.Padding    = new System.Windows.Forms.Padding(0);
            this.pnlPalette.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // palette header bar
            this.lblPaletteHeader.Text      = "Field Types";
            this.lblPaletteHeader.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPaletteHeader.ForeColor = System.Drawing.Color.White;
            this.lblPaletteHeader.BackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblPaletteHeader.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPaletteHeader.Height    = 36;
            this.lblPaletteHeader.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblPaletteHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPaletteHeader.Name      = "lblPaletteHeader";

            this.lblPaletteHint.Text      = "คลิกเพื่อเพิ่ม field ลงใน Section";
            this.lblPaletteHint.Font      = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblPaletteHint.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblPaletteHint.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.lblPaletteHint.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPaletteHint.Height    = 24;
            this.lblPaletteHint.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblPaletteHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPaletteHint.Name      = "lblPaletteHint";

            // scrollable body of palette
            this.pnlPaletteScroll.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.pnlPaletteScroll.AutoScroll = true;
            this.pnlPaletteScroll.BackColor = System.Drawing.Color.White;
            this.pnlPaletteScroll.Padding   = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.pnlPaletteScroll.Name      = "pnlPaletteScroll";

            // section labels — rebuilt at runtime by InitializePalette()
            this.lblPaletteSection1.Text = "พื้นฐาน";
            this.lblPaletteSection1.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection1.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection1.BackColor = System.Drawing.Color.White;
            this.lblPaletteSection1.Location  = new System.Drawing.Point(0, 0);
            this.lblPaletteSection1.Size      = new System.Drawing.Size(174, 18);
            this.lblPaletteSection1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.flowPaletteBasic.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteBasic.WrapContents  = false;
            this.flowPaletteBasic.AutoScroll    = false;
            this.flowPaletteBasic.Size          = new System.Drawing.Size(174, 74);
            this.flowPaletteBasic.BackColor     = System.Drawing.Color.White;

            this.lblPaletteSection2.Text = "เลือก";
            this.lblPaletteSection2.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection2.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection2.BackColor = System.Drawing.Color.White;
            this.lblPaletteSection2.Location  = new System.Drawing.Point(0, 110);
            this.lblPaletteSection2.Size      = new System.Drawing.Size(174, 18);
            this.lblPaletteSection2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.flowPaletteSelect.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteSelect.WrapContents  = false;
            this.flowPaletteSelect.AutoScroll    = false;
            this.flowPaletteSelect.Size          = new System.Drawing.Size(174, 110);
            this.flowPaletteSelect.BackColor     = System.Drawing.Color.White;

            this.lblPaletteSection3.Text = "ขั้นสูง";
            this.lblPaletteSection3.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection3.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection3.BackColor = System.Drawing.Color.White;
            this.lblPaletteSection3.Location  = new System.Drawing.Point(0, 232);
            this.lblPaletteSection3.Size      = new System.Drawing.Size(174, 18);
            this.lblPaletteSection3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.flowPaletteAdvanced.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteAdvanced.WrapContents  = false;
            this.flowPaletteAdvanced.AutoScroll    = false;
            this.flowPaletteAdvanced.Size          = new System.Drawing.Size(174, 110);
            this.flowPaletteAdvanced.BackColor     = System.Drawing.Color.White;

            this.lblPaletteSection4.Text = "เค้าโครง";
            this.lblPaletteSection4.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblPaletteSection4.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblPaletteSection4.BackColor = System.Drawing.Color.White;
            this.lblPaletteSection4.Location  = new System.Drawing.Point(0, 356);
            this.lblPaletteSection4.Size      = new System.Drawing.Size(174, 18);
            this.lblPaletteSection4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.flowPaletteLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPaletteLayout.WrapContents  = false;
            this.flowPaletteLayout.AutoScroll    = false;
            this.flowPaletteLayout.Size          = new System.Drawing.Size(174, 74);
            this.flowPaletteLayout.BackColor     = System.Drawing.Color.White;

            this.pnlPaletteScroll.Controls.Add(this.flowPaletteLayout);
            this.pnlPaletteScroll.Controls.Add(this.lblPaletteSection4);
            this.pnlPaletteScroll.Controls.Add(this.flowPaletteAdvanced);
            this.pnlPaletteScroll.Controls.Add(this.lblPaletteSection3);
            this.pnlPaletteScroll.Controls.Add(this.flowPaletteSelect);
            this.pnlPaletteScroll.Controls.Add(this.lblPaletteSection2);
            this.pnlPaletteScroll.Controls.Add(this.flowPaletteBasic);
            this.pnlPaletteScroll.Controls.Add(this.lblPaletteSection1);

            this.pnlPalette.Controls.Add(this.pnlPaletteScroll);
            this.pnlPalette.Controls.Add(this.lblPaletteHint);
            this.pnlPalette.Controls.Add(this.lblPaletteHeader);

            // ── RIGHT: Properties ──
            this.pnlProperties.BackColor  = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlProperties.Dock       = System.Windows.Forms.DockStyle.Right;
            this.pnlProperties.Width      = 270;
            this.pnlProperties.AutoScroll = true;

            // properties header
            this.lblPropertiesTitle.Text      = "⚙  Field Properties";
            this.lblPropertiesTitle.Font      = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblPropertiesTitle.ForeColor = System.Drawing.Color.White;
            this.lblPropertiesTitle.BackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.lblPropertiesTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPropertiesTitle.Height    = 36;
            this.lblPropertiesTitle.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblPropertiesTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPropertiesTitle.Name      = "lblPropertiesTitle";

            this.lblPropertiesSub.Text      = "คลิก field ในฟอร์มเพื่อดูคุณสมบัติ";
            this.lblPropertiesSub.Font      = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblPropertiesSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblPropertiesSub.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.lblPropertiesSub.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblPropertiesSub.Height    = 24;
            this.lblPropertiesSub.Padding   = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.lblPropertiesSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPropertiesSub.Name      = "lblPropertiesSub";

            this.lblNoSelection.Text      = "ยังไม่ได้เลือก field\n\nคลิกที่ field ในฟอร์มกลาง\nเพื่อแก้ไขคุณสมบัติ";
            this.lblNoSelection.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNoSelection.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblNoSelection.Location  = new System.Drawing.Point(12, 72);
            this.lblNoSelection.Size      = new System.Drawing.Size(246, 80);
            this.lblNoSelection.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblNoSelection.Name      = "lblNoSelection";

            // field properties edit panel
            this.pnlFieldProps.BackColor  = System.Drawing.Color.FromArgb(249, 250, 251);
            this.pnlFieldProps.Location   = new System.Drawing.Point(0, 60);
            this.pnlFieldProps.Size       = new System.Drawing.Size(270, 700);
            this.pnlFieldProps.Padding    = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlFieldProps.Name       = "pnlFieldProps";
            this.pnlFieldProps.Visible    = false;
            this.pnlFieldProps.AutoScroll = true;

            this.lblPropLabel.Text = "ชื่อ Field (Label)";
            this.lblPropLabel.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropLabel.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropLabel.Location  = new System.Drawing.Point(12, 10);
            this.lblPropLabel.Size      = new System.Drawing.Size(242, 17);
            this.txtPropLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropLabel.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropLabel.Location    = new System.Drawing.Point(12, 30);
            this.txtPropLabel.Size        = new System.Drawing.Size(242, 23);

            this.lblPropFieldName.Text = "Field ID (ภาษาอังกฤษ)";
            this.lblPropFieldName.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropFieldName.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropFieldName.Location  = new System.Drawing.Point(12, 65);
            this.lblPropFieldName.Size      = new System.Drawing.Size(242, 17);
            this.txtPropFieldName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropFieldName.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropFieldName.Location    = new System.Drawing.Point(12, 85);
            this.txtPropFieldName.Size        = new System.Drawing.Size(242, 23);

            this.lblPropHelpText.Text = "คำอธิบาย (Help Text)";
            this.lblPropHelpText.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropHelpText.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropHelpText.Location  = new System.Drawing.Point(12, 120);
            this.lblPropHelpText.Size      = new System.Drawing.Size(242, 17);
            this.txtPropHelpText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropHelpText.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropHelpText.Location    = new System.Drawing.Point(12, 140);
            this.txtPropHelpText.Size        = new System.Drawing.Size(242, 55);
            this.txtPropHelpText.Multiline   = true;

            this.chkRequired.Text      = "จำเป็นต้องกรอก (Required)";
            this.chkRequired.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkRequired.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkRequired.Location  = new System.Drawing.Point(12, 208);
            this.chkRequired.AutoSize  = true;
            this.chkRequired.Name      = "chkRequired";

            this.chkVisible.Text      = "แสดงผล (Visible)";
            this.chkVisible.Checked   = true;
            this.chkVisible.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.chkVisible.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.chkVisible.Location  = new System.Drawing.Point(12, 232);
            this.chkVisible.AutoSize  = true;
            this.chkVisible.Name      = "chkVisible";

            this.pnlDivider1.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.pnlDivider1.Location  = new System.Drawing.Point(12, 260);
            this.pnlDivider1.Size      = new System.Drawing.Size(242, 1);
            this.pnlDivider1.Name      = "pnlDivider1";

            this.lblPropSource.Text = "แหล่งข้อมูล (Source)";
            this.lblPropSource.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropSource.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropSource.Location  = new System.Drawing.Point(12, 268);
            this.lblPropSource.Size      = new System.Drawing.Size(242, 17);
            this.cmbPropSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPropSource.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbPropSource.Location  = new System.Drawing.Point(12, 288);
            this.cmbPropSource.Size      = new System.Drawing.Size(242, 25);
            this.cmbPropSource.Name      = "cmbPropSource";

            this.lblPropDefault.Text = "ค่าเริ่มต้น (Default Value)";
            this.lblPropDefault.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropDefault.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropDefault.Location  = new System.Drawing.Point(12, 320);
            this.lblPropDefault.Size      = new System.Drawing.Size(242, 17);
            this.txtPropDefault.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropDefault.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropDefault.Location    = new System.Drawing.Point(12, 340);
            this.txtPropDefault.Size        = new System.Drawing.Size(242, 23);

            this.lblPropConditional.Text = "เงื่อนไข (Conditional Logic)";
            this.lblPropConditional.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropConditional.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropConditional.Location  = new System.Drawing.Point(12, 374);
            this.lblPropConditional.Size      = new System.Drawing.Size(242, 17);
            this.txtPropConditional.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropConditional.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropConditional.Location    = new System.Drawing.Point(12, 394);
            this.txtPropConditional.Size        = new System.Drawing.Size(242, 55);
            this.txtPropConditional.Multiline   = true;

            this.lblPropMapContext.Text = "Map to Context Key";
            this.lblPropMapContext.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPropMapContext.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPropMapContext.Location  = new System.Drawing.Point(12, 462);
            this.lblPropMapContext.Size      = new System.Drawing.Size(242, 17);
            this.txtPropMapContext.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPropMapContext.Font        = new System.Drawing.Font("Segoe UI", 9F);
            this.txtPropMapContext.Location    = new System.Drawing.Point(12, 482);
            this.txtPropMapContext.Size        = new System.Drawing.Size(242, 23);

            this.btnApplyProps.Text             = "✓  บันทึกการเปลี่ยนแปลง";
            this.btnApplyProps.BackColor        = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnApplyProps.ForeColor        = System.Drawing.Color.White;
            this.btnApplyProps.FlatStyle        = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyProps.FlatAppearance.BorderSize = 0;
            this.btnApplyProps.Font             = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnApplyProps.Location         = new System.Drawing.Point(12, 520);
            this.btnApplyProps.Size             = new System.Drawing.Size(242, 34);
            this.btnApplyProps.UseVisualStyleBackColor = false;
            this.btnApplyProps.Name             = "btnApplyProps";

            this.pnlFieldProps.Controls.Add(this.lblPropLabel);
            this.pnlFieldProps.Controls.Add(this.txtPropLabel);
            this.pnlFieldProps.Controls.Add(this.lblPropFieldName);
            this.pnlFieldProps.Controls.Add(this.txtPropFieldName);
            this.pnlFieldProps.Controls.Add(this.lblPropHelpText);
            this.pnlFieldProps.Controls.Add(this.txtPropHelpText);
            this.pnlFieldProps.Controls.Add(this.chkRequired);
            this.pnlFieldProps.Controls.Add(this.chkVisible);
            this.pnlFieldProps.Controls.Add(this.pnlDivider1);
            this.pnlFieldProps.Controls.Add(this.lblPropSource);
            this.pnlFieldProps.Controls.Add(this.cmbPropSource);
            this.pnlFieldProps.Controls.Add(this.lblPropDefault);
            this.pnlFieldProps.Controls.Add(this.txtPropDefault);
            this.pnlFieldProps.Controls.Add(this.lblPropConditional);
            this.pnlFieldProps.Controls.Add(this.txtPropConditional);
            this.pnlFieldProps.Controls.Add(this.lblPropMapContext);
            this.pnlFieldProps.Controls.Add(this.txtPropMapContext);
            this.pnlFieldProps.Controls.Add(this.btnApplyProps);

            this.pnlProperties.Controls.Add(this.pnlFieldProps);
            this.pnlProperties.Controls.Add(this.lblNoSelection);
            this.pnlProperties.Controls.Add(this.lblPropertiesSub);
            this.pnlProperties.Controls.Add(this.lblPropertiesTitle);

            // ── CENTER: Canvas ──
            this.pnlCanvas.BackColor  = System.Drawing.Color.FromArgb(243, 244, 246);
            this.pnlCanvas.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.Padding    = new System.Windows.Forms.Padding(20, 14, 20, 14);
            this.pnlCanvas.AutoScroll = true;
            this.pnlCanvas.Name       = "pnlCanvas";

            // hint banner
            this.lblCanvasHint.Text      = "  ℹ  คลิก field จากแผงซ้ายเพื่อเพิ่ม  |  คลิก field ในฟอร์มเพื่อแก้ไขคุณสมบัติด้านขวา  |  กด × เพื่อลบ field";
            this.lblCanvasHint.Font      = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblCanvasHint.ForeColor = System.Drawing.Color.FromArgb(30, 58, 138);
            this.lblCanvasHint.BackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            this.lblCanvasHint.Location  = new System.Drawing.Point(0, 0);
            this.lblCanvasHint.Size      = new System.Drawing.Size(800, 28);
            this.lblCanvasHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCanvasHint.Name      = "lblCanvasHint";

            this.txtFormTitle.Font        = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.txtFormTitle.ForeColor   = System.Drawing.Color.FromArgb(30, 58, 138);
            this.txtFormTitle.BackColor   = System.Drawing.Color.White;
            this.txtFormTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFormTitle.Location    = new System.Drawing.Point(0, 36);
            this.txtFormTitle.Size        = new System.Drawing.Size(800, 36);
            this.txtFormTitle.Text        = "ชื่อฟอร์ม";
            this.txtFormTitle.Name        = "txtFormTitle";

            this.pnlSectionsContainer.Location  = new System.Drawing.Point(0, 80);
            this.pnlSectionsContainer.Size      = new System.Drawing.Size(800, 2000);
            this.pnlSectionsContainer.BackColor = System.Drawing.Color.FromArgb(243, 244, 246);
            this.pnlSectionsContainer.AutoSize  = true;
            this.pnlSectionsContainer.Name      = "pnlSectionsContainer";

            this.pnlCanvas.Controls.Add(this.pnlSectionsContainer);
            this.pnlCanvas.Controls.Add(this.txtFormTitle);
            this.pnlCanvas.Controls.Add(this.lblCanvasHint);

            // assemble main
            this.pnlMain.Controls.Add(this.pnlCanvas);
            this.pnlMain.Controls.Add(this.pnlProperties);
            this.pnlMain.Controls.Add(this.pnlPalette);

            // ═══════════════════════════════════════════════════════════
            //  FORM  — ลด height ให้พอดี 1080p (เผื่อ taskbar ~40px)
            //  ClientSize: 1300 x 790
            // ═══════════════════════════════════════════════════════════
            //  Docking order ใน Controls.Add:
            //    → Add Bottom ก่อน Fill เพื่อให้ DockStyle ถูกต้อง
            // ═══════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.White;
            this.ClientSize          = new System.Drawing.Size(1300, 790);
            this.Font                = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize         = new System.Drawing.Size(1100, 680);
            this.MaximizeBox         = true;
            this.MinimizeBox         = true;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text                = "ออกแบบฟอร์ม — ขั้นตอนที่ 3";

            // ลำดับ Add สำคัญมาก: Bottom ต้องก่อน Fill
            this.Controls.Add(this.pnlMain);        // Fill (add last)
            this.Controls.Add(this.pnlButtons);     // Bottom
            this.Controls.Add(this.pnlBreadcrumb);  // Top
            this.Controls.Add(this.pnlHeader);      // Top

            this.ResumeLayout(false);
        }

        // ─── helpers ─────────────────────────────────────────────────────────
        private static void ConfigureSectionLabel(
            System.Windows.Forms.Label lbl, string text, int top)
        {
            lbl.Text      = text;
            lbl.Font      = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lbl.BackColor = System.Drawing.Color.White;
            lbl.Location  = new System.Drawing.Point(0, top);
            lbl.Size      = new System.Drawing.Size(174, 18);
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        private static void ConfigureFlowPalettePanel(
            System.Windows.Forms.FlowLayoutPanel panel, int height)
        {
            panel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            panel.WrapContents  = false;
            panel.AutoScroll    = false;
            panel.Size          = new System.Drawing.Size(174, height);
            panel.BackColor     = System.Drawing.Color.White;
        }

        private static void BuildPropRow(
            System.Windows.Forms.Label lbl, string text,
            System.Windows.Forms.TextBox txt, int lblTop, int txtTop)
        {
            lbl.Text      = text;
            lbl.Font      = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            lbl.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lbl.Location  = new System.Drawing.Point(12, lblTop);
            lbl.Size      = new System.Drawing.Size(242, 17);

            if (txt != null)
            {
                txt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                txt.Font        = new System.Drawing.Font("Segoe UI", 9F);
                txt.Location    = new System.Drawing.Point(12, txtTop);
                txt.Size        = new System.Drawing.Size(242, 23);
            }
        }

        private static void BuildMultilineProp(
            System.Windows.Forms.Label lbl, string text,
            System.Windows.Forms.TextBox txt, int lblTop, int txtTop, int height)
        {
            BuildPropRow(lbl, text, txt, lblTop, txtTop);
            if (txt != null)
            {
                txt.Multiline = true;
                txt.Size      = new System.Drawing.Size(242, height);
            }
        }

        // ─── field declarations ───────────────────────────────────────────────
        private System.Windows.Forms.Panel  pnlHeader;
        private System.Windows.Forms.Label  lblTitle;
        private System.Windows.Forms.Panel  pnlBreadcrumb;
        private System.Windows.Forms.Label  lblBreadcrumb;
        private System.Windows.Forms.Panel  pnlButtons;
        private System.Windows.Forms.Label  lblFieldCount;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnSaveDraft;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Panel  pnlMain;

        // Palette
        private System.Windows.Forms.Panel           pnlPalette;
        private System.Windows.Forms.Label           lblPaletteHeader;
        private System.Windows.Forms.Label           lblPaletteHint;
        private System.Windows.Forms.Panel           pnlPaletteScroll;
        private System.Windows.Forms.Label           lblPaletteSection1;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteBasic;
        private System.Windows.Forms.Label           lblPaletteSection2;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteSelect;
        private System.Windows.Forms.Label           lblPaletteSection3;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteAdvanced;
        private System.Windows.Forms.Label           lblPaletteSection4;
        private System.Windows.Forms.FlowLayoutPanel flowPaletteLayout;

        // Canvas
        private System.Windows.Forms.Panel   pnlCanvas;
        private System.Windows.Forms.Panel   pnlCanvasInner;
        private System.Windows.Forms.Label   lblCanvasHint;
        private System.Windows.Forms.TextBox txtFormTitle;
        private System.Windows.Forms.Panel   pnlSectionsContainer;

        // Properties
        private System.Windows.Forms.Panel    pnlProperties;
        private System.Windows.Forms.Label    lblPropertiesTitle;
        private System.Windows.Forms.Label    lblPropertiesSub;
        private System.Windows.Forms.Label    lblNoSelection;
        private System.Windows.Forms.Panel    pnlFieldProps;
        private System.Windows.Forms.Label    lblPropLabel;
        private System.Windows.Forms.TextBox  txtPropLabel;
        private System.Windows.Forms.Label    lblPropFieldName;
        private System.Windows.Forms.TextBox  txtPropFieldName;
        private System.Windows.Forms.Label    lblPropHelpText;
        private System.Windows.Forms.TextBox  txtPropHelpText;
        private System.Windows.Forms.CheckBox chkRequired;
        private System.Windows.Forms.CheckBox chkVisible;
        private System.Windows.Forms.Panel    pnlDivider1;
        private System.Windows.Forms.Label    lblPropSource;
        private System.Windows.Forms.ComboBox cmbPropSource;
        private System.Windows.Forms.Label    lblPropDefault;
        private System.Windows.Forms.TextBox  txtPropDefault;
        private System.Windows.Forms.Label    lblPropConditional;
        private System.Windows.Forms.TextBox  txtPropConditional;
        private System.Windows.Forms.Label    lblPropMapContext;
        private System.Windows.Forms.TextBox  txtPropMapContext;
        private System.Windows.Forms.Button   btnApplyProps;
    }
}
