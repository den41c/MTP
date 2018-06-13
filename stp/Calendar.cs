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

namespace stp
{
    class Account
    {
        public bool Enabled { get; set; } = true;
        public string Email { get; set; }
        public Color Color { get; set; }
        public Account(bool enabled, string email, int color)
        {
            Enabled = enabled;
            Email = email;
            Color = Color.FromArgb(color);
        }
        public Account(bool enabled, string email, Color color)
        {
            Enabled = enabled;
            Email = email;
            Color = color;
        }
    }

    static class Calendars
    {
        public static ObservableCollection<Account> Accounts = new ObservableCollection<Account>();

        static string ApplicationName = "ToDo";
        static string[] Scopes = { CalendarService.Scope.Calendar, GmailService.Scope.GmailMetadata};

        static List<UserCredential> UserCredentials = new List<UserCredential>();

        static public Events events;

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
            Accounts.Add(new Account(true, LoadMail(credential), Color.Aqua));
            LoadEvents(credential);
        }

        public static void Authorization(string fileName)
        {
            var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(Path.GetDirectoryName(fileName), true)).Result;
            UserCredentials.Add(credential);
            Accounts.Add(new Account(true, LoadMail(credential), Color.Aqua));
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

        public static void LoadEvents(UserCredential credential)
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
            events = request.Execute();
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
