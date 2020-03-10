using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AutoVideo
{
    public class Template
    {
        public string Background { get; set; }
        public Rectangle Content { get; set; }

        public Size VideoSize { get; set; }

        public Point MusicInfoPosition { get; set; }
        public int MusicInfoFontSize { get; set; }


    }
}
