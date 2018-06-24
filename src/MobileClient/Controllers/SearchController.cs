using Media.Application;
using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileClient.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/

        public ActionResult Search(SearchModel model)
        {
            ViewBag.Message = "Resultados para " + model.SearchString;

            IMovieService movieService = new MovieService();

            var movies = movieService.SearchMovies(model.SearchString);

            ViewBag.ResultCount = movies.Count();

            return View("SearchResults", movies);
        }
    }
}
