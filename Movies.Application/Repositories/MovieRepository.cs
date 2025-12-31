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
    
    public async Task<Movie> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        // Use an explicit transaction and execute the genre inserts using Dapper's ability
        // to execute the same statement for an enumerable of parameter objects.
        using var transaction = connection.BeginTransaction();
        try
        {
            // Ensure slug uniqueness: if computed slug already exists, append a short suffix.
            var baseSlug = movie.Slug;
            var candidateSlug = baseSlug;
            while (await connection.ExecuteScalarAsync<int>("select count(1) from movies where slug = @slug", new { slug = candidateSlug }, transaction: transaction) > 0)
            {
                candidateSlug = baseSlug + "-" + Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            var result = await connection.ExecuteAsync(
                "INSERT INTO movies (id, title, slug, yearofrelease) VALUES (@Id, @Title, @Slug, @YearOfRelease);",
                new { movie.Id, movie.Title, Slug = candidateSlug, YearOfRelease = movie.YearOfRelease }, transaction: transaction);

            if (result > 0 && movie.Genres?.Any() == true)
            {
                var insertGenreSql = "insert into genres (movieid, name) values (@MovieId, @Name);";
                var parameters = movie.Genres.Select(g => new { MovieId = movie.Id, Name = g });
                // Dapper will execute the statement once per item in the enumerable.
                await connection.ExecuteAsync(insertGenreSql, parameters, transaction: transaction);
            }

            transaction.Commit();
            // Set the persisted slug on the returned movie so caller can use it
            movie.Slug = candidateSlug;
            return movie;
        }
        catch
        {
            try { transaction.Rollback(); } catch { /* swallow rollback exceptions */ }
            throw;
        }
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        // Movie objects with their Genres populated.
        var sql = @"
            SELECT m.id, m.title, m.slug, m.yearofrelease, g.name
            FROM movies m
            LEFT JOIN genres g ON g.movieid = m.id
            ORDER BY m.title;
        ";

        var movieLookup = new Dictionary<Guid, Movie>();

        await connection.QueryAsync<Movie, string, Movie>(sql,
            (movie, genre) =>
            {
                if (!movieLookup.TryGetValue(movie.Id, out var existing))
                {
                    existing = movie;
                    // Ensure Genres list exists and is empty before adding
                    existing.Genres = existing.Genres ?? new List<string>();
                    existing.Genres.Clear();
                    movieLookup.Add(existing.Id, existing);
                }

                if (!string.IsNullOrEmpty(genre))
                {
                    movieLookup[existing.Id].Genres.Add(genre);
                }

                return existing;
            }, splitOn: "name");

        return movieLookup.Values;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection =  await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                    select * from movies where id = @Id
                """, new { Id = id }));
        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                    select name from genres where movieid = @MovieId
                """, new { MovieId = id }));

        movie.Genres.AddRange(genres);
        return movie;
        
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "select count(1) from movies where id = @Id",
                new { Id = id }));
        return count > 0;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection =  await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                    select * from movies where slug = @slug
                """, new { slug }));
        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                    select name from genres where movieid = @id
                """, new { id = movie.Id }));

        movie.Genres.AddRange(genres);
        return movie;
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            new CommandDefinition("""
                                    delete from genres where movieid = @MovieId
                                  """, new { MovieId = movie.Id })
        );

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                        update movies set slug = @Slug, yearofrelease = @YearOfRelease,
                                                          title = @Title where id = @Id
               """, movie));
        
        transaction.Commit();
        return result > 0;
    }
    
    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
              delete from movies where id = @Id
            """, new { Id = id }
            ));
        
        transaction.Commit();
        return result > 0;
    }
}