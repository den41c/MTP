using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Google.Apis.Gmail.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Xceed.Wpf.Toolkit.Core;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace stp
{
    [Serializable]
    class AccountsForSerialization
    {
        public ObservableCollection<Account> Accounts;
    }

    [Serializable]
    public class Account
    {
        public bool Enabled { get; set; } = true;
        public bool ToDoEnable { get; set; } = true;
        public string Email { get; set; }

        [NonSerialized]
        public Events Events;
        public string PathToCredentials { get; set; }
        private byte A;
        private byte B;
        private byte G;
        private byte R;        
        [NonSerialized]
        private System.Windows.Media.Color _Color;
        public System.Windows.Media.Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
            }
        }
        public Account(bool enabled, string email, byte color1, byte color2, byte color3)
        {
            Enabled = enabled;
            Email = email;
            Color = System.Windows.Media.Color.FromRgb(color1,color2,color3);
        }
        public Account(bool enabled, string email, System.Windows.Media.Color color, string pathToCredentials)
        {
            Enabled = enabled;
            Email = email;
            Color = color;
            PathToCredentials = pathToCredentials;
        }
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            A = _Color.A;
            B = _Color.B;
            G = _Color.G;
            R = _Color.R;
        }
        [OnDeserialized]
        private void OnDeserialised(StreamingContext context)
        {
            _Color = System.Windows.Media.Color.FromArgb(A, R, G, B);
        }
    }

    static class Calendars
    {
        public static DateTime SelectedDateTime = DateTime.Now;

        public static AccountsForSerialization AccountsForSerialization = new AccountsForSerialization();

        public static ObservableCollection<Account> Accounts = new ObservableCollection<Account>();

        static string ApplicationName = "ToDo";
        static string[] Scopes = { CalendarService.Scope.Calendar, GmailService.Scope.GmailMetadata};

        static List<UserCredential> UserCredentials = new List<UserCredential>();

        public static void RunJob()
        {
            string[] fileNames;
            try
            {
                fileNames = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "todo_credentials"), "*", SearchOption.AllDirectories);
                if (fileNames.Length < 0)
                    AuthorizationNew();
                else
                    foreach (var fileName in fileNames)
                        Authorization(fileName);
            }
            catch (Exception)
            {
                AuthorizationNew();
            }
            foreach (var credential in UserCredentials)
            {
                LoadEvents(credential);
            }
        }

        public static void AuthorizationNew()
        {
            var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, "todo_credentials", Guid.NewGuid().ToString());

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            UserCredentials.Add(credential);
            Accounts.Add(new Account(true, LoadMail(credential), System.Windows.Media.Color.FromRgb(1,1,1), credPath));
            Accounts[Accounts.Count - 1].Events = LoadEvents(credential);
        }

        public static void Authorization(string fileName)
        {
            string credPath = Path.GetDirectoryName(fileName);
            var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            UserCredentials.Add(credential);
            foreach (var account in Accounts)
            {
                if (account.PathToCredentials == credPath)
                    account.Events = LoadEvents(credential);
            }
        }

        public static string LoadMail(UserCredential credential)
        {
            var service2 = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            var request2 = service2.Users.GetProfile("me");
            var response = request2.Execute();
            return response.EmailAddress;
        }

        public static Events LoadEvents(UserCredential credential)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = int.MaxValue;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            return request.Execute();
        }

        static void DoIt()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            Console.Read();
        }
    }
}
