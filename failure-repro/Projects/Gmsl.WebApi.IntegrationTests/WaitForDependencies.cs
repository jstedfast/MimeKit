namespace Gmsl.WebApi.IntegrationTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Polly;

    [SetUpFixture]
    [Explicit]
    public class WaitForDependencies
    {
        [OneTimeSetUp]
        public async Task Wait_for_dependencies_to_exist()
        {
            await WaitUntilOkayAsync();
        }

        private static async Task WaitUntilOkayAsync()
        {
            var url = Settings.GetWebApiUrl();

            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode != HttpStatusCode.OK)
                .WaitAndRetryAsync(
                    30,
                    _ => TimeSpan.FromSeconds(2),
                    onRetry: (_, _) =>
                    {
                        Console.WriteLine($"Failed to locate '{url}' Web API. Retrying...");
                    });

            var response = await retryPolicy.ExecuteAsync(async () => await GetWebApiStatusAsync(url));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"'{url}' Web API does not exist");
            }
        }

        private static async Task<HttpResponseMessage> GetWebApiStatusAsync(string url)
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri(url) };

            var response = await httpClient.GetAsync("status");

            return response;
        }
    }
}
