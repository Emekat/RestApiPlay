using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository :IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    private readonly List<Movie> _movies = new();
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var transaction = connection.BeginTransaction();
        
        var result = await connection.ExecuteAsync(
            "INSERT INTO movies (id, title, slug, yearofrelease) " +
            "VALUES (@Id, @Title, @Slug, @YearOfRelease);",
            movie);

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                                                                    insert into genres (movieid, name)
                                                                    values (@MovieId, @Name);
                                                                    """, new {MovieId = movie.Id, Name = genre}
                                                                    ));
            }
        }
        transaction.Commit();
        return result > 0;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movies = (await connection.QueryAsync<Movie>(
            new CommandDefinition(
                """
                    select * from movies
                """))).ToList();

        if (!movies.Any())
            return movies;

        // Populate genres for each movie (simple approach)
        foreach (var movie in movies)
        {
            var genres = await connection.QueryAsync<string>(
                new CommandDefinition(
                    """
                        select name from genres where movieid = @MovieId
                    """, new { MovieId = movie.Id }));

            movie.Genres.AddRange(genres);
        }

        return movies;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection =  await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                    select * from movies where id = @id
                """, new {id}));
        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                    select name from genres where movieid = @MovieId
                """, new {id}));

        movie.Genres.AddRange(genres);
        return movie;
        
    }

    public Task<bool> ExistsByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Movie?> GetBySlugAsync(string slug)
    {
        var movie = _movies.SingleOrDefault(x => x.Slug == slug);
        return Task.FromResult(movie);
    }

    public Task<bool> UpdateAsync(Movie movie)
    {
        var movieIndex = _movies.FindIndex(x => x.Id == movie.Id);
        if (movieIndex == -1)  
            return Task.FromResult(false);
        
        _movies[movieIndex] = movie;
        return Task.FromResult(true);
    }
    
    public Task<bool> DeleteByIdAsync(Guid id)
    {
        var removedCount = _movies.RemoveAll(x => x.Id == id);
        var removedMovie = removedCount > 0;
        return Task.FromResult(removedMovie);
    }
}