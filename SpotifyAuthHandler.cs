using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

using Newtonsoft.Json;

namespace MusicBeePlugin
{
    public static class SpotifyAuthHandler
    {
        public static string spotifyTokenPath;
        private static string clientId = "59f6ab927e5143f48ce9dc850340b767";

        public async static Task<SpotifyClient> GetSpotifyClient()
        {
            if (!File.Exists(spotifyTokenPath))
            {
                DialogResult result = MessageBox.Show("CopySpotifyURL requires you to connect a Spotify account to function, would you like to do that now?",
                    "Spotify Connection",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                    await BeginWebAuth();
                else return null;
            }

            string json = File.ReadAllText(spotifyTokenPath);
            PKCETokenResponse token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

            if (token.IsExpired)
            {
                DialogResult result = MessageBox.Show("CopySpotifyURL needs a refreshed Spotify token, would you like to get that now?",
                    "Spotify Connection",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                    await BeginWebAuth();
                else return null;
            }

            PKCEAuthenticator authenticator = new PKCEAuthenticator(clientId, token, spotifyTokenPath);
            authenticator.TokenRefreshed += (sender, authToken) => File.WriteAllText(spotifyTokenPath, JsonConvert.SerializeObject(authToken));

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(authenticator);
            return new SpotifyClient(config);
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
            catch { Console.WriteLine("Unable to open URL, manually open: {0}", uri); }

            // prevents it from returning early
            while (!hasReceived)
                await Task.Delay(500);
        }
    }
}
