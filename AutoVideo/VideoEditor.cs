using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoVideo
{
    public static class VideoEditor
    {
        private static string overlayVideo;

        private static string contentVideo;

        public static List<Music> Musics = new List<Music>();

        public static Template Template { get; set; }

        public static async Task GenerateVideo(string fileName)
        {
            await GeneratePartialVideos("tmp_overlay_video.mp4", "tmp_content_video.mp4");

            await CMD.FFmpeg("-y " +
                             $"-i {overlayVideo} -i {contentVideo} " +
                             $"-filter_complex \"[0:v][1:v] overlay={Template.Content.X}:{Template.Content.Y}\" " +
                             $"-pix_fmt yuv420p -c:a copy " +
                             $"{fileName}");
            File.Delete(overlayVideo);
            File.Delete(contentVideo);
        }

        private static async Task GeneratePartialVideos(string overlayFileName, string contentFileName)
        {
            try
            {
                var overlayVideos = "";
                var contentVideos = "";
                var filters = "";
                var overlayLinks = "";
                var contentLinks = "";
                var i = 0;

                foreach (var music in Musics)
                {
                    // Création des assets pour l'overlay
                    music.CreateOverlayImage($"tmp_overlay_image_{i + 1}.bmp");

                    // Création de l'image pour le contenu
                    music.CreateContentImage($"tmp_content_image_{i + 1}.bmp");

                    // Création de l'audio
                    await music.CreateAudio($"tmp_audio_{i + 1}.mp3");

                    // Création de la vidéo pour l'overlay
                    await music.CreateOverlayVideo($"tmp_overlay_video_{i + 1}.mp4");

                    // Création de la vidéo pour le contenu
                    await music.CreateContentVideo($"tmp_content_video_{i + 1}.mp4");

                    // Création des paramètres pour la jointure
                    overlayVideos += " -i " + music.OverlayVideo;
                    contentVideos += " -i " + music.ContentVideo;

                    var length = music.GetAudioLength();

                    filters += $"[{i}:v] fade=t=in:st=0:d=2, fade=t=out:st={length - 2}:d=2 [{i}v]; ";
                    filters += $"[{i}:a] afade=t=in:st=0:d=2, afade=t=out:st={length - 2}:d=2 [{i}a]; ";

                    overlayLinks += $"[{i}v] ";
                    contentLinks += $"[{i}v] [{i}a] ";

                    i++;
                }

                // Création de la vidéo overlay
                overlayVideo = overlayFileName;
                await CMD.FFmpeg("-y " +
                                 $"{overlayVideos} " +
                                 "-filter_complex \"" +
                                 overlayLinks +
                                 $"concat=n={Musics.Count}:v=1 [v] \" " +
                                 $"-map \"[v]\" -c:v libx264 {overlayVideo}");

                // Création de la vidéo contenu
                contentVideo = contentFileName;
                await CMD.FFmpeg("-y " +
                                 $"{contentVideos} " +
                                 "-filter_complex \"" +
                                 filters +
                                 contentLinks +
                                 $"concat=n={Musics.Count}:v=1:a=1 [v] [a]\" " +
                                 $"-map \"[v]\" -map \"[a]\" -c:v libx264 -c:a aac {contentVideo}");

                Musics.ForEach(m => m.Clear());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task GenerateVideoFromAudioAndImage(string audioFile, string image, string outputFile)
        {
            await CMD.FFmpeg(
                $"-y -loop 1 -i {image} -i {audioFile} -shortest -acodec copy -vcodec copy {outputFile}");
        }

        public static async Task GenerateVideoFromImageAndDuration(string image, int duration, string outputFile)
        {
            await CMD.FFmpeg($"-y -loop 1 -i {image} -c:v libx264 -t {duration} {outputFile}");
        }
    }
}