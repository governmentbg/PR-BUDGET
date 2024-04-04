namespace CielaDocs.DocStoreService.Models
{
    public class RequestFileUpload
    {
        public string FileName { get; set; }
        public int? CreatedBy { get; set; }
        public long? ParentID { get; set; }
        public long? FkID { get; set; }
        public int GroupTableID { get; set; }
        public string TmpGuid { get; set; }
        public int DocumentTypeID { get; set; }
    }
}
