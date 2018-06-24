using System;
using SignalR.Client.Hubs;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using Media.Application;
using Media.Entities;

namespace MediaClient
{
    public partial class MainWindow
    {
        HubConnection _connection;
        IHubProxy proxy;
        string _remoteGroup;
        string _url;
        List<Movie> _movies;
        readonly IMovieService movieService;


        public MainWindow()
        {
            InitializeComponent();

            movieService = new MovieService();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // hardcoded! 
            _url = "http://cuca.azurewebsites.net/";

            _remoteGroup = cmbGroupName.Text;
            _connection = new HubConnection(_url);
            proxy = _connection.CreateProxy("RelayHub");
            await _connection.Start();
            await proxy.Invoke("JoinRelay", _remoteGroup);
            grdMediaButtons.IsEnabled = true;
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                proxy.Invoke(btn.Tag.ToString(), _remoteGroup);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lstMovies.Items.Clear();
            txtSinopsis.Text = string.Empty;
            txtSearch.Text = string.Empty;

            _movies = movieService.GetAllMovies().ToList();
            _movies.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));
            foreach (var movie in _movies)
            {
                lstMovies.Items.Add(movie.Name);
            }
        }

        private Movie SelectedMovie
        {
            get
            {
                if (lstMovies.SelectedIndex > -1)
                {
                    return _movies[lstMovies.SelectedIndex];
                }

                return null;
            }
        }

        private void lstMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedMovie != null)
            {
                txtSinopsis.Text = SelectedMovie.Sinopsis;
            }
        }

        private void btnViewMovie_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMovie != null)
            {
                proxy.Invoke("PlayMovie", SelectedMovie.Id, _remoteGroup);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            int index = _movies.FindIndex(m => m.Name.ToUpper().Contains(txtSearch.Text.ToUpper()));
            if (index > -1)
            {
                lstMovies.SelectedIndex = index;
                lstMovies.ScrollIntoView(lstMovies.SelectedItem);
            }
        }
    }
}