using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AutoVideo
{
    public static class AutoVideoMaker
    {
        public static int Width = 1920;
        public static int Height = 1080;

        public static List<Music> Musics = new List<Music>();

        public static async Task GenerateVideo()
        {
            try
            {
                var files = "";
                var filters = "";
                var links = "";
                var i = 0;

                foreach (var music in Musics)
                {
                    Console.WriteLine("Start " + music.Audio);

                    music.CreateImage($"tmp_image_{i + 1}.bmp");
                    await music.CreateAudio($"tmp_audio_{i + 1}.mp3");
                    music.Video = $"tmp_video_{i + 1}.mp4";

                    await GenerateVideoFromAudioAndImage(music.Audio, music.Image, music.Video);

                    files += " -i " + music.Video;

                    int length = music.GetAudioLength();

                    filters += $"[{i}:v] fade=t=in:st=0:d=2, fade=t=out:st={length - 2}:d=2 [{i}v]; ";
                    filters += $"[{i}:a] afade=t=in:st=0:d=2, afade=t=out:st={length - 2}:d=2 [{i}a]; ";

                    links += $"[{i}v] [{i}a] ";

                    Console.WriteLine("End" + music.Audio);
                    i++;
                }


                Console.WriteLine("Start join");

                await CMD.FFmpeg("-y " +
                                 $"{files} " +
                                 "-filter_complex \"" +
                                 filters +
                                 links +
                                 $"concat=n={Musics.Count}:v=1:a=1 [v] [a]\" " +
                                 $"-map \"[v]\" -map \"[a]\" -c:v libx264 -c:a aac video.mp4");

                Console.WriteLine("End join");

                Console.WriteLine("Start clear");

                Musics.ForEach(m => m.Clear());

                Console.WriteLine("End clear");


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
    }

    public class Music
    {
        public Music(string audio, string image, int start, int end)
        {
            Audio = audio ?? throw new ArgumentNullException(nameof(audio));
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Start = start;
            End = end;
            Generated = false;
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Audio { get; set; }
        public string Image { get; set; }
        public string Video { get; set; }
        public bool Generated { get; set; }

        public int Start { get; set; }
        public int End { get; set; }

        public void CreateImage(string fileName)
        {
            try
            {
                var background = System.Drawing.Image.FromFile("background.jpg");
                var template = System.Drawing.Image.FromFile("template.png");
                var img = System.Drawing.Image.FromFile(Image);
                var output = new Bitmap(background.Width, background.Height);

                using (var g = Graphics.FromImage(output))
                {
                    g.DrawImage(background, new Rectangle(new Point(), background.Size));
                    g.DrawImage(template, new Rectangle(new Point(), template.Size));
                    g.DrawImage(img,
                        new Rectangle(
                            new Point(50, 50),
                            img.Size));
                }

                output.Save(fileName);
                Image = fileName;
                background.Dispose();
                template.Dispose();
                img.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task CreateAudio(string fileName)
        {
            try
            {
                await CMD.FFmpeg($"-y -i {Audio} -ss {Start} -to {End} -c copy {fileName}");
                Audio = fileName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Clear()
        {
            File.Delete(Audio);
            File.Delete(Image);
            File.Delete(Video);
        }

        public int GetAudioLength()
        {
            var fi = new Mp3FileReader(Audio);
            int length = (int) Math.Floor(fi.TotalTime.TotalSeconds);
            fi.Close();
            return length;
        }
    }
}