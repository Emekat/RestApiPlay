using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required int YearOfRelease { get; init; }
    // Make Genres mutable so repositories can populate it after mapping
    public List<string> Genres { get; set; } = new();
    private string? _slug;
    // Allow Slug to be set (persisted value); if not set, compute from Title+Year
    public string Slug
    {
        get => _slug ?? GenerateSlug();
        set => _slug = value;
    }
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