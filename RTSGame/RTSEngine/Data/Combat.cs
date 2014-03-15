using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public struct BaseCombatData {
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