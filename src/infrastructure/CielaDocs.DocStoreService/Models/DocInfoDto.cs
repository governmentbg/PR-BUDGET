namespace CielaDocs.DocStoreService.Models
{
    public record DocInfoDto(byte[] DocumentContent, string Filename, long Size);
}
