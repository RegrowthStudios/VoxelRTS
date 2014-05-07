using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Algorithms;
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;

namespace RTS.Default.Tutorials {
    public class Tut0 : ACGameTypeController {

        public int state;

        RTSTeam pTeam;
        public override void Load(GameState s, FileInfo mapFile) {
            // Give The Player Team Starting Capital
            pTeam = null;
            for(int i = 0; i < s.activeTeams.Length; i++) {
                var at = s.activeTeams[i];
                if(pTeam == null && at.Team.Type == RTSInputType.Player) {
                    pTeam = at.Team;
                    pTeam.Input.AddEvent(new CapitalEvent(pTeam.Index, 1000));
                    pTeam.PopulationCap = 100;
                }
            }
            pTeam.Input.AddEvent(new SpawnUnitEvent(pTeam.Index, 0, new Vector2(29, 29)));
            pTeam.Input.OnNewSelection += (ic, ns) => {
                //s.SendPopup(@"Content\Textures\InduZtry.png", new Rectangle(10, 60, 400, 300));
                System.Threading.Interlocked.Exchange(ref state, 3);
            };
            state = 0;
        }

        public override int? GetVictoriousTeam(GameState s) {
            // Mercy Time
            if(s.CurrentFrame < 100) return null;

            // If All The Buildings And Units Are Destroyed
            foreach(var e in pTeam.Units) {
                if(e.WorldPosition.Y < 64.5)
                    return null;
            }
            return 1;
        }

        public override void Tick(GameState s) {
            switch(state) {
                case 0:
                    if(s.CurrentFrame > 100) {
                        //s.SendPopup(@"Content\Textures\InduZtry.png", new Rectangle(10, 60, 400, 300));
                        System.Threading.Interlocked.Increment(ref state);
                    }
                    break;
                case 1:
                    if(s.CurrentFrame > 900) {
                        //s.SendPopup(@"Content\Textures\InduZtry.png", new Rectangle(10, 60, 400, 300));
                        System.Threading.Interlocked.Increment(ref state);
                    }
                    break;
            }
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s, GameState state) {
        }
    }
}