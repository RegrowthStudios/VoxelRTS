using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Algorithms;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Graphics {
    public struct SeenEntity {
        public Point GridPos;
        public bool IsBuilding;
        public int TeamIndex;
    }
    public class ScanTask : ACBudgetedTask {
        int xs, xe;
        CollisionGrid cg;
        Queue<SeenEntity> queue;
        RTSTeam team;
        private int lastAdded;

        public ScanTask(int s, int e, CollisionGrid g, RTSTeam t, Queue<SeenEntity> q)
            : base(1) {
            xs = s;
            xe = e;
            cg = g;
            team = t;
            queue = q;
            lastAdded = 0;
        }

        public override void DoWork(float dt) {
            // Remove Last Added Elements
            while(lastAdded > 0) {
                queue.Dequeue();
                lastAdded--;
            }

            lastAdded = 0;
            for(int xi = xs; xi < xe; xi++) {
                for(int y = 0; y < cg.numCells.Y; y++) {
                    var ae = cg.EDynamic[xi, y];
                    SeenEntity se = new SeenEntity();
                    se.GridPos.X = xi;
                    se.GridPos.Y = y;
                    se.IsBuilding = false;
                    se.TeamIndex = -1;
                    bool use = false;

                    // Check For Units
                    if(ae != null) {
                        for(int i = 0; i < ae.Count; i++) {
                            se.TeamIndex = ae[i].Team.Index;
                            use = true;
                            // Enemy Trumps Friendly Visuals
                            if(ae[i].Team != team) break;
                        }
                    }

                    // Building Overrides Units
                    if(cg.EStatic[xi, y] != null) {
                        se.TeamIndex = cg.EStatic[xi, y].Team.Index;
                        se.IsBuilding = true;
                        use = true;
                    }

                    // Add The Seen Entity
                    if(use) {
                        queue.Enqueue(se);
                        lastAdded++;
                    }
                }
            }
        }
    }

    public class Minimap {
        public const int SCAN_BINS = 30;
        private static readonly short[] FRUSTUM_INDS = {
            5, 4, 7, 7, 4, 6,
            2, 3, 6, 6, 3, 7,
            1, 5, 3, 3, 5, 7,
            4, 0, 6, 6, 0, 2,
            4, 5, 0, 0, 5, 1,
            0, 1, 2, 2, 1, 3
        };
        private static readonly Color CAMERA_COLOR_START = new Color(1f, 1f, 0f, 0.3f);
        private static readonly Color CAMERA_COLOR_END = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        public int ImageWidth {
            get;
            private set;
        }
        public int ImageHeight {
            get;
            private set;
        }

        // Terrain View
        private bool refreshFOW;
        public RenderTarget2D Terrain {
            get;
            private set;
        }

        // Entity View
        public Texture2D TeamMap {
            get;
            private set;
        }

        // Camera Visualization
        private Matrix mVP, mV, mP;
        private Plane[] mapPlanes;

        // For Seeing Enemies
        private TimeBudget tbScanner;
        private Queue<SeenEntity> qSeen;

        private BasicEffect fxCamera;

        public int TeamIndex {
            get;
            private set;
        }
        public RTSTeam Team {
            get;
            private set;
        }

        public Minimap() {
        }

        public void Hook(RTSRenderer renderer, GameState s, int ti) {
            ImageWidth = s.CGrid.numCells.X;
            ImageHeight = s.CGrid.numCells.Y;
            Terrain = renderer.CreateRenderTarget2D(ImageWidth, ImageHeight, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, RenderTargetUsage.DiscardContents);
            TeamIndex = ti;
            Team = s.teams[TeamIndex];

            Vector3 center = new Vector3(s.CGrid.size.X * 0.5f, 0, s.CGrid.size.Y * 0.5f);

            float h = Grey.Vox.Region.HEIGHT + Camera.INITIAL_HIGH_SETTINGS.MaxDistance;
            mV = Matrix.CreateLookAt(center + Vector3.Up * (h + 1f), center, -Vector3.UnitZ);
            mP = Matrix.CreateOrthographic(s.CGrid.size.X, s.CGrid.size.Y, 0, h + 2f);
            mVP = mV * mP;

            fxCamera = renderer.CreateEffect();
            fxCamera.LightingEnabled = false;
            fxCamera.FogEnabled = false;
            fxCamera.TextureEnabled = false;
            fxCamera.VertexColorEnabled = true;
            fxCamera.View = mV;
            fxCamera.Projection = mP;
            fxCamera.World = Matrix.Identity;

            mapPlanes = new Plane[6];
            float off = s.CGrid.size.Length() * 0.25f;
            mapPlanes[0] = new Plane(Vector3.UnitX, off);
            mapPlanes[1] = new Plane(Vector3.UnitY, 0);
            mapPlanes[2] = new Plane(Vector3.UnitZ, off);
            mapPlanes[3] = new Plane(-Vector3.UnitX, s.CGrid.size.X + off);
            mapPlanes[4] = new Plane(-Vector3.UnitY, h + 2f);
            mapPlanes[5] = new Plane(-Vector3.UnitZ, s.CGrid.size.Y + off);

            refreshFOW = true;
            s.CGrid.OnFOWChange += (x, y, p, f) => {
                refreshFOW = refreshFOW | (p == ti);
            };

            TeamMap = renderer.CreateTexture2D(s.CGrid.numCells.X, s.CGrid.numCells.Y, SurfaceFormat.Color, false);
            qSeen = new Queue<SeenEntity>();
            tbScanner = new TimeBudget(SCAN_BINS);
            float r = (float)s.CGrid.numCells.X / SCAN_BINS;
            for(int i = 0; i < SCAN_BINS - 1; i++) {
                tbScanner.AddTask(new ScanTask((int)(r * i), (int)(r * (i + 1)), s.CGrid, Team, qSeen));
            }
            tbScanner.AddTask(new ScanTask((int)(r * (SCAN_BINS - 1)), s.CGrid.numCells.X, s.CGrid, Team, qSeen));
        }

        public void Refresh(RTSRenderer renderer) {
            if(refreshFOW) {
                refreshFOW = false;
                RenderMap(renderer);
            }
            tbScanner.DoTasks(1f);
            RenderEntities();
        }
        public void DrawCamera(RTSRenderer renderer, Rectangle scissor) {
            BoundingFrustum f = new BoundingFrustum(renderer.Camera.View * renderer.Camera.Projection);
            Vector3[] corners = f.GetCorners();
            for(int i = 0; i < 4; i++) {
                Vector3 dir = corners[i + 4] - corners[i];
                float min = dir.Length();
                dir /= min;
                Ray r = new Ray(corners[i], dir);

                foreach(Plane p in mapPlanes) {
                    float? v = r.Intersects(p);
                    if(v != null && v.Value < min) {
                        min = v.Value;
                    }
                }

                corners[i + 4] = r.Position + (r.Direction * min);
            }
            VertexPositionColor[] verts = {
                new VertexPositionColor(corners[0], CAMERA_COLOR_START),
                new VertexPositionColor(corners[1], CAMERA_COLOR_START),
                new VertexPositionColor(corners[3], CAMERA_COLOR_START),
                new VertexPositionColor(corners[2], CAMERA_COLOR_START),
                new VertexPositionColor(corners[4], CAMERA_COLOR_END),
                new VertexPositionColor(corners[5], CAMERA_COLOR_END),
                new VertexPositionColor(corners[7], CAMERA_COLOR_END),
                new VertexPositionColor(corners[6], CAMERA_COLOR_END)
            };
            renderer.G.BlendState = BlendState.Additive;
            renderer.G.DepthStencilState = DepthStencilState.None;
            renderer.G.RasterizerState = RasterizerState.CullNone;
            fxCamera.CurrentTechnique.Passes[0].Apply();
            Viewport vp = renderer.G.Viewport;
            renderer.G.Viewport = new Viewport(scissor);
            renderer.G.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 8, FRUSTUM_INDS, 0, 12);
            renderer.G.Viewport = vp;
        }
        public void RenderEntities() {
            Color[] c = new Color[TeamMap.Width * TeamMap.Height];
            Array.Clear(c, 0, c.Length);
            foreach(var se in qSeen) {
                Color cc = se.TeamIndex == TeamIndex ? Color.Blue : Color.Red;
                c[se.GridPos.Y * TeamMap.Width + se.GridPos.X] = cc;
            }
            TeamMap.SetData(c);
        }
        public void RenderMap(RTSRenderer renderer) {
            renderer.G.SetRenderTarget(Terrain);
            renderer.G.Clear(Color.Black);
            renderer.DrawMap(mV, mP);
            renderer.G.SetRenderTarget(null);
        }
    }
}