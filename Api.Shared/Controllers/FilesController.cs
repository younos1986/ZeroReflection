using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private static readonly Dictionary<string, (string FileName, byte[] Content)> Store = new();

    [HttpPost]
    public FileUploadResponse Upload([FromBody] FileUploadRequest request)
    {
        var fileId = Guid.NewGuid().ToString("N")[..8];
        var bytes = Convert.FromBase64String(request.ContentBase64);
        Store[fileId] = (request.FileName, bytes);
        return new FileUploadResponse { FileId = fileId, FileName = request.FileName, Size = bytes.Length };
    }

    [HttpGet("{fileId}")]
    public FileDownloadResponse? Download([FromRoute] string fileId)
    {
        if (!Store.TryGetValue(fileId, out var file)) return null;
        return new FileDownloadResponse { FileName = file.FileName, ContentBase64 = Convert.ToBase64String(file.Content) };
    }
}
