namespace CMS.Presentation
{
    partial class OrgUnitPickerDialog
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
            this.components = new System.ComponentModel.Container();

            this.pnlHeader         = new System.Windows.Forms.Panel();
            this.lblHeader         = new System.Windows.Forms.Label();
            this.pnlMain           = new System.Windows.Forms.Panel();
            this.lblStep1          = new System.Windows.Forms.Label();
            this.lblDivisionCode   = new System.Windows.Forms.Label();
            this.txtDivisionCode   = new System.Windows.Forms.TextBox();
            this.lblStep2          = new System.Windows.Forms.Label();
            this.lblLevel          = new System.Windows.Forms.Label();
            this.cmbLevel          = new System.Windows.Forms.ComboBox();
            this.lblParent         = new System.Windows.Forms.Label();
            this.cmbParent         = new System.Windows.Forms.ComboBox();
            this.lblStep3          = new System.Windows.Forms.Label();
            this.lblDepartmentCode = new System.Windows.Forms.Label();
            this.txtDepartmentCode = new System.Windows.Forms.TextBox();
            this.lblStep4          = new System.Windows.Forms.Label();
            this.lblOrgCode        = new System.Windows.Forms.Label();
            this.txtOrgCode        = new System.Windows.Forms.TextBox();
            this.lblStep5          = new System.Windows.Forms.Label();
            this.lblOrgNameEN      = new System.Windows.Forms.Label();
            this.txtOrgNameEN      = new System.Windows.Forms.TextBox();
            this.btnSearchEN       = new System.Windows.Forms.Button();
            this.lblStep6          = new System.Windows.Forms.Label();
            this.lblOrgNameTH      = new System.Windows.Forms.Label();
            this.txtOrgNameTH      = new System.Windows.Forms.TextBox();
            this.btnSearchTH       = new System.Windows.Forms.Button();
            this.pnlFooter         = new System.Windows.Forms.Panel();
            this.btnOk             = new System.Windows.Forms.Button();
            this.btnCancel         = new System.Windows.Forms.Button();

            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 73, 125);
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock   = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 50;
            this.pnlHeader.Name   = "pnlHeader";

            this.lblHeader.Dock      = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Name      = "lblHeader";
            this.lblHeader.Padding   = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblHeader.Text      = "สร้างหน่วยงานใหม่";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlMain
            this.pnlMain.BackColor = System.Drawing.Color.White;
            this.pnlMain.Controls.Add(this.lblStep1);
            this.pnlMain.Controls.Add(this.lblDivisionCode);
            this.pnlMain.Controls.Add(this.txtDivisionCode);
            this.pnlMain.Controls.Add(this.lblStep2);
            this.pnlMain.Controls.Add(this.lblLevel);
            this.pnlMain.Controls.Add(this.cmbLevel);
            this.pnlMain.Controls.Add(this.lblParent);
            this.pnlMain.Controls.Add(this.cmbParent);
            this.pnlMain.Controls.Add(this.lblStep3);
            this.pnlMain.Controls.Add(this.lblDepartmentCode);
            this.pnlMain.Controls.Add(this.txtDepartmentCode);
            this.pnlMain.Controls.Add(this.lblStep4);
            this.pnlMain.Controls.Add(this.lblOrgCode);
            this.pnlMain.Controls.Add(this.txtOrgCode);
            this.pnlMain.Controls.Add(this.lblStep5);
            this.pnlMain.Controls.Add(this.lblOrgNameEN);
            this.pnlMain.Controls.Add(this.txtOrgNameEN);
            this.pnlMain.Controls.Add(this.btnSearchEN);
            this.pnlMain.Controls.Add(this.lblStep6);
            this.pnlMain.Controls.Add(this.lblOrgNameTH);
            this.pnlMain.Controls.Add(this.txtOrgNameTH);
            this.pnlMain.Controls.Add(this.btnSearchTH);
            this.pnlMain.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Name     = "pnlMain";
            this.pnlMain.Padding  = new System.Windows.Forms.Padding(20);
            // Step 1 - Division Code
            this.lblStep1.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep1.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep1.ForeColor = System.Drawing.Color.White;
            this.lblStep1.Location  = new System.Drawing.Point(30, 30);
            this.lblStep1.Name      = "lblStep1";
            this.lblStep1.Size      = new System.Drawing.Size(35, 35);
            this.lblStep1.Text      = "1";
            this.lblStep1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblDivisionCode.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDivisionCode.Location = new System.Drawing.Point(80, 38);
            this.lblDivisionCode.Name     = "lblDivisionCode";
            this.lblDivisionCode.Size     = new System.Drawing.Size(150, 20);
            this.lblDivisionCode.Text     = "รหัสฝ่าย (2 หลัก):";

            this.txtDivisionCode.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDivisionCode.Location  = new System.Drawing.Point(250, 35);
            this.txtDivisionCode.MaxLength = 2;
            this.txtDivisionCode.Name      = "txtDivisionCode";
            this.txtDivisionCode.Size      = new System.Drawing.Size(100, 25);
            this.txtDivisionCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Step 2 - Level and Parent
            this.lblStep2.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep2.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep2.ForeColor = System.Drawing.Color.White;
            this.lblStep2.Location  = new System.Drawing.Point(30, 80);
            this.lblStep2.Name      = "lblStep2";
            this.lblStep2.Size      = new System.Drawing.Size(35, 35);
            this.lblStep2.Text      = "2";
            this.lblStep2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblLevel.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLevel.Location = new System.Drawing.Point(80, 88);
            this.lblLevel.Name     = "lblLevel";
            this.lblLevel.Size     = new System.Drawing.Size(150, 20);
            this.lblLevel.Text     = "ระดับ:";

            this.cmbLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbLevel.Location      = new System.Drawing.Point(250, 85);
            this.cmbLevel.Name          = "cmbLevel";
            this.cmbLevel.Size          = new System.Drawing.Size(300, 25);

            this.lblParent.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblParent.Location = new System.Drawing.Point(80, 118);
            this.lblParent.Name     = "lblParent";
            this.lblParent.Size     = new System.Drawing.Size(150, 20);
            this.lblParent.Text     = "สังกัด:";

            this.cmbParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbParent.Font          = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbParent.Location      = new System.Drawing.Point(250, 115);
            this.cmbParent.Name          = "cmbParent";
            this.cmbParent.Size          = new System.Drawing.Size(300, 25);

            // Step 3 - Department Code
            this.lblStep3.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep3.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep3.ForeColor = System.Drawing.Color.White;
            this.lblStep3.Location  = new System.Drawing.Point(30, 160);
            this.lblStep3.Name      = "lblStep3";
            this.lblStep3.Size      = new System.Drawing.Size(35, 35);
            this.lblStep3.Text      = "3";
            this.lblStep3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblDepartmentCode.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDepartmentCode.Location = new System.Drawing.Point(80, 168);
            this.lblDepartmentCode.Name     = "lblDepartmentCode";
            this.lblDepartmentCode.Size     = new System.Drawing.Size(150, 20);
            this.lblDepartmentCode.Text     = "รหัสแผนก (2 หลัก):";

            this.txtDepartmentCode.Font      = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDepartmentCode.Location  = new System.Drawing.Point(250, 165);
            this.txtDepartmentCode.MaxLength = 2;
            this.txtDepartmentCode.Name      = "txtDepartmentCode";
            this.txtDepartmentCode.Size      = new System.Drawing.Size(100, 25);
            this.txtDepartmentCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Step 4 - Organization Code (Auto-generated)
            this.lblStep4.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep4.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep4.ForeColor = System.Drawing.Color.White;
            this.lblStep4.Location  = new System.Drawing.Point(30, 210);
            this.lblStep4.Name      = "lblStep4";
            this.lblStep4.Size      = new System.Drawing.Size(35, 35);
            this.lblStep4.Text      = "4";
            this.lblStep4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblOrgCode.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOrgCode.Location = new System.Drawing.Point(80, 218);
            this.lblOrgCode.Name     = "lblOrgCode";
            this.lblOrgCode.Size     = new System.Drawing.Size(150, 20);
            this.lblOrgCode.Text     = "รหัสรวม (4 หลัก):";

            this.txtOrgCode.Font     = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.txtOrgCode.Location = new System.Drawing.Point(250, 215);
            this.txtOrgCode.Name     = "txtOrgCode";
            this.txtOrgCode.ReadOnly = true;
            this.txtOrgCode.Size     = new System.Drawing.Size(100, 25);
            this.txtOrgCode.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.txtOrgCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Step 5 - Organization Name (EN)
            this.lblStep5.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep5.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep5.ForeColor = System.Drawing.Color.White;
            this.lblStep5.Location  = new System.Drawing.Point(30, 260);
            this.lblStep5.Name      = "lblStep5";
            this.lblStep5.Size      = new System.Drawing.Size(35, 35);
            this.lblStep5.Text      = "5";
            this.lblStep5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblOrgNameEN.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOrgNameEN.Location = new System.Drawing.Point(80, 268);
            this.lblOrgNameEN.Name     = "lblOrgNameEN";
            this.lblOrgNameEN.Size     = new System.Drawing.Size(150, 20);
            this.lblOrgNameEN.Text     = "ชื่อหน่วยงาน (EN):";

            this.txtOrgNameEN.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtOrgNameEN.Location = new System.Drawing.Point(250, 265);
            this.txtOrgNameEN.Name     = "txtOrgNameEN";
            this.txtOrgNameEN.Size     = new System.Drawing.Size(450, 25);

            this.btnSearchEN.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnSearchEN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchEN.FlatAppearance.BorderSize = 0;
            this.btnSearchEN.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchEN.ForeColor = System.Drawing.Color.White;
            this.btnSearchEN.Location  = new System.Drawing.Point(710, 265);
            this.btnSearchEN.Name      = "btnSearchEN";
            this.btnSearchEN.Size      = new System.Drawing.Size(70, 25);
            this.btnSearchEN.Text      = "🔍 ค้นหา";
            this.btnSearchEN.Click    += new System.EventHandler(this.btnSearchEN_Click);

            // Step 6 - Organization Name (TH)
            this.lblStep6.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.lblStep6.Font      = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStep6.ForeColor = System.Drawing.Color.White;
            this.lblStep6.Location  = new System.Drawing.Point(30, 310);
            this.lblStep6.Name      = "lblStep6";
            this.lblStep6.Size      = new System.Drawing.Size(35, 35);
            this.lblStep6.Text      = "6";
            this.lblStep6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblOrgNameTH.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.lblOrgNameTH.Location = new System.Drawing.Point(80, 318);
            this.lblOrgNameTH.Name     = "lblOrgNameTH";
            this.lblOrgNameTH.Size     = new System.Drawing.Size(150, 20);
            this.lblOrgNameTH.Text     = "ชื่อหน่วยงาน (TH):";

            this.txtOrgNameTH.Font     = new System.Drawing.Font("Segoe UI", 10F);
            this.txtOrgNameTH.Location = new System.Drawing.Point(250, 315);
            this.txtOrgNameTH.Name     = "txtOrgNameTH";
            this.txtOrgNameTH.Size     = new System.Drawing.Size(450, 25);

            this.btnSearchTH.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnSearchTH.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchTH.FlatAppearance.BorderSize = 0;
            this.btnSearchTH.Font      = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSearchTH.ForeColor = System.Drawing.Color.White;
            this.btnSearchTH.Location  = new System.Drawing.Point(710, 315);
            this.btnSearchTH.Name      = "btnSearchTH";
            this.btnSearchTH.Size      = new System.Drawing.Size(70, 25);
            this.btnSearchTH.Text      = "🔍 ค้นหา";
            this.btnSearchTH.Click    += new System.EventHandler(this.btnSearchTH_Click);

            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlFooter.Controls.Add(this.btnOk);
            this.pnlFooter.Controls.Add(this.btnCancel);
            this.pnlFooter.Dock   = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 60;
            this.pnlFooter.Name   = "pnlFooter";
            this.pnlFooter.Padding = new System.Windows.Forms.Padding(8);
            this.pnlFooter.Resize += new System.EventHandler(this.pnlFooter_Resize);

            this.btnOk.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.FromArgb(31, 73, 125);
            this.btnOk.Enabled   = true;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.Font      = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location  = new System.Drawing.Point(580, 13);
            this.btnOk.Name      = "btnOk";
            this.btnOk.Size      = new System.Drawing.Size(100, 35);
            this.btnOk.Text      = "บันทึก";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click    += new System.EventHandler(this.btnOk_Click);

            this.btnCancel.Anchor       = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor    = System.Drawing.Color.White;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle    = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnCancel.Font         = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancel.ForeColor    = System.Drawing.Color.FromArgb(80, 80, 80);
            this.btnCancel.Location     = new System.Drawing.Point(690, 13);
            this.btnCancel.Name         = "btnCancel";
            this.btnCancel.Size         = new System.Drawing.Size(100, 35);
            this.btnCancel.Text         = "ยกเลิก";
            this.btnCancel.UseVisualStyleBackColor = false;

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton        = this.btnCancel;
            this.ClientSize          = new System.Drawing.Size(800, 420);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "OrgUnitPickerDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "สร้างหน่วยงานใหม่";
            this.Load           += new System.EventHandler(this.OrgUnitPickerDialog_Load);

            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel      pnlHeader;
        private System.Windows.Forms.Label      lblHeader;
        private System.Windows.Forms.Panel      pnlMain;
        private System.Windows.Forms.Label      lblStep1;
        private System.Windows.Forms.Label      lblDivisionCode;
        private System.Windows.Forms.TextBox    txtDivisionCode;
        private System.Windows.Forms.Label      lblStep2;
        private System.Windows.Forms.Label      lblLevel;
        private System.Windows.Forms.ComboBox   cmbLevel;
        private System.Windows.Forms.Label      lblParent;
        private System.Windows.Forms.ComboBox   cmbParent;
        private System.Windows.Forms.Label      lblStep3;
        private System.Windows.Forms.Label      lblDepartmentCode;
        private System.Windows.Forms.TextBox    txtDepartmentCode;
        private System.Windows.Forms.Label      lblStep4;
        private System.Windows.Forms.Label      lblOrgCode;
        private System.Windows.Forms.TextBox    txtOrgCode;
        private System.Windows.Forms.Label      lblStep5;
        private System.Windows.Forms.Label      lblOrgNameEN;
        private System.Windows.Forms.TextBox    txtOrgNameEN;
        private System.Windows.Forms.Button     btnSearchEN;
        private System.Windows.Forms.Label      lblStep6;
        private System.Windows.Forms.Label      lblOrgNameTH;
        private System.Windows.Forms.TextBox    txtOrgNameTH;
        private System.Windows.Forms.Button     btnSearchTH;
        private System.Windows.Forms.Panel      pnlFooter;
        private System.Windows.Forms.Button     btnOk;
        private System.Windows.Forms.Button     btnCancel;
    }
}