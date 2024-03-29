﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AutoVideo
{
    public static class CMD
    {
        public static void Run(string command)
        {
            Process.Start("CMD.exe", "/C " + command);
        }
        public static async Task<int> FFmpeg(string command)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C C:\\FFmpeg\\bin\\ffmpeg.exe " + command
                },
                EnableRaisingEvents = true
            };
            Console.WriteLine("C:\\FFmpeg\\bin\\ffmpeg.exe " + command);

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };
            process.Start();

            return await tcs.Task;
        }
    }
}
