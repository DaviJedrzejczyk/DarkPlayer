using TagLib;
using System;
using System.Collections.Generic;
using System.IO;
using TagLib.Mpeg;

#if ANDROID
using Android.Provider;
#endif

namespace Entities
{
    public class AudioItem
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public string Author { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public ImageSource? AlbumArt { get; set; }

        public AudioItem()
        {

        }

        public AudioItem(string name, string path, TimeSpan duration, string author, string album, string genre, ImageSource? imageSource)
        {
            Name = name;
            FilePath = path;
            Duration = duration;
            Author = author;
            Album = album;
            Genre = genre;
            AlbumArt = imageSource ?? ImageSource.FromFile("standartimage.png");
        }

        public List<AudioItem> LoadMusicsFromDirectory(string musicDirectory)
        {
            var audioItems = new List<AudioItem>();
#if ANDROID
            try
            {
                Android.Net.Uri uri = MediaStore.Audio.Media.ExternalContentUri;

                string[] projection = { MediaStore.Audio.AudioColumns.Data };

                using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, null, null, null);

                if (cursor != null && cursor.MoveToFirst())
                {
                    do
                    {
                        string filePath = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Audio.AudioColumns.Data));

                        if (filePath.StartsWith("/storage/emulated/0/snaptube/Download/SnapTube Audio"))
                        {
                            var tagFile = TagLib.File.Create(filePath);

                            var name = Path.GetFileNameWithoutExtension(filePath);
                            var artist = tagFile.Tag.FirstPerformer ?? "Desconhecido";
                            var album = tagFile.Tag.Album ?? "Desconhecido";
                            var genre = tagFile.Tag.FirstGenre ?? "Desconhecido";
                            var duration = tagFile.Properties.Duration;

                            ImageSource? albumArt = null;
                            if (tagFile.Tag.Pictures.Length > 0)
                            {
                                var picture = tagFile.Tag.Pictures[0];
                                var imageData = picture.Data.Data;

                                albumArt = ImageSource.FromStream(() => new MemoryStream(imageData));
                            }

                            var audioItem = new AudioItem(name, filePath, duration, artist, album, genre, albumArt);

                            audioItems.Add(audioItem);
                        }

                    } while (cursor.MoveToNext());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
#endif
            return audioItems;
        }


        public AudioItem LoadMusicFromDirectory(string musicDirectory)
        {
            var audioItem = new AudioItem();
#if ANDROID
            try
            {
                Android.Net.Uri uri = MediaStore.Audio.Media.ExternalContentUri;

                string[] projection = { MediaStore.Audio.AudioColumns.Data };

                using var cursor = Android.App.Application.Context.ContentResolver.Query(uri, projection, null, null, null);


                if (cursor != null && cursor.MoveToFirst())
                {
                    string currentFilePath = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Audio.AudioColumns.Data));

                    var file = TagLib.File.Create(currentFilePath);

                    if (file.Tag.Pictures.Length > 0)
                    {
                        var picture = file.Tag.Pictures[0];
                        var imageData = picture.Data.Data;

                        audioItem.AlbumArt = ImageSource.FromStream(() => new MemoryStream(imageData));
                    }
                    audioItem = new AudioItem(file.Tag.Title, currentFilePath, file.Properties.Duration, file.Tag.FirstPerformer, 
                                              file.Tag.Album, file.Tag.FirstGenre, audioItem.AlbumArt);
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
#endif
            return audioItem;
        }
    }
}
