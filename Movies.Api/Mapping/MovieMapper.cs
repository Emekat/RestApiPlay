using Movies.Application.Models;
using Movies.Contract.Responses;
using Riok.Mapperly.Abstractions;

namespace Movies.Api.Mapping;

[Mapper]
public partial class MovieMapper
{
    public partial MovieResponse MovieToMovieResponse(Movie movie);
    
}