namespace Gmsl.WebApi.IntegrationTests;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using MimeKit;
using MimeKit.Utils;

internal class MimeBuilder
{
    public HttpContent BuildMimeContent(XElement envelope)
    {
        var multipart = new Multipart();

        var soapPart = BuildSoapPart(envelope);
        multipart.Add(soapPart);

        var content = GetMultipartContent(multipart);

        var httpContent = new ByteArrayContent(content);

        var contentType = new MediaTypeHeaderValue("multipart/related");
        contentType.Parameters.Add(new NameValueHeaderValue("boundary", $"\"{multipart.Boundary}\""));
        contentType.Parameters.Add(new NameValueHeaderValue("type", "\"application/soap+xml\""));
        contentType.Parameters.Add(new NameValueHeaderValue("start", $"\"{soapPart.ContentId}\""));

        httpContent.Headers.ContentType = contentType;
        httpContent.Headers.ContentLength = content.LongLength;

        return httpContent;
    }

    private static MimePart BuildSoapPart(XElement envelope)
    {
        var mimePart = new MimePart("application/soap+xml")
        {
            ContentId = MimeUtils.GenerateMessageId(),
            ContentTransferEncoding = ContentEncoding.Binary,
        };

        mimePart.ContentType.Charset = "utf-8";

        mimePart.Content = new MimeContent(CreateSoapStream(envelope));

        return mimePart;
    }

    private static byte[] GetMultipartContent(Multipart multipart)
    {
        using var stream = new MemoryStream();

        var formatOptions = new FormatOptions
        {
            NewLineFormat = NewLineFormat.Dos,
        };

        multipart.WriteTo(formatOptions, stream, true);

        return stream.ToArray();
    }

    private static Stream CreateSoapStream(XElement envelope)
    {
        var stream = new MemoryStream();
        var streamWriter = new StreamWriter(stream, new UTF8Encoding(false));

        envelope.Save(streamWriter, SaveOptions.DisableFormatting);

        stream.Position = 0;

        return stream;
    }
}
