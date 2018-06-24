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
    public partial class MainWindow
    {
        HubConnection _connection;
        IHubProxy proxy;
        string _remoteGroup;
        readonly string _mediaCenterProcessName;
        readonly string _relayUrl;
        SynchronizationContext _magic;
        private readonly IMovieService _movieService;
        IntPtr _handle;
        
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
            _mediaCenterProcessName = ConfigurationManager.AppSettings["MediaCenterProcessName"] ?? string.Empty;
            if (string.IsNullOrEmpty(_mediaCenterProcessName))
            {
                Log(MediaHost.Resources.Messages.MediaCenterProcessNameNotConfigured);
            }

            _relayUrl = ConfigurationManager.AppSettings["RelayURL"] ?? string.Empty;
            if (string.IsNullOrEmpty(_relayUrl))
            {
                Log(MediaHost.Resources.Messages.RelayURLNotConfigured);
            }

            _movieService = new MovieService();
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
            if (!string.IsNullOrEmpty(ProcessToKill))
            {
                var process = Process.GetProcessesByName(ProcessToKill);
                if (process.Length > 0)
                {
                    process[0].Kill();
                    Thread.Sleep(3000);
                }
            }

            try
            {

                Process.Start(MediaPlayerPath,
                    $@"""{Path.Combine(MoviesFolderPath, $@"{name}\{fileName}")}""");
                Log(string.Format(MediaHost.Resources.Messages.MoviePlayed, name, fileName));
            }
            catch
            {
                Log(string.Format(MediaHost.Resources.Messages.ErrorTryingToPlay, name, fileName));
            }
        }

        private bool IsMediaCenterRunning()
        {
            if(!string.IsNullOrEmpty(_mediaCenterProcessName))
            {
                Process process = Process.GetProcessesByName(_mediaCenterProcessName).FirstOrDefault();

                if (process != null)
                {
                    _handle = process.MainWindowHandle;
                    return true;
                }
            }

            return false;
        }

        private void SendWindowsMessage(int commandId)
        {
            if (IsMediaCenterRunning())
            {
                SendMessageW(_handle, WM_APPCOMMAND, _handle, (IntPtr)commandId);
            }
            else
            {
                Log(string.Format(MediaHost.Resources.Messages.MediaCenterNotConfiguredOrNotRunning, _mediaCenterProcessName));
            }
        }

        private async Task InitializeConnection()
        {
            _remoteGroup = cmbGroupName.Text;
            _connection = new HubConnection(_relayUrl);
            proxy = _connection.CreateProxy("RelayHub");
            _magic = SynchronizationContext.Current;

            proxy.On("PlayPause", () =>
            {
                _magic.Post((_) =>
                {
                    Log("Play / Pause");
                    SendWindowsMessage(APPCOMMAND_MEDIA_PLAY_PAUSE);
                }, null);
            });

            proxy.On("MuteUnmute", () =>
            {
                _magic.Post((_) =>
                {
                    Log("Mute / Unmute");
                    SendWindowsMessage(APPCOMMAND_VOLUME_MUTE);
                }, null);
            });

            proxy.On("VolumeUp", () =>
            {
                _magic.Post((_) =>
                {
                    Log("Volume Up");
                    SendWindowsMessage(APPCOMMAND_VOLUME_UP);
                }, null);
            });

            proxy.On("VolumeDown", () =>
            {
                _magic.Post((_) =>
                {
                    Log("Volume Down");
                    SendWindowsMessage(APPCOMMAND_VOLUME_DOWN);
                }, null);
            });

            proxy.On("Stop", () =>
            {
                _magic.Post((_) =>
                {
                    Log("Stop");
                    SendWindowsMessage(APPCOMMAND_MEDIA_STOP);
                }, null);
            });

            proxy.On("PlayMovie", (movieId) =>
            {
                _magic.Post((_) =>
                {
                    Movie movie = _movieService.GetMovieById((int)movieId);
                    if (movie != null)
                    {
                        PlayMovie(movie.Name, movie.FileName);
                    }
                    Log(string.Format("Playing Movie Id: " + movieId.ToString()));
                }, null);
            });

            try
            {
                await _connection.Start();
                Log("After connection.Start()");
                await proxy.Invoke("JoinRelay", _remoteGroup);
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