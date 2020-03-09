using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace AutoVideo
{
    public static class AutoVideoMaker
    {
        public const string PATH = "D:\\Users\\cyril\\source\\repos\\AutoVideo\\AutoVideo\\";
        public static int Width = 1920;
        public static int Height = 1080;

        public static List<Music> Musics = new List<Music>();

        public static async Task GenerateVideo()
        {
            try
            {
                var files = "";
                var i = 0;
                Console.WriteLine("Start");

                await CMD.FFmpeg("-y " +
                                 "-f lavfi -i \"color=black:s=1920x1080:r=25\" " +
                                 "-f lavfi -i \"anullsrc=r=44100:cl=stereo\" " +
                                 $"-c:v libx264 -c:a aac -t 1 {PATH}empty_1s.mp4");
                files += " -i " + PATH + "empty_1s.mp4";

                foreach (var music in Musics)
                {
                    Console.WriteLine("Start " + music.Audio);
                    i++;
                    music.CreateImage($"tmp_image_{i}.bmp");
                    var output = $"tmp_video_{i}.mp4";
                    //await GenerateVideoFromAudioAndImage(music.Audio, music.Image, output);
                    files += " -i " + PATH + output;
                    files += " -i " + PATH + "empty_1s.mp4";
                    Console.WriteLine("End" + music.Audio);
                }


                Console.WriteLine("Start join");
                await CMD.FFmpeg($"-y " +
                                 $"{files} " +
                                 $"-filter_complex \"" +
                                 $"[1:v] fade=t=in:st=0:d=2, fade=t=out:st=58:d=2 [1v]; " +
                                 $"[1:a] afade=t=in:st=0:d=2, afade=t=out:st=58:d=2 [1a]; " +
                                 $"[3:v] fade=t=in:st=0:d=2, fade=t=out:st=118:d=2 [3v]; " +
                                 $"[3:a] afade=t=in:st=0:d=1, afade=t=out:st=118:d=2 [3a]; " +
                                 $"[0:v] [0:a] [1v] [1a] [2:v] [2:a] [3v] [3a] [4:v] [4:a] concat=n=5:v=1:a=1 [v] [a]\" " +
                                 $"-map \"[v]\" -map \"[a]\" -c:v libx264 -c:a aac {PATH}video.mp4");
                Console.WriteLine("End join");

                Console.WriteLine("End");
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
                $"-y -loop 1 -i {PATH + image} -i {PATH + audioFile} -c:v libx264 -tune stillimage -c:a aac -b:a 192k -pix_fmt yuv420p -shortest {PATH + outputFile}");
        }
    }

    public class Music
    {
        public Music(string audio, string image)
        {
            Audio = audio ?? throw new ArgumentNullException(nameof(audio));
            Image = image ?? throw new ArgumentNullException(nameof(image));
            Generated = false;
        }

        public string Audio { get; set; }
        public string Image { get; set; }
        public bool Generated { get; set; }

        public void CreateImage(string fileName)
        {
            try
            {
                Image template = System.Drawing.Image.FromFile(AutoVideoMaker.PATH + "template.png");
                Image img = System.Drawing.Image.FromFile(AutoVideoMaker.PATH + Image);
                Bitmap output = new Bitmap(template.Width, template.Height);

                using (Graphics g = Graphics.FromImage(output))
                {
                    g.DrawImage(template, new Rectangle(new Point(), template.Size));
                    g.DrawImage(img, new Rectangle(new Point(template.Width / 2 - img.Width / 2, template.Height / 2 - img.Height / 2), img.Size));
                }

                output.Save(AutoVideoMaker.PATH + fileName);
                Image = fileName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}