namespace Gmsl.WebApi.IntegrationTests;

using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

public static class Settings
{
    public static string GetWebApiUrl()
    {
        var configuration = GetConfiguration();

        return configuration["webapi_test_subject_url"];
    }

    private static IConfiguration GetConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        return configuration;
    }
}
