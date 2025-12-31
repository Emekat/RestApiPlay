using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    public async Task InitializeAsync()
    {
        using var connection =  await _dbConnectionFactory.CreateConnectionAsync();
        
        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS movies (
                                                    id UUID PRIMARY KEY,
                                                    title TEXT NOT NULL,
                                                    slug TEXT NOT NULL UNIQUE,
                                                    yearofrelease integer NOT NULL
                                                );
                                      """);

        await connection.ExecuteAsync("""
                                      CREATE UNIQUE INDEX concurrently iF NOT EXISTS idx_movies_slug
                                      ON movies 
                                      using btree(slug);
                                      """);
        
        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS genres (
                                                    movieid UUID references movies(id),
                                                    name TEXT NOT NULL
                                                );
                                      """);
        
    }
}