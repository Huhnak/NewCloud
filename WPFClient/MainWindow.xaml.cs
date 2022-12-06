using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client;
        public MainWindow()
        {
            InitializeComponent();
            client = new HttpClient();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CentralTextBox.Text = "Process has started";
            Dictionary<string, string> dict1 = new Dictionary<string, string>()
            {
                {"Username", usernameTextBox.Text},
                {"Password", passwordTextBox.Text},
            };
            var stringContent1 = new StringContent(JsonConvert.SerializeObject(dict1), Encoding.UTF8, "application/json");
            var response = client.PostAsync($"http://localhost:5164/login", stringContent1).Result;
            var token = response.Content.ReadAsStringAsync().Result;
            try
            {
                var accessToken = JObject.Parse(token).SelectToken("value.access_token").ToString();
                Console.WriteLine($"token: {accessToken}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            catch
            {
                return;
            }
            var response2 = client.GetAsync($"http://localhost:5164/data").Result;
            var data = response2.Content.ReadAsStringAsync().Result;
            //var response = client.GetAsync($"http://localhost:5000/{str}").Result;
            //var response = client.GetAsync($"http://localhost:5164/{str}").Result;

            CentralTextBox.Text = data;
        }
    }
}
