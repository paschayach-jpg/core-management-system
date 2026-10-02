using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Domain.Models
{
    /// <summary>หมวดหนึ่งตำแหน่งในต้นไม้หมวดตามลำดับการแสดงผลแบบ tree พร้อมระดับชั้น (0 = root)</summary>
    public sealed class KBCategoryTreeItem
    {
        public KBCategory Category { get; set; }
        public int        Depth    { get; set; }
    }

    /// <summary>
    /// ช่วยอ่านต้นไม้หมวด (KMS.m_KBCategory ที่อ้าง ParentCategoryID) แบบกี่ชั้นก็ได้ — เช่น กฎหมาย › พระราชบัญญัติประกันชีวิต (PLI)
    /// › พระราชบัญญัติประกันชีวิต2535 (PLI2535) ใช้ร่วมกันในหน้าที่ต้องแสดง/เลือก/นับตามลำดับชั้นของหมวด แทนที่จะเขียนไล่ 2 ชั้นเอง
    /// ทุกเมธอดกันวนลูป (ParentCategoryID ชี้วนกลับ) และไม่เรียกฐานข้อมูล
    /// </summary>
    public static class KBCategoryTree
    {
        /// <summary>
        /// เรียงหมวดเป็นลำดับ tree: root ก่อน แต่ละหมวดตามด้วยลูกหลานของมัน (เรียง SortOrder แล้วตามชื่อ เหมือนทุกหน้าจอ)
        /// หมวดที่ parent ไม่อยู่ในรายการ (เช่น parent ถูกปิดใช้งานเมื่อ <paramref name="activeOnly"/>) ถือเป็น root เพื่อไม่ให้หายไปจากรายการ
        /// </summary>
        public static IList<KBCategoryTreeItem> Flatten(IEnumerable<KBCategory> categories, bool activeOnly)
        {
            var all = (categories ?? Enumerable.Empty<KBCategory>())
                .Where(c => c != null && (!activeOnly || c.IsActive))
                .ToList();
            var ids = new HashSet<int>(all.Select(c => c.CategoryID));

            var childrenOf = new Dictionary<int, List<KBCategory>>();
            var roots      = new List<KBCategory>();
            foreach (var c in all)
            {
                if (c.ParentCategoryID.HasValue && c.ParentCategoryID.Value != c.CategoryID && ids.Contains(c.ParentCategoryID.Value))
                {
                    List<KBCategory> siblings;
                    if (!childrenOf.TryGetValue(c.ParentCategoryID.Value, out siblings))
                        childrenOf[c.ParentCategoryID.Value] = siblings = new List<KBCategory>();
                    siblings.Add(c);
                }
                else
                {
                    roots.Add(c);
                }
            }

            var result  = new List<KBCategoryTreeItem>(all.Count);
            var visited = new HashSet<int>();

            foreach (var root in Sorted(roots))
                Walk(root, 0, childrenOf, visited, result);

            // หมวดที่เหลือ = อยู่ในวงวน parent (ไม่มีทางไล่ถึงจาก root) — ใส่ท้ายรายการเป็น root ไม่ให้หาย
            foreach (var rest in Sorted(all.Where(c => !visited.Contains(c.CategoryID)).ToList()))
                Walk(rest, 0, childrenOf, visited, result);

            return result;
        }

        /// <summary>CategoryID ของลูกหลานทุกชั้นของ <paramref name="categoryId"/> (ไม่รวมตัวเอง และรวมหมวดที่ปิดใช้งานด้วย)</summary>
        public static ISet<int> GetDescendantIds(IEnumerable<KBCategory> categories, int categoryId)
        {
            var childrenOf = (categories ?? Enumerable.Empty<KBCategory>())
                .Where(c => c != null && c.ParentCategoryID.HasValue)
                .GroupBy(c => c.ParentCategoryID.Value)
                .ToDictionary(g => g.Key, g => g.Select(c => c.CategoryID).ToList());

            var result = new HashSet<int>();
            var queue  = new Queue<int>();
            queue.Enqueue(categoryId);

            while (queue.Count > 0)
            {
                List<int> children;
                if (!childrenOf.TryGetValue(queue.Dequeue(), out children)) continue;

                foreach (int child in children)
                {
                    if (child != categoryId && result.Add(child))
                        queue.Enqueue(child);
                }
            }
            return result;
        }

        /// <summary>เส้นทางจาก root ถึงหมวดนั้น เช่น "กฎหมาย › พระราชบัญญัติประกันชีวิต › พระราชบัญญัติประกันชีวิต2535" — ว่างถ้าไม่พบหมวด</summary>
        public static string GetPath(IEnumerable<KBCategory> categories, int categoryId, string separator)
        {
            var byId = new Dictionary<int, KBCategory>();
            foreach (var c in categories ?? Enumerable.Empty<KBCategory>())
            {
                if (c != null) byId[c.CategoryID] = c;
            }

            var names   = new List<string>();
            var visited = new HashSet<int>();
            KBCategory current;
            int id = categoryId;

            while (byId.TryGetValue(id, out current) && visited.Add(id))
            {
                names.Insert(0, current.CategoryName ?? string.Empty);
                if (!current.ParentCategoryID.HasValue) break;
                id = current.ParentCategoryID.Value;
            }

            return string.Join(separator ?? " > ", names);
        }

        private static void Walk(
            KBCategory category, int depth,
            Dictionary<int, List<KBCategory>> childrenOf, HashSet<int> visited, List<KBCategoryTreeItem> result)
        {
            if (!visited.Add(category.CategoryID)) return;

            result.Add(new KBCategoryTreeItem { Category = category, Depth = depth });

            List<KBCategory> children;
            if (!childrenOf.TryGetValue(category.CategoryID, out children)) return;

            foreach (var child in Sorted(children))
                Walk(child, depth + 1, childrenOf, visited, result);
        }

        private static List<KBCategory> Sorted(IEnumerable<KBCategory> categories)
        {
            return categories.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName).ToList();
        }
    }
}
