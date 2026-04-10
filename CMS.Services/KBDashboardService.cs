using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace CMS.Services
{
    public class KBDashboardService : IKBDashboardService
    {
        private readonly IKBDashboardRepository _dashboardRepo;
        private readonly ILoggingService _logger;

        public KBDashboardService(
            IKBDashboardRepository dashboardRepo,
            ILoggingService logger)
        {
            if (dashboardRepo == null) throw new ArgumentNullException("dashboardRepo");
            if (logger == null) throw new ArgumentNullException("logger");
            _dashboardRepo = dashboardRepo;
            _logger = logger;
        }

        public KBDashboardStats GetDashboardStats(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            try
            {
                _logger.Info(string.Format("GetDashboardStats by {0}", user.AdIdentity));
                return _dashboardRepo.GetDashboardStats();
            }
            catch (Exception ex)
            {
                _logger.Error("GetDashboardStats failed", ex);
                throw;
            }
        }

        public IEnumerable<KBMonthlyArticle> GetMonthlyArticles(UserContext user, int months)
        {
            if (user == null) throw new ArgumentNullException("user");
            try
            {
                _logger.Info(string.Format("GetMonthlyArticles by {0}, months={1}", user.AdIdentity, months));
                return _dashboardRepo.GetMonthlyArticles(months);
            }
            catch (Exception ex)
            {
                _logger.Error("GetMonthlyArticles failed", ex);
                throw;
            }
        }

        public IEnumerable<KBTopArticle> GetTopArticles(UserContext user, int topN)
        {
            if (user == null) throw new ArgumentNullException("user");
            try
            {
                _logger.Info(string.Format("GetTopArticles by {0}, topN={1}", user.AdIdentity, topN));
                return _dashboardRepo.GetTopArticles(topN);
            }
            catch (Exception ex)
            {
                _logger.Error("GetTopArticles failed", ex);
                throw;
            }
        }
    }
}
