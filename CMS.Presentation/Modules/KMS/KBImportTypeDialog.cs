using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// Dialog เลือกประเภทบทความก่อนเข้า wizard นำเข้า Excel
    /// 1 = กฎหมาย → ใช้ KBImportWizardForm เดิม
    /// 8 = คำสั่ง  → ใช้ KBOrderImportWizardForm
    /// ประเภทอื่นยังไม่เปิดใช้งาน
    /// </summary>
    public partial class KBImportTypeDialog : Form
    {
        /// <summary>ประเภทที่เปิดใช้งานแล้ว</summary>
        private static readonly HashSet<int> SupportedCodes = new HashSet<int> { 1, 8 };

        /// <summary>ประเภทที่ user เลือก (อ่านได้เมื่อ ShowDialog คืน DialogResult.OK)</summary>
        public int SelectedTypeCode { get; private set; }

        public KBImportTypeDialog()
        {
            InitializeComponent();
            BindTypes();
        }

        private void BindTypes()
        {
            cboType.DisplayMember = "Text";
            cboType.ValueMember   = "Code";
            cboType.DataSource = new[]
            {
                new TypeItem(1,  "กฎหมาย"),
                new TypeItem(2,  "AC  พระราชบัญญัติ"),
                new TypeItem(3,  "EA  พระราชกำหนด"),
                new TypeItem(4,  "RD  พระราชกฤษฎีกา"),
                new TypeItem(5,  "MR  กฎกระทรวง"),
                new TypeItem(6,  "RG  ระเบียบ"),
                new TypeItem(7,  "AN  ประกาศ"),
                new TypeItem(8,  "OR  คำสั่ง"),
                new TypeItem(9,  "CI  หนังสือเวียน"),
                new TypeItem(10, "GL  แนวปฏิบัติ"),
                new TypeItem(11, "RS  มติ / คำวินิจฉัย"),
            };
            cboType.SelectedIndex = 0;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            var item = cboType.SelectedItem as TypeItem;
            if (item == null) return;

            if (!SupportedCodes.Contains(item.Code))
            {
                MessageBox.Show(
                    string.Format("ประเภท \"{0}\" อยู่ระหว่างการพัฒนา", item.Text),
                    "อยู่ระหว่างการพัฒนา",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;   // ไม่ปิด dialog ให้เลือกใหม่ได้
            }

            SelectedTypeCode = item.Code;
            DialogResult     = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private sealed class TypeItem
        {
            public int    Code { get; private set; }
            public string Text { get; private set; }

            public TypeItem(int code, string text)
            {
                Code = code;
                Text = text;
            }

            public override string ToString() { return Text; }
        }
    }
}
