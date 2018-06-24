using SignalR.Client.Hubs;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using Media.Application;
using Media.Entities;

namespace MediaClient
{
    public partial class MainWindow : Window
    {
        HubConnection connection = null;
        IHubProxy proxy = null;
        string remoteGroup;
        string url;
        List<Movie> movies;
        IMovieService movieService;


        public MainWindow()
        {
            InitializeComponent();

            movieService = new MovieService();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // hardcoded! 
            url = "http://cuca.azurewebsites.net/";

            remoteGroup = cmbGroupName.Text;
            connection = new HubConnection(url);
            proxy = connection.CreateProxy("RelayHub");
            await connection.Start();
            await proxy.Invoke("JoinRelay", remoteGroup);
            grdMediaButtons.IsEnabled = true;
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                proxy.Invoke(btn.Tag.ToString(), remoteGroup);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lstMovies.Items.Clear();
            txtSinopsis.Text = string.Empty;
            txtSearch.Text = string.Empty;

            movies = movieService.GetAllMovies().ToList();
            movies.Sort((x, y) => x.Name.CompareTo(y.Name));
            foreach (var movie in movies)
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
                    return movies[lstMovies.SelectedIndex];
                }

                return null;
            }
        }

        private void lstMovies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedMovie != null)
            {
                txtSinopsis.Text = this.SelectedMovie.Sinopsis;
            }
        }

        private void btnViewMovie_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedMovie != null)
            {
                proxy.Invoke("PlayMovie", this.SelectedMovie.Id, remoteGroup);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            int index = movies.FindIndex(m => m.Name.ToUpper().Contains(txtSearch.Text.ToUpper()));
            if (index > -1)
            {
                lstMovies.SelectedIndex = index;
                lstMovies.ScrollIntoView(lstMovies.SelectedItem);
            }
        }
    }
}