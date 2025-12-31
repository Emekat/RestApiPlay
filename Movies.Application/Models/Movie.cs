using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required int YearOfRelease { get; init; }
    public List<string> Genres { get; init; } = new();
    public string Slug => GenerateSlug();
    private string GenerateSlug()
    {
        //"[^0-9A-Za-z _-]"  
        var slugTitle = MyRegex().Replace(Title,  string.Empty)
            .ToLower().Replace(" ", "-");
        return $"{slugTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
   private static partial Regex MyRegex();
}