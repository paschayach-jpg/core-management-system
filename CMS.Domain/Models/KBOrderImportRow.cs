using System;
using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// 1 แถวจากไฟล์ Excel ตาราง ประกาศ/คำสั่ง (layout "law-matrix-formulas")
    /// A=Code กฎหมายแม่  B=ประเภทลูก  C=เอกสารลูก  D=Code (สูตร = A+B+C ต่อบรรทัด)
    /// E=ชื่อ  F=คำสั่งที่  G=วันที่ประกาศ  H=วันที่มีผลบังคับใช้
    /// I=บัญญัติไว้ว่า  J=ประเด็นสำคัญ  K=ชื่อไฟล์(+hyperlink)  L=กฎหมายอ้างอิง
    /// ใช้กับ KBOrderImportWizard เท่านั้น (คนละ layout กับ KBImportRow ของ template เดิม)
    /// </summary>
    public class KBOrderImportRow
    {
        /// <summary>เลขแถวใน Excel (1-based) ใช้อ้างอิงตอนแจ้ง error</summary>
        public int RowNumber { get; set; }

        public string Title              { get; set; }   // E  ชื่อ
        public string TitleCategory      { get; set; }   // F  คำสั่งที่
        public string PublishedDateRaw   { get; set; }   // G  วันที่ประกาศ
        public string EffectiveDateRaw   { get; set; }   // H  วันที่มีผลบังคับใช้
        public string ContentHTML        { get; set; }   // I  บัญญัติไว้ว่า (เก็บข้อความดิบ)
        public string Summary            { get; set; }   // J  ประเด็นสำคัญ
        public string AttachmentFileName { get; set; }   // K  ชื่อไฟล์ (ข้อความในเซลล์)
        public string AttachmentUrl      { get; set; }   // K  URL จริง (hyperlink ของเซลล์)
        public string SectionsRaw        { get; set; }   // L  กฎหมายอ้างอิง (ข้อความ ใช้แสดงผลเท่านั้น)

        public DateTime? PublishedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }

        /// <summary>ArticleCode ของกฎหมายแม่ที่อ้างถึง จาก column A (1 รายการต่อ 1 บรรทัดในเซลล์)</summary>
        public IList<string> ParentCodes { get; set; }

        /// <summary>
        /// ArticleCode ของ "คำสั่ง" นี้ ถ้านับเป็นลูกของกฎหมายแม่แต่ละตัวใน ParentCodes (index ตรงกัน)
        /// มาจาก column D ซึ่งเป็นสูตรสำเร็จรูป = ParentCodes[i] + ประเภทลูก(B) + เอกสารลูก(C)
        /// </summary>
        public IList<string> FinalCodeChoices { get; set; }

        /// <summary>"Valid" | "Warn" | "Error"</summary>
        public string Status { get; set; }
        public string StatusNote { get; set; }

        // ── ผลลัพธ์ระหว่าง import ────────────────────────────────────────────
        public int  ImportedArticleId { get; set; }
        public int? UploadedFileId    { get; set; }

        /// <summary>
        /// ArticleCode ที่ insert จริง = FinalCodeChoices[0] (ลูกของกฎหมายแม่ตัวแรกใน ParentCodes)
        /// อ่านได้ทันทีตอน parse ไฟล์ (column D คำนวณไว้แล้วโดยสูตรใน Excel ไม่ต้องรอ resolve ใดๆ)
        /// </summary>
        public string FinalArticleCode { get; set; }

        public KBOrderImportRow()
        {
            ParentCodes      = new List<string>();
            FinalCodeChoices = new List<string>();
            Status = "Valid";
        }
    }
}
