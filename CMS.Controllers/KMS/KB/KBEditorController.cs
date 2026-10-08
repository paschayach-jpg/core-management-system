using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CMS.Controllers.KMS.KB
{
    /// <summary>
    /// Presenter for KBEditor dialog (MOD-006)
    /// Redesigned from ArticleEditorDialogPresenter
    /// Handles: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public class KBEditorController
    {
        private readonly IKBEditorController _view;
        private readonly IKBCategoryService  _categoryService;
        private readonly IKBArticleService   _articleService;
        private readonly UserContext         _user;
        private KBArticle                   _editingArticle;  // Changed from readonly to allow updates

        /// <summary>หมวดทั้งหมด (รวมที่ปิดใช้งาน) ใช้ตรวจ Code รวม/หาหมวดตามกฎเดียวกับ import (<see cref="KBArticleCodeAnalyzer"/>)</summary>
        private List<KBCategory>            _allCategories = new List<KBCategory>();
        private bool                        _analyzing;       // กันวนซ้ำ: การตั้งหมวด/ชื่อจากผลตรวจ Code ทำให้ view แจ้งเหตุการณ์กลับมา

        /// <summary>true = บทความ Archived: แก้อะไรไม่ได้ (รวมการแนบ/ลบไฟล์และ link ที่เขียน DB ทันที)</summary>
        public bool IsReadOnly { get { return _editingArticle != null && _editingArticle.Status == 4; } }

        /// <summary>FileID ที่ผูกกับบทความแต่ไม่ได้แสดงในตาราง (โหลดข้อมูลไฟล์ไม่ได้) — เก็บไว้ส่งกลับตอนบันทึกไฟล์แนบ</summary>
        private List<int>                   _unlistedAttachmentIds = new List<int>();

        /// <summary>
        /// Expose the editing article for search exclusion
        /// </summary>
        public KBArticle EditingArticle => _editingArticle;

        public KBEditorController(
            IKBEditorController view,
            IKBCategoryService  categoryService,
            IKBArticleService   articleService,
            UserContext         user,
            KBArticle           article)   // null = create new
        {
            if (view            == null) throw new ArgumentNullException("view");
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");

            _view            = view;
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
            _editingArticle  = article == null ? article : _articleService.GetById(article.ArticleID);
        }

        /// <summary>
        /// Initialize the view with data
        /// </summary>
        public void Initialize()
        {
            LoadDropdowns();
            _view.StatusEnabled = false;   // สถานะเปลี่ยนผ่านปุ่ม ส่งขอ Review / เผยแพร่ / Archive เท่านั้น (ChangeStatus)

            if (_editingArticle == null)
            {
                // Create mode — พิมพ์ Code รวม 29 ตัวเอง (กฎ/หมวดเดียวกับ import) แทน KB-YYYY-NNN
                _view.HeaderText           = "  ✏️ เพิ่มบทความใหม่";
                _view.ArticleCode          = string.Empty;
                _view.ArticleCodeEditable  = true;
                _view.CategoryEditable     = true;
                _view.TitleEditable        = true;
                _view.SelectedStatus       = 1;  // Default to Draft status
                _view.ArchiveButtonVisible = false;
                _view.PublishButtonText    = "📤 ส่งขอ Review";
                _view.VersionText          = "Version: 1.0";
                _view.AuthorText           = string.Format("สร้างโดย: {0}", _user.AdIdentity);
                _view.DateInfoText         = string.Format("วันที่: {0}", DateTime.Now.ToString("dd/MM/yyyy"));
                OnArticleCodeChanged();
            }
            else
            {
                // Edit mode — Code และหมวดแก้ไม่ได้ (หมวดย้ายที่หน้า "ย้ายบทความ"); ชื่อ "มาตรา N" ของกฎหมาย/พระราชกฤษฎีกา import คำนวณจาก Code เสมอ
                _view.ArticleCodeEditable = false;
                _view.CategoryEditable    = false;
                // กฎหมาย/พระราชกฤษฎีกาเก็บ ArticleCode 24 ตัวแรก (ไม่ใช่ 29) — ชื่อ "มาตรา N" ของบทความเหล่านี้ import คำนวณจาก Code เสมอ จึงไม่ให้แก้ที่นี่
                string storedCode = _editingArticle.ArticleCode ?? string.Empty;
                _view.TitleEditable = storedCode.Length != KBImportLayoutTypeCatalog.ParentCodeLength;

                _view.HeaderText = string.Format("  ✏️ แก้ไขบทความ: {0}", _editingArticle.Title ?? "");
                PopulateFields(_editingArticle);
                LoadArticleLinks(_editingArticle.ArticleID);
                UpdateStatusButtons(_editingArticle.Status);
            }
        }

        /// <summary>
        /// ตรวจ Code รวมที่พิมพ์ (บทความใหม่): ชนิด Law/Decree/NonLaw จาก Code, หมวด/ชื่อที่คำนวณได้ → ตั้งลงฟอร์มแล้วล็อกช่องที่คำนวณจาก Code,
        /// แสดงผลการตรวจข้างช่อง Code. View เรียกทุกครั้งที่ Code หรือหมวดเปลี่ยน
        /// </summary>
        public void OnArticleCodeChanged()
        {
            if (_editingArticle != null || _analyzing) return;

            _analyzing = true;
            try
            {
                string code = (_view.ArticleCode ?? string.Empty).Trim();
                var analysis = AnalyzeCurrentCode();

                if (code.Length == 0)
                {
                    _view.SetArticleCodeHint("พิมพ์ Code รวม " + KBImportLayoutTypeCatalog.LawCodeLength + " ตัว", false);
                    _view.CategoryEditable = true;
                    _view.TitleEditable    = true;
                    return;
                }

                if (analysis.Layout == null)
                {
                    _view.SetArticleCodeHint(code.Length < KBImportLayoutTypeCatalog.LawCodeLength
                        ? string.Format("Code รวมต้องยาว {0} ตัว (พิมพ์แล้ว {1} ตัว)", KBImportLayoutTypeCatalog.LawCodeLength, code.Length)
                        : analysis.Error, true);
                    _view.CategoryEditable = true;
                    _view.TitleEditable    = true;
                    return;
                }

                // ชื่อ: "มาตรา N" ของกฎหมาย/พระราชกฤษฎีกา คำนวณจาก Code (แก้ไม่ได้ ไม่งั้น import ซ้ำจะทับ); ไม่ใช่กฎหมายตั้งชื่อเอง
                _view.TitleEditable = analysis.Title == null;
                if (analysis.Title != null)
                {
                    _view.Title = analysis.Title;
                    if (string.IsNullOrWhiteSpace(_view.Tags)) _view.Tags = analysis.Title;
                }

                // หมวด: กฎหมาย/ไม่ใช่กฎหมายคำนวณจาก Code; พระราชกฤษฎีกาเลือกหมวดกลุ่มเอง (ใต้ DDD00)
                _view.CategoryEditable = !analysis.CategoryFromCode;
                if (analysis.CategoryFromCode && analysis.Category != null)
                    _view.SelectedCategoryId = analysis.Category.CategoryID;

                if (analysis.IsValid)
                {
                    // ข้อความสั้น (ช่องแคบ): ชนิด + ชื่อที่คำนวณได้ — ไม่ใช่กฎหมายแสดงรหัสหมวดที่จะลง (หมวดที่เลือกแสดงที่ช่องหมวดอยู่แล้ว)
                    string kind = analysis.LayoutLabel;
                    _view.SetArticleCodeHint(analysis.Title != null
                        ? string.Format("{0} ({1})", kind, analysis.Title)
                        : string.Format("{0} → {1}", kind, analysis.Category != null ? analysis.Category.CategoryCode : string.Empty), false);
                }
                else
                {
                    _view.SetArticleCodeHint(analysis.Error, true);
                }
            }
            finally
            {
                _analyzing = false;
            }
        }

        private KBArticleCodeAnalysis AnalyzeCurrentCode()
        {
            KBCategory chosen = _allCategories.FirstOrDefault(c => c.CategoryID == _view.SelectedCategoryId);
            return KBArticleCodeAnalyzer.Analyze(_allCategories, _view.ArticleCode, chosen);
        }

        /// <summary>
        /// ตรวจก่อนบันทึก (ฟอร์มเรียกใน ValidateInput): บทความใหม่ต้องมี Code 29 ตัวที่ผ่านกฎชนิด/หมวด และไม่ซ้ำ; พระราชกฤษฎีกาต้องผูกมาตราของกฎหมายแม่อย่างน้อย 1.
        /// คืนข้อความภาษาไทยเมื่อไม่ผ่าน, null = ผ่าน (บทความเดิมไม่ตรวจที่นี่)
        /// </summary>
        public string ValidateBeforeSave()
        {
            if (_editingArticle != null) return null;

            var analysis = AnalyzeCurrentCode();
            if (!analysis.IsValid) return analysis.Error;
            if (_articleService.ArticleCodeExists(analysis.StoredCode))
                return string.Format("ArticleCode '{0}' มีอยู่ในระบบแล้ว (รวมบทความที่ถูกลบ)", analysis.StoredCode);

            if (analysis.Layout == KBImportLayoutTypeCatalog.Decree)
            {
                int linked = 0;
                foreach (System.Windows.Forms.DataGridViewRow row in _view.GetRelatedDataGridView().Rows)
                {
                    int id;
                    if (TryGetArticleId(row.Tag, out id)) linked++;
                }
                if (linked == 0)
                    return "พระราชกฤษฎีกาต้องผูกกับมาตราของกฎหมายที่อ้างอิงอย่างน้อย 1 มาตรา (แท็บ \"บทความที่เกี่ยวข้อง\")";
            }

            return null;
        }

        /// <summary>
        /// Load dropdown data (categories). บทความใหม่เลือกได้เฉพาะหมวดที่เปิดใช้งาน; บทความเดิมแสดงทุกหมวด
        /// เพื่อให้หมวดปัจจุบันที่ถูกปิดไปแล้วยังแสดงชื่อ (หมวดแก้ไม่ได้ในหน้านี้)
        /// </summary>
        private void LoadDropdowns()
        {
            try
            {
                _allCategories = _categoryService.GetAll().ToList();
                _view.BindCategories(_editingArticle == null
                    ? _allCategories.Where(c => c.IsActive)
                    : (IEnumerable<KBCategory>)_allCategories);
            }
            catch
            {
                _allCategories = new List<KBCategory>();
                _view.BindCategories(new List<KBCategory>());
            }
        }

        /// <summary>
        /// Populate view fields from article entity
        /// </summary>
        private void PopulateFields(KBArticle a)
        {
            _view.ArticleCode         = a.ArticleCode ?? string.Empty;
            _view.Title               = a.Title       ?? string.Empty;
            _view.Summary             = a.Summary     ?? string.Empty;
            _view.TitleCategory       = a.TitleCategory ?? string.Empty;
            _view.ContentHTML         = a.ContentHTML ?? string.Empty;
            _view.Tags                = a.Tags        ?? string.Empty;
            _view.SelectedCrafLevelCode = a.ConfidentialityLevel;
            _view.SelectedCategoryId  = a.CategoryID;
            _view.SelectedArticleType = (byte)a.ArticleType;
            _view.SelectedStatus      = a.Status;

            if (a.PublishedDate.HasValue)
                _view.PublishedDate = a.PublishedDate.Value;

            if (a.EffectiveDate.HasValue)
                _view.EffectiveDate = a.EffectiveDate.Value;

            if (a.ExpiryDate.HasValue)
                _view.ExpiryDate = a.ExpiryDate.Value;

            _view.ViewCountText  = string.Format("👁  ดู {0} ครั้ง", a.ViewCount);
            _view.LikeCountText  = string.Format("👍  {0} Like", a.LikeCount);

            // Get latest version from KMS.t_KBVersion table via service
            string versionText = "v1.0";
            try
            {
                int latestVer = _articleService.GetLatestVersionNumber(a.ArticleID);
                if (latestVer > 0)
                    versionText = string.Format("v{0}.0", latestVer);
                else if (a.CurrentVersion > 0)
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
            }
            catch
            {
                if (a.CurrentVersion > 0)
                    versionText = string.Format("v{0}.0", a.CurrentVersion);
            }

            _view.VersionText    = string.Format("Version: {0}", versionText);
            _view.CurVersionText = versionText;
            _view.AuthorText     = string.Format("สร้างโดย: {0}", a.CreatedBy ?? "—");
            _view.DateInfoText   = string.Format("วันที่: {0}", a.CreatedDate.ToString("dd/MM/yyyy"));
        }

        /// <summary>
        /// Update button visibility and text based on article status
        /// </summary>
        private void UpdateStatusButtons(byte status)
        {
            bool isArchived  = (status == 4);
            bool isPublished = (status == 3);

            _view.SaveEnabled = !isArchived;

            if (status == 1 || status == 2)
            {
                // Draft or Review
                _view.PublishButtonText  = status == 1 ? "📤 ส่งขอ Review" : "✅ เผยแพร่";
                _view.PublishButtonColor = status == 1
                    ? Color.FromArgb(46, 116, 181)
                    : Color.FromArgb(39, 174, 96);
                _view.ArchiveButtonVisible = false;
            }
            else if (status == 3)
            {
                // Published
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = true;
            }
            else if (status == 4)
            {
                // Archived
                _view.PublishButtonVisible = false;
                _view.ArchiveButtonVisible = false;
                _view.SaveEnabled          = false;
            }
        }

        /// <summary>
        /// บันทึก (สร้างใหม่หรือแก้ไข) โดยไม่เปลี่ยนสถานะ — บทความใหม่เริ่มที่ Draft; สถานะเปลี่ยนผ่านปุ่ม Review/Publish/Archive เท่านั้น
        /// </summary>
        public void Save(string changeDesc)
        {
            try
            {
                PersistArticle(changeDesc);
                _view.ShowInfo("บันทึกเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("บันทึกไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// ส่งขอ Review (สถานะ 2): บันทึกก่อน (สร้างใหม่ = Draft ก่อน) แล้วเปลี่ยนสถานะผ่าน <c>ChangeStatus</c>
        /// </summary>
        public void SendReview()
        {
            try
            {
                int articleId = PersistArticle("ส่งขอ Review");

                _articleService.ChangeStatus(articleId, 2, _user.AdIdentity);
                _editingArticle = _articleService.GetById(articleId) ?? _editingArticle;

                _view.ShowInfo("ส่งขอ Review เรียบร้อยแล้ว");
                _view.CloseOk();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ส่งขอ Review ไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// สร้างหรือแก้บทความจากค่าในฟอร์ม แล้วบันทึก link/ไฟล์แนบ — คืน ArticleID.
        /// การแก้ไขเขียนเฉพาะฟิลด์ที่ฟอร์มแก้ได้ (หมวด/สถานะ/ReviewDueDate/IsOfficialPolicy คงเดิม ดู <c>IKBArticleRepository.Update</c>)
        /// </summary>
        private int PersistArticle(string changeDesc)
        {
            if (IsReadOnly)
                throw new InvalidOperationException("บทความที่ Archive แล้วแก้ไขไม่ได้");

            bool created = _editingArticle == null;

            // บทความใหม่: ตรวจซ้ำที่นี่ด้วย (ไม่พึ่งว่า view เรียก ValidateBeforeSave แล้ว) และใช้ชื่อ/หมวดที่คำนวณจาก Code
            KBArticleCodeAnalysis analysis = null;
            if (created)
            {
                string problem = ValidateBeforeSave();
                if (problem != null) throw new InvalidOperationException(problem);
                analysis = AnalyzeCurrentCode();
            }

            var article = new KBArticle
            {
                ArticleCode          = created ? analysis.StoredCode : _editingArticle.ArticleCode,
                Title                = created && analysis.Title != null ? analysis.Title : _view.Title,
                Summary              = _view.Summary,
                TitleCategory        = _view.TitleCategory,
                ContentHTML          = _view.ContentHTML,
                CategoryID           = created ? analysis.Category.CategoryID : _editingArticle.CategoryID,
                ArticleType          = _view.SelectedArticleType,
                ConfidentialityLevel = _view.SelectedCrafLevelCode,
                Tags                 = _view.Tags,
                PublishedDate        = _view.PublishedDate,
                EffectiveDate        = _view.EffectiveDate,
                ExpiryDate           = _view.ExpiryDate,
                CreatedBy            = _user.AdIdentity,
            };

            int articleId;
            if (created)
            {
                articleId = _articleService.Create(article);   // Status = Draft เสมอ
            }
            else
            {
                article.ArticleID = _editingArticle.ArticleID;
                _articleService.Update(article, changeDesc, _user.AdIdentity);
                articleId = _editingArticle.ArticleID;
            }

            // โหลดจาก DB ใหม่ (ได้ CreatedDate/ReviewDueDate/CurrentVersion ฯลฯ ครบ) แทนใช้ object ที่สร้างจากฟอร์ม
            article.ArticleID = articleId;
            _editingArticle   = _articleService.GetById(articleId) ?? article;

            if (created)
            {
                _view.HeaderText          = string.Format("  ✏️ แก้ไขบทความ: {0}", _editingArticle.Title ?? "");
                _view.ArticleCodeEditable = false;   // บันทึกแล้ว Code/หมวดแก้ไม่ได้อีก (เหมือนบทความเดิม)
                _view.CategoryEditable    = false;
                _view.SetArticleCodeHint(string.Empty, false);
            }

            SaveArticleLinks(articleId);

            // บทความที่ไม่ใช่กฎหมาย: ผูก link สองทางกับมาตราของกฎหมายแม่ (24 ตัวแรกของ Code) เหมือน import — ถ้ากฎหมายแม่ยังไม่มีในระบบข้าม (import ก็แค่เตือน)
            if (created && analysis.ParentCode != null)
            {
                var parent = _articleService.GetByCode(analysis.ParentCode);
                if (parent != null)
                    _articleService.LinkArticles(articleId, parent.ArticleID, _user.AdIdentity);
            }

            SaveArticleAttachments(articleId);
            return articleId;
        }

        /// <summary>
        /// Publish article (status = 3) - Only for existing articles in Review status
        /// </summary>
        public void Publish()
        {
            if (_editingArticle == null)
            {
                _view.ShowError("ไม่สามารถเผยแพร่บทความที่ยังไม่ได้บันทึกได้");
                return;
            }

            try
            {
                // บันทึกการแก้ไขที่ค้างอยู่ก่อน แล้วเปลี่ยนสถานะเป็น Published (ผ่าน ChangeStatus: ตั้ง PublishedDate/ReviewedBy + audit)
                int articleId = PersistArticle("เผยแพร่");
                _articleService.ChangeStatus(articleId, 3, _user.AdIdentity);
                _editingArticle = _articleService.GetById(articleId) ?? _editingArticle;

                _view.ShowInfo("เผยแพร่บทความเรียบร้อยแล้ว");
                _view.CloseOk();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("เผยแพร่ไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Move to Archived status
        /// </summary>
        public void Archive()
        {
            if (_editingArticle == null) return;

            _articleService.ChangeStatus(_editingArticle.ArticleID, 4, _user.AdIdentity);
            _view.CloseOk();
        }

        /// <summary>
        /// Search published articles for inline search in Related tab
        /// </summary>
        public IEnumerable<KBArticle> SearchPublishedArticles(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<KBArticle>();

            return _articleService.SearchPublishedArticles(searchText);
        }

        /// <summary>
        /// Get all active categories from KB
        /// </summary>
        public IEnumerable<KBCategory> GetAllCategories()
        {
            try
            {
                return _categoryService.GetAllActive();
            }
            catch
            {
                return Enumerable.Empty<KBCategory>();
            }
        }

        /// <summary>
        /// Get all published articles from KB
        /// </summary>
        public IEnumerable<KBArticle> GetAllPublishedArticles()
        {
            try
            {
                // Get all articles with Status = 3 (Published)
                return _articleService.Search(null, null, 3, null);
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Search articles with filters
        /// </summary>
        public IEnumerable<KBArticle> SearchArticles(string searchText, int? categoryId, byte? status, byte? articleType)
        {
            try
            {
                return _articleService.Search(searchText, categoryId, status, articleType);
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Get linked articles for the current article
        /// </summary>
        public IEnumerable<KBArticle> GetLinkedArticles(int articleId)
        {
            try
            {
                // Get article-to-article links (EntityType = Law, matching ArticleViewerController)
                var links = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Law);
                var linkeds = links.Select(l => l.EntityID.ToString()).ToList();

                if (!linkeds.Any())
                    return Enumerable.Empty<KBArticle>();

                // Retrieve full article details for each linked article
                var linkedArticles = new List<KBArticle>();
                foreach (var id in linkeds)
                {
                    var article = _articleService.GetById(Convert.ToInt32(id));
                    if (article != null && !article.IsDelete)
                        linkedArticles.Add(article);
                }

                return linkedArticles;
            }
            catch
            {
                return Enumerable.Empty<KBArticle>();
            }
        }

        /// <summary>
        /// Add a related article link
        /// </summary>
        public void AddRelatedArticleLink(int relatedArticleId)
        {
            if (_editingArticle == null)
                throw new InvalidOperationException("เพิ่มความเชื่อมโยงไม่ได้: ยังไม่ได้บันทึกบทความ");

            try
            {
                if (IsReadOnly) throw new InvalidOperationException("บทความที่ Archive แล้วแก้ไขไม่ได้");
                _articleService.LinkArticles(_editingArticle.ArticleID, relatedArticleId, _user.AdIdentity);   // สองทาง เหมือน import
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("เพิ่มความเชื่อมโยงกับบทความไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Remove a related article link
        /// </summary>
        public void RemoveRelatedArticleLink(int relatedArticleId)
        {
            if (_editingArticle == null)
                throw new InvalidOperationException("ลบความเชื่อมโยงไม่ได้: ยังไม่ได้บันทึกบทความ");

            try
            {
                if (IsReadOnly) throw new InvalidOperationException("บทความที่ Archive แล้วแก้ไขไม่ได้");
                _articleService.UnlinkArticles(_editingArticle.ArticleID, relatedArticleId);   // ยกเลิกทั้งสองทาง
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ลบความเชื่อมโยงกับบทความไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Persist attachments/links immediately for an already-saved article (e.g. right after
        /// an upload), mirroring what Save() does. No-op if the article has not been saved yet -
        /// in that case attachments stay local until the user clicks Save Draft.
        /// </summary>
        public void SaveAttachmentsAndLinksIfExisting()
        {
            if (_editingArticle == null || IsReadOnly) return;

            int articleId = _editingArticle.ArticleID;
            SaveArticleLinks(articleId);
            SaveArticleAttachments(articleId);
        }

        /// <summary>
        /// Remove an attached file's link (KMS.t_KBArticleLink, EntityType = Folder/File) by FileID
        /// </summary>
        public void RemoveArticleAttachmentLink(int fileId)
        {
            if (_editingArticle == null) return; // Not saved yet - nothing to delete in DB
            if (IsReadOnly) throw new InvalidOperationException("บทความที่ Archive แล้วแก้ไขไม่ได้");

            try
            {
                _articleService.RemoveArticleLink(
                    _editingArticle.ArticleID, fileId, (byte)KBEntityType.Folder);
                _articleService.RemoveArticleAttachment(_editingArticle.ArticleID, fileId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ลบการเชื่อมโยงไฟล์แนบไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// บันทึก link "บทความที่เกี่ยวข้อง" (EntityType = Law ตาม ArticleViewerController) — link ชนิดอื่น (Task/Program/ไฟล์) ไม่ถูกแตะ
        /// </summary>
        private void SaveArticleLinks(int articleId)
        {
            try
            {
                var links = new List<KBArticleLink>();

                var dgvRelated = _view.GetRelatedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvRelated.Rows)
                {
                    int relatedId;
                    if (TryGetArticleId(row.Tag, out relatedId))
                        links.Add(new KBArticleLink
                        {
                            EntityType  = (byte)KBEntityType.Law,
                            EntityID    = relatedId.ToString(),
                            CreatedBy   = _user.AdIdentity,
                            CreatedDate = DateTime.Now
                        });
                }

                _articleService.SaveArticleLinks(articleId, links);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("บันทึกความเชื่อมโยงกับบทความไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        private static bool TryGetArticleId(object tag, out int id)
        {
            id = 0;
            if (tag is int) { id = (int)tag; return id > 0; }
            return tag != null && int.TryParse(Convert.ToString(tag), out id) && id > 0;
        }

        private void SaveArticleAttachments(int articleId)
        {
            try
            {
                var fileIds = new List<int>();
                var dgvAttach = _view.GetAttachLinkedDataGridView();
                foreach (System.Windows.Forms.DataGridViewRow row in dgvAttach.Rows)
                {
                    if (row.Tag != null && row.Tag is int fileId)
                    {
                        fileIds.Add(fileId);
                    }
                }

                // ไฟล์ที่ผูกไว้แต่โหลดไม่ขึ้นหน้าจอ (เช่น ไฟล์ใน DMS ถูกลบ) — SaveArticleAttachments ลบแล้วเขียนใหม่ทั้งชุด
                // จึงต้องส่งกลับไปด้วย ไม่งั้นแถวผูกไฟล์หายเงียบ ๆ
                foreach (int hiddenId in _unlistedAttachmentIds)
                {
                    if (!fileIds.Contains(hiddenId)) fileIds.Add(hiddenId);
                }
                _articleService.SaveArticleAttachments(articleId, fileIds, _user.AdIdentity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("บันทึกไฟล์แนบไม่สำเร็จ: " + ex.Message, ex);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        /// <summary>
        /// Load article links from database and populate DataGridViews
        /// </summary>
        private void LoadArticleLinks(int articleId)
        {
            try
            {
                // Load Related Articles (EntityType = Law, matching ArticleViewerController)
                var articleLinks = _articleService.GetArticleLinks(articleId, (byte)KBEntityType.Law);
                var dgvRelated = _view.GetRelatedDataGridView();
                dgvRelated.Rows.Clear();

                int index = 1;
                foreach (var link in articleLinks)
                {
                    // Retrieve the full article details including Title using ArticleID (EntityID)
                    var linkedArticle = _articleService.GetById(Convert.ToInt32(link.EntityID));

                    if (linkedArticle != null && !linkedArticle.IsDelete)
                    {
                        int rowIndex = dgvRelated.Rows.Add(
                            index.ToString(),
                            linkedArticle.ArticleCode ?? "-",
                            linkedArticle.Title ?? "-",
                            linkedArticle.TitleCategory ?? "-",
                            linkedArticle.Summary ?? "-",
                            linkedArticle.ArticleTypeName ?? "-"
                        );

                        dgvRelated.Rows[rowIndex].Tag = linkedArticle.ArticleID;
                        dgvRelated.Rows[rowIndex].DefaultCellStyle.BackColor =
                            index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        index++;
                    }
                }

                // Load Attachments
                var attachmentFiles = _articleService.GetArticleAttachmentFiles(articleId).ToList();
                var shownFileIds    = new HashSet<int>(attachmentFiles.Select(f => f.FileID));
                _unlistedAttachmentIds = _articleService.GetArticleAttachments(articleId)
                                                        .Select(a => a.FileID)
                                                        .Where(id => !shownFileIds.Contains(id))
                                                        .ToList();
                var dgvAttach = _view.GetAttachLinkedDataGridView();
                dgvAttach.Rows.Clear();

                index = 1;
                foreach (var file in attachmentFiles)
                {
                    int rowIndex = dgvAttach.Rows.Add(
                        index.ToString(),
                        file.OriginalFileName,
                        FormatFileSize(file.FileSizeBytes),
                        file.UploadedDate.ToString("dd/MM/yyyy HH:mm")
                    );

                    dgvAttach.Rows[rowIndex].Tag = file.FileID;
                    dgvAttach.Rows[rowIndex].DefaultCellStyle.BackColor =
                        index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    index++;
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("โหลดข้อมูลความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
