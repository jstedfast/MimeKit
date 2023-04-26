namespace Gmsl.WebApi;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.IO;

[Route("/mime")]
[ApiController]
public class EntryController : ControllerBase
{
    [HttpPost("fails")]
    public async Task<IActionResult> ParseFailsAsync()
    {
        var contentType = ContentType.Parse(this.HttpContext.Request.ContentType);
        
        await MimeEntity.LoadAsync(
            contentType,
            this.HttpContext.Request.Body);

        return this.Ok();
    }

    [HttpPost("succeeds")]
    public async Task<IActionResult> ParseSucceedsAsync()
    {
        var contentType = ContentType.Parse(this.HttpContext.Request.ContentType);

        // FormatOptions.CloneDefault is internal, so using what looks like the equivalent
        var formatOptions = new FormatOptions
        {
            NewLineFormat = NewLineFormat.Dos
        };

        // contentType.Encode is internal, so using what looks like the equivalent
        var encodedContentType = contentType.ToString(formatOptions, Encoding.UTF8, encode: true) + "\r\n\r\n";
        
        using (var chainedStream = new ChainedStream())
        {
            chainedStream.Add(new MemoryStream(Encoding.UTF8.GetBytes(encodedContentType), false));
            chainedStream.Add(this.HttpContext.Request.Body, true);
            
            await MimeEntity.LoadAsync(ParserOptions.Default, chainedStream, false, CancellationToken.None);
        }

        return this.Ok();
    }
}
