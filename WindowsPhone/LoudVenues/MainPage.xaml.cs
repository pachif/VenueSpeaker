using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using FourSquare.SharpSquare.Entities;
using FoursquareService;
using Microsoft.Phone.Controls;
using TranslatorService.Speech;
using Microsoft.Phone.Controls.Maps.Core;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Media;

namespace LoudVenues
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SpeechSynthesizer speech;
        GeoCoordinateWatcher geoWatcher = null;
        private VenuesProvider fourSquare;
        private SoundLibrary.SoundCube cube;
        private const double ZOOM = 15;

        public MainPage()
        {
            InitializeComponent();
            AppLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
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
            Dispatcher.BeginInvoke(() => ClearPlaces());
            foreach (Venue item in list)
            {
                if (item == null)
                    continue;
                
                string textToSpeech = string.Format("at {0} meters, you have a {1} and it is called {2}"
                    , item.location.distance, item.categories.Count == 0 ? "place" : item.categories[0].name, item.name);

                if (!AppLanguage.StartsWith("en"))
                {
                    textToSpeech = speech.Translate(textToSpeech, AppLanguage);
                }

                Dispatcher.BeginInvoke(() =>
                {
                    int count = places.Items.Count;

                    NearList.Items.Add(new TextBlock() { Text = string.Format("({0}) - {1}", count, textToSpeech), TextWrapping = TextWrapping.Wrap });

                    Pushpin p = CreatePushpin(item, count);
                    //now we add the Pushpin to the map
                    places.Items.Add(p);
                });

                speech.GetSpeakStreamAsync(textToSpeech, false, AppLanguage);
            }

            Dispatcher.BeginInvoke(() => miniMap.UpdateLayout());
        }

        private void ClearPlaces()
        {
            NearList.Items.Clear();
            places.Items.Clear();
            places.Items.Add(new Pushpin()
            {
                Location = new GeoCoordinate(Latitude, Longitude),
                Content = "You Are Here"
            });
        }

        private static Pushpin CreatePushpin(Venue item, int count)
        {
            Pushpin p = new Pushpin();
            //define it's graphic properties 
            p.Background = new SolidColorBrush(Colors.Yellow);
            p.Foreground = new SolidColorBrush(Colors.Black);
            //where to put the Pushpin 
            p.Location = new GeoCoordinate(item.location.lat, item.location.lng);
            //What to write on it
            p.Content = string.Format("{0} - {1}", count, item.name);
            return p;
        }

        private void geoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //TODO codigo de tolerancia de movimiento

            Latitude = e.Position.Location.Latitude;
            Longitude = e.Position.Location.Longitude;
            miniMap.SetView(new GeoCoordinate(Latitude, Longitude), ZOOM);
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
            fourSquare.GetNearVenues(Latitude, Longitude);
        }

        private void PauseApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            cube.StopPlayingQueue();
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            fourSquare.GetNearVenues(Latitude, Longitude);
        }

        private void NearList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textbox = (TextBlock)NearList.SelectedItem;
            string speak = textbox.Text.Remove(0, textbox.Text.IndexOf('-') + 1);
            speech.GetSpeakStreamAsync(speak,false,AppLanguage);
        }

        public string AppLanguage { get; set; }
    }
}