using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using SpotifyAPI.Web;
using FuzzyString;
using System.Runtime.InteropServices;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        public static MusicBeeApiInterface mbApiInterface;

        private PluginInfo about = new PluginInfo();

        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public Plugin()
        {
            // taken from https://github.com/sll552/DiscordBee/blob/master/DiscordBee.cs
            AppDomain.CurrentDomain.AssemblyResolve += (object _, ResolveEventArgs args) =>
            {
                string assemblyFile = args.Name.Contains(",")
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name;

                assemblyFile += ".dll";

                string absoluteFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                string targetPath = Path.Combine(absoluteFolder, "CopySpotify", assemblyFile);

                try
                {
                    return Assembly.LoadFile(targetPath);
                }
                catch (Exception ex)
                {
                    return null;
                }
            };
        }

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Copy Spotify URL";
            about.Description = "Allows you to copy the Spotify link for (almost) any song or album in your library";
            about.Author = "trev";
            about.Type = PluginType.General;
            about.VersionMajor = 1;
            about.VersionMinor = 2;
            about.Revision = 0;
            about.MinInterfaceVersion = 40;
            about.MinApiRevision = 52;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
            about.ConfigurationPanelHeight = 0;
            return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();

            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
                Label prompt = new Label();
                prompt.AutoSize = true;
                prompt.Location = new Point(0, 0);
                prompt.Text = "prompt:";
                TextBox textBox = new TextBox();
                textBox.Bounds = new Rectangle(60, 0, 100, textBox.Height);
                configPanel.Controls.AddRange(new Control[] { prompt, textBox });
            }
            return false;
        }

        public void SaveSettings()
        {
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
        }

        public async void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    SpotifyAuthHandler.spotifyTokenPath = System.IO.Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "spotify_token.json");
                    SpotifyAuthHandler.spotifyAuthPath = System.IO.Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "spotify_dev_token.json");
                    await SpotifyAuthHandler.SetupSpotifyClient();

                    if (SpotifyAuthHandler.Instance == null)
                        return;

                    mbApiInterface.MB_AddMenuItem($"context.Main/Copy Spotify URL", "", MenuClicked);
                    break;
            }
        }

        public void MenuClicked(object sender, EventArgs args)
        {
            mbApiInterface.Library_QueryFilesEx("domain=SelectedFiles", out string[] files);
            if (files == null) return;

            SearchRequest.Types searchType = files.Length == 1 ? SearchRequest.Types.Track : SearchRequest.Types.Album;
            Search(searchType, files[0]);
        }

        public async void Search(SearchRequest.Types searchType, string sourceFileUrl)
        {
            bool isTrack = searchType == SearchRequest.Types.Track;

            // Custom class, makes things slightly cleaner
            MBSong song = new MBSong(sourceFileUrl);

            // Creates a search string of "[item] [artist]", item changing depending on `searchType`
            string searchStr = CleanString((isTrack ? song.Name : song.AlbumName) + $" {song.Artist}");

            SearchRequest searchRequest = new SearchRequest(searchType, searchStr);
            SearchResponse response;

            try
            {
                response = await SpotifyAuthHandler.Instance.Search.Item(searchRequest);
            }
            catch (APIUnauthorizedException)
            {
                await SpotifyAuthHandler.RefreshDevKeyClient();
                response = await SpotifyAuthHandler.Instance.Search.Item(searchRequest);
            }

            SpotifyItem spotifyItem = FindItem(response, searchType, song);

            if (spotifyItem == null)
            {
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("That item cannot be found on Spotify.");
                return;
            }

            Clipboard.SetText(spotifyItem.Url);
            System.Media.SystemSounds.Exclamation.Play();
        }

        public SpotifyItem FindItem(SearchResponse searchResponse, SearchRequest.Types searchType, MBSong wantedSong)
        {
            if (searchResponse == null) return null;

            // Not the cleanest
            if (searchType == SearchRequest.Types.Track)
            {
                foreach (FullTrack track in searchResponse.Tracks.Items)
                {
                    if (SimilarStrings(track.Name, wantedSong.Name) && SimilarStrings(track.Artists[0].Name, wantedSong.Artist))
                        return new SpotifyItem(track);
                }
            }
            else if (searchType == SearchRequest.Types.Album)
            {
                foreach (SimpleAlbum album in searchResponse.Albums.Items)
                {
                    if (SimilarStrings(album.Name, wantedSong.AlbumName) && SimilarStrings(album.Artists[0].Name, wantedSong.Artist))
                        return new SpotifyItem(album);
                }
            }

            return null;
        }

        public bool SimilarStrings(string str1, string str2)
        {
            bool result = str1.ApproximatelyEquals(str2, FuzzyStringComparisonTolerance.Normal, [FuzzyStringComparisonOptions.UseOverlapCoefficient,
                FuzzyStringComparisonOptions.UseLongestCommonSubsequence,
                FuzzyStringComparisonOptions.UseLongestCommonSubstring]);
            return result;
        }

        public string CleanString(string str)
        {
            string newStr = Regex.Replace(str, "[^a-zA-Z0-9 -]", " "); // Remove non-alphanumeric characters
            return Regex.Replace(newStr, @"\s+", " "); // Remove duplicate spaces
        }
    }
}