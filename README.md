# Copy Spotify URL [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/S6S244CYE)
MusicBee plugin that allows you to copy the Spotify link for (almost) any song or album in your library

## Installation
1. Download the latest release
2. In the Plugin section of MusicBee's preferences select Add Plugin
3. Choose the zip that was downloaded
4. Restart MusicBee and connect your Spotify account

## Usage
1. Right click a song or album in your library
2. Select `Copy Spotify URL` and wait
3. When you hear a sound, the URL will be copied to your clipboard.
If you get a message box, the item could not be found on Spotify.
If you are sure that your item is on Spotify and the names are similar, please create an issue.

## How to Switch Auth Methods
As of now, I have not added a single button to change auth methods so it takes a tiny bit more work.
1. Press the keys `Windows + R` and a box labeled `Run` will appear.
2. In that box, put the following, `%appdata%\MusicBee` and press Enter.
NOTE: If you are using the Portable version of MusicBee, go to your MusicBee folder then AppData instead of using Run
3. Once File Explorer is open, delete the following files if they exist.
`spotify_token.json`
`spotify_dev_token.json`
4. Restart MusicBee and the plugin should ask you to choose a new auth method.
