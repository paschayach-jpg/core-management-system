using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IKBExcelReaderService
    {
        IList<KBImportRow> ReadRows(string filePath);
    }
}
