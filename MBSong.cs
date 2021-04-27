using static MusicBeePlugin.Plugin;

namespace MusicBeePlugin
{
    public class MBSong
    {
        public string Name { get; private set; }
        public string AlbumName { get; private set; }
        public string Artist { get; private set; }

        public MBSong(string sourceFileUrl)
        {
            Name = mbApiInterface.Library_GetFileTag(sourceFileUrl, MetaDataType.TrackTitle);
            AlbumName = mbApiInterface.Library_GetFileTag(sourceFileUrl, MetaDataType.Album);
            Artist = mbApiInterface.Library_GetFileTag(sourceFileUrl, MetaDataType.AlbumArtist);
        }
    }
}
