using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEU.Physics;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public class GameState {
        public const int PLAYER_TEAM_INDEX = 0;

        public Tank PlayerTank {
            get;
            private set;
        }
        private Team[] teams;
        public Team[] Teams {
            get { return teams; }
        }

        public CollisionGrid CGrid {
            get;
            private set;
        }

        // Keeping Track Of Time
        private int curFrame;
        public int CurrentFrame {
            get { return curFrame; }
        }
        private float timePlayed;
        public float TotalGameTime {
            get { return timePlayed; }
        }

        // Particle Events
        private object lckParticles;
        private List<Particle> particles;
        private List<Particle> tmpParticles;

        public GameState() {
            curFrame = 0;
            timePlayed = 0;

            lckParticles = new object();
            particles = new List<Particle>();
            tmpParticles = new List<Particle>();

            teams = new Team[1];
        }

        public void CreatePlayer(Race r, int t, Vector2 pos) {
            Teams[PLAYER_TEAM_INDEX] = new Team(0, Team.TYPE_PLAYER, r);
            PlayerTank = Teams[PLAYER_TEAM_INDEX].AddUnit(t, pos);
        }
        public Team CreateTeam(Race r) {
            int i = Teams.Length;
            Array.Resize(ref teams, Teams.Length + 1);
            Teams[i] = new Team(i, Team.TYPE_COMPUTER, r);
            return Teams[i];
        }

        public void CreateWorld(int gw, int gh) {
            CGrid = new CollisionGrid(gw, gh);
        }

        // Glorified Way For The GameplayController To Keep Track Of Time
        public void IncrementFrame(float dt) {
            curFrame++;
            timePlayed += dt;
            if(tmpParticles.Count > 0) {
                lock(lckParticles) {
                    particles.AddRange(tmpParticles);
                }
                tmpParticles = new List<Particle>();
            }
        }

        // Particle Effects
        public List<Particle> GetParticles() {
            if(particles.Count > 0) {
                List<Particle> p;
                lock(lckParticles) {
                    p = particles;
                    particles = new List<Particle>();
                }
                return p;
            }
            return null;
        }
        public void AddParticle(Particle p) {
            lock(lckParticles)
                tmpParticles.Add(p);
        }
        public void AddParticles(IEnumerable<Particle> p) {
            lock(lckParticles)
                tmpParticles.AddRange(p);
        }
    }
}