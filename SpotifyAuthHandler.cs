using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MusicBeePlugin
{
    public static class SpotifyAuthHandler
    {
        public static SpotifyClient Instance;

        public static string spotifyTokenPath;
        public static string spotifyAuthPath;
        private static string clientId = "59f6ab927e5143f48ce9dc850340b767";

        public async static Task SetupSpotifyClient()
        {
            {
                using System.Net.WebClient client = new();
                try
                {
                    // using robots.txt cause its smaller than downloading the whole front page
                    _ = await client.DownloadStringTaskAsync("https://spotify.com/robots.txt");
                }
                catch
                {
                    MessageBox.Show("Unable to connect to the internet, CopySpotifyURL will not work.");
                    return;
                }
            }

            bool doesUserTokenExist = File.Exists(spotifyTokenPath);
            bool doesDevIdsExist = File.Exists(spotifyAuthPath);
            if (!doesUserTokenExist && !doesDevIdsExist)
            {
                DialogResult result = MessageBox.Show(
                    "CopySpotifyURL requires you to connect a Spotify account or developer app IDs to function, " +
                    "would you like to add one now?",
                    "Spotify Connection",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    CustomMsgBox msgBox = new CustomMsgBox(
                        "Please choose an authentication option.\n" +
                        "Do note that Web Auth tokens expire very quickly",
                        "Authentication Options");
                    msgBox.SetButtons("Web Auth", "Developer App IDs");
                    msgBox.ShowDialog();

                    if (msgBox.DialogBoxResult == DialogBoxResult.Button1)
                    {
                        await BeginWebAuth();
                        doesUserTokenExist = File.Exists(spotifyTokenPath);
                    }
                    else if (msgBox.DialogBoxResult == DialogBoxResult.Button2)
                    {
                        using (DevTokenForm form = new DevTokenForm())
                            form.ShowDialog();
                        doesDevIdsExist = File.Exists(spotifyAuthPath);
                    }
                }
                else return;
            }

            SpotifyClientConfig spotifyConfig = null;

            if (doesDevIdsExist)
            {
                string jsonData = File.ReadAllText(spotifyAuthPath);
                JObject newJObject = JObject.Parse(jsonData);
                string devClientId = newJObject["client_id"]?.ToString();
                string devClientSecret = newJObject["client_secret"]?.ToString();

                spotifyConfig = SpotifyClientConfig.CreateDefault();
                ClientCredentialsRequest request = new ClientCredentialsRequest(devClientId, devClientSecret);
                ClientCredentialsTokenResponse response = await new OAuthClient(spotifyConfig).RequestToken(request);
                spotifyConfig = spotifyConfig.WithToken(response.AccessToken);
            }
            else if (doesUserTokenExist)
            {
                string json = File.ReadAllText(spotifyTokenPath);
                PKCETokenResponse token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

                if (token.IsExpired)
                {
                    DialogResult result = MessageBox.Show("CopySpotifyURL needs a refreshed Spotify token, would you like to get that now?",
                        "Spotify Connection",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                        await BeginWebAuth();
                    else return;
                }

                PKCEAuthenticator authenticator = new PKCEAuthenticator(clientId, token, spotifyTokenPath);
                authenticator.TokenRefreshed += (sender, authToken) => File.WriteAllText(spotifyTokenPath, JsonConvert.SerializeObject(authToken));

                spotifyConfig = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
            }

            Instance = new SpotifyClient(spotifyConfig);
        }

        public static async Task RefreshDevKeyClient()
        {
            string jsonData = File.ReadAllText(spotifyAuthPath);
            JObject newJObject = JObject.Parse(jsonData);
            string devClientId = newJObject["client_id"]?.ToString();
            string devClientSecret = newJObject["client_secret"]?.ToString();

            SpotifyClientConfig spotifyConfig = SpotifyClientConfig.CreateDefault();
            ClientCredentialsRequest request = new ClientCredentialsRequest(devClientId, devClientSecret);
            ClientCredentialsTokenResponse response = await new OAuthClient(spotifyConfig).RequestToken(request);
            spotifyConfig = spotifyConfig.WithToken(response.AccessToken);
            Instance = new SpotifyClient(spotifyConfig);
        }

        private async static Task BeginWebAuth()
        {
            (string, string) tuple = PKCEUtil.GenerateCodes(120);

            Uri localhostUri = new Uri("http://localhost:5000/callback");

            var loginRequest = new LoginRequest(localhostUri, clientId, LoginRequest.ResponseType.Code);
            loginRequest.CodeChallengeMethod = "S256";
            loginRequest.CodeChallenge = tuple.Item2;

            bool hasReceived = false;

            Uri uri = loginRequest.ToUri();
            EmbedIOAuthServer server = new EmbedIOAuthServer(localhostUri, 5000);
            server.PkceReceived += async (sender, response) =>
            {
                await server.Stop();

                PKCETokenResponse tokenResponse = await new OAuthClient().RequestToken(new PKCETokenRequest(clientId, response.Code, server.BaseUri, tuple.Item1));
                File.WriteAllText(spotifyTokenPath, JsonConvert.SerializeObject(tokenResponse));
                hasReceived = true;
            };
            await server.Start();

            try { BrowserUtil.Open(uri); }
            catch { MessageBox.Show("Unable to open URL, manually open: " + uri); }

            // prevents it from returning early
            while (!hasReceived)
                await Task.Delay(500);
        }
    }
}
