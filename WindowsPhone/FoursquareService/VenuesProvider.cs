using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FourSquare.SharpSquare.Entities;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FourSquare.SharpSquare.Core;
using Newtonsoft.Json;

namespace FoursquareService
{
    public class VenuesProvider
    {
        private WebConsumer consumer;
        public event EventHandler<ResultEventArgs> ProcessResponseEnded;

        public VenuesProvider(string clientId, string secretId)
        {
            ClientID = clientId;
            SecretID = secretId;
            consumer = new WebConsumer();
            consumer.ResponseEnded += new EventHandler<ResultEventArgs>(consumer_ResponseEnded);
        }

        private void consumer_ResponseEnded(object sender, ResultEventArgs e)
        {
            if (e.HasFail)
            {

            }
            else
            {
                string rawString = (string) e.Result;
                FourSquareMultipleResponse<Venue> venueResp = JsonConvert.DeserializeObject<FourSquareMultipleResponse<Venue>>(rawString);
                TriggerResponseEnded(sender, new ResultEventArgs(false, venueResp.response.venues));
            }
        }

        private void TriggerResponseEnded(object sender, ResultEventArgs resultEventArgs)
        {
            if (ProcessResponseEnded!=null)
            {
                ProcessResponseEnded(sender, resultEventArgs);
            }
        }

        public void GetNearVenues(double lat, double longitude)
        {
            string url = string.Format("https://api.foursquare.com/v2/venues/search?ll={0},{1}&client_id={2}&client_secret={3}&v=20130815&limit=10", lat, longitude, ClientID, SecretID);
            consumer.GetUrlAsync(url);
        }

        public string SecretID { get; set; }
        public string ClientID { get; set; }
    }
}
