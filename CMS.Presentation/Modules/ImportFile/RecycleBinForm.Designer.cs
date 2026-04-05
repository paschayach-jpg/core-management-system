using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation
{
    partial class RecycleBinForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ────────────────────────────────────────────────────────────
        private ToolStrip            _toolbar;
        private ToolStripButton      _btnRestore;
        private ToolStripButton      _btnDelete;
        private ToolStripButton      _btnRefresh;
        private ToolStripButton      _btnEmptyBin;
        private ToolStripSeparator   _sep1;
        private ToolStripLabel       _lblOrgUnit;
        private ToolStripComboBox    _cmbOrgUnit;

        private ListView             _listView;
        private StatusStrip          _statusBar;
        private ToolStripStatusLabel _lblStatus;
        private Panel                _emptyPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // ── Form ──────────────────────────────────────────────────────────
            this.Text          = "🗑️ ถังขยะ — Recycle Bin";
            this.Size          = new Size(900, 600);
            this.MinimumSize   = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font          = new Font("Segoe UI", 9f);
            this.BackColor     = Color.White;
            this.Name          = "RecycleBinForm";

            // ── Toolbar ───────────────────────────────────────────────────────
            _toolbar = new ToolStrip
            {
                BackColor = Color.FromArgb(240, 244, 250),
                GripStyle = ToolStripGripStyle.Hidden,
                Padding   = new Padding(4, 0, 0, 0),
                Dock      = DockStyle.Top
            };

            _btnRestore = new ToolStripButton("♻️  คืนค่า (Restore)")
            {
                ToolTipText = "คืน Folder ที่เลือกกลับไปยังตำแหน่งเดิม",
                Enabled     = false
            };
            _btnRestore.Click += BtnRestore_Click;

            _btnDelete = new ToolStripButton("🗑️  ลบถาวร")
            {
                ToolTipText = "ลบ Folder ที่เลือกออกจากระบบถาวร (ไม่สามารถกู้คืนได้)",
                Enabled     = false,
                ForeColor   = Color.FromArgb(180, 30, 30)
            };
            _btnDelete.Click += BtnDelete_Click;

            _sep1 = new ToolStripSeparator();

            _btnEmptyBin = new ToolStripButton("🗑️  ล้างถังขยะทั้งหมด")
            {
                ToolTipText = "ลบ Folder ทั้งหมดในถังขยะออกถาวร",
                ForeColor   = Color.FromArgb(180, 30, 30),
                Enabled     = false
            };
            _btnEmptyBin.Click += BtnEmptyBin_Click;

            _btnRefresh = new ToolStripButton("↻  Refresh")
            {
                ToolTipText = "โหลดข้อมูลใหม่",
                Alignment   = ToolStripItemAlignment.Right
            };
            _btnRefresh.Click += (s, e) => LoadData();

            _lblOrgUnit = new ToolStripLabel("หน่วยงาน:")
            {
                Alignment = ToolStripItemAlignment.Right
            };

            _cmbOrgUnit = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width         = 180,
                Alignment     = ToolStripItemAlignment.Right,
                ToolTipText   = "กรองตามหน่วยงาน"
            };
            _cmbOrgUnit.SelectedIndexChanged += CmbOrgUnit_SelectedIndexChanged;

            _toolbar.Items.AddRange(new ToolStripItem[]
            {
                _btnRestore, _btnDelete, _sep1, _btnEmptyBin,
                _btnRefresh, _lblOrgUnit, _cmbOrgUnit
            });

            // ── ListView ──────────────────────────────────────────────────────
            _listView = new ListView
            {
                Dock          = DockStyle.Fill,
                View          = View.Details,
                FullRowSelect = true,
                GridLines     = false,
                MultiSelect   = true,
                CheckBoxes    = false,
                Font          = new Font("Segoe UI", 9f),
                BorderStyle   = BorderStyle.None,
                BackColor     = Color.White
            };

            _listView.Columns.Add("ชื่อ Folder",  250);
            _listView.Columns.Add("หน่วยงาน",      160);
            _listView.Columns.Add("ตำแหน่งเดิม",   200);
            _listView.Columns.Add("วันที่ลบ",      140);
            _listView.Columns.Add("คงเหลือ (วัน)", 100);
            _listView.Columns.Add("ระดับ",           80);

            _listView.OwnerDraw = true;
            _listView.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            _listView.DrawItem         += (s, e) => { };
            _listView.DrawSubItem      += ListViewDrawSubItem;

            _listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            _listView.DoubleClick          += ListView_DoubleClick;
            _listView.ColumnClick          += ListView_ColumnClick;
            _listView.ListViewItemSorter    = new ListViewColumnSorter();

            // ── Empty State Panel ─────────────────────────────────────────────
            _emptyPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.White,
                Visible   = false
            };

            var _iconLabel = new Label
            {
                Text      = "🗑️",
                Font      = new Font("Segoe UI", 48f),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Top,
                Height    = 120
            };
            var _msgLabel = new Label
            {
                Text      = "ถังขยะว่างเปล่า",
                Font      = new Font("Segoe UI", 14f),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Top,
                Height    = 36
            };
            var _subLabel = new Label
            {
                Text      = "Folder ที่ถูกลบจะปรากฏที่นี่",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Top,
                Height    = 24
            };

            _emptyPanel.Controls.Add(_subLabel);
            _emptyPanel.Controls.Add(_msgLabel);
            _emptyPanel.Controls.Add(_iconLabel);

            // ── Status Bar ────────────────────────────────────────────────────
            _statusBar = new StatusStrip { BackColor = Color.FromArgb(240, 244, 250) };
            _lblStatus = new ToolStripStatusLabel("พร้อมใช้งาน")
            {
                Spring    = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _statusBar.Items.Add(_lblStatus);

            // ── Add to Form ───────────────────────────────────────────────────
            this.Controls.Add(_listView);
            this.Controls.Add(_emptyPanel);
            this.Controls.Add(_toolbar);
            this.Controls.Add(_statusBar);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
