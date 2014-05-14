using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Parsers;

namespace RTS {
    public class UICCompanyLogo {
        [ZXParse("ColorBackground")]
        public Color ColorBackground;

        [ZXParse("FireEntranceTime")]
        public float FireEntranceTime;
        [ZXParse("ScreenExitTime")]
        public float ScreenExitTime;

        [ZXParse("FireImageNoise")]
        public string FireImageNoise;
        [ZXParse("FireImageColor")]
        public string FireImageColor;
        [ZXParse("FireImageAlpha")]
        public string FireImageAlpha;

        [ZXParse("FireSound")]
        public string FireSound;
        [ZXParse("FireSoundVolume")]
        public float FireSoundVolume;

        [ZXParse("FireShader")]
        public string FireShader;
        [ZXParse("FireMaxDistort")]
        public float FireMaxDistort;
        [ZXParse("FireTintEnd")]
        public Vector4 FireTintEnd;
    }
}
