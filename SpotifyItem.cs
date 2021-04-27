using SpotifyAPI.Web;

namespace MusicBeePlugin
{
    public class SpotifyItem
    {
        public FullTrack FullTrack { get; private set; }
        public SimpleAlbum SimpleAlbum { get; private set; }

        public string Url { get; private set; }

        public SpotifyItem(FullTrack track)
        {
            FullTrack = track;
            Url = SpotifyUrlFromUri(FullTrack.Uri);
        }

        public SpotifyItem(SimpleAlbum album)
        {
            SimpleAlbum = album;
            Url = SpotifyUrlFromUri(SimpleAlbum.Uri);
        }

        public string SpotifyUrlFromUri(string uri)
        {
            string[] splitUri = uri.Split(':');
            string url = $"https://open.spotify.com/{splitUri[1]}/{splitUri[2]}";
            return url;
        }
    }
}
