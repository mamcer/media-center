using Media.Application;
using Media.Entities;
using SignalR.Client.Hubs;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MediaHost
{
    public partial class MainWindow : Window
    {
        HubConnection connection = null;
        IHubProxy proxy = null;
        string remoteGroup;
        string mediaCenterProcessName;
        string relayURL;
        SynchronizationContext magic;
        IMovieService movieService;
        IntPtr handle;
        
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;
        private const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
        private const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;
        private const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
        private const int APPCOMMAND_MEDIA_STOP = 0xD0000;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            mediaCenterProcessName = ConfigurationManager.AppSettings["MediaCenterProcessName"] ?? string.Empty;
            if (string.IsNullOrEmpty(mediaCenterProcessName))
            {
                Log(MediaHost.Resources.Messages.MediaCenterProcessNameNotConfigured);
            }

            relayURL = ConfigurationManager.AppSettings["RelayURL"] ?? string.Empty;
            if (string.IsNullOrEmpty(relayURL))
            {
                Log(MediaHost.Resources.Messages.RelayURLNotConfigured);
            }

            movieService = new MovieService();
        }

        public string MediaPlayerPath
        {
            get
            {
                string mediaPlayerPathKey = "MediaPlayerPath";
                if (ConfigurationManager.AppSettings[mediaPlayerPathKey] != null)
                {
                    return ConfigurationManager.AppSettings[mediaPlayerPathKey];
                }

                return string.Empty;
            }
        }

        public string MoviesFolderPath
        {
            get
            {
                string moviesFolderPath = "MoviesFolderPath";
                if (ConfigurationManager.AppSettings[moviesFolderPath] != null)
                {
                    return ConfigurationManager.AppSettings[moviesFolderPath];
                }

                return string.Empty;
            }
        }

        public string ProcessToKill
        {
            get
            {
                string processToKill = "ProcessToKill";
                if (ConfigurationManager.AppSettings[processToKill] != null)
                {
                    return ConfigurationManager.AppSettings[processToKill];
                }

                return string.Empty;
            }
        }

        private void PlayMovie(string name, string fileName)
        {
            if (!string.IsNullOrEmpty(this.ProcessToKill))
            {
                var process = Process.GetProcessesByName(this.ProcessToKill);
                if (process.Count() > 0)
                {
                    process[0].Kill();
                    System.Threading.Thread.Sleep(3000);
                }
            }

            try
            {

                Process.Start(this.MediaPlayerPath, string.Format(@"""{0}""", Path.Combine(this.MoviesFolderPath, string.Format(@"{0}\{1}", name, fileName))));
                this.Log(string.Format(MediaHost.Resources.Messages.MoviePlayed, name, fileName));
            }
            catch
            {
                this.Log(string.Format(MediaHost.Resources.Messages.ErrorTryingToPlay, name, fileName));
            }
        }

        private bool IsMediaCenterRunning()
        {
            if(!string.IsNullOrEmpty(mediaCenterProcessName))
            {
                Process process = Process.GetProcessesByName(mediaCenterProcessName).FirstOrDefault();

                if (process != null)
                {
                    this.handle = process.MainWindowHandle;
                    return true;
                }
            }

            return false;
        }

        private void SendWindowsMessage(int commandId)
        {
            if (IsMediaCenterRunning())
            {
                SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)commandId);
            }
            else
            {
                Log(string.Format(MediaHost.Resources.Messages.MediaCenterNotConfiguredOrNotRunning, mediaCenterProcessName));
            }
        }

        private async Task InitializeConnection()
        {
            remoteGroup = cmbGroupName.Text;
            connection = new HubConnection(relayURL);
            proxy = connection.CreateProxy("RelayHub");
            magic = SynchronizationContext.Current;

            proxy.On("PlayPause", () =>
            {
                magic.Post((_) =>
                {
                    Log(string.Format("Play / Pause"));
                    SendWindowsMessage(APPCOMMAND_MEDIA_PLAY_PAUSE);
                }, null);
            });

            proxy.On("MuteUnmute", () =>
            {
                magic.Post((_) =>
                {
                    Log(string.Format("Mute / Unmute"));
                    SendWindowsMessage(APPCOMMAND_VOLUME_MUTE);
                }, null);
            });

            proxy.On("VolumeUp", () =>
            {
                magic.Post((_) =>
                {
                    Log(string.Format("Volume Up"));
                    SendWindowsMessage(APPCOMMAND_VOLUME_UP);
                }, null);
            });

            proxy.On("VolumeDown", () =>
            {
                magic.Post((_) =>
                {
                    Log(string.Format("Volume Down"));
                    SendWindowsMessage(APPCOMMAND_VOLUME_DOWN);
                }, null);
            });

            proxy.On("Stop", () =>
            {
                magic.Post((_) =>
                {
                    Log(string.Format("Stop"));
                    SendWindowsMessage(APPCOMMAND_MEDIA_STOP);
                }, null);
            });

            proxy.On("PlayMovie", (movieId) =>
            {
                magic.Post((_) =>
                {
                    Movie movie = movieService.GetMovieById((int)movieId);
                    if (movie != null)
                    {
                        this.PlayMovie(movie.Name, movie.FileName);
                    }
                    Log(string.Format("Playing Movie Id: " + movieId.ToString()));
                }, null);
            });

            try
            {
                await connection.Start();
                Log("After connection.Start()");
                await proxy.Invoke("JoinRelay", remoteGroup);
                Log("After JoinRelay");
            }
            catch (Exception pants)
            {
                var foo = (WebException)pants.GetBaseException();
                StreamReader r = new StreamReader(foo.Response.GetResponseStream());
                string yousuck = r.ReadToEnd();
                Log(yousuck);
                throw;
            }
        }

        private void Log(string message)
        {
            txtLog.Text += DateTime.Now.ToString("HH:mm:ss - ") + message + Environment.NewLine;
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            await InitializeConnection();
        }
    }
}