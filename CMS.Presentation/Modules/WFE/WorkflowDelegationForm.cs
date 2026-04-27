using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// Delegation settings page in WFE style.
    /// </summary>
    public partial class WorkflowDelegationForm : Form
    {
        private readonly Color _navyDark = Color.FromArgb(31, 56, 100);
        private readonly Color _navHover = Color.FromArgb(245, 249, 255);

        public WorkflowDelegationForm()
        {
            InitializeComponent();
        }

        private void WorkflowDelegationForm_Load(object sender, EventArgs e)
        {
            BuildRibbonTabs();
            BuildToolbar();
            Panel initialNav;
            WorkflowNavigationBuilder.BuildSidebar(
                pnlSidebarContainer, "delegation", OnNavItemClick, out initialNav);

            InitializeFormData();
            LoadHistoryRows();
        }

        private void InitializeFormData()
        {
            txtFromUser.Text = "วินัย มีสุข (Manager - Legal)";

            cmbDelegateTo.Items.Clear();
            cmbDelegateTo.Items.AddRange(new object[]
            {
                "สิทธิพร พงศ์ชัย (Sr. Legal Officer)",
                "ธนพล ภักดี (Legal Officer)",
                "สุโขภา รัตน์ (Legal Officer)"
            });
            cmbDelegateTo.SelectedIndex = 0;

            cmbScope.Items.Clear();
            cmbScope.Items.AddRange(new object[]
            {
                "ทุก Workflow ของฉัน",
                "เฉพาะ WFD-003 (KB)",
                "เฉพาะ WFD-005 (ลา)"
            });
            cmbScope.SelectedIndex = 0;

            dtpStart.Value = new DateTime(2026, 4, 17, 9, 0, 0);
            dtpEnd.Value = new DateTime(2026, 4, 24, 17, 0, 0);

            chkEmail.Checked = true;
            chkOutOfOffice.Checked = true;
            chkRequireReason.Checked = false;

            txtReason.Text = "ลาพักร้อน 7 วัน - ติดต่อคุณสิทธิพรสำหรับงานเร่งด่วน";
        }

        private void LoadHistoryRows()
        {
            dgvDelegations.Rows.Clear();
            dgvDelegations.Rows.Add("วินัย มีสุข (ฉัน) → สิทธิพร พงศ์ชัย", "ทุก Workflow", "17/04/69", "24/04/69", "กำลังใช้งาน", "ลาพักร้อน - ติดต่อแทน");
            dgvDelegations.Rows.Add("วินัย มีสุข (ฉัน) → ธนพล ภักดี", "เฉพาะ WFD-003 (KB)", "01/05/69", "15/05/69", "กำหนดไว้", "ลาป่วยล่วงหน้า");
            dgvDelegations.Rows.Add("วินัย มีสุข (ฉัน) → สุโขภา รัตน์", "WFD-005 (ลา)", "01/04/69", "10/04/69", "สิ้นสุดแล้ว", "-");
        }

        private void BuildRibbonTabs()
        {
            var tabs = new[]
            {
                new { Text = "แฟ้ม", Active = false },
                new { Text = "หน้าแรก", Active = false },
                new { Text = "ดำเนินการ", Active = false },
                new { Text = "มุมมอง", Active = false },
                new { Text = "เครื่องมือ", Active = true },
                new { Text = "ช่วยเหลือ", Active = false },
            };

            int x = 8;
            foreach (var t in tabs)
            {
                var btn = new Button
                {
                    Text = t.Text,
                    Location = new Point(x, 0),
                    Size = new Size(MeasureTabWidth(t.Text) + 24, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = t.Active ? Color.White : Color.FromArgb(190, 210, 245),
                    Font = t.Active ? new Font("Segoe UI", 9.5F, FontStyle.Bold) : new Font("Segoe UI", 9.5F),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 80, 130);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 70, 120);

                if (t.Active)
                {
                    btn.Paint += (s, pe) =>
                        pe.Graphics.FillRectangle(Brushes.White, 0, btn.Height - 3, btn.Width, 3);
                }

                pnlRibbonTabs.Controls.Add(btn);
                x += btn.Width + 2;
            }
        }

        private int MeasureTabWidth(string text)
        {
            using (var g = CreateGraphics())
            using (var f = new Font("Segoe UI", 9.5F))
                return (int)g.MeasureString(text, f).Width;
        }

        private void BuildToolbar()
        {
            var groups = new[]
            {
                new { Header = "เพิ่ม", Items = new[]
                {
                    new { Icon = "+", Text = "สร้างใหม่", Tag = "new" },
                    new { Icon = "✎", Text = "แก้ไข", Tag = "edit" },
                    new { Icon = "⌫", Text = "ยกเลิก", Tag = "cancel" },
                }},
                new { Header = "มุมมอง", Items = new[]
                {
                    new { Icon = "◷", Text = "ประวัติ", Tag = "history" },
                    new { Icon = "✓", Text = "Active", Tag = "active" },
                }},
            };

            int x = 8;
            foreach (var group in groups)
            {
                int groupStartX = x;
                foreach (var item in group.Items)
                {
                    var tag = item.Tag;
                    var pnlBtn = new Panel
                    {
                        Location = new Point(x, 2),
                        Size = new Size(58, 60),
                        BackColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = tag,
                    };

                    var lblIcon = new Label
                    {
                        Text = item.Icon,
                        Font = new Font("Segoe UI", 16F),
                        ForeColor = _navyDark,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 6),
                        Size = new Size(58, 30),
                    };
                    var lblText = new Label
                    {
                        Text = item.Text,
                        Font = new Font("Segoe UI", 7.5F),
                        ForeColor = Color.FromArgb(55, 65, 90),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Location = new Point(0, 36),
                        Size = new Size(58, 20),
                    };

                    EventHandler enter = (s, ev) =>
                    {
                        pnlBtn.BackColor = _navHover;
                        lblIcon.BackColor = _navHover;
                        lblText.BackColor = _navHover;
                    };
                    EventHandler leave = (s, ev) =>
                    {
                        pnlBtn.BackColor = Color.White;
                        lblIcon.BackColor = Color.White;
                        lblText.BackColor = Color.White;
                    };
                    EventHandler click = (s, ev) => OnToolbarClick(tag);

                    pnlBtn.MouseEnter += enter; lblIcon.MouseEnter += enter; lblText.MouseEnter += enter;
                    pnlBtn.MouseLeave += leave; lblIcon.MouseLeave += leave; lblText.MouseLeave += leave;
                    pnlBtn.Click += click; lblIcon.Click += click; lblText.Click += click;

                    pnlBtn.Controls.Add(lblIcon);
                    pnlBtn.Controls.Add(lblText);
                    pnlToolbarContainer.Controls.Add(pnlBtn);
                    x += 60;
                }

                var lblGroup = new Label
                {
                    Text = group.Header,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 150),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(groupStartX, 62),
                    Size = new Size(x - groupStartX, 10),
                };
                pnlToolbarContainer.Controls.Add(lblGroup);

                if (group != groups[groups.Length - 1])
                {
                    pnlToolbarContainer.Controls.Add(new Panel
                    {
                        BackColor = Color.FromArgb(220, 225, 235),
                        Location = new Point(x + 2, 8),
                        Size = new Size(1, 54),
                    });
                    x += 10;
                }
            }

            pnlToolbarContainer.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(220, 225, 235),
                Dock = DockStyle.Bottom,
                Height = 1,
            });
        }

        private void OnToolbarClick(string tag)
        {
            switch (tag)
            {
                case "new":
                    InitializeFormData();
                    break;
                case "history":
                    dgvDelegations.Focus();
                    break;
                default:
                    MessageBox.Show(string.Format("ฟังก์ชัน '{0}' อยู่ระหว่างพัฒนา", tag),
                        "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void OnNavItemClick(string tag)
        {
            if (tag == "delegation")
            {
                return;
            }

            MessageBox.Show(string.Format("หน้า '{0}' อยู่ระหว่างพัฒนา", tag),
                "ข้อมูล", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("บันทึกการมอบหมายสำเร็จ", "สำเร็จ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadHistoryRows();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            InitializeFormData();
        }

        private void dgvDelegations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colStatus.Index || e.Value == null)
            {
                return;
            }

            var status = Convert.ToString(e.Value);
            if (status == "กำลังใช้งาน")
            {
                e.CellStyle.ForeColor = Color.FromArgb(23, 133, 49);
            }
            else if (status == "กำหนดไว้")
            {
                e.CellStyle.ForeColor = Color.FromArgb(197, 120, 16);
            }
            else
            {
                e.CellStyle.ForeColor = Color.FromArgb(107, 114, 128);
            }
            e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        }
    }
}
