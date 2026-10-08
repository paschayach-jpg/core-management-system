using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IDownloadApproverConfigRepository
    {
        /// <summary>ทุก config ที่ Active (พร้อมชื่อ/ตำแหน่ง/Level ของผู้อนุมัติ) เรียงตาม Scope, OrgCode, SortOrder</summary>
        IEnumerable<DownloadApproverConfig> GetAll();

        /// <summary>config ที่ Active ของแผนก/ฝ่ายเดียว เรียงตาม SortOrder</summary>
        IEnumerable<DownloadApproverConfig> GetByOrg(string scopeType, string orgCode);

        /// <summary>
        /// แทนที่รายชื่อผู้อนุมัติของแผนก/ฝ่าย ทั้งชุดใน transaction เดียว:
        /// คนที่ไม่อยู่ในรายการใหม่ → IsActive = 0; คนในรายการ → upsert ตามลำดับ (SortOrder = index)
        /// </summary>
        void ReplaceForOrg(string scopeType, string orgCode, IList<int> approverUserIds, int updatedBy);
    }
}
