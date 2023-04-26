namespace Gmsl.WebApi.IntegrationTests;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;

// ReSharper disable once InconsistentNaming
[Category("Integration")]
[Explicit]
public class DuplicateTests
{
    [Test]
    public async Task Failing_mime_parsing([Range(1, 100)]int _)
    {
        var xml = await File.ReadAllTextAsync("TestEnvelope.xml");

        var envelope = XElement.Parse(xml);

        var response = await WebApiPostSoapMessageAsync("/mime/fails", envelope);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Successful_mime_parsing([Range(1, 100)]int _)
    {
        var xml = await File.ReadAllTextAsync("TestEnvelope.xml");

        var envelope = XElement.Parse(xml);

        var response = await WebApiPostSoapMessageAsync("/mime/succeeds", envelope);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private static async Task<HttpResponseMessage> WebApiPostSoapMessageAsync(
        string path,
        XElement envelope)
    {
        var mimeBuilder = new MimeBuilder();
        var content = mimeBuilder.BuildMimeContent(envelope);

        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri(Settings.GetWebApiUrl()),
        };

        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };

        return await httpClient.SendAsync(request);
    }
}
