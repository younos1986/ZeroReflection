namespace Api.Models;

public class FileUploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}

public class FileUploadResponse
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class FileDownloadResponse
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
}
