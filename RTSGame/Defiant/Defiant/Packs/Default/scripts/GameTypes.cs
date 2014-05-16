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
using Grey.Vox;
using Grey.Graphics;

namespace RTS.Default {
    namespace GameTypes {
        public class SPEscapeThePlanet : ACGameTypeController {
            private static readonly Color[] TEAM_COLORS = new Color[]{
                Color.Blue, Color.Red, Color.Yellow,
                Color.Orange, Color.Brown,
                Color.Purple, Color.Gray
            };
            public const int MERCY_TIME = 100;
            public const int STARTING_CAPITAL = 1500;
            public const float DUV = 1f / 8f;

            // The Teams
            RTSTeam pTeam, eTeam;
            List<RTSTeam> cTeams;

            public override void Load(GameState s, DirectoryInfo mapDir) {
                // Give The Player Team Starting Capital
                pTeam = null;
                eTeam = null;
                cTeams = new List<RTSTeam>();
                for(int i = 0; i < s.activeTeams.Length; i++) {
                    var at = s.activeTeams[i];
                    switch(at.Team.Type) {
                        case RTSInputType.Player:
                            if(pTeam != null) continue;
                            pTeam = at.Team;
                            break;
                        case RTSInputType.Environment:
                            if(eTeam != null) continue;
                            eTeam = at.Team;
                            break;
                        case RTSInputType.AI:
                            cTeams.Add(at.Team);
                            break;
                        default:
                            break;
                    }
                }


                // TODO: Correct Load

                // Give Teams Capital
                if(pTeam != null) {
                    pTeam.Input.AddEvent(new CapitalEvent(pTeam.Index, STARTING_CAPITAL));
                }
                foreach(var cTeam in cTeams) {
                    cTeam.Input.AddEvent(new CapitalEvent(cTeam.Index, STARTING_CAPITAL));
                }

                // Open File
                FileInfo fi = new FileInfo(mapDir.FullName + @"\horde.dat");
                if(fi.Exists) {
                    BinaryReader r = new BinaryReader(fi.OpenRead());

                    while(true) {
                        int team = r.ReadInt32();
                        if(team == -1) break;
                        int type = r.ReadInt32();
                        int x = r.ReadInt32();
                        int z = r.ReadInt32();
                        if(team == 0 && pTeam != null) {
                            if(pTeam.Race.Buildings[type] != null) {
                                pTeam.Input.AddEvent(new SpawnBuildingEvent(pTeam.Index, type, new Point(x, z), true));
                            }
                        }
                        else if(team - 1 < cTeams.Count) {
                            team--;
                            RTSTeam cTeam = cTeams[team];
                            if(cTeam.Race.Buildings[type] != null) {
                                cTeam.Input.AddEvent(new SpawnBuildingEvent(cTeam.Index, type, new Point(x, z), true));
                            }
                        }
                    }
                    r.BaseStream.Dispose();
                }
            }

            public override int? GetVictoriousTeam(GameState s) {
                // Mercy Time
                if(s.CurrentFrame < MERCY_TIME)
                    return null;

                // Lose If All Headquarters Are Destroyed
                if(pTeam.Race.Buildings[0].CurrentCount <= 0)
                    return -1;

                // Lose If All The Buildings And Units Are Destroyed
                //if(pTeam.Buildings.Count < 1 && pTeam.Units.Count < 1)
                //    return -1;

                // Win If All Enemies Destroyed
                int c = 0;
                foreach(var cTeam in cTeams) {
                    c += cTeam.Buildings.Count + cTeam.Units.Count;
                }
                if(c == 0)
                    return pTeam.Index;

                return null;
            }

            public override void Tick(GameState s) {

            }
            public override void ApplyFrame(GameState s, float dt) {
                if(s.CurrentFrame % 5 == 0) {
                    for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                        var team = s.activeTeams[ti].Team;
                        for(int i = 0; i < team.Buildings.Count; i++) {
                            var b = team.Buildings[i];
                            if(b.BuildAmountLeft > 0)
                                b.BuildAmountLeft -= team.Race.GlobalBuildSpeed;
                        }
                    }
                }
            }

            #region Level Editor
            List<LEVoxel> voxels;
            ushort minID, maxID;
            public override List<LEVoxel> CreateVoxels(VoxAtlas atlas) {
                voxels = new List<LEVoxel>(GameState.MAX_NONENV_PLAYERS * 2);
                LEVoxel lev;
                VGPCube vgp;
                for(int i = 0; i < GameState.MAX_NONENV_PLAYERS; i++) {
                    // Create HQ Voxel
                    lev = new LEVoxel("Team " + (i + 1) + " HQ", atlas);
                    lev.VData.FaceType.SetAllTypes(0x00000001u);
                    lev.VData.FaceType.SetAllMasks(0xfffffffeu);
                    vgp = new VGPCube();
                    vgp.Color = TEAM_COLORS[i];
                    vgp.UVRect = new Vector4(DUV * 0, DUV * 1, DUV, DUV);
                    lev.VData.GeoProvider = vgp;
                    voxels.Add(lev);

                    // Create Barracks Voxel
                    lev = new LEVoxel("Team " + (i + 1) + " Barracks", atlas);
                    lev.VData.FaceType.SetAllTypes(0x00000001u);
                    lev.VData.FaceType.SetAllMasks(0xfffffffeu);
                    vgp = new VGPCube();
                    vgp.Color = TEAM_COLORS[i];
                    vgp.UVRect = new Vector4(DUV * 1, DUV * 1, DUV, DUV);
                    lev.VData.GeoProvider = vgp;
                    voxels.Add(lev);
                }
                minID = voxels[0].VData.ID;
                maxID = voxels[voxels.Count - 1].VData.ID;
                return voxels;
            }
            public override void LESave(VoxWorld world, int w, int h, DirectoryInfo dir) {
                // Create File
                FileInfo fi = new FileInfo(dir.FullName + @"\horde.dat");
                BinaryWriter s = new BinaryWriter(fi.Create());

                // Search Through Columns
                Vector3I loc = Vector3I.Zero;
                for(loc.Z = 0; loc.Z < h; loc.Z++) {
                    for(loc.X = 0; loc.X < w; loc.X++) {
                        loc.Y = 0;
                        VoxLocation vl = new VoxLocation(loc);
                        Region r = world.regions[vl.RegionIndex];

                        // Search Through The Region
                        int team, type;
                        for(; vl.VoxelLoc.Y < Region.HEIGHT; vl.VoxelLoc.Y++) {
                            ushort id = r.voxels[vl.VoxelIndex].ID;
                            if(id < minID || id > maxID) continue;

                            // Write Team And Type
                            team = id - minID;
                            type = team & 0x01;
                            team >>= 1;

                            s.Write(team);
                            s.Write(type);
                            s.Write(loc.X);
                            s.Write(loc.Z);
                            break;
                        }
                    }
                }
                s.Write(-1);

                // Flush And Close
                s.Flush();
                s.BaseStream.Dispose();
            }
            #endregion

            public override void Serialize(BinaryWriter s) {
            }
            public override void Deserialize(BinaryReader s, GameState state) {
            }
        }
    }
}