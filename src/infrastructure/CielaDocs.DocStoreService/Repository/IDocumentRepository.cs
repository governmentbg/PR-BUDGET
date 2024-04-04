using CielaDocs.DocStoreService.Models;
using Grpc.Core;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CielaDocs.DocStoreService.Repository
{
    public interface IDocumentRepository
    {
        Task<bool> CheckDocumentExists(long Id);
        Task<long> GetDocumentIDbyTmpGuid(string sTmpGuid);
        Task<IEnumerable<long?>> GetDocumentsByTmpGuid(string sTmpGuid);
        Task<DocInfoDto?> GetDocumentContentByIdAsync(long Id, CancellationToken ct);
        Task<long> SaveDocummentAsync(byte[] doc, string fileName = "", int createdBy = 0, long parentID = 0, long fkId = 0, int groupTableId = 0, string tmpGuid = "", int documentTypeId = 0);
        Task<IEnumerable<DocumentDto>> GetFilteredDocumentsAsync(long fkid, int tableid, CancellationToken ct);
        Task<int> DeleteDocumentByIdAsync(long id, CancellationToken ct);

    }
}
