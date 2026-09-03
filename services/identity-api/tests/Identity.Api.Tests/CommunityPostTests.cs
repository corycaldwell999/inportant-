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

public class CommunityPostTests : IClassFixture<CommunityPostWebApplicationFactory>
{
    private readonly CommunityPostWebApplicationFactory _factory;

    public CommunityPostTests(CommunityPostWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreatePost_CreatesCommunityPostAndListsIt()
    {
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "post-user@example.com",
            password = "Password123!",
            displayName = "Post User"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "post-user@example.com",
            password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(loginPayload);
        Assert.False(string.IsNullOrWhiteSpace(loginPayload.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var communityResponse = await client.PostAsJsonAsync("/api/v1/communities", new
        {
            name = "Support Circle",
            description = "A community for peer support and reflection.",
            privacyLevel = "public"
        });

        Assert.Equal(HttpStatusCode.Created, communityResponse.StatusCode);

        var communityPayload = await communityResponse.Content.ReadFromJsonAsync<CommunityResponse>();

        Assert.NotNull(communityPayload);
        Assert.False(Guid.Empty == communityPayload.Id);

        var createPostResponse = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            communityId = communityPayload.Id,
            title = "First check-in",
            content = "I am grateful for today's small wins.",
            postType = "text",
            visibility = "community"
        });

        Assert.Equal(HttpStatusCode.Created, createPostResponse.StatusCode);

        var postPayload = await createPostResponse.Content.ReadFromJsonAsync<PostResponse>();
        Assert.NotNull(postPayload);
        Assert.Equal("First check-in", postPayload.Title);

        var listResponse = await client.GetAsync($"/api/v1/posts?communityId={communityPayload.Id}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var listPayload = await listResponse.Content.ReadFromJsonAsync<PostListResponse>();
        Assert.NotNull(listPayload);
        Assert.NotNull(listPayload.Data);
        Assert.True(listPayload.Data.Count > 0);

        var commentResponse = await client.PostAsJsonAsync($"/api/v1/posts/{postPayload.Id}/comments", new
        {
            content = "Thank you for sharing this.",
            parentCommentId = (Guid?)null
        });

        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

        var commentPayload = await commentResponse.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(commentPayload);
        Assert.Equal("Thank you for sharing this.", commentPayload.Content);

        var reactionResponse = await client.PostAsJsonAsync($"/api/v1/posts/{postPayload.Id}/reactions", new
        {
            reactionType = "thinking-of-you"
        });

        Assert.Equal(HttpStatusCode.Created, reactionResponse.StatusCode);

        var invalidReactionResponse = await client.PostAsJsonAsync($"/api/v1/posts/{postPayload.Id}/reactions", new
        {
            reactionType = "downvote"
        });

        Assert.Equal(HttpStatusCode.BadRequest, invalidReactionResponse.StatusCode);
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
    private sealed record CommunityResponse(Guid Id, string Name, string Description, string Slug);
    private sealed record PostResponse(Guid Id, string Title, string Content, string PostType, string Visibility);
    private sealed record PostListResponse(List<PostResponse> Data);
    private sealed record CommentResponse(Guid Id, Guid PostId, string Content);
}

public class CommunityPostWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
            services.RemoveAll<IdentityDbContext>();

            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase("CommunityPostTestsDb");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
