using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Movies.Api;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contract.Requests;
using Movies.Contract.Responses;
using MovieMapper = Movies.Api.Mapping.MovieMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost(ApiEndpoints.Movies.Create, async (IMovieRepository movieRepository, CreateMovieRequest request) =>
{
    var movie = new Movie
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        YearOfRelease = request.YearOfRelease,
        Genres = request.Genres.ToList()
    };
    await movieRepository.CreateAsync(movie);

    var mapper = new MovieMapper();
    var response = mapper.MovieToMovieResponse(movie);
    return Results.CreatedAtRoute("Get", new { movie.Id }, response);
})
.WithOpenApi();

app.MapGet(ApiEndpoints.Movies.Get, async Task<Results<Ok<MovieResponse>, NotFound>> (IMovieRepository movieRepository, Guid id) =>
{
    var movie = await movieRepository.GetByIdAsync(id);
    if (movie == null)
        return TypedResults.NotFound();
    var mapper = new MovieMapper();
    var response = mapper.MovieToMovieResponse(movie);
    return TypedResults.Ok(response);
})
.WithName("Get");

app.MapGet(ApiEndpoints.Movies.GetAll, async (IMovieRepository movieRepository) =>
{
    var movies = await movieRepository.GetAllAsync();
    var mapper = new MovieMapper();
    var response = movies.Select(movie => mapper.MovieToMovieResponse(movie)).ToArray();
    var result = new MoviesResponse()
    {
        Items = response
    };
    return TypedResults.Ok(result);
    
});

app.MapPut(ApiEndpoints.Movies.Update, async (IMovieRepository movieRepository, UpdateMovieRequest request, [FromRoute] Guid id) =>
{
    var movie = request.MapUpdateMovieRequestToMovie(id);
    var updated = await movieRepository.UpdateAsync(movie);
    if(!updated)
        return Results.NotFound();
    var response = new MovieMapper().MovieToMovieResponse(movie);
    return Results.Ok(response);
});

app.MapDelete(ApiEndpoints.Movies.Delete, async (IMovieRepository movieRepository, Guid id) =>
{
   var deleted = await movieRepository.DeleteByIdAsync(id);
   return !deleted ? Results.NotFound() : Results.Ok();
});

app.Run();