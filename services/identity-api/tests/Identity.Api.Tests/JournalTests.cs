using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Identity.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Identity.Api.Tests;

public class JournalTests : IClassFixture<JournalWebApplicationFactory>
{
    private readonly JournalWebApplicationFactory _factory;

    public JournalTests(JournalWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task JournalEntry_IsCreatedAndListedOnlyForAuthenticatedOwner()
    {
        var client = _factory.CreateClient();
        var email = $"journal-{Guid.NewGuid():N}@example.com";

        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password123!",
            displayName = "Journal User"
        });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "Password123!"
        });
        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginPayload);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var createResponse = await client.PostAsJsonAsync("/api/v1/journal/entries", new
        {
            title = "A quiet moment",
            content = "I noticed one small thing that helped today.",
            moodRating = 7,
            moodDescription = "A little steadier",
            tags = new[] { "reflection", "support" }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var entry = await createResponse.Content.ReadFromJsonAsync<JournalEntryResponse>();

        Assert.NotNull(entry);
        Assert.True(entry.IsPrivate);
        Assert.Equal(7, entry.MoodRating);

        var listResponse = await client.GetAsync("/api/v1/journal/entries");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<JournalListResponse>();

        Assert.NotNull(list);
        Assert.Single(list.Data);
        Assert.Equal(entry.Id, list.Data[0].Id);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/journal/entries/{entry.Id}", new
        {
            title = "A steadier moment",
            content = "I noticed two small things that helped today.",
            moodRating = 8,
            moodDescription = "More grounded",
            tags = new[] { "reflection", "grounding" }
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedListResponse = await client.GetAsync("/api/v1/journal/entries");
        var updatedList = await updatedListResponse.Content.ReadFromJsonAsync<JournalListResponse>();
        Assert.NotNull(updatedList);
        Assert.Equal("A steadier moment", updatedList.Data[0].Title);
        Assert.Equal(8, updatedList.Data[0].MoodRating);

        var invalidMoodResponse = await client.PostAsJsonAsync("/api/v1/journal/entries", new
        {
            content = "This should not be saved.",
            moodRating = 11
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidMoodResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/v1/journal/entries/{entry.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var emptyListResponse = await client.GetAsync("/api/v1/journal/entries");
        var emptyList = await emptyListResponse.Content.ReadFromJsonAsync<JournalListResponse>();
        Assert.NotNull(emptyList);
        Assert.Empty(emptyList.Data);
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
    private sealed record JournalEntryResponse(Guid Id, string? Title, string Content, int? MoodRating, bool IsPrivate);
    private sealed record JournalListResponse(List<JournalEntryResponse> Data);
}

public class JournalWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
            services.RemoveAll<IdentityDbContext>();
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseInMemoryDatabase("JournalTestsDb"));

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        });
    }
}
