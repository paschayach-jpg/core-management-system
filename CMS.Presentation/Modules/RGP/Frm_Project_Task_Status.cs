using System;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.RGP
{
    /// <summary>Dialog อัปเดต Status + % ของ Task</summary>
    public partial class Frm_Project_Task_Status : Form
    {
        public string NewStatus { get; private set; }
        public int    NewPct    { get; private set; }

        private static readonly (string Value, string Label)[] _statuses =
        {
            ("Todo",       "📌 รอดำเนินการ"),
            ("InProgress", "🔄 กำลังทำ"),
            ("InReview",   "🔍 รอตรวจสอบ"),
            ("Done",       "✅ เสร็จแล้ว"),
            ("OnHold",     "⏸ On Hold"),
        };

        public Frm_Project_Task_Status(int taskId, string currentStatus, int currentPct)
        {
            InitializeComponent();

            // set current values
            for (int i = 0; i < _statuses.Length; i++)
                if (_statuses[i].Value == currentStatus)
                { cbbStatus.SelectedIndex = i; break; }

            nudPct.Value = currentPct;
        }

        private void cbbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Done → 100%, Todo → 0% อัตโนมัติ
            string val = _statuses[cbbStatus.SelectedIndex].Value;
            if (val == "Done")   nudPct.Value = 100;
            if (val == "Todo")   nudPct.Value = 0;

            // On Hold: แช่แข็ง % ไว้ที่ค่าเดิม ห้ามแก้ไขขณะอยู่สถานะนี้
            nudPct.Enabled = val != "OnHold";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            NewStatus         = _statuses[cbbStatus.SelectedIndex].Value;
            NewPct            = (int)nudPct.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
