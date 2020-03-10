using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AutoVideo
{
    public class Music
    {
        public Music(string title, string artist, string audio, string image, int start, int end)
        {
            Title = title;
            Artist = artist;
            Audio = audio ?? throw new ArgumentNullException(nameof(audio));
            ContentImage = image ?? throw new ArgumentNullException(nameof(image));
            Start = start;
            End = end;
            Generated = false;
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Audio { get; set; }
        public string ContentImage { get; set; }
        public string ContentVideo { get; set; }
        public string OverlayImage { get; set; }
        public string OverlayVideo { get; set; }
        public bool Generated { get; set; }

        public int Start { get; set; }
        public int End { get; set; }

        public void CreateOverlayImage(string fileName)
        {
            try
            {
                var template = VideoEditor.Template;

                var background = Image.FromFile(template.Background);
                var output = new Bitmap(template.VideoSize.Width, template.VideoSize.Height);

                var titlePosition = template.MusicInfoPosition;
                var artistPosition = titlePosition;
                artistPosition.Y += template.MusicInfoFontSize + 10;

                using (var g = Graphics.FromImage(output))
                {
                    g.DrawImage(background, ScaleFill(background, template.VideoSize));

                    DrawString(g, Title, titlePosition, template.MusicInfoFontSize);
                    DrawString(g, Artist, artistPosition, template.MusicInfoFontSize);
                }

                output.Save(fileName);
                OverlayImage = fileName;
                background.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void DrawString(Graphics g, string text, Point position, int fontSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var path = new GraphicsPath();
            path.AddString(text, new FontFamily("Heebo"), 1, fontSize, position, StringFormat.GenericDefault);
            g.FillPath(Brushes.White, path);
            g.DrawPath(new Pen(Color.Black, 3), path);
        }

        public void CreateContentImage(string fileName)
        {
            try
            {
                var background = Image.FromFile("blank.png");
                var img = Image.FromFile(ContentImage);
                var output = new Bitmap(VideoEditor.Template.Content.Size.Width, VideoEditor.Template.Content.Size.Height);

                using (var g = Graphics.FromImage(output))
                {
                    g.DrawImage(background, new Rectangle(new Point(0, 0), VideoEditor.Template.Content.Size));
                    g.DrawImage(img, ScaleAdjust(img, VideoEditor.Template.Content.Size));
                }

                output.Save(fileName);
                ContentImage = fileName;
                background.Dispose();
                img.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Rectangle ScaleAdjust(Image image, Size content)
        {
            float scale;
            if (image.Width < content.Width && image.Height < content.Height)
            {
                if (content.Width / image.Width < content.Height / image.Height)
                    scale = content.Width / (float) image.Width;
                else
                    scale = content.Height / (float) image.Height;
            }
            else if (image.Width > content.Width && image.Height < content.Height)
            {
                scale = content.Width / (float) image.Width;
            }
            else if (image.Width < content.Width && image.Height > content.Height)
            {
                scale = content.Height / (float) image.Height;
            }
            else
            {
                if (content.Width / image.Width > content.Height / image.Height)
                    scale = content.Width / (float) image.Width;
                else
                    scale = content.Height / (float) image.Height;
            }

            return new Rectangle(
                (int) ((content.Width - image.Width * scale) / 2),
                (int) ((content.Height - image.Height * scale) / 2),
                (int) (image.Width * scale),
                (int) (image.Height * scale));
        }

        public Rectangle ScaleFill(Image image, Size content)
        {
            float scale;
            if (image.Width > content.Width && image.Height > content.Height)
            {
                if (content.Width / image.Width > content.Height / image.Height)
                    scale = content.Width / (float) image.Width;
                else
                    scale = content.Height / (float) image.Height;
            }
            else if (image.Width < content.Width && image.Height > content.Height)
            {
                scale = content.Width / (float) image.Width;
            }
            else if (image.Width > content.Width && image.Height < content.Height)
            {
                scale = content.Height / (float) image.Height;
            }
            else
            {
                if (content.Width / image.Width < content.Height / image.Height)
                    scale = content.Width / (float) image.Width;
                else
                    scale = content.Height / (float) image.Height;
            }

            return new Rectangle(
                (int) ((content.Width - image.Width * scale) / 2),
                (int) ((content.Height - image.Height * scale) / 2),
                (int) (image.Width * scale),
                (int) (image.Height * scale));
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

        public async Task CreateOverlayVideo(string fileName)
        {
            OverlayVideo = fileName;
            await VideoEditor.GenerateVideoFromImageAndDuration(OverlayImage, GetAudioLength(), OverlayVideo);
        }

        public async Task CreateContentVideo(string fileName)
        {
            ContentVideo = fileName;
            await VideoEditor.GenerateVideoFromAudioAndImage(Audio, ContentImage, ContentVideo);
        }

        public void Clear()
        {
            File.Delete(Audio);
            File.Delete(OverlayImage);
            File.Delete(ContentImage);
            File.Delete(OverlayVideo);
            File.Delete(ContentVideo);
        }

        public int GetAudioLength()
        {
            var fi = new Mp3FileReader(Audio);
            var length = (int) Math.Floor(fi.TotalTime.TotalSeconds);
            fi.Close();
            return length;
        }
    }
}