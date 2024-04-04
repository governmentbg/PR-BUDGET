using System;

namespace CielaDocs.DocStoreService.Models
{
    public record DocumentDto( 
        long Id,
        string DocName , 
        string Description ,
        DateTime? LastModifiedOn,
        int LastModifiedBy,
        long? ParentID , 
        long? FkID,
        int? GroupTableID,
        string TmpGuid,
        int? DocumentTypeID,
        int? StatusID,
        string DocumentTypeName,
        string DocumentStatusName);
   
}
