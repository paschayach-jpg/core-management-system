using System;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow
{
    /// <summary>
    /// ทดสอบง่ายๆ - เปิด WorkflowCreateTemplateForm
    /// </summary>
    public partial class WorkflowCreateTemplateFormTest : Form
    {
        private readonly IWorkflowTemplateService _templateService;
        private readonly ILoggingService _loggingService;

        public WorkflowCreateTemplateFormTest(
            IWorkflowTemplateService templateService,
            ILoggingService loggingService)
        {
            InitializeComponent();
            _templateService = templateService;
            _loggingService = loggingService;
        }

        private void WorkflowCreateTemplateFormTest_Load(object sender, EventArgs e)
        {
            this.Text = "Test - Workflow Template Form";
            this.Width = 900;
            this.Height = 200;
            this.StartPosition = FormStartPosition.CenterScreen;

            try
            {
                var userContext = new UserContext
                {
                    UserID = 1,
                    ADUserName = "test.user",
                    FullName = "Test User",
                    EmailAddress = "test@example.com"
                };

                var wizardState = new WorkflowWizardState();

                _loggingService?.Info("=== Test Form: Creating WorkflowCreateTemplateForm ===");

                var form = new WorkflowCreateTemplateForm(
                    _templateService,
                    _loggingService,
                    userContext,
                    wizardState);

                _loggingService?.Info($"Test Form: Form created, Width={form.Width}, Height={form.Height}");
                _loggingService?.Info($"Test Form: Form Controls count = {form.Controls.Count}");

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _loggingService?.Info("Test Form: Form returned OK");
                }
                else
                {
                    _loggingService?.Info("Test Form: Form was cancelled");
                }
            }
            catch (Exception ex)
            {
                _loggingService?.Error($"Test Form Exception: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", ex);
                MessageBox.Show(
                    $"ERROR:\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}
