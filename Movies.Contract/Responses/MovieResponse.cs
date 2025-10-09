namespace Movies.Contract.Responses;

public class MovieResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public int YearOfRelease { get; init; }
    public IEnumerable<string> Genres { get; init; } = Enumerable.Empty<string>();
}