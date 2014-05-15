using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public static class BehaviorFSM {
        // State Codes
        public const int None = 0;
        public const int Rest = None + 1;
        public const int Walking = Rest + 1;
        public const int PrepareCombatRanged = Walking + 1;
        public const int CombatRanged = PrepareCombatRanged + 1;
        public const int CombatMelee = CombatRanged + 1;
        public const int Death = CombatMelee + 1;
        public const int Special0 = Death + 1;
        public const int Special1 = Special0 + 1;
        public const int Harvest = Special1 + 1;
        public const int Deposit = Harvest + 1;
        public const int Repair = Deposit + 1;
        public const int Build = Repair + 1;

        // Targeting Order Codes - Will Influence Targeting Behavior
        public const int TargetNone = 0;
        public const int TargetPassively = TargetNone + 1;
        public const int TargetAggressively = TargetPassively + 1;
        public const int TargetAsSquad = TargetAggressively + 1;

        // Combat Order Codes - Will Influence Behavior While In Combat
        public const int UseMeleeAttack = 0;
        public const int UseRangedAttack = UseMeleeAttack + 1;

        // Movement Order Codes - Will Influence Movement Behavior
        public const int JustMove = 0;
        // TODO: Add Support For A-Move
        public const int AttackMove = JustMove + 1;

        public static int GetState(int behaviorCode) {
            return GetByte(behaviorCode, 0);
        }
        public static int GetTargetingOrders(int behaviorCode) {
            return GetByte(behaviorCode, 1);
        }
        public static int GetCombatOrders(int behaviorCode) {
            return GetByte(behaviorCode, 2);
        }
        public static int GetMovementOrders(int behaviorCode) {
            return GetByte(behaviorCode, 3);
        }

        // Retrieve The Specified Byte From A 32-Bit Int
        public static int GetByte(int data, int index) {
            return (data >> 8 * index) & 255;
        }

        // Set The Specified Byte In A 32-Bit Int
        public static int SetByte(int data, int b, int index) {
            return (data & ~(255 << 8 * index)) | (b << 8 * index);
        }
    }

}
