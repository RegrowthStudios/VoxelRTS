using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data {
    public struct EnvironmentData {
        public static EnvironmentData Default {
            get {
                return new EnvironmentData() {
                    FloraType = 0,
                    OreType = 1,
                    MinionType = 0,
                    TankType = 1,
                    TitanType = 2,
                    DisasterTime = 20,
                    L1Impact = 1000,
                    L2Impact = 2000,
                    L3Impact = 3000,
                    NoLongerRecoverImpact = 2500,
                    L1SpawnCap = 10,
                    L2SpawnCap = 20,
                    L3SpawnCap = 30,
                    OreRecoverHealth = 50,
                    SpawnOffset = 10,
                    L1MinNumSpawn = new int[] { 3, 1, 0 },
                    L2MinNumSpawn = new int[] { 6, 3, 1 },
                    L3MinNumSpawn = new int[] { 12, 5, 2 },
                    L1MaxNumSpawn = new int[] { 6, 3, 1 },
                    L2MaxNumSpawn = new int[] { 12, 5, 2 },
                    L3MaxNumSpawn = new int[] { 15, 10, 5 },
                    LightningHitP = 60,
                    LightningDamage = 1000,
                    EarthquakeHitP = 40,
                    EarthquakeDamage = 2000,
                    FireHitUnitP = 70,
                    FireUnitDamage = 150,
                    FireHitBuildingP = 70,
                    FireBuildingDamage = 5,
                    FireSpreadP = 10
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

        public int OreRecoverHealth;
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
        public int FireHitUnitP;
        public int FireUnitDamage;
        public int FireHitBuildingP;
        public int FireBuildingDamage;
        public int FireSpreadP;
    }
}