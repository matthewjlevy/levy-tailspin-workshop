using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TailspinToys.Api;
using TailspinToys.Api.Models;

namespace TailspinToys.Api.Tests;

public class TestPublishersRoutes : IDisposable
{
    private readonly string _dbPath;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private static readonly string[] TestPublisherNames =
    [
        "DevGames Inc",
        "Scrum Masters"
    ];

    private const string PublishersApiPath = "/api/publishers";

    public TestPublishersRoutes()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"TestDb_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={_dbPath}";

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TailspinToysContext>();
        SeedTestData(db);
    }

    private static void SeedTestData(TailspinToysContext db)
    {
        db.Publishers.AddRange(TestPublisherNames.Select(name => new Publisher
        {
            Name = name
        }));
        db.SaveChanges();
    }

    [Fact]
    public async Task GetPublishers_ReturnsAllPublishersWithIdAndName()
    {
        var response = await _client.GetAsync(PublishersApiPath);
        var data = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(data);
        Assert.Equal(TestPublisherNames.Length, data.Count);

        for (var i = 0; i < data.Count; i++)
        {
            var publisher = data[i];

            Assert.Equal(2, publisher.Count);
            Assert.True(publisher.ContainsKey("id"));
            Assert.True(publisher.ContainsKey("name"));
            Assert.Equal(TestPublisherNames[i], publisher["name"]?.ToString());
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();

        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
