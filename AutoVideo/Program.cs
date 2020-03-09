using System;

namespace AutoVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoVideoMaker.Musics.Add(new Music("audio1.mp3", "i1.jpg", 60, 90));
            AutoVideoMaker.Musics.Add(new Music("audio2.mp3", "i2.jpg", 60, 90));
            AutoVideoMaker.GenerateVideo();

            Console.ReadLine();
        }
    }
}
