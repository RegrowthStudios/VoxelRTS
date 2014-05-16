using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Parsers;

namespace RTS {
    public class UICMMButton {
        [ZXParse("Text")]
        public string Text;
        [ZXParse("Image")]
        public string ImageFile;
        [ZXParse("ColorInactive")]
        public Color ColorInactive;
        [ZXParse("ColorActive")]
        public Color ColorActive;
        [ZXParse("ColorText")]
        public Color ColorText;

        public int ActionIndex;
    }

    public class UICMainMenu {
        [ZXParse("ColorBackground")]
        public Color ColorBackground;

        [ZXParse("Font")]
        public string Font;
        [ZXParse("FontRes")]
        public int FontResolution;

        [ZXParse("TitleTextSize")]
        public int TitlePanelTextSize;
        [ZXParse("TitleText")]
        public string TitlePanelText;
        [ZXParse("ColorTitleText")]
        public Color ColorTitleText;

        [ZXParse("Buttons")]
        public UICMMButton[] Buttons;
        [ZXParse("ButtonSpacing")]
        public Point ButtonSpacing;
        [ZXParse("ButtonTextSize")]
        public int ButtonTextSize;

        public string ButtonImageUp;
        public string ButtonImageDown;
        public Color ButtonUDInactiveColor;
        public Color ButtonUDActiveColor;

        [ZXParse("SoundClick")]
        public string SoundClick;
        [ZXParse("SoundHover")]
        public string SoundHover;
    }
}