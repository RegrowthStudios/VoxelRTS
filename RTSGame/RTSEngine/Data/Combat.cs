using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data {
    public class BaseCombatData {
        public static void Serialize(BinaryWriter s, BaseCombatData data) {
            s.Write(data.MinRange);
            s.Write(data.MaxRange);
            s.Write(data.AttackDamage);
            s.Write(data.CriticalDamage);
            s.Write(data.AttackTimer);
            s.Write(data.Armor);
            s.Write(data.CriticalChance);
        }
        public static void Deserialize(BinaryReader s, BaseCombatData data) {
            data.MinRange = s.ReadInt32();
            data.MaxRange = s.ReadInt32();
            data.AttackDamage = s.ReadInt32();
            data.CriticalDamage = s.ReadInt32();
            data.AttackTimer = s.ReadSingle();
            data.Armor = s.ReadInt32();
            data.CriticalChance = s.ReadDouble();
        }

        // Maximum And Minimum Range Of Attacking
        [ZXParse("MAXRANGE")]
        public int MaxRange;
        [ZXParse("MINRANGE")]
        public int MinRange;

        // Amount Of Damage Done Per Hit
        [ZXParse("ATTACKDAMAGE")]
        public int AttackDamage;
        [ZXParse("CRITICALDAMAGE")]
        public int CriticalDamage;

        // Amount Of Time To Wait Before An Attack Can Be Utilized
        [ZXParse("ATTACKTIMER")]
        public float AttackTimer;
        [ZXParse("SETUPTIMER")]
        public float SetupTimer;

        // Subtracts Source Damage By Armor Amount
        [ZXParse("ARMOR")]
        public int Armor;

        // [0 - 1] Ratio Of Critical Hits To Be Obtained
        [ZXParse("CRITICALCHANCE")]
        public double CriticalChance;

        // Find The Correct Damage Dealt Based Off Of A Random Number
        public int ComputeDamageDealt(double rand) {
            // Compare To Critical Chance
            return rand <= CriticalChance ? CriticalDamage : AttackDamage;
        }

        // Find The Correct Damage Received From A Damage Source
        public int ComputeDamageReceived(int d) {
            // Apply Armor
            d -= Armor;

            // Can't Deal Negative Damage
            return d < 0 ? 0 : d;
        }
    }
}