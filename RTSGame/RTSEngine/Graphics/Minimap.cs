using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Controllers;
using RTSEngine.Data;

namespace RTSEngine.Graphics {
    public class Minimap {
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

        public RenderTarget2D Terrain {
            get;
            private set;
        }
        private Matrix mVP, mV, mP;
        private Plane[] mapPlanes;

        private BasicEffect fxCamera;

        public int TeamDisplay {
            get;
            private set;
        }

        public Minimap() {
        }

        public void Hook(RTSRenderer renderer, GameState s, int ti) {
            ImageWidth = s.CGrid.numCells.X;
            ImageHeight = s.CGrid.numCells.Y;
            Terrain = renderer.CreateRenderTarget2D(ImageWidth, ImageHeight, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, RenderTargetUsage.DiscardContents);

            TeamDisplay = ti;

            Vector3 center = new Vector3(s.CGrid.size.X * 0.5f, 0, s.CGrid.size.Y * 0.5f);

            float h = s.Map.ScaleY + Camera.INITIAL_HIGH_SETTINGS.MaxDistance;
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
        }

        public void Refresh(RTSRenderer renderer) {
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

            renderer.G.SetRenderTarget(Terrain);
            renderer.G.Clear(Color.Black);

            renderer.DrawMap(mVP);

            renderer.G.BlendState = BlendState.Additive;
            renderer.G.DepthStencilState = DepthStencilState.None;
            renderer.G.RasterizerState = RasterizerState.CullNone;
            fxCamera.CurrentTechnique.Passes[0].Apply();
            renderer.G.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 8, FRUSTUM_INDS, 0, 12);

            renderer.G.SetRenderTarget(null);
        }
    }
}