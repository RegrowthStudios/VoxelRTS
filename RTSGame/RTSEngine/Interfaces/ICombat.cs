using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public interface IDestructibleEntity : IEntity {
        // The Current Health Of The Entity
        int Health { get; }

        // Applies Damage To Health
        void Damage(int d);
    }

    public interface ICombatEntity : IEntity {
        // Information About Whether This Entity Can Attack Yet
        bool CanAttack { get; }

        // Computes The Damage To Deal With Access To A Random Number
        int DealDamage(double rand);
    }

    // Extension For Resolving Combat With Given Interfaces
    public static class CombatResolver {
        public static void Damage(this ICombatEntity source, IDestructibleEntity target, double rand) {
            if(source.CanAttack)
                target.Damage(source.DealDamage(rand));
        }
    }
}