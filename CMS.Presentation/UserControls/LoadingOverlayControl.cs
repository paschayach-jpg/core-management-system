using System.Windows.Forms;

namespace CMS.Presentation.UserControls
{
    public partial class LoadingOverlayControl : UserControl
    {
        public LoadingOverlayControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Makes the overlay visible and flushes the paint queue so it renders
        /// before the caller's synchronous work begins.
        /// </summary>
        public void ShowOverlay()
        {
            Visible = true;
            BringToFront();
            Application.DoEvents();
        }

        public void HideOverlay()
        {
            Visible = false;
        }
    }
}
