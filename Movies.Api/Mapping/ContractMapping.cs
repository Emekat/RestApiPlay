using Movies.Application.Models;
using Movies.Contract.Requests;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapUpdateMovieRequestToMovie(this UpdateMovieRequest request, Guid id)
    {
        return new Movie()
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }
}