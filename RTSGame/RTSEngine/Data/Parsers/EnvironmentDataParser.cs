using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    public struct EnvironmentData {
        public static EnvironmentData Default {
            get {
                return new EnvironmentData() {
                    FloraType = 0,
                    OreType = 1,
                    MinionType = 0,
                    TankType = 1,
                    TitanType = 2,
                    DisasterTime = 5,
                    L1Impact = 1000,
                    L2Impact = 2000,
                    L3Impact = 3000,
                    NoLongerRecoverImpact = 2500,
                    L1SpawnCap = 10,
                    L2SpawnCap = 20,
                    L3SpawnCap = 30,
                    RecoverImpact = 500,
                    SpawnOffset = 30,
                    L1MinNumSpawn = new int[] {3,1,0},
                    L2MinNumSpawn = new int[] { 6, 3, 1 },
                    L3MinNumSpawn = new int[] { 12, 5, 2 },
                    L1MaxNumSpawn = new int[] {6,3,1},
                    L2MaxNumSpawn = new int[] {12,5,2},
                    L3MaxNumSpawn = new int[] {15,10,5},
                    LightningHitP = 70,
                    LightningDamage = 10,
                    EarthquakeHitP = 40,
                    EarthquakeDamage = 20
                };
            }
        }

        public int FloraType;
        public int OreType;
        public int MinionType;
        public int TankType;
        public int TitanType;

        public int DisasterTime;

        public int L1Impact;
        public int L2Impact;
        public int L3Impact;
        public int NoLongerRecoverImpact;

        public int L1SpawnCap;
        public int L2SpawnCap;
        public int L3SpawnCap;

        public int RecoverImpact;
        public int SpawnOffset;

        public int[] L1MinNumSpawn;
        public int[] L2MinNumSpawn;
        public int[] L3MinNumSpawn;
        public int[] L1MaxNumSpawn;
        public int[] L2MaxNumSpawn;
        public int[] L3MaxNumSpawn;

        public int LightningHitP;
        public int LightningDamage;
        public int EarthquakeHitP;
        public int EarthquakeDamage;
        
    }

    public class EnvironmentDataParser {
        // Data Detection
        private static readonly Regex rgxFlora = RegexHelper.GenerateInteger("FLORA");
        private static readonly Regex rgxOre = RegexHelper.GenerateInteger("ORE");
        private static readonly Regex rgxMinion = RegexHelper.GenerateInteger("MINION");
        private static readonly Regex rgxTank = RegexHelper.GenerateInteger("TANK");
        private static readonly Regex rgxTitan = RegexHelper.GenerateInteger("TITAN");
        private static readonly Regex rgxRecoverTime = RegexHelper.GenerateInteger("RECOVERTIME");
        private static readonly Regex rgxDisasterTime = RegexHelper.GenerateInteger("DISASTERTIME");
        private static readonly Regex rgxL1Impact = RegexHelper.GenerateInteger("LEVELONEIMPACT");
        private static readonly Regex rgxL2Impact = RegexHelper.GenerateInteger("LEVELTWOIMPACT");
        private static readonly Regex rgxL3Impact = RegexHelper.GenerateInteger("LEVELTHREEIMPACT");
        private static readonly Regex rgxNoLongerRecoverImpact = RegexHelper.GenerateInteger("NOLONGERRECOVERIMPACT");
        private static readonly Regex rgxL1SpawnCap = RegexHelper.GenerateInteger("LEVELONESPAWNCAP");
        private static readonly Regex rgxL2SpawnCap = RegexHelper.GenerateInteger("LEVELTWOSPAWNCAP");
        private static readonly Regex rgxL3SpawnCap = RegexHelper.GenerateInteger("LEVELTHREESPAWNCAP");
        private static readonly Regex rgxRecoverImpact = RegexHelper.GenerateInteger("RECOVERIMPACT");
        private static readonly Regex rgxSpawnOffset = RegexHelper.GenerateInteger("SPAWNOFFSET");
        private static readonly Regex rgxL1MinNumSpawn = RegexHelper.GenerateVec3Int("L1MINNUMSPAWN");
        private static readonly Regex rgxL2MinNumSpawn = RegexHelper.GenerateVec3Int("L2MINNUMSPAWN");
        private static readonly Regex rgxL3MinNumSpawn = RegexHelper.GenerateVec3Int("L3MINNUMSPAWN");
        private static readonly Regex rgxL1MaxNumSpawn = RegexHelper.GenerateVec3Int("L1MAXNUMSPAWN");
        private static readonly Regex rgxL2MaxNumSpawn = RegexHelper.GenerateVec3Int("L2MAXNUMSPAWN");
        private static readonly Regex rgxL3MaxNumSpawn = RegexHelper.GenerateVec3Int("L3MAXNUMSPAWN");
        private static readonly Regex rgxLightningHitP = RegexHelper.GenerateInteger("LIGHTNINGHITP");
        private static readonly Regex rgxLightningDamage = RegexHelper.GenerateInteger("LIGHTNINGDAMAGE");
        private static readonly Regex rgxEarthquakeHitP = RegexHelper.GenerateInteger("EARTHQUAKEHITP");
        private static readonly Regex rgxEarthquakeDamage = RegexHelper.GenerateInteger("EARTHQUAKEDAMAGE");
        

        public static EnvironmentData Parse(FileInfo infoFile) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return EnvironmentData.Default;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Match Tokens
            Match[] mp = {
                rgxFlora.Match(mStr),
                rgxOre.Match(mStr),
                rgxMinion.Match(mStr),
                rgxTank.Match(mStr),
                rgxTitan.Match(mStr),
                rgxRecoverTime.Match(mStr),
                rgxDisasterTime.Match(mStr),
                rgxL1Impact.Match(mStr),
                rgxL2Impact.Match(mStr),
                rgxL3Impact.Match(mStr),
                rgxNoLongerRecoverImpact.Match(mStr),
                rgxL1SpawnCap.Match(mStr),
                rgxL2SpawnCap.Match(mStr),
                rgxL3SpawnCap.Match(mStr),
                rgxRecoverImpact.Match(mStr),
                rgxSpawnOffset.Match(mStr),
                rgxL1MinNumSpawn.Match(mStr),
                rgxL1MaxNumSpawn.Match(mStr),
                rgxL2MinNumSpawn.Match(mStr),
                rgxL2MaxNumSpawn.Match(mStr),
                rgxL3MinNumSpawn.Match(mStr),
                rgxL3MaxNumSpawn.Match(mStr),
                rgxLightningHitP.Match(mStr),
                rgxLightningDamage.Match(mStr),
                rgxEarthquakeHitP.Match(mStr),
                rgxEarthquakeDamage.Match(mStr)
            };
            foreach(var m in mp) if(!m.Success) return EnvironmentData.Default;
            int i = 0;
            EnvironmentData eid;
            eid.FloraType = RegexHelper.ExtractInt(mp[i++]);
            eid.OreType = RegexHelper.ExtractInt(mp[i++]);
            eid.MinionType = RegexHelper.ExtractInt(mp[i++]);
            eid.TankType = RegexHelper.ExtractInt(mp[i++]);
            eid.TitanType = RegexHelper.ExtractInt(mp[i++]);
            eid.DisasterTime = RegexHelper.ExtractInt(mp[i++]);
            eid.L1Impact = RegexHelper.ExtractInt(mp[i++]);
            eid.L2Impact = RegexHelper.ExtractInt(mp[i++]);
            eid.L3Impact = RegexHelper.ExtractInt(mp[i++]);
            eid.NoLongerRecoverImpact = RegexHelper.ExtractInt(mp[i++]);
            eid.L1SpawnCap = RegexHelper.ExtractInt(mp[i++]);
            eid.L2SpawnCap = RegexHelper.ExtractInt(mp[i++]);
            eid.L3SpawnCap = RegexHelper.ExtractInt(mp[i++]);
            eid.RecoverImpact = RegexHelper.ExtractInt(mp[i++]);
            eid.SpawnOffset = RegexHelper.ExtractInt(mp[i++]);
            eid.L1MinNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.L1MaxNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.L2MinNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.L2MaxNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.L3MinNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.L3MaxNumSpawn = RegexHelper.ExtractVec3I(mp[i++]);
            eid.LightningHitP = RegexHelper.ExtractInt(mp[i++]);
            eid.LightningDamage = RegexHelper.ExtractInt(mp[i++]);
            eid.EarthquakeHitP = RegexHelper.ExtractInt(mp[i++]);
            eid.EarthquakeDamage = RegexHelper.ExtractInt(mp[i++]);
            return eid;
        }
    }
}