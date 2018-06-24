using Media.Data;
using Media.Entities;
using System.Collections.Generic;

namespace Media.Application
{
    public interface IMovieService
    {
        IEnumerable<Movie> GetAllMovies();

        Movie GetMovieById(int movieId);

        List<Movie> SearchMovies(string searchString);
    }
}
