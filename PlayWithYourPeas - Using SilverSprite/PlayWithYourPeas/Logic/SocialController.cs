using System;
using System.Net;
using Facebook;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PlayWithYourPeas.Data;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using PlayWithYourPeas.Engine.Services;

namespace PlayWithYourPeas.Logic
{
#if SILVERLIGHT
    using MessageBox = System.Windows.MessageBox;
    using System.Windows.Browser;
#endif

    public static class SocialController
    {
        private static FacebookClient _client;
        private static FacebookClient _app;
        private static User _user;

        /// <summary>
        /// Current connection state
        /// </summary>
        public static FacebookState Connection { get; private set; }

        /// <summary>
        /// Setups the application link.
        /// </summary>
        public static void Setup()
        {
            _app = new FacebookClient();
            _app.AppId = "223453524434397";
            _app.AppSecret = "3b6769b53e52b0d122d2572950d770bd";

            dynamic parameters = new ExpandoObject();
            parameters.client_id = _app.AppId;
            parameters.client_secret = _app.AppSecret;
            parameters.grant_type = "client_credentials";

            Task<Object> connect = _app.GetTaskAsync("https://graph.facebook.com/oauth/access_token", parameters);
            connect.ContinueWith(task => {
                try
                {
                    // Save the access token
                    _app.AccessToken = JObject.Parse(task.Result.ToString()).Value<String>("access_token");
                    Connection |= FacebookState.Connected;
                }
                catch (AggregateException aex)
                {
                    aex.Handle(e =>
                    {
                        if (e is FacebookOAuthException)
                        {

                        }

                        return true;
                    });
                }
            });
        }

        /// <summary>
        /// Connect socially
        /// </summary>
        /// <param name="token">Access token for this session</param>
        public static void Connect(String token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
#if SILVERLIGHT

                HtmlPage.Window.Invoke("ShowConnect");
#endif
                return;
            }

            MessageBox.Show(String.Format("Connecting with token: {0}", token));

            _client = new FacebookClient(token);
            _client.AppId = "223453524434397";
            _client.AppSecret = "3b6769b53e52b0d122d2572950d770bd";

            dynamic parameters = new ExpandoObject();
            parameters.access_token = _client.AccessToken;

            Task<Object> connect = _client.GetTaskAsync("https://graph.facebook.com/me", parameters);
            connect.ContinueWith(OnConnectReceived);
        }

        /// <summary>
        /// On Connected
        /// </summary>
        /// <param name="task"></param>
        private static void OnConnectReceived(Task<Object> task)
        {
            try
            {
                _user = new User(task.Result.ToString());

                // Load facebook achievements and progress
                PlayerProgress.Load(_user.Id);

                // Set Authorized
                Connection |= FacebookState.Authorized;

                //string profilePictureUrl = String.Format("/{0}/picture?type={1}", _user.Id, "square");

            }
            catch (AggregateException aex)
            {
                aex.Handle((e) =>
                    {
                        if (e is FacebookOAuthException)
                        {

                        }

                        return false;
                    });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void RegisterAchievements()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="achievement"></param>
        public static void Unlock(Achievement achievement)
        {
            if (!Connection.HasFlag(FacebookState.Connected) || !Connection.HasFlag(FacebookState.Authorized))
                return;

            dynamic parameters = new ExpandoObject();
            parameters.achievement = achievement.Url;
            parameters.access_token = _client.AccessToken;

            // Unlocks the achievement (user token)
            Task<Object> unlock = _client.PostTaskAsync("https://graph.facebook.com/me/playwithyourpeas:unlock", parameters);
            unlock.ContinueWith(task => achievement.FacebookId = JObject.Parse(task.Result.ToString()).Value<String>("id"));

            // Tries creating it (app token)
            dynamic otherparameters = new ExpandoObject();
            otherparameters.achievement = achievement.Url;
            otherparameters.access_token = _app.AccessToken;

            Task<Object> create = _app.PostTaskAsync("https://graph.facebook.com/app/achievements", otherparameters);
            create.ContinueWith(task =>
            {
                System.Diagnostics.Debug.WriteLine(task.Result.ToString());

                // Tries registering it (can return already registered or non existant)
                Task<Object> register = _app.PostTaskAsync("https://graph.facebook.com/me/achievements", otherparameters);
                register.ContinueWith(innertask => 
                    
                    System.Diagnostics.Debug.WriteLine(innertask.Result.ToString())

                    );

            });
        }

#if !SILVERLIGHT
        class MessageBox
        {
            public static void Show(String s)
            {
                Console.WriteLine(s);
            }
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        public struct User
        {
            /// <summary>
            /// User Data
            /// </summary>
            public String Id, Name, FirstName, LastName, Link, UserName;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="json"></param>
            public User(String json)
            {
                JObject jObject = JObject.Parse(json);
                this.Id = jObject["id"].Value<String>();
                this.Name = jObject["name"].Value<String>();
                this.FirstName = jObject["first_name"].Value<String>();
                this.LastName = jObject["last_name"].Value<String>();
                this.Link = jObject["link"].Value<String>();
                this.UserName = jObject["username"].Value<String>();
            }

            /// <summary>
            /// Creates JSON string
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return JObject.FromObject(this).ToString();
            }
        }

        [Flags]
        public enum FacebookState
        {
            None = 0,
            Connected = (1 << 0),
            Authorized = (1 << 1),
        }

    }
}
