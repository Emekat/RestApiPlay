namespace Movies.Contract.Requests;

public class CreateMovieRequest
{
    public required string Title { get; init; }
    public int YearOfRelease { get; init; }
    public IEnumerable<string> Genres { get; init; } = Enumerable.Empty<string>();
}