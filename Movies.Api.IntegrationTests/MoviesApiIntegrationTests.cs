using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using Movies.Contract.Requests;
using Movies.Contract.Responses;

namespace Movies.Api.IntegrationTests;

public class MoviesApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MoviesApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostMovie_ReturnsCreated_And_GetReturnsSameMovie()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createRequest = new CreateMovieRequest
        {
            Title = "Integration Test Movie",
            YearOfRelease = 2025,
            Genres = new[] { "Drama", "Test" }
        };

        // Act - Create
        var postResponse = await client.PostAsJsonAsync("api/movies", createRequest);

        // Assert - Created
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Read Location header
        postResponse.Headers.Location.Should().NotBeNull();
        var location = postResponse.Headers.Location!.ToString();

        // Act - Get the created movie
        var getResponse = await client.GetAsync(location);

        // Assert - OK
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var movie = await getResponse.Content.ReadFromJsonAsync<MovieResponse>();
        movie.Should().NotBeNull();
        movie!.Title.Should().Be(createRequest.Title);
        movie.YearOfRelease.Should().Be(createRequest.YearOfRelease);
        movie.Genres.Should().BeEquivalentTo(createRequest.Genres);
    }
}

