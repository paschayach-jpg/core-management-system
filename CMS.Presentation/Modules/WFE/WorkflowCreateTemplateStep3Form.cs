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

        public WorkflowCreateTemplateStep3Form(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService,
            UserContext userContext,
            WorkflowWizardState wizardState)
        {
            InitializeComponent();
            _templateService = templateService;
            _loggingService = loggingService;
            _userContext = userContext;
            _wizardState = wizardState;

            this.Load += WorkflowCreateTemplateStep3Form_Load;
            this.Closing += (s, e) =>
            {
                if (this.DialogResult == DialogResult.None)
                    this.DialogResult = DialogResult.Cancel;
            };
        }

        private void WorkflowCreateTemplateStep3Form_Load(object sender, EventArgs e)
        {
            btnPrev.Click += (s, ev) => { this.DialogResult = DialogResult.Retry; this.Close(); };
            btnNext.Click += (s, ev) => OnNextClick();
            btnSaveDraft.Click += (s, ev) => OnSaveDraftClick();
            btnApplyProps.Click += (s, ev) => OnApplyPropsClick();

            InitializePalette();
            InitializeFormData();
            RenderSections();
        }

        private void InitializePalette()
        {
            var basicFields = new[] { "Text", "Textarea", "Number", "Date", "DateTime", "Dropdown" };
            var selectFields = new[] { "Radio", "Checkbox", "Toggle", "Multi-select" };
            var advancedFields = new[] { "File Upload", "User Picker", "Entity Link", "Rich Text" };
            var layoutFields = new[] { "Section", "Divider", "Info Box", "2 Columns" };

            ConfigureFlowPanel(flowPaletteBasic);
            ConfigureFlowPanel(flowPaletteSelect);
            ConfigureFlowPanel(flowPaletteAdvanced);
            ConfigureFlowPanel(flowPaletteLayout);

            foreach (var field in basicFields)
                AddPaletteButton(flowPaletteBasic, field);

            foreach (var field in selectFields)
                AddPaletteButton(flowPaletteSelect, field);

            foreach (var field in advancedFields)
                AddPaletteButton(flowPaletteAdvanced, field);

            foreach (var field in layoutFields)
                AddPaletteButton(flowPaletteLayout, field);
        }

        private void ConfigureFlowPanel(FlowLayoutPanel panel)
        {
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = true;
            panel.AutoSize = false;
        }

        private void AddPaletteButton(FlowLayoutPanel container, string fieldTypeName)
        {
            var btn = new Button
            {
                Text = GetFieldTypeIcon(fieldTypeName) + "\n" + fieldTypeName,
                Width = 75,
                Height = 65,
                Margin = new Padding(3, 3, 3, 3),
                Tag = fieldTypeName,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(31, 56, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 210, 220);
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(240, 245, 250);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.White;

            btn.Click += (s, e) => OnPaletteButtonClick(fieldTypeName);
            container.Controls.Add(btn);
        }

        private void OnPaletteButtonClick(string fieldTypeName)
        {
            if (fieldTypeName == "Section")
            {
                AddNewSection();
            }
            else
            {
                AddNewField(fieldTypeName);
            }
        }

        private void AddNewSection()
        {
            int sectionIndex = _wizardState.FormSections.Count + 1;
            var section = new FormSectionDefinition
            {
                Title = $"{sectionIndex}. ส่วน {sectionIndex}",
                Order = sectionIndex
            };
            _wizardState.FormSections.Add(section);
            RenderSections();
        }

        private void AddNewField(string fieldTypeName)
        {
            if (_wizardState.FormSections.Count == 0)
            {
                MessageBox.Show("กรุณาสร้าง Section ก่อน", "ข้อมูลไม่สมบูรณ์");
                return;
            }

            var fieldType = GetFieldTypeFromName(fieldTypeName);
            var lastSection = _wizardState.FormSections.Last();

            var field = new FormFieldDefinition(
                fieldType,
                $"Field {_wizardState.FormFields.Count + 1}",
                $"field_{Guid.NewGuid().ToString().Substring(0, 8)}"
            )
            {
                SectionId = lastSection.Id,
                Order = _wizardState.FormFields.Count(f => f.SectionId == lastSection.Id) + 1
            };

            _wizardState.FormFields.Add(field);
            RenderSections();
        }

        private void InitializeFormData()
        {
            txtFormTitle.Text = _wizardState.FormTitle ?? "ชื่อฟอร์ม";

            // If no sections exist, create default 3 sections with fields
            if (_wizardState.FormSections.Count == 0)
            {
                CreateDefaultSections();
            }
        }

        private void CreateDefaultSections()
        {
            // Section 1: ข้อมูลทั่วไป
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

            // Section 2: ไฟล์แนบ
            var sec2 = new FormSectionDefinition("2. ไฟล์แนบ", 2);
            _wizardState.FormSections.Add(sec2);
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.FileUpload, "ไฟล์หลัก (PDF/DOCX)", "main_file", required: true)
            { SectionId = sec2.Id, Order = 1 });

            // Section 3: ผู้เกี่ยวข้อง
            var sec3 = new FormSectionDefinition("3. ผู้เกี่ยวข้อง", 3);
            _wizardState.FormSections.Add(sec3);
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.UserPicker, "ผู้ตรวจสอบเพิ่มเติม (ถ้ามี)", "additional_reviewer")
            { SectionId = sec3.Id, Order = 1 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.EntityLink, "เชื่อม KB Article", "kb_article")
            { SectionId = sec3.Id, Order = 2 });
            _wizardState.FormFields.Add(new FormFieldDefinition(FormFieldType.Checkbox, "รับทราบว่าเอกสารนี้สอดคล้องกับ PDPA 2562", "pdpa_acknowledgement")
            { SectionId = sec3.Id, Order = 3 });
        }

        private void RenderSections()
        {
            pnlSectionsContainer.Controls.Clear();
            int yPos = 10;

            foreach (var section in _wizardState.FormSections.OrderBy(s => s.Order))
            {
                yPos += RenderSection(section, yPos);
            }
        }

        private int RenderSection(FormSectionDefinition section, int yPos)
        {
            var sectionPanel = new Panel
            {
                Location = new Point(5, yPos),
                Width = 690,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(0, 15, 0, 10)
            };

            // Section header
            var headerLabel = new Label
            {
                Text = section.Title,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 58, 138),
                Location = new Point(0, 0),
                Width = 690,
                Height = 38,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            sectionPanel.Controls.Add(headerLabel);

            int fieldYPos = 43;
            var fieldsInSection = _wizardState.FormFields
                .Where(f => f.SectionId == section.Id)
                .OrderBy(f => f.Order)
                .ToList();

            foreach (var field in fieldsInSection)
            {
                fieldYPos += RenderFieldRow(sectionPanel, field, fieldYPos);
            }

            // Add field button
            var addBtn = new Button
            {
                Text = "+ Add Field",
                Location = new Point(15, fieldYPos),
                Width = 110,
                Height = 30,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(30, 58, 138),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            addBtn.FlatAppearance.BorderSize = 1;
            addBtn.FlatAppearance.BorderColor = Color.FromArgb(148, 163, 184);
            addBtn.MouseEnter += (s, e) => addBtn.BackColor = Color.FromArgb(203, 213, 225);
            addBtn.MouseLeave += (s, e) => addBtn.BackColor = Color.FromArgb(226, 232, 240);
            addBtn.Click += (s, e) => { /* Handle add field to this section */ };
            sectionPanel.Controls.Add(addBtn);

            fieldYPos += 38;
            sectionPanel.Height = fieldYPos;
            pnlSectionsContainer.Controls.Add(sectionPanel);

            return fieldYPos + 20;
        }

        private int RenderFieldRow(Panel sectionPanel, FormFieldDefinition field, int yPos)
        {
            var rowPanel = new Panel
            {
                Location = new Point(10, yPos),
                Width = 660,
                Height = FIELD_ROW_HEIGHT,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(0, 1, 0, 1),
                Cursor = Cursors.Hand,
                Tag = field
            };

            // Icon + Label
            var iconLabel = new Label
            {
                Text = GetFieldTypeIcon(field.FieldType) + " " + field.Label,
                Location = new Point(10, 10),
                Width = 540,
                Height = 20,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(31, 56, 100),
                AutoEllipsis = true
            };
            rowPanel.Controls.Add(iconLabel);

            // Required indicator
            if (field.Required)
            {
                var reqLabel = new Label
                {
                    Text = "*",
                    Location = new Point(560, 10),
                    Width = 20,
                    Height = 20,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 38, 38)
                };
                rowPanel.Controls.Add(reqLabel);
            }

            // Delete button
            var delBtn = new Button
            {
                Text = "×",
                Location = new Point(605, 8),
                Width = 45,
                Height = 24,
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Tag = field,
                FlatAppearance = { BorderSize = 0 }
            };
            delBtn.MouseEnter += (s, e) => delBtn.BackColor = Color.FromArgb(220, 38, 38);
            delBtn.MouseLeave += (s, e) => delBtn.BackColor = Color.FromArgb(239, 68, 68);
            delBtn.Click += (s, e) => OnDeleteFieldClick(field);
            rowPanel.Controls.Add(delBtn);

            // Hover effects
            rowPanel.MouseEnter += (s, e) => rowPanel.BackColor = Color.FromArgb(249, 250, 251);
            rowPanel.MouseLeave += (s, e) =>
            {
                if (rowPanel.Tag == _selectedField)
                    rowPanel.BackColor = Color.FromArgb(239, 246, 255);
                else
                    rowPanel.BackColor = Color.White;
            };

            rowPanel.Click += (s, e) => OnFieldRowClick(field, rowPanel);
            foreach (Control ctrl in rowPanel.Controls)
            {
                ctrl.Click += (s, e) => OnFieldRowClick(field, rowPanel);
            }

            sectionPanel.Controls.Add(rowPanel);
            return FIELD_ROW_HEIGHT + 3;
        }

        private void OnFieldRowClick(FormFieldDefinition field, Panel rowPanel)
        {
            _selectedField = field;
            ShowFieldProperties(field);

            // Update visual selection
            foreach (Control ctrl in pnlSectionsContainer.Controls)
            {
                if (ctrl is Panel sectionPanel)
                {
                    foreach (Control fieldRow in sectionPanel.Controls)
                    {
                        if (fieldRow is Panel fp)
                        {
                            if (fp.Tag == field)
                            {
                                fp.BackColor = Color.FromArgb(226, 239, 255);
                                fp.BorderStyle = BorderStyle.FixedSingle;
                            }
                            else
                            {
                                fp.BackColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void OnDeleteFieldClick(FormFieldDefinition field)
        {
            if (MessageBox.Show("ลบฟิลด์นี้?", "ยืนยัน", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _wizardState.FormFields.Remove(field);
                _selectedField = null;
                RenderSections();
                lblNoSelection.Visible = true;
                pnlFieldProps.Visible = false;
            }
        }

        private void ShowFieldProperties(FormFieldDefinition field)
        {
            lblNoSelection.Visible = false;
            pnlFieldProps.Visible = true;

            txtPropLabel.Text = field.Label;
            txtPropFieldName.Text = field.FieldName;
            txtPropHelpText.Text = field.HelpText ?? "";
            chkRequired.Checked = field.Required;
            chkVisible.Checked = field.Visible;
            cmbPropSource.Text = field.Source ?? "";
            txtPropDefault.Text = field.DefaultValue ?? "";
            txtPropConditional.Text = field.ConditionalLogic ?? "";
            txtPropMapContext.Text = field.MapToContext ?? "";
        }

        private void OnApplyPropsClick()
        {
            if (_selectedField == null) return;

            _selectedField.Label = txtPropLabel.Text;
            _selectedField.FieldName = txtPropFieldName.Text;
            _selectedField.HelpText = txtPropHelpText.Text;
            _selectedField.Required = chkRequired.Checked;
            _selectedField.Visible = chkVisible.Checked;
            _selectedField.Source = cmbPropSource.Text;
            _selectedField.DefaultValue = txtPropDefault.Text;
            _selectedField.ConditionalLogic = txtPropConditional.Text;
            _selectedField.MapToContext = txtPropMapContext.Text;

            MessageBox.Show("บันทึกเรียบร้อย", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RenderSections();
        }

        private void OnSaveDraftClick()
        {
            _wizardState.FormTitle = txtFormTitle.Text;
            MessageBox.Show("บันทึก Draft แล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OnNextClick()
        {
            if (string.IsNullOrWhiteSpace(txtFormTitle.Text))
            {
                MessageBox.Show("กรุณากรอกชื่อฟอร์ม", "ข้อมูลไม่สมบูรณ์");
                return;
            }

            if (_wizardState.FormFields.Count == 0)
            {
                MessageBox.Show("กรุณาเพิ่มฟิลด์อย่างน้อย 1 ฟิลด์", "ข้อมูลไม่สมบูรณ์");
                return;
            }

            _wizardState.FormTitle = txtFormTitle.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string GetFieldTypeIcon(string fieldTypeName)
        {
            switch (fieldTypeName)
            {
                case "Text": return "T";
                case "Textarea": return "¶";
                case "Number": return "#";
                case "Date": return "📅";
                case "DateTime": return "📅";
                case "Dropdown": return "▾";
                case "Radio": return "◉";
                case "Checkbox": return "☑";
                case "Toggle": return "⊙";
                case "Multi-select": return "☑☑";
                case "File Upload": return "📎";
                case "User Picker": return "👤";
                case "Entity Link": return "🔗";
                case "Rich Text": return "📝";
                case "Section": return "≡";
                case "Divider": return "─";
                case "Info Box": return "ⓘ";
                case "2 Columns": return "⊞";
                default: return "•";
            }
        }

        private string GetFieldTypeIcon(FormFieldType fieldType)
        {
            switch (fieldType)
            {
                case FormFieldType.Text: return "T";
                case FormFieldType.Textarea: return "¶";
                case FormFieldType.Number: return "#";
                case FormFieldType.Date: return "📅";
                case FormFieldType.DateTime: return "📅";
                case FormFieldType.Dropdown: return "▾";
                case FormFieldType.Radio: return "◉";
                case FormFieldType.Checkbox: return "☑";
                case FormFieldType.Toggle: return "⊙";
                case FormFieldType.MultiSelect: return "☑☑";
                case FormFieldType.FileUpload: return "📎";
                case FormFieldType.UserPicker: return "👤";
                case FormFieldType.EntityLink: return "🔗";
                case FormFieldType.RichText: return "📝";
                case FormFieldType.Section: return "≡";
                case FormFieldType.Divider: return "─";
                case FormFieldType.InfoBox: return "ⓘ";
                case FormFieldType.TwoColumns: return "⊞";
                default: return "•";
            }
        }

        private FormFieldType GetFieldTypeFromName(string fieldTypeName)
        {
            switch (fieldTypeName)
            {
                case "Text": return FormFieldType.Text;
                case "Textarea": return FormFieldType.Textarea;
                case "Number": return FormFieldType.Number;
                case "Date": return FormFieldType.Date;
                case "DateTime": return FormFieldType.DateTime;
                case "Dropdown": return FormFieldType.Dropdown;
                case "Radio": return FormFieldType.Radio;
                case "Checkbox": return FormFieldType.Checkbox;
                case "Toggle": return FormFieldType.Toggle;
                case "Multi-select": return FormFieldType.MultiSelect;
                case "File Upload": return FormFieldType.FileUpload;
                case "User Picker": return FormFieldType.UserPicker;
                case "Entity Link": return FormFieldType.EntityLink;
                case "Rich Text": return FormFieldType.RichText;
                case "Section": return FormFieldType.Section;
                case "Divider": return FormFieldType.Divider;
                case "Info Box": return FormFieldType.InfoBox;
                case "2 Columns": return FormFieldType.TwoColumns;
                default: return FormFieldType.Text;
            }
        }
    }
}
