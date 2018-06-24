using Media.Application;
using Media.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Last()
        {
            return View();
        }

        public ActionResult All()
        {
            IMovieService movieService = new MovieService(); 

            return View(movieService.GetAllMovies());
        }

        public ActionResult IndexSearch()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Search()
        {
            //ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult MediaControl()
        {
            return RedirectToAction("Initialize", "MediaPlayer");
        }

        public ActionResult Favourites()
        {
            IMovieService movieService = new MovieService();
            var movies = movieService.SearchMovies("tinker");
            
            return View(movies);
        }
    }
}
