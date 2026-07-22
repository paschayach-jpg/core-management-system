using RG_Project.Login;
using RG_Project.Project.Forms;
using System;
using System.Windows.Forms;

namespace RG_Project
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool loggedIn = false;
                var login = new LoginFrmNew();
                login.LoginSuccess += () =>
                {
                    loggedIn = true;
                };
                login.ShowDialog();

                if (loggedIn)
                {
                    try
                    {
                        Application.Run(new Frm_Project_List());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "เกิดข้อผิดพลาดในการโหลดหน้าโครงการ: " + ex.Message,
                            "ข้อผิดพลาด",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "เกิดข้อผิดพลาดวิกฤติ: " + ex.Message,
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
