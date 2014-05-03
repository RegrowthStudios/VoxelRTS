using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Parsers;

namespace RTS {
    public struct LightningColors {
        [ZXParse("Back")]
        public Color Background;
        [ZXParse("Fore")]
        public Color Foreground;
    }

    public struct LightningBoltArgs {
        [ZXParse("Width")]
        public float Width;
        [ZXParse("Jag")]
        public float Jag;
        [ZXParse("MinLength")]
        public float MinLength;
        [ZXParse("MaxLength")]
        public float MaxLength;
    }

    public class UICInduZtry {
        [ZXParse("ColorBackground")]
        public Color ColorBackground;
        [ZXParse("LightningSound")]
        public string LightningSound;
        [ZXParse("ThunderSound")]
        public string ThunderSound;

        [ZXParse("LightningAlphaImage")]
        public string LightningAlphaImage;
        [ZXParse("LightningBoltImage")]
        public string LightningBoltImage;
        [ZXParse("ColorCombos")]
        public LightningColors[] ColorCombos;

        [ZXParse("LightningLerpTime")]
        public float LightningLerpTime;
        [ZXParse("ThunderTime")]
        public float ThunderTime;
        [ZXParse("LightningPlayPause")]
        public int LightningPlayPause;

        [ZXParse("Bolt")]
        public LightningBoltArgs Bolt;
        [ZXParse("Branch")]
        public LightningBoltArgs Branch;
        [ZXParse("BranchingSlope")]
        public float BranchingSlope;
        [ZXParse("BranchProbability")]
        public double BranchProbability;
    }
}
