using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using FourSquare.SharpSquare.Entities;
using FoursquareService;
using Microsoft.Phone.Controls;
using TranslatorService.Speech;

namespace LoudVenues
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SpeechSynthesizer speech;
        GeoCoordinateWatcher geoWatcher = null;
        private VenuesProvider fourSquare;
        private SoundLibrary.SoundCube cube;
        
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            speech = new SpeechSynthesizer();
            fourSquare = new VenuesProvider("P2TJQ3QA2F2XMCFRBVNUTSNWXQDTWZGXFCPV404WVOAIHDCD", "V4M41DEGW11WORMOALN4T30CKLHCHYXE5KDZ0J3QAJQUNN4F");
            geoWatcher = new GeoCoordinateWatcher();
            cube = new SoundLibrary.SoundCube();
        }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        private void fourSquare_ProcessResponseEnded(object sender, ResultEventArgs e)
        {
            if (e.HasFail)
            {
                MessageBox.Show("Error Obteniendo Proximo Lugares");
                return;
            }

            var list = (List<Venue>)e.Result;
            Dispatcher.BeginInvoke(() => NearList.Items.Clear());
            foreach (Venue item in list)
            {
                if (item == null)
                    continue;
                string textToSpeech = string.Format("at {0} meters, you have a {1} and it is called {2}"
                        , item.location.distance, item.categories[0].name, item.name);

                Dispatcher.BeginInvoke(() =>
                {
                    NearList.Items.Add(new TextBlock() { Text = textToSpeech, TextWrapping = TextWrapping.Wrap });
                });

                string leng = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                speech.GetSpeakStreamAsync(textToSpeech, false, leng);
            }
        }

        private void geoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Latitude = e.Position.Location.Latitude;
            Longitude = e.Position.Location.Longitude;
            CurrLocText.Text = string.Format("Latitud {0} - Longitud:{1}", Latitude, Longitude);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            BusyIndicator.Visibility = Visibility.Collapsed;
            GetCurrentLocation();
            speech.GetSpeakStreamCompleted += new EventHandler<GetSpeakStreamEventArgs>(speech_GetSpeakStreamCompleted);
            fourSquare.ProcessResponseEnded += new EventHandler<ResultEventArgs>(fourSquare_ProcessResponseEnded);
            geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(geoWatcher_PositionChanged);
        }

        private void GetCurrentLocation()
        {
            geoWatcher.Start();
        }

        private void speech_GetSpeakStreamCompleted(object sender, GetSpeakStreamEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                cube.AddToQueue(e.Stream);
                cube.StartPlayingQueue();
            });
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            BusyIndicator.Visibility = Visibility.Visible;
            string text = SpeechText.Text;
            string leng = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

            speech.GetSpeakStreamAsync(text, false, leng);
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            fourSquare.GetNearVenues(Latitude, Longitude);
        }
    }
}