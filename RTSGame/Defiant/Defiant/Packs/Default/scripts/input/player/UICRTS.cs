using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Parsers;

namespace RTS.Input {
    public class UICRTS {
        public string Font;
        public int FontSize;

        public UICMinimap UICMinimap;
        public UICAlertQueue UICAlertQueue;

        public int BBRows;
        public int BBColumns;
        public int BBIconSize;
        public int BBIconBuffer;
        public string BBTexture;

        public int SelectionRows;
        public int SelectionColumns;
        public int SelectionIconSize;
        public int SelectionIconBuffer;
        public string SelectionTexture;
    }
}
