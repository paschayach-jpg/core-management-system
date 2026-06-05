using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowCreateTemplateStep3Form : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;
        private readonly UserContext _userContext;
        private readonly WorkflowWizardState _wizardState;

        private FormFieldDefinition _selectedField = null;
        private const int FIELD_ROW_HEIGHT = 40;
        private const int FILE_ROW_HEIGHT  = 56; // taller row for FileUpload to show preview hint

        public WorkflowCreateTemplateStep3Form(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            WorkflowWizardState wizardState)
        {
            InitializeComponent();
            _templateService = templateService;
            _loggingService  = loggingService;
            _userContext     = userContext;
            _wizardState     = wizardState;

            this.Load    += WorkflowCreateTemplateStep3Form_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateStep3Form_Load(object sender, EventArgs e)
        {
            // ── 1. Fix button bar first (Anchor+fixed-position breaks inside Dock=Bottom) ──
            SetupButtonBar();

            // ── 2. Wire events ──
            btnPrev.Click       += (s, ev) => { this.DialogResult = DialogResult.Retry; this.Close(); };
            btnNext.Click       += (s, ev) => OnNextClick();
            btnSaveDraft.Click  += (s, ev) => OnSaveDraftClick();
            btnApplyProps.Click += (s, ev) => OnApplyPropsClick();

            var tip = new ToolTip { InitialDelay = 400 };
            tip.SetToolTip(btnNext, "ขั้นตอนที่ 4: ส่งเสนอขออนุมัติ และ Publish เทมเพลตให้ใช้งาน");

            // ── 3. Canvas responsive ──
            pnlCanvas.Resize += (s, ev) => AdjustCanvasWidths();

            // ── 4. Clarify designer mode in hint banner ──
            lblCanvasHint.Text =
                "  ✏  โหมดออกแบบเทมเพลต — กำลังกำหนดโครงสร้างฟอร์ม (ไม่ใช่กรอกฟอร์มจริง)" +
                "   |   คลิก Field จากแผงซ้ายเพื่อเพิ่ม   |   คลิก Field ในฟอร์มเพื่อแก้ไขคุณสมบัติ   |   × เพื่อลบ";

            // ── 5. Build palette + data ──
            InitializePalette();
            InitializeFormData();
            RenderSections();
            AdjustCanvasWidths();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  BUTTON BAR — rebuild with FlowLayoutPanel DockRight (always visible)
        // ═══════════════════════════════════════════════════════════════════════

        private void SetupButtonBar()
        {
            // Remove old buttons from pnlButtons (they had broken Anchor positions)
            pnlButtons.Controls.Remove(btnPrev);
            pnlButtons.Controls.Remove(btnSaveDraft);
            pnlButtons.Controls.Remove(btnNext);

            // Right-docked flow: always sticks to right edge regardless of window width
            var rightFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Width         = 340,
                Padding       = new Padding(0, 9, 8, 0),
                BackColor     = pnlButtons.BackColor
            };

            // Reset control geometry for flow layout
            foreach (var btn in new[] { btnPrev, btnSaveDraft, btnNext })
            {
                btn.Anchor   = AnchorStyles.None;
                btn.Location = new Point(0, 0);
            }
            btnPrev.Margin     = new Padding(0, 0, 8, 0);
            btnSaveDraft.Margin = new Padding(0, 0, 8, 0);
            btnNext.Margin     = new Padding(0, 0, 0, 0);

            rightFlow.Controls.Add(btnPrev);
            rightFlow.Controls.Add(btnSaveDraft);
            rightFlow.Controls.Add(btnNext);

            pnlButtons.Controls.Add(rightFlow);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  PALETTE — build directly in pnlPaletteScroll (absolute y + AutoScroll)
        //  FlowPanels จาก Designer ถูกทิ้งไว้ (ไม่ visible) — เราสร้างใหม่ในโค้ด
        // ═══════════════════════════════════════════════════════════════════════

        private void InitializePalette()
        {
            // Clear everything from pnlPaletteScroll (including abandoned FlowPanels from Designer)
            pnlPaletteScroll.Controls.Clear();

            var categories = new[]
            {
                new { Title = "พื้นฐาน",  Items = new[] { "Text","Textarea","Number","Date","DateTime","Dropdown" } },
                new { Title = "เลือก",    Items = new[] { "Radio","Checkbox","Toggle","Multi-select" } },
                new { Title = "ขั้นสูง",  Items = new[] { "File Upload","User Picker","Entity Link","Rich Text" } },
                new { Title = "เค้าโครง", Items = new[] { "Section","Divider","Info Box","2 Columns" } },
            };

            int y = 4;
            foreach (var cat in categories)
            {
                // Section header label
                var header = new Label
                {
                    Text      = cat.Title,
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    BackColor = Color.FromArgb(248, 250, 252),
                    Location  = new Point(0, y),
                    Size      = new Size(174, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(4, 0, 0, 0)
                };
                pnlPaletteScroll.Controls.Add(header);
                y += 20;

                foreach (var fieldName in cat.Items)
                {
                    pnlPaletteScroll.Controls.Add(BuildPaletteItem(fieldName, y));
                    y += 32;
                }
                y += 6; // gap between categories
            }
            // AutoScroll reads max extent of children — no extra setup needed
        }

        private Button BuildPaletteItem(string fieldTypeName, int y)
        {
            var btn = new Button
            {
                Text      = GetFieldTypeIcon(fieldTypeName) + "  " + fieldTypeName,
                Location  = new Point(4, y),
                Size      = new Size(158, 30),
                Tag       = fieldTypeName,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 0, 0, 0),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(239, 246, 255); btn.ForeColor = Color.FromArgb(37, 99, 235); };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.White; btn.ForeColor = Color.FromArgb(51, 65, 85); };
            btn.Click      += (s, e) => OnPaletteButtonClick(fieldTypeName);
            return btn;
        }

        private void OnPaletteButtonClick(string fieldTypeName)
        {
            if (fieldTypeName == "Section")
                AddNewSection();
            else
                AddNewField(fieldTypeName);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  CANVAS — responsive width
        // ═══════════════════════════════════════════════════════════════════════

        private void AdjustCanvasWidths()
        {
            int w = Math.Max(pnlCanvas.ClientSize.Width - 40, 400);
            lblCanvasHint.Size         = new Size(w, 28);
            txtFormTitle.Size          = new Size(w, 36);
            pnlSectionsContainer.Width = w;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SECTION / FIELD management
        // ═══════════════════════════════════════════════════════════════════════

        private void AddNewSection()
        {
            int idx = _wizardState.FormSections.Count + 1;
            _wizardState.FormSections.Add(new FormSectionDefinition
            {
                Title = $"{idx}. ส่วน {idx}",
                Order = idx
            });
            RenderSections();
        }

        private void AddNewField(string fieldTypeName)
        {
            if (_wizardState.FormSections.Count == 0)
            {
                MessageBox.Show(
                    "กรุณาสร้าง Section ก่อน\n(คลิก 'Section' ในแผงซ้ายมือ แล้วกลับมาคลิก Field อีกครั้ง)",
                    "ข้อมูลไม่สมบูรณ์", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var lastSection = _wizardState.FormSections.Last();
            _wizardState.FormFields.Add(new FormFieldDefinition(
                GetFieldTypeFromName(fieldTypeName),
                $"Field {_wizardState.FormFields.Count + 1}",
                $"field_{Guid.NewGuid().ToString().Substring(0, 8)}")
            {
                SectionId = lastSection.Id,
                Order     = _wizardState.FormFields.Count(f => f.SectionId == lastSection.Id) + 1
            });
            RenderSections();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  INIT DATA
        // ═══════════════════════════════════════════════════════════════════════

        private void InitializeFormData()
        {
            txtFormTitle.Text = _wizardState.FormTitle ?? "ชื่อเทมเพลตฟอร์ม";
            if (_wizardState.FormSections.Count == 0)
                CreateDefaultSections();
        }

        private void CreateDefaultSections()
        {
            var sec1 = new FormSectionDefinition("1. ข้อมูลทั่วไป", 1);
            _wizardState.FormSections.Add(sec1);
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Text, "หัวข้อเอกสาร", "title", required: true)
                { SectionId = sec1.Id, Order = 1 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Dropdown, "CRAF Classification", "craf_classification", required: true)
                { SectionId = sec1.Id, Order = 2 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Dropdown, "ระดับความสำคัญ", "priority")
                { SectionId = sec1.Id, Order = 3 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Textarea, "รายละเอียด", "description")
                { SectionId = sec1.Id, Order = 4 });

            var sec2 = new FormSectionDefinition("2. ไฟล์แนบ", 2);
            _wizardState.FormSections.Add(sec2);
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.FileUpload, "ไฟล์หลัก (PDF/DOCX)", "main_file", required: true)
                { SectionId = sec2.Id, Order = 1 });

            var sec3 = new FormSectionDefinition("3. ผู้เกี่ยวข้อง", 3);
            _wizardState.FormSections.Add(sec3);
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.UserPicker, "ผู้ตรวจสอบเพิ่มเติม (ถ้ามี)", "additional_reviewer")
                { SectionId = sec3.Id, Order = 1 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.EntityLink, "เชื่อม KB Article", "kb_article")
                { SectionId = sec3.Id, Order = 2 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Checkbox, "รับทราบว่าเอกสารนี้สอดคล้องกับ PDPA 2562", "pdpa_acknowledgement")
                { SectionId = sec3.Id, Order = 3 });
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  RENDER
        // ═══════════════════════════════════════════════════════════════════════

        private void RenderSections()
        {
            pnlSectionsContainer.Controls.Clear();
            int yPos = 10;

            foreach (var section in _wizardState.FormSections.OrderBy(s => s.Order))
                yPos += RenderSection(section, yPos);

            int total = _wizardState.FormFields.Count;
            lblFieldCount.Text = total == 0
                ? "ยังไม่มี Field — คลิก Field จากแผงซ้ายเพื่อเพิ่ม"
                : $"ฟิลด์ทั้งหมด {total} รายการ ใน {_wizardState.FormSections.Count} Section";
        }

        private int RenderSection(FormSectionDefinition section, int yPos)
        {
            int sw = Math.Max(pnlSectionsContainer.Width - 10, 400);

            var sectionPanel = new Panel
            {
                Location    = new Point(5, yPos),
                Width       = sw,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White
            };

            var header = new Label
            {
                Text      = section.Title,
                Font      = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 58, 138),
                Location  = new Point(0, 0),
                Width     = sw,
                Height    = 38,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(15, 0, 0, 0)
            };
            sectionPanel.Controls.Add(header);

            int fy = 43;
            foreach (var field in _wizardState.FormFields.Where(f => f.SectionId == section.Id).OrderBy(f => f.Order))
                fy += RenderFieldRow(sectionPanel, field, fy, sw);

            // "Add field to this section" quick-add button
            var addBtn = new Button
            {
                Text      = "+ เพิ่ม Field ใน Section นี้",
                Location  = new Point(15, fy),
                Size      = new Size(170, 28),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(30, 58, 138),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F),
                Cursor    = Cursors.Hand
            };
            addBtn.FlatAppearance.BorderSize  = 1;
            addBtn.FlatAppearance.BorderColor = Color.FromArgb(148, 163, 184);
            addBtn.MouseEnter += (s, e) => addBtn.BackColor = Color.FromArgb(203, 213, 225);
            addBtn.MouseLeave += (s, e) => addBtn.BackColor = Color.FromArgb(226, 232, 240);
            addBtn.Click += (s, e) =>
            {
                _wizardState.FormFields.Add(new FormFieldDefinition(
                    FormFieldType.Text,
                    $"Field {_wizardState.FormFields.Count + 1}",
                    $"field_{Guid.NewGuid().ToString().Substring(0, 8)}")
                {
                    SectionId = section.Id,
                    Order     = _wizardState.FormFields.Count(f => f.SectionId == section.Id) + 1
                });
                RenderSections();
            };
            sectionPanel.Controls.Add(addBtn);

            fy += 36;
            sectionPanel.Height = fy;
            pnlSectionsContainer.Controls.Add(sectionPanel);
            return fy + 16;
        }

        private int RenderFieldRow(Panel sectionPanel, FormFieldDefinition field, int yPos, int sw)
        {
            bool isFileUpload = field.FieldType == FormFieldType.FileUpload;
            int  rowH         = isFileUpload ? FILE_ROW_HEIGHT : FIELD_ROW_HEIGHT;
            int  rw           = sw - 20;
            int  delX         = rw - 50;
            int  iconW        = rw - 80;

            var row = new Panel
            {
                Location    = new Point(10, yPos),
                Size        = new Size(rw, rowH),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White,
                Cursor      = Cursors.Hand,
                Tag         = field
            };

            // Main label row
            var iconLbl = new Label
            {
                Text         = GetFieldTypeIcon(field.FieldType) + "  " + field.Label,
                Location     = new Point(10, 10),
                Size         = new Size(iconW, 20),
                Font         = new Font("Segoe UI", 9.5F),
                ForeColor    = Color.FromArgb(31, 56, 100),
                AutoEllipsis = true
            };
            row.Controls.Add(iconLbl);

            // FileUpload: show a "preview" hint so users understand this is a field definition
            if (isFileUpload)
            {
                var previewLbl = new Label
                {
                    Text      = "    [ 📎 เลือกไฟล์… ]   ← ช่องนี้จะแสดงให้ผู้กรอกฟอร์มอัปโหลดไฟล์ที่นี่",
                    Location  = new Point(10, 28),
                    Size      = new Size(iconW + 30, 18),
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    BackColor = Color.Transparent
                };
                row.Controls.Add(previewLbl);
            }

            if (field.Required)
            {
                var reqLbl = new Label
                {
                    Text      = "*",
                    Location  = new Point(delX - 22, 8),
                    Size      = new Size(18, 22),
                    Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 38, 38)
                };
                row.Controls.Add(reqLbl);
            }

            var delBtn = new Button
            {
                Text      = "×",
                Location  = new Point(delX, 8),
                Size      = new Size(40, 24),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                Tag       = field
            };
            delBtn.FlatAppearance.BorderSize = 0;
            delBtn.MouseEnter += (s, e) => delBtn.BackColor = Color.FromArgb(220, 38, 38);
            delBtn.MouseLeave += (s, e) => delBtn.BackColor = Color.FromArgb(239, 68, 68);
            delBtn.Click      += (s, e) => OnDeleteFieldClick(field);
            row.Controls.Add(delBtn);

            row.MouseEnter += (s, e) => { if (row.Tag != (object)_selectedField) row.BackColor = Color.FromArgb(249, 250, 251); };
            row.MouseLeave += (s, e) => { if (row.Tag != (object)_selectedField) row.BackColor = Color.White; };
            row.Click      += (s, e) => OnFieldRowClick(field, row);
            foreach (Control c in row.Controls)
                c.Click += (s, e) => OnFieldRowClick(field, row);

            sectionPanel.Controls.Add(row);
            return rowH + 3;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SELECTION & PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════

        private void OnFieldRowClick(FormFieldDefinition field, Panel row)
        {
            _selectedField = field;
            ShowFieldProperties(field);

            foreach (Control sc in pnlSectionsContainer.Controls)
            {
                if (!(sc is Panel sectionPanel)) continue;
                foreach (Control child in sectionPanel.Controls)
                {
                    if (child is Panel fp && fp.Tag is FormFieldDefinition)
                        fp.BackColor = fp.Tag == field ? Color.FromArgb(226, 239, 255) : Color.White;
                }
            }
        }

        private void OnDeleteFieldClick(FormFieldDefinition field)
        {
            if (MessageBox.Show("ลบฟิลด์นี้ออกจากเทมเพลต?", "ยืนยัน",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _wizardState.FormFields.Remove(field);
                if (_selectedField == field)
                {
                    _selectedField         = null;
                    lblNoSelection.Visible = true;
                    pnlFieldProps.Visible  = false;
                }
                RenderSections();
            }
        }

        private void ShowFieldProperties(FormFieldDefinition field)
        {
            lblNoSelection.Visible = false;
            pnlFieldProps.Visible  = true;

            txtPropLabel.Text       = field.Label;
            txtPropFieldName.Text   = field.FieldName;
            txtPropHelpText.Text    = field.HelpText ?? "";
            chkRequired.Checked     = field.Required;
            chkVisible.Checked      = field.Visible;
            cmbPropSource.Text      = field.Source ?? "";
            txtPropDefault.Text     = field.DefaultValue ?? "";
            txtPropConditional.Text = field.ConditionalLogic ?? "";
            txtPropMapContext.Text  = field.MapToContext ?? "";
        }

        private void OnApplyPropsClick()
        {
            if (_selectedField == null) return;

            _selectedField.Label            = txtPropLabel.Text;
            _selectedField.FieldName        = txtPropFieldName.Text;
            _selectedField.HelpText         = txtPropHelpText.Text;
            _selectedField.Required         = chkRequired.Checked;
            _selectedField.Visible          = chkVisible.Checked;
            _selectedField.Source           = cmbPropSource.Text;
            _selectedField.DefaultValue     = txtPropDefault.Text;
            _selectedField.ConditionalLogic = txtPropConditional.Text;
            _selectedField.MapToContext     = txtPropMapContext.Text;

            RenderSections();
            MessageBox.Show("บันทึกเรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  NAVIGATION
        // ═══════════════════════════════════════════════════════════════════════

        private void OnSaveDraftClick()
        {
            _wizardState.FormTitle = txtFormTitle.Text;
            MessageBox.Show("บันทึก Draft เรียบร้อย\nสามารถกลับมาแก้ไขต่อได้ภายหลัง",
                "บันทึก Draft", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OnNextClick()
        {
            if (string.IsNullOrWhiteSpace(txtFormTitle.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อเทมเพลตฟอร์ม", "ข้อมูลไม่สมบูรณ์",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFormTitle.Focus();
                return;
            }

            if (_wizardState.FormFields.Count == 0)
            {
                MessageBox.Show("กรุณาเพิ่มฟิลด์อย่างน้อย 1 รายการ\n(คลิก Field จากแผงซ้ายมือ)",
                    "ข้อมูลไม่สมบูรณ์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _wizardState.FormTitle = txtFormTitle.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  ICON HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private string GetFieldTypeIcon(string n)
        {
            switch (n)
            {
                case "Text":         return "T";
                case "Textarea":     return "¶";
                case "Number":       return "#";
                case "Date":         return "📅";
                case "DateTime":     return "📅";
                case "Dropdown":     return "▾";
                case "Radio":        return "◉";
                case "Checkbox":     return "☑";
                case "Toggle":       return "⊙";
                case "Multi-select": return "☑☑";
                case "File Upload":  return "📎";
                case "User Picker":  return "👤";
                case "Entity Link":  return "🔗";
                case "Rich Text":    return "📝";
                case "Section":      return "≡";
                case "Divider":      return "─";
                case "Info Box":     return "ⓘ";
                case "2 Columns":    return "⊞";
                default:             return "•";
            }
        }

        private string GetFieldTypeIcon(FormFieldType t)
        {
            switch (t)
            {
                case FormFieldType.Text:        return "T";
                case FormFieldType.Textarea:    return "¶";
                case FormFieldType.Number:      return "#";
                case FormFieldType.Date:        return "📅";
                case FormFieldType.DateTime:    return "📅";
                case FormFieldType.Dropdown:    return "▾";
                case FormFieldType.Radio:       return "◉";
                case FormFieldType.Checkbox:    return "☑";
                case FormFieldType.Toggle:      return "⊙";
                case FormFieldType.MultiSelect: return "☑☑";
                case FormFieldType.FileUpload:  return "📎";
                case FormFieldType.UserPicker:  return "👤";
                case FormFieldType.EntityLink:  return "🔗";
                case FormFieldType.RichText:    return "📝";
                case FormFieldType.Section:     return "≡";
                case FormFieldType.Divider:     return "─";
                case FormFieldType.InfoBox:     return "ⓘ";
                case FormFieldType.TwoColumns:  return "⊞";
                default:                        return "•";
            }
        }

        private FormFieldType GetFieldTypeFromName(string n)
        {
            switch (n)
            {
                case "Text":         return FormFieldType.Text;
                case "Textarea":     return FormFieldType.Textarea;
                case "Number":       return FormFieldType.Number;
                case "Date":         return FormFieldType.Date;
                case "DateTime":     return FormFieldType.DateTime;
                case "Dropdown":     return FormFieldType.Dropdown;
                case "Radio":        return FormFieldType.Radio;
                case "Checkbox":     return FormFieldType.Checkbox;
                case "Toggle":       return FormFieldType.Toggle;
                case "Multi-select": return FormFieldType.MultiSelect;
                case "File Upload":  return FormFieldType.FileUpload;
                case "User Picker":  return FormFieldType.UserPicker;
                case "Entity Link":  return FormFieldType.EntityLink;
                case "Rich Text":    return FormFieldType.RichText;
                case "Section":      return FormFieldType.Section;
                case "Divider":      return FormFieldType.Divider;
                case "Info Box":     return FormFieldType.InfoBox;
                case "2 Columns":    return FormFieldType.TwoColumns;
                default:             return FormFieldType.Text;
            }
        }
    }
}
