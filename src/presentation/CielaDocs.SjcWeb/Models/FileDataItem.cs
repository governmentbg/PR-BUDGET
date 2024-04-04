using System.ComponentModel.DataAnnotations;

namespace CielaDocs.SjcWeb.Models
{
    public class FileDataItem
    {
        [Key]
        public string Key { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool IsDirectory { get; set; }

        public bool HasSubDirectories { get; set; }

    }
}
