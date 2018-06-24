using System.Web.Mvc;

namespace MobileClient.Controllers
{
    public class MediaPlayerController : Controller
    {
        public ActionResult Initialize()
        {
            return View("MediaPlayer");
        }

        public ActionResult PlayPause()
        {
            SignalRHub.Proxy.Invoke("PlayPause", SignalRHub.remoteGroup);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Stop()
        {
            SignalRHub.Proxy.Invoke("Stop", SignalRHub.remoteGroup);
            return RedirectToAction("Index", "Home");
        }
        
        public ActionResult VolumeUp()
        {
            SignalRHub.Proxy.Invoke("VolumeUp", SignalRHub.remoteGroup);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult VolumeDown()
        {
            SignalRHub.Proxy.Invoke("VolumeDown", SignalRHub.remoteGroup);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Mute()
        {
            SignalRHub.Proxy.Invoke("MuteUnmute", SignalRHub.remoteGroup);
            return RedirectToAction("Index", "Home");
        }
    }
}