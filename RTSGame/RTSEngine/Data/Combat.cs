using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public struct BaseCombatData {
        public static void Serialize(BinaryWriter s, ref BaseCombatData data) {
            s.Write(data.MinRange);
            s.Write(data.MaxRange);
            s.Write(data.AttackDamage);
            s.Write(data.CriticalDamage);
            s.Write(data.AttackTimer);
            s.Write(data.Armor);
            s.Write(data.CriticalChance);
        }
        public static void Deserialize(BinaryReader s, ref BaseCombatData data) {
            data.MinRange = s.ReadInt32();
            data.MaxRange = s.ReadInt32();
            data.AttackDamage = s.ReadInt32();
            data.CriticalDamage = s.ReadInt32();
            data.AttackTimer = s.ReadSingle();
            data.Armor = s.ReadInt32();
            data.CriticalChance = s.ReadDouble();
        }

        // Maximum And Minimum Range Of Attacking
        public int MaxRange, MinRange;

        // Amount Of Damage Done Per Hit
        public int AttackDamage;
        public int CriticalDamage;

        // Amount Of Time To Wait Before An Attack Can Be Utilized
        public float AttackTimer;

        // Subtracts Source Damage By Armor Amount
        public int Armor;

        // [0 - 1] Ratio Of Critical Hits To Be Obtained
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