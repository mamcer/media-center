using Media.Data;
using Media.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Media.Application
{
    public class MovieService : IMovieService
    {
        XDocument doc;

        public XDocument XMLDataSource 
        { 
            get
            {
                if (doc == null)
                {
                    doc = XDocument.Load(@"http://cucamovies.azurewebsites.net/Movies.xml");
                }


                return doc;
            }
        }

        public IEnumerable<Movie> GetAllMovies()
        {
            var result = from item in XMLDataSource.Descendants("Movie")
                         select new Movie
                         {
                             Id = Convert.ToInt32(item.Element("Id").Value),
                             Sinopsis = item.Element("Sinopsis").Value,
                             Name = item.Element("Name").Value,
                             FileName = item.Element("FileName").Value
                         };

            return result;
        }

        public Movie GetMovieById(int movieId)
        {
            string stringMovieId = movieId.ToString();
            var result = from item in XMLDataSource.Descendants("Movie")
                         where item.Element("Id").Value == stringMovieId
                         select new Movie
                         {
                             Id = Convert.ToInt32(item.Element("Id").Value),
                             Sinopsis = item.Element("Sinopsis").Value,
                             Name = item.Element("Name").Value,
                             FileName = item.Element("FileName").Value
                         };

            return result.FirstOrDefault();
        }


        public List<Movie> SearchMovies(string searchString)
        {
            var result = from item in XMLDataSource.Descendants("Movie")
                         where item.Element("Name").Value.ToUpper().StartsWith(searchString.ToUpper())
                         select new Movie
                         {
                             Id = Convert.ToInt32(item.Element("Id").Value),
                             Sinopsis = item.Element("Sinopsis").Value,
                             Name = item.Element("Name").Value,
                             FileName = item.Element("FileName").Value
                         };

            List<Movie> values = result.ToList();
            values.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));

            return values;
        }
    }
}
