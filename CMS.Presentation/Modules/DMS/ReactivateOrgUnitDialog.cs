using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>เลือกหน่วยงานที่ปิดใช้งานอยู่ เพื่อเปิดใช้งานกลับคืน</summary>
    public class ReactivateOrgUnitDialog : Form
    {
        /// <summary>ห่อ OrgUnit เพื่อแสดงรหัส+ชื่อ+ประเภท ในบรรทัดเดียว กันชื่อซ้ำเลือกผิดตัว</summary>
        private class ListEntry
        {
            public OrgUnit Unit { get; set; }
            public override string ToString()
            {
                string deactivatedInfo = Unit.DeletedDate.HasValue
                    ? Unit.DeletedDate.Value.ToString("dd/MM/yyyy HH:mm")
                    : "ไม่ทราบวันที่";
                return string.Format("[{0}] {1}  ({2})  ปิดเมื่อ {3}", Unit.OrgUnitCode, Unit.Name, Unit.Tier, deactivatedInfo);
            }
        }

        private readonly ListBox _list;
        private readonly Button  _btnOk;
        private readonly Button  _btnCancel;

        public OrgUnit SelectedOrgUnit { get; private set; }

        public ReactivateOrgUnitDialog(IEnumerable<OrgUnit> inactiveUnits)
        {
            Text            = "เปิดใช้งานหน่วยงาน";
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox     = false;
            MaximizeBox     = false;
            Font            = new Font("Segoe UI", 9.5f);
            ClientSize      = new Size(660, 400);

            var lbl = new Label
            {
                Text     = "เลือกหน่วยงานที่ต้องการเปิดใช้งาน (เรียงจากปิดล่าสุดก่อน — รหัสไม่ซ้ำกัน ใช้แยกรายการชื่อซ้ำ):",
                Dock     = DockStyle.Top,
                Height   = 44,
                Padding  = new Padding(8, 8, 8, 0)
            };

            _list = new ListBox
            {
                Dock              = DockStyle.Fill,
                Font              = new Font("Segoe UI", 10f),
                IntegralHeight    = false,
                HorizontalScrollbar = true
            };
            foreach (var u in inactiveUnits.OrderByDescending(u => u.DeletedDate))
                _list.Items.Add(new ListEntry { Unit = u });
            _list.DoubleClick += (s, e) => AcceptSelection();

            var pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 44 };

            _btnOk = new Button
            {
                Text     = "🔓 เปิดใช้งาน",
                DialogResult = DialogResult.None,
                Location = new Point(460, 8),
                Size     = new Size(100, 28)
            };
            _btnOk.Click += (s, e) => AcceptSelection();

            _btnCancel = new Button
            {
                Text     = "ยกเลิก",
                DialogResult = DialogResult.Cancel,
                Location = new Point(568, 8),
                Size     = new Size(80, 28)
            };

            pnlButtons.Controls.Add(_btnOk);
            pnlButtons.Controls.Add(_btnCancel);

            Controls.Add(_list);
            Controls.Add(pnlButtons);
            Controls.Add(lbl);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void AcceptSelection()
        {
            var entry = _list.SelectedItem as ListEntry;
            SelectedOrgUnit = entry != null ? entry.Unit : null;
            if (SelectedOrgUnit == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
