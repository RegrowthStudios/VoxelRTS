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
using RTSEngine.Graphics;

namespace RTS.Default.Tutorials {
    public class Tut0 : ACGameTypeController {

        public int state;
        public float targetHeight;
        Vector3 fireLocation;

        RTSTeam pTeam;
        public override void Load(GameState s, DirectoryInfo mapDir) {
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

            float[] heights = new float[s.CGrid.numCells.X * s.CGrid.numCells.Y];
            Vector2[] p = new Vector2[heights.Length];
            for(int y = 0, i = 0; y < s.CGrid.numCells.Y; y++) {
                for(int x = 0; x < s.CGrid.numCells.X; x++) {
                    p[i] = new Vector2(x + 0.5f, y + 0.5f) * s.CGrid.cellSize;
                    heights[i] = s.CGrid.HeightAt(p[i]);
                    i++;
                }
            }
            Array.Sort(heights, p, 0, heights.Length);
            int cS = 1, cE = 1;
            while(cS < heights.Length && heights[cS] == heights[0])
                cS++;
            while(cE < heights.Length && heights[heights.Length - 1 - cE] == heights[heights.Length - 1])
                cE++;
            Random r = new Random();
            Vector2 spawnPos = p[r.Next(cS)];
            int ti = heights.Length - 1 - r.Next(cE);
            targetHeight = heights[ti] - 0.5f;
            fireLocation = new Vector3(p[ti].X, targetHeight, p[ti].Y);

            pTeam.Input.AddEvent(new SpawnUnitEvent(pTeam.Index, 0, spawnPos));
            pTeam.Input.AddEvent(new SpawnUnitEvent(pTeam.Index, 0, spawnPos));
            pTeam.Input.AddEvent(new SpawnUnitEvent(pTeam.Index, 0, spawnPos));
            pTeam.Input.AddEvent(new SpawnUnitEvent(pTeam.Index, 0, spawnPos));
            DevConsole.AddCommand("franz ferdinand");
            pTeam.Input.OnNewSelection += (ic, ns) => {
                if(ns.Count < 1) return;
                s.SendPopup(@"Packs\presets\Tutorial0\2.png", new Rectangle(10, 60, 400, 300));
                System.Threading.Interlocked.Exchange(ref state, 3);
                s.AddParticle(new AlertParticle(fireLocation, 2, Color.Transparent, fireLocation + Vector3.Up * 3, 1, Color.OrangeRed, s.TotalGameTime, 4f));
            };
            state = 0;
        }

        public override int? GetVictoriousTeam(GameState s) {
            // Mercy Time
            if(s.CurrentFrame < 100) return null;

            // If All The Buildings And Units Are Destroyed
            foreach(var e in pTeam.Units) {
                if(e.WorldPosition.Y < targetHeight)
                    return null;
            }
            return 1;
        }

        public override void Tick(GameState s) {
            switch(state) {
                case 0:
                    if(s.CurrentFrame > 100) {
                        s.SendPopup(@"Packs\presets\Tutorial0\0.png", new Rectangle(10, 60, 400, 300));
                        System.Threading.Interlocked.Increment(ref state);
                    }
                    break;
                case 1:
                    if(s.CurrentFrame > 900) {
                        s.SendPopup(@"Packs\presets\Tutorial0\1.png", new Rectangle(10, 60, 400, 300));
                        System.Threading.Interlocked.Increment(ref state);
                    }
                    break;
            }
        }
        public override void ApplyFrame(GameState s, float dt) {

        }

        public override List<LEVoxel> CreateVoxels(Grey.Vox.VoxAtlas atlas) {
            return null;
        }
        public override void LESave(Grey.Vox.VoxWorld world, int w, int h, DirectoryInfo dir) {
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s, GameState state) {
        }

    }
}