using CMS.Controllers.KMS.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;

namespace CMS.Controllers.KMS.KB
{
    public class KBDashboardPresenter
    {
        private readonly IKBDashboardView _view;
        private readonly IKBDashboardService _dashboardService;
        private readonly UserContext _user;

        public KBDashboardPresenter(
            IKBDashboardView view,
            IKBDashboardService dashboardService,
            UserContext user)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (dashboardService == null) throw new ArgumentNullException("dashboardService");
            if (user == null) throw new ArgumentNullException("user");
            
            _view = view;
            _dashboardService = dashboardService;
            _user = user;
        }

        public void Initialize()
        {
            LoadDashboard();
        }

        public void LoadDashboard()
        {
            try
            {
                _view.SetStatus("กำลังโหลดข้อมูล Dashboard...");
                
                // Load stats
                var stats = _dashboardService.GetDashboardStats(_user);
                _view.DisplayStats(stats);
                
                // Load monthly chart (last 6 months)
                var monthlyData = _dashboardService.GetMonthlyArticles(_user, 6);
                _view.DisplayMonthlyChart(monthlyData);
                
                // Load top 5 articles
                var topArticles = _dashboardService.GetTopArticles(_user, 5);
                _view.DisplayTopArticles(topArticles);
                
                _view.SetStatus(string.Format("อัพเดตล่าสุด: {0:dd/MM/yyyy HH:mm}", DateTime.Now));
            }
            catch (Exception ex)
            {
                _view.ShowError("เกิดข้อผิดพลาดในการโหลดข้อมูล Dashboard: " + ex.Message);
                _view.SetStatus("โหลดข้อมูลล้มเหลว");
            }
        }

        public void RefreshDashboard()
        {
            LoadDashboard();
        }

        public void ExportToExcel()
        {
            _view.ShowInfo("ฟีเจอร์ Export Excel อยู่ระหว่างพัฒนา");
        }

        public void PrintReport()
        {
            _view.ShowInfo("ฟีเจอร์พิมพ์รายงานอยู่ระหว่างพัฒนา");
        }

        public void SendReport()
        {
            _view.ShowInfo("ฟีเจอร์ส่งรายงานทาง Email อยู่ระหว่างพัฒนา");
        }
    }
}
