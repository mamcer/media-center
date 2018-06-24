using Media.Application;
using Media.Entities;
using System.Web.Mvc;

namespace MobileClient.Controllers
{
    public class MovieController : Controller
    {
        public ActionResult ShowDetails(int movieId)
        {
            IMovieService movieService = new MovieService();
            Movie movie = movieService.GetMovieById(movieId);

            return View("Detail", movie);
        }

        public ActionResult ViewMovie(int movieId)
        {
            SignalRHub.Proxy.Invoke("PlayMovie", movieId, SignalRHub.remoteGroup);
            
            return RedirectToActionPermanent("Index", "Home");
        }
    }
}