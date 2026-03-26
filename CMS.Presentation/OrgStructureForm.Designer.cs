namespace CMS.Presentation
{
    partial class OrgStructureForm
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

            this.toolStrip      = new System.Windows.Forms.ToolStrip();
            this.btnCreate      = new System.Windows.Forms.ToolStripButton();
            this.btnEdit        = new System.Windows.Forms.ToolStripButton();
            this.btnMove        = new System.Windows.Forms.ToolStripButton();
            this.btnDeactivate  = new System.Windows.Forms.ToolStripButton();
            this.separator1     = new System.Windows.Forms.ToolStripSeparator();

            this.splitterMain   = new System.Windows.Forms.Splitter();
            this.pnlLeft        = new System.Windows.Forms.Panel();
            this.lblViewMode    = new System.Windows.Forms.Label();
            this.viewPanel      = new System.Windows.Forms.Panel();
            this.radioOrgView   = new System.Windows.Forms.RadioButton();
            this.radioConfView  = new System.Windows.Forms.RadioButton();
            this.lblConfLevel   = new System.Windows.Forms.Label();
            this.pnlConfLevels  = new System.Windows.Forms.Panel();
            this.chk1HC         = new System.Windows.Forms.CheckBox();
            this.chk2SC         = new System.Windows.Forms.CheckBox();
            this.chk3NC         = new System.Windows.Forms.CheckBox();
            this.chk4PC         = new System.Windows.Forms.CheckBox();
            this.lblSearch      = new System.Windows.Forms.Label();
            this.txtSearch      = new System.Windows.Forms.TextBox();

            this.pnlRight       = new System.Windows.Forms.Panel();
            this.treeView       = new System.Windows.Forms.TreeView();

            this.toolStrip.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.viewPanel.SuspendLayout();
            this.pnlConfLevels.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.SuspendLayout();

            // ---------------------------------------------------------------
            // toolStrip
            // ---------------------------------------------------------------
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.btnCreate,
                this.btnEdit,
                this.btnMove,
                this.separator1,
                this.btnDeactivate
            });
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name     = "toolStrip";
            this.toolStrip.Size     = new System.Drawing.Size(1000, 25);
            this.toolStrip.TabIndex = 0;

            // btnCreate
            this.btnCreate.Name        = "btnCreate";
            this.btnCreate.Text        = "➕ สร้างใหม่";
            this.btnCreate.ToolTipText = "สร้างหน่วยงานใหม่";
            this.btnCreate.Click      += new System.EventHandler(this.btnCreate_Click);

            // btnEdit
            this.btnEdit.Name        = "btnEdit";
            this.btnEdit.Text        = "✏️ แก้ไข";
            this.btnEdit.ToolTipText = "แก้ไขชื่อหน่วยงาน";
            this.btnEdit.Click      += new System.EventHandler(this.btnEdit_Click);

            // btnMove
            this.btnMove.Name        = "btnMove";
            this.btnMove.Text        = "↔️ ย้าย";
            this.btnMove.ToolTipText = "ย้ายหน่วยงานไปยังหน่วยงานแม่อื่น";
            this.btnMove.Click      += new System.EventHandler(this.btnMove_Click);

            // separator1
            this.separator1.Name = "separator1";

            // btnDeactivate
            this.btnDeactivate.Name        = "btnDeactivate";
            this.btnDeactivate.Text        = "🚫 ปิดใช้งาน";
            this.btnDeactivate.ToolTipText = "ปิดใช้งานหน่วยงาน";
            this.btnDeactivate.Click      += new System.EventHandler(this.btnDeactivate_Click);

            // ---------------------------------------------------------------
            // pnlLeft (Left Panel with filters)
            // ---------------------------------------------------------------
            this.pnlLeft.Dock       = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width      = 280;
            this.pnlLeft.BackColor  = System.Drawing.Color.FromArgb(240, 240, 240);
            this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeft.Padding    = new System.Windows.Forms.Padding(8);

            this.lblViewMode.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblViewMode.Height    = 24;
            this.lblViewMode.Text      = "มุมมอง:";
            this.lblViewMode.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.lblViewMode.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);

            this.viewPanel.Dock     = System.Windows.Forms.DockStyle.Top;
            this.viewPanel.Height   = 60;
            this.viewPanel.BackColor = System.Drawing.Color.White;
            this.viewPanel.Padding  = new System.Windows.Forms.Padding(4);

            this.radioOrgView.AutoSize = true;
            this.radioOrgView.Location = new System.Drawing.Point(8, 8);
            this.radioOrgView.Text     = "📊 มุมมององค์กร";
            this.radioOrgView.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.radioOrgView.CheckedChanged += new System.EventHandler(this.radioOrgView_CheckedChanged);

            this.radioConfView.AutoSize = true;
            this.radioConfView.Location = new System.Drawing.Point(8, 32);
            this.radioConfView.Text     = "🔐 มุมมองความลับ";
            this.radioConfView.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.radioConfView.CheckedChanged += new System.EventHandler(this.radioConfView_CheckedChanged);

            this.viewPanel.Controls.Add(this.radioOrgView);
            this.viewPanel.Controls.Add(this.radioConfView);

            this.lblConfLevel.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblConfLevel.Height    = 24;
            this.lblConfLevel.Text      = "โครงระดับ:";
            this.lblConfLevel.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.lblConfLevel.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblConfLevel.Margin    = new System.Windows.Forms.Padding(0, 8, 0, 0);

            this.pnlConfLevels.Dock       = System.Windows.Forms.DockStyle.Top;
            this.pnlConfLevels.Height     = 100;
            this.pnlConfLevels.BackColor  = System.Drawing.Color.White;
            this.pnlConfLevels.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlConfLevels.Padding    = new System.Windows.Forms.Padding(8);

            this.chk1HC.AutoSize = true;
            this.chk1HC.Location = new System.Drawing.Point(8, 8);
            this.chk1HC.Text     = "🔴 1HC — High Confidential";
            this.chk1HC.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.chk1HC.Checked  = true;
            this.chk1HC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);

            this.chk2SC.AutoSize = true;
            this.chk2SC.Location = new System.Drawing.Point(8, 28);
            this.chk2SC.Text     = "🟠 2SC — Semi Confidential";
            this.chk2SC.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.chk2SC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);

            this.chk3NC.AutoSize = true;
            this.chk3NC.Location = new System.Drawing.Point(8, 48);
            this.chk3NC.Text     = "🟡 3NC — Normal";
            this.chk3NC.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.chk3NC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);

            this.chk4PC.AutoSize = true;
            this.chk4PC.Location = new System.Drawing.Point(8, 68);
            this.chk4PC.Text     = "🟢 4PC — Public";
            this.chk4PC.Font     = new System.Drawing.Font("Segoe UI", 9f);
            this.chk4PC.CheckedChanged += new System.EventHandler(this.OnConfidentialityLevelChanged);

            this.pnlConfLevels.Controls.Add(this.chk1HC);
            this.pnlConfLevels.Controls.Add(this.chk2SC);
            this.pnlConfLevels.Controls.Add(this.chk3NC);
            this.pnlConfLevels.Controls.Add(this.chk4PC);

            this.lblSearch.Dock      = System.Windows.Forms.DockStyle.Top;
            this.lblSearch.Height    = 24;
            this.lblSearch.Text      = "ค้นหา:";
            this.lblSearch.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            this.lblSearch.ForeColor = System.Drawing.Color.FromArgb(0, 51, 102);
            this.lblSearch.Margin    = new System.Windows.Forms.Padding(0, 8, 0, 0);

            this.txtSearch.Dock   = System.Windows.Forms.DockStyle.Top;
            this.txtSearch.Height = 28;
            this.txtSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtSearch.Font   = new System.Drawing.Font("Segoe UI", 9f);

            this.pnlLeft.Controls.Add(this.txtSearch);
            this.pnlLeft.Controls.Add(this.lblSearch);
            this.pnlLeft.Controls.Add(this.pnlConfLevels);
            this.pnlLeft.Controls.Add(this.lblConfLevel);
            this.pnlLeft.Controls.Add(this.viewPanel);
            this.pnlLeft.Controls.Add(this.lblViewMode);

            // ---------------------------------------------------------------
            // splitterMain
            // ---------------------------------------------------------------
            this.splitterMain.Dock      = System.Windows.Forms.DockStyle.Left;
            this.splitterMain.Width     = 4;
            this.splitterMain.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);

            // ---------------------------------------------------------------
            // pnlRight (Right Panel with TreeView)
            // ---------------------------------------------------------------
            this.pnlRight.Dock       = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.BackColor  = System.Drawing.Color.White;

            this.treeView.Dock     = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Font     = new System.Drawing.Font("Segoe UI", 9.5f);
            this.treeView.ShowLines = true;
            this.treeView.ShowPlusMinus = true;

            this.pnlRight.Controls.Add(this.treeView);

            // ---------------------------------------------------------------
            // OrgStructureForm
            // ---------------------------------------------------------------
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.splitterMain);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.toolStrip);
            this.Name  = "OrgStructureForm";
            this.Text  = "MOD-001: โครงสร้างองค์กร";
            this.Load += new System.EventHandler(this.OrgStructureForm_Load);

            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.viewPanel.ResumeLayout(false);
            this.viewPanel.PerformLayout();
            this.pnlConfLevels.ResumeLayout(false);
            this.pnlConfLevels.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStrip          toolStrip;
        private System.Windows.Forms.ToolStripButton    btnCreate;
        private System.Windows.Forms.ToolStripButton    btnEdit;
        private System.Windows.Forms.ToolStripButton    btnMove;
        private System.Windows.Forms.ToolStripButton    btnDeactivate;
        private System.Windows.Forms.ToolStripSeparator separator1;
        private System.Windows.Forms.Splitter           splitterMain;
        private System.Windows.Forms.Panel              pnlLeft;
        private System.Windows.Forms.Label              lblViewMode;
        private System.Windows.Forms.Panel              viewPanel;
        private System.Windows.Forms.RadioButton        radioOrgView;
        private System.Windows.Forms.RadioButton        radioConfView;
        private System.Windows.Forms.Label              lblConfLevel;
        private System.Windows.Forms.Panel              pnlConfLevels;
        private System.Windows.Forms.CheckBox           chk1HC;
        private System.Windows.Forms.CheckBox           chk2SC;
        private System.Windows.Forms.CheckBox           chk3NC;
        private System.Windows.Forms.CheckBox           chk4PC;
        private System.Windows.Forms.Label              lblSearch;
        private System.Windows.Forms.TextBox            txtSearch;
        private System.Windows.Forms.Panel              pnlRight;
        private System.Windows.Forms.TreeView           treeView;
    }
}
