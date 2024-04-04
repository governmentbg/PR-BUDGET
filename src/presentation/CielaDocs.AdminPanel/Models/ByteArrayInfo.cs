namespace CielaDocs.AdminPanel.Models
{
    public class ByteArrayInfo
    {
        public ByteArrayInfo(byte[] fileData, string fileName)
        {
            Data = fileData;
            FileName = fileName;
            FileExtension = System.IO.Path.GetExtension(FileName).ToLower();
        }

        public byte[] Data { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }
    }
}
