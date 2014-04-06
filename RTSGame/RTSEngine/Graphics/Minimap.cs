using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;

namespace RTSEngine.Graphics {
    public class Minimap {
        public int ImageWidth {
            get;
            private set;
        }
        public int ImageHeight {
            get;
            private set;
        }

        public RenderTarget2D Terrain {
            get;
            private set;
        }
        private bool refreshTerrain;
        private Matrix mVP;

        public int TeamDisplay {
            get;
            private set;
        }

        public Minimap() {
            refreshTerrain = false;
        }

        public void Hook(RTSRenderer renderer, GameState s, int ti) {
            ImageWidth = s.CGrid.numCells.X;
            ImageHeight = s.CGrid.numCells.Y;
            Terrain = renderer.CreateRenderTarget2D(ImageWidth, ImageHeight, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, RenderTargetUsage.DiscardContents);

            TeamDisplay = ti;

            s.CGrid.OnFOWChange += OnFOWChange;
            Vector3 center = new Vector3(s.CGrid.size.X * 0.5f, 0, s.CGrid.size.Y * 0.5f);
            mVP =
                Matrix.CreateLookAt(center + Vector3.Up * s.Map.ScaleY, center, -Vector3.UnitZ) *
                Matrix.CreateOrthographic(s.CGrid.size.X, s.CGrid.size.Y, 0, s.Map.ScaleY);
            refreshTerrain = true;
        }

        private void OnFOWChange(int x, int y, int ti, FogOfWar f) {
            if(ti == TeamDisplay) {
                refreshTerrain = true;
            }
        }

        public void Refresh(RTSRenderer renderer) {
            if(refreshTerrain) {
                renderer.G.SetRenderTarget(Terrain);
                renderer.G.Clear(Color.Black);
                renderer.DrawMap(mVP);
                renderer.G.SetRenderTarget(null);
            }
        }
    }
}