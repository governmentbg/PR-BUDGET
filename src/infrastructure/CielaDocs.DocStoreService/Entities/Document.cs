using System;
using System.ComponentModel.DataAnnotations;

namespace CielaDocs.DocStoreService.Entities
{
    public class Document
    {
        [Key]
        public long Id { get; set; }
        public string DocType { get; set; }
        public string DocName { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public long? ParentID { get; set; }
        public bool? IsDeleted { get; set; }
        public long? FkID { get; set; }
        public int? GroupTableID { get; set; }
        public string TmpGuid { get; set; }
        public int? DocumentTypeID { get; set; }
        public int? StatusID { get; set; }

    }
}
