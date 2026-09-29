using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    public class KBMainController
    {
        private readonly IKBMainController  _view;
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly UserContext        _user;

        private int _currentPage = 1;

        /// <summary>
        /// เมื่อไม่ใช่ null แปลว่ากำลังดูกลุ่ม cross-reference (มาตรา ↔ คำสั่ง/หมวดภายใน) ที่คลิกจากโหนด
        /// 🔗 ใน treeCategories — LoadArticlesAtPage จะดึงเฉพาะ ArticleID เหล่านี้แทนการ query ตาม
        /// CategoryID (ซึ่งกลุ่มนี้ไม่มี CategoryID เดียวที่ใช้ query ได้) ถูกเคลียร์เมื่อสลับไปโหมดอื่น
        /// </summary>
        private IList<int> _linkedArticleIds;

        // ── Cross-reference folders: กฎหมาย (Act ใด ๆ ใต้หมวด "กฎหมาย"/LW) ↔ คำสั่ง (OR) / หมวดภายใน (AN) ──
        // Resolved จาก CategoryCode สด ๆ ทุกครั้งที่โหลด (ดู ResolveCrossReferenceCategoryIds/IsLawCategory
        // ด้านล่าง) แทนที่จะ hardcode ตัวเลข CategoryID ตรง ๆ — ตัวเลข CategoryID เปลี่ยนได้ถ้าตาราง
        // m_KBCategory ถูกสร้างใหม่ (เคยเกิดมาแล้วครั้งหนึ่ง ทำให้ค่า hardcode เดิม 46/54/55 หลุด) แต่
        // CategoryCode เป็น business key ที่ตั้งใจให้คงที่
        private const string OrderFolderLabel    = "คำสั่ง (นายทะเบียน/กระทรวง)";
        private const string InternalFolderLabel = "หมวดภายใน";
        private const string LawFolderLabel      = "กฎหมาย";

        public KBMainController(
            IKBMainController  view,
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user)
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
        }

        public void Initialize() { LoadCategoryTree(); }

        public void LoadCategoryTree()
        {
            try
            {
                var categories       = _categoryService.GetAll().ToList();
                var counts           = new Dictionary<int, int>();
                // CategoryID → (Title → article count with that exact title)
                var titlesByCategory = new Dictionary<int, IDictionary<string, int>>();
                // CategoryID → (Title → link folder อ้างอิงข้ามหมวดที่ต้องแสดงใต้ title-leaf นั้น)
                var linkedFoldersByCategoryTitle = new Dictionary<int, IDictionary<string, List<KBLinkedFolder>>>();

                try
                {
                    // Archived (Status = 4) articles only ever appear under the "Archived"
                    // folder — exclude them from per-category counts/titles here.
                    var activeArticles = _articleService.GetAll().Where(a => a.Status != 4).ToList();
                    var articlesById   = activeArticles.ToDictionary(a => a.ArticleID);

                    // CategoryID → (Title → ArticleIDs ในกลุ่มนั้น) ใช้ resolve cross-reference links
                    var groupArticleIds = new Dictionary<int, Dictionary<string, List<int>>>();

                    foreach (var a in activeArticles)
                    {
                        // per-category article count
                        if (!counts.ContainsKey(a.CategoryID)) counts[a.CategoryID] = 0;
                        counts[a.CategoryID]++;

                        // per-title count within each category
                        string t = (a.Title ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(t)) continue;

                        if (!titlesByCategory.ContainsKey(a.CategoryID))
                            titlesByCategory[a.CategoryID] =
                                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        var titleMap = titlesByCategory[a.CategoryID];
                        if (!titleMap.ContainsKey(t)) titleMap[t] = 0;
                        titleMap[t]++;

                        if (!groupArticleIds.ContainsKey(a.CategoryID))
                            groupArticleIds[a.CategoryID] =
                                new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

                        var idMap = groupArticleIds[a.CategoryID];
                        if (!idMap.ContainsKey(t)) idMap[t] = new List<int>();
                        idMap[t].Add(a.ArticleID);
                    }

                    // ── Cross-reference folders: กฎหมาย (Act ใด ๆ ใต้ LW) ↔ คำสั่ง(OR)/หมวดภายใน(AN) ──
                    // (ถือลิงก์เป็นสองทิศทางเสมอ) — resolve CategoryID ของ OR/AN จาก CategoryCode สด ๆ
                    // และเช็คฝั่งกฎหมายด้วยการไล่ ParentCategoryID แทนเทียบ CategoryID คงที่ตัวเดียว
                    // เพื่อให้ใช้ได้กับกฎหมายทุกฉบับ (PVI, PLI, OIC, ...) ไม่ใช่แค่ฉบับที่ hardcode ไว้
                    var linkGraph    = BuildLawLinkGraph();
                    var categoryById = categories.ToDictionary(c => c.CategoryID);
                    int? orderCategoryId    = FindCategoryIdByCode(categories, "OR");
                    int? internalCategoryId = FindCategoryIdByCode(categories, "AN");

                    foreach (var catEntry in groupArticleIds)
                    {
                        List<Tuple<Func<KBArticle, bool>, string>> rules = null;

                        if (IsLawCategory(catEntry.Key, categoryById))
                        {
                            rules = new List<Tuple<Func<KBArticle, bool>, string>>();
                            if (orderCategoryId.HasValue)
                                rules.Add(Tuple.Create<Func<KBArticle, bool>, string>(
                                    art => art.CategoryID == orderCategoryId.Value, OrderFolderLabel));
                            if (internalCategoryId.HasValue)
                                rules.Add(Tuple.Create<Func<KBArticle, bool>, string>(
                                    art => art.CategoryID == internalCategoryId.Value, InternalFolderLabel));
                        }
                        else if ((orderCategoryId.HasValue && catEntry.Key == orderCategoryId.Value) ||
                                 (internalCategoryId.HasValue && catEntry.Key == internalCategoryId.Value))
                        {
                            rules = new List<Tuple<Func<KBArticle, bool>, string>>
                            {
                                Tuple.Create<Func<KBArticle, bool>, string>(
                                    art => IsLawCategory(art.CategoryID, categoryById), LawFolderLabel),
                            };
                        }

                        if (rules == null || rules.Count == 0) continue;

                        foreach (var titleEntry in catEntry.Value)
                        {
                            var relatedIds = new HashSet<int>();
                            foreach (var articleId in titleEntry.Value)
                            {
                                HashSet<int> linked;
                                if (linkGraph.TryGetValue(articleId, out linked))
                                    relatedIds.UnionWith(linked);
                            }
                            if (relatedIds.Count == 0) continue;

                            var folders = new List<KBLinkedFolder>();
                            foreach (var rule in rules)
                            {
                                var    matches = rule.Item1;
                                string label   = rule.Item2;

                                var matched = relatedIds
                                    .Select(id => { KBArticle art; articlesById.TryGetValue(id, out art); return art; })
                                    .Where(art => art != null && matches(art))
                                    .ToList();

                                if (matched.Count > 0)
                                    folders.Add(new KBLinkedFolder { Label = label, Articles = matched });
                            }

                            if (folders.Count == 0) continue;

                            if (!linkedFoldersByCategoryTitle.ContainsKey(catEntry.Key))
                                linkedFoldersByCategoryTitle[catEntry.Key] =
                                    new Dictionary<string, List<KBLinkedFolder>>(StringComparer.OrdinalIgnoreCase);

                            linkedFoldersByCategoryTitle[catEntry.Key][titleEntry.Key] = folders;
                        }
                    }
                }
                catch { /* article counts / titles / linked folders are non-critical */ }

                _view.BindCategoryTree(new KBCategoryTreeData
                {
                    Categories                   = categories,
                    ArticleCounts                = counts,
                    TitlesByCategory             = titlesByCategory,
                    LinkedFoldersByCategoryTitle = linkedFoldersByCategoryTitle,
                });
            }
            catch (Exception ex) { _view.ShowError("Load category failed: " + ex.Message); }
        }

        /// <summary>
        /// สร้าง undirected graph ของลิงก์ KMS.t_KBArticleLink (EntityType=Law) — เพิ่ม edge ทั้ง 2 ทิศทางเสมอ
        /// ไม่ว่า DB จะเก็บทิศทางไหนจริง (รองรับทั้งลิงก์ที่ import สร้างแบบ 2 ทิศทาง และลิงก์จากหน้า editor ที่สร้างทิศทางเดียว)
        /// </summary>
        private Dictionary<int, HashSet<int>> BuildLawLinkGraph()
        {
            var graph = new Dictionary<int, HashSet<int>>();
            var links = _articleService.GetAllLinksByType((byte)KBEntityType.Law);

            foreach (var link in links)
            {
                int entityId;
                if (!int.TryParse(link.EntityID, out entityId)) continue;

                AddEdge(graph, link.ArticleID, entityId);
                AddEdge(graph, entityId, link.ArticleID);
            }

            return graph;
        }

        private static void AddEdge(Dictionary<int, HashSet<int>> graph, int from, int to)
        {
            HashSet<int> set;
            if (!graph.TryGetValue(from, out set))
            {
                set = new HashSet<int>();
                graph[from] = set;
            }
            set.Add(to);
        }

        /// <summary>คืน CategoryID ของ category ที่มี CategoryCode ตรงกับที่ระบุ (case-insensitive) หรือ null ถ้าไม่เจอ</summary>
        private static int? FindCategoryIdByCode(IList<KBCategory> categories, string categoryCode)
        {
            var match = categories.FirstOrDefault(c => string.Equals(c.CategoryCode, categoryCode, StringComparison.OrdinalIgnoreCase));
            return match != null ? (int?)match.CategoryID : null;
        }

        /// <summary>
        /// true ถ้า <paramref name="categoryId"/> ไล่ ParentCategoryID ขึ้นไปเจอ category ที่ CategoryCode = "LW"
        /// (หมวด "กฎหมาย" ราก) — ใช้ระบุว่าเป็นกฎหมายฉบับใดก็ได้ (PVI, PLI, OIC, ...) ไม่ใช่แค่ฉบับเดียว
        /// </summary>
        private static bool IsLawCategory(int categoryId, Dictionary<int, KBCategory> categoryById)
        {
            var visited = new HashSet<int>();
            int current = categoryId;
            KBCategory cat;
            while (categoryById.TryGetValue(current, out cat))
            {
                if (string.Equals(cat.CategoryCode, "LW", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!cat.ParentCategoryID.HasValue || !visited.Add(current))
                    return false;
                current = cat.ParentCategoryID.Value;
            }
            return false;
        }

        /// <summary>
        /// Called when the user clicks a cross-reference folder (or one of its child leaves)
        /// in treeCategories. Renders exactly the given articles, bypassing the normal
        /// category/title search since the IDs are already known from the link graph.
        /// </summary>
        public void OnLinkedArticlesSelected(IList<int> articleIds, string folderLabel)
        {
            _view.IsArchivedMode       = false;
            _view.SelectedCategoryId   = null;
            _view.SelectedArticleTitle = null;
            _view.ArticleTitleText     = string.Format("  {0}", folderLabel);
            _linkedArticleIds          = articleIds;
            LoadArticles();
        }

        public void OnArchivedSelected()
        {
            _view.IsArchivedMode       = true;
            _view.SelectedCategoryId   = null;
            _view.SelectedArticleTitle = null;
            _view.ArticleTitleText     = "  \U0001F4E6 Archived";
            _linkedArticleIds          = null;
            LoadArticles();
        }

        public void OnCategorySelected(int? categoryId, string categoryName)
        {
            _view.IsArchivedMode       = false;
            _view.SelectedCategoryId   = categoryId;
            _view.SelectedArticleTitle = null;   // clear title-subcategory filter
            _view.ArticleTitleText     = categoryId.HasValue
                ? string.Format("  {0}", categoryName)
                : "  ทุกหมวดหมู่";
            _linkedArticleIds          = null;
            LoadArticles();
        }

        /// <summary>
        /// Called when the user clicks a title leaf-node in treeCategories.
        /// Filters the article list to rows whose Title matches exactly.
        /// </summary>
        public void OnTitleSelected(int? categoryId, string categoryName, string title)
        {
            _view.IsArchivedMode       = false;
            _view.SelectedCategoryId   = categoryId;
            _view.SelectedArticleTitle = title;
            _view.ArticleTitleText     = string.Format("  {0}  ›  {1}", categoryName, title);
            _linkedArticleIds          = null;
            LoadArticles();
        }

        public void LoadArticles()
        {
            _currentPage = 1;
            LoadArticlesAtPage();
        }

        public void GoToPage(int page)
        {
            _currentPage = page;
            LoadArticlesAtPage();
        }

        private void LoadArticlesAtPage()
        {
            try
            {
                _view.ShowLoading();

                bool   isArchived    = _view.IsArchivedMode;
                string keyword       = _view.SearchKeyword;
                int    searchFieldId = _view.SearchFieldId;
                byte?  statusFilter  = isArchived ? (byte?)4 : _view.StatusFilter;
                byte?  typeFilter    = _view.TypeFilter;
                int?   catId         = isArchived ? (int?)null : _view.SelectedCategoryId;
                string titleFilter   = _view.SelectedArticleTitle;
                int    pageSize      = _view.PageSize;

                List<KBArticle> list;
                if (_linkedArticleIds != null)
                {
                    // Cross-reference group (มาตรา ↔ คำสั่ง/หมวดภายใน) — ไม่มี CategoryID เดียวให้ query
                    // ตาม ดึงเฉพาะ ArticleID ที่ resolve ไว้แล้วจาก link graph แล้วค้นหา/กรองคำค้นบน
                    // รายการนี้โดยตรง ด้วย criteria เดียวกับ SearchByType (เดิมจุดนี้ไม่รองรับการค้นหาเลย)
                    list = _linkedArticleIds
                        .Select(id => _articleService.GetById(id))
                        .Where(a => a != null && a.Status != 4)
                        .ToList();
                    list = _articleService.FilterByKeyword(list, keyword, searchFieldId).ToList();
                }
                else
                {
                    if (!isArchived && !catId.HasValue) return;

                    list = _articleService.SearchByType(keyword, catId, statusFilter, typeFilter, searchFieldId).ToList();

                    // Archived (Status = 4) articles must only show up under the "Archived"
                    // folder, never mixed into a category's regular listing.
                    if (!isArchived)
                        list = list.Where(a => a.Status != 4).ToList();
                }

                if (!string.IsNullOrEmpty(titleFilter))
                    list = list.Where(a => string.Equals(
                        (a.Title ?? string.Empty).Trim(),
                        titleFilter.Trim(),
                        StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var article in list)
                    ResolveLatestVersion(article);

                int totalCount = list.Count;
                int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
                if (_currentPage > totalPages) _currentPage = totalPages;
                if (_currentPage < 1)          _currentPage = 1;

                var pageItems = list.Skip((_currentPage - 1) * pageSize).Take(pageSize).ToList();
                pageItems.ForEach(x => x.ContentHTML = StripHtmlTags(x.ContentHTML));
                _view.RenderArticles(pageItems);
                _view.ArticleCountText = string.Format("{0} บทความ", totalCount);
                _view.RenderPagination(totalCount, _currentPage, pageSize);
            }
            catch (Exception ex) { _view.ShowError("Load articles failed: " + ex.Message); }
            finally
            {
                _view.HideLoading();
            }
        }

        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<br\s*/?>",  " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?p>",      " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?div>",    " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"</?li>",     " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>",    string.Empty);
            html = System.Net.WebUtility.HtmlDecode(html);
            html = System.Text.RegularExpressions.Regex.Replace(html, @"\s+",        " ");
            return html.Trim();
        }

        public void OnArticleClicked(int articleId)
        {
            try { _articleService.IncrementViewCount(articleId); } catch { }
        }

        public void LikeArticle(int articleId)
        {
            try { _articleService.IncrementLikeCount(articleId); } catch { }
        }

        /// <summary>
        /// Send article for review - Change status from Draft (1) to Review (2)
        /// </summary>
        public void SendArticleForReview(int articleId)
        {
            try
            {
                _articleService.ChangeStatus(articleId, 2, _user.AdIdentity);
                // Success - no need to show message, the UI will refresh
            }
            catch (Exception ex)
            {
                _view.ShowError("ส่งขอ Review ไม่สำเร็จ: " + ex.Message);
            }
        }

        public void DeleteArticle(int articleId)
        {
            try
            {
                _articleService.SoftDelete(articleId);
                LoadArticles();
            }
            catch (Exception ex) { _view.ShowError("ลบบทความไม่สำเร็จ: " + ex.Message); }
        }

        public void UpdateArticleExpiryDate(int articleId, DateTime? expiryDate)
        {
            try   { _articleService.UpdateExpiryDate(articleId, expiryDate); }
            catch (Exception ex) { _view.ShowError("บันทึกวันหมดอายุไม่สำเร็จ: " + ex.Message); }
        }

        public void ShowReport()
        {
            try
            {
                var all = _articleService.GetAll().ToList();
                string report = string.Format(
                    "📊 สรุปรายงาน Knowledge Base\n\n" +
                    "บทความทั้งหมด: {0}\n" +
                    "📝 Draft:      {1}\n" +
                    "🔍 Review:     {2}\n" +
                    "✅ Published:  {3}\n" +
                    "📦 Archived:   {4}\n\n" +
                    "👁 Views ทั้งหมด: {5}\n" +
                    "👍 Likes ทั้งหมด: {6}",
                    all.Count,
                    all.Count(a => a.Status == 1),
                    all.Count(a => a.Status == 2),
                    all.Count(a => a.Status == 3),
                    all.Count(a => a.Status == 4),
                    all.Sum(a => a.ViewCount),
                    all.Sum(a => a.LikeCount));
                _view.ShowReport(report);
            }
            catch (Exception ex) { _view.ShowError("Load report failed: " + ex.Message); }
        }

        /// <summary>
        /// Resolves the latest VersionNumber from KMS.t_KBVersion and writes it
        /// back to article.CurrentVersion so lblMeta always shows the DB value.
        /// </summary>
        private void ResolveLatestVersion(KBArticle article)
        {
            try
            {
                int latest = _articleService.GetLatestVersionNumber(article.ArticleID);
                if (latest > 0)
                    article.CurrentVersion = latest;
            }
            catch { /* non-critical — keep whatever CurrentVersion was already set */ }
        }
    }
}
