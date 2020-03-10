using System;
using System.Drawing;

namespace AutoVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            VideoEditor.Template = new Template()
            {
                Content = new Rectangle(50, 50, 1422, 800),
                VideoSize = new Size(1920, 1080),
                Background = "background.jpg",
                MusicInfoFontSize = 80,
                MusicInfoPosition = new Point(50, 850)
            };
            VideoEditor.Musics.Add(new Music("Next Sparkling!!", "Aqours", "audio1.mp3", "i1.jpg", 75, 95));
            VideoEditor.Musics.Add(new Music("僕らの走ってきた道は…", "Aqours", "audio2.mp3", "i2.jpg", 60, 80));
            VideoEditor.GenerateVideo("video.mp4");

            Console.ReadLine();
        }
    }
}
