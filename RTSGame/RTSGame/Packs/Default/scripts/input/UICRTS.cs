using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Parsers;

namespace RTS.Input {
    public class UICRTS {
        [ZXParse("Font")]
        public string Font;
        [ZXParse("FontSize")]
        public int FontSize;

        [ZXParse("BottomPanelHeight")]
        public int BottomPanelHeight;
        [ZXParse("BottomPanelColor")]
        public Color BottomPanelColor;

        [ZXParse("MinimapBorder")]
        public int MinimapBorder;

        [ZXParse("SelectionRows")]
        public int SelectionRows;
        [ZXParse("SelectionColumns")]
        public int SelectionColumns;
        [ZXParse("SelectionIconSize")]
        public int SelectionIconSize;
        [ZXParse("SelectionIconBuffer")]
        public int SelectionIconBuffer;
    }
}
