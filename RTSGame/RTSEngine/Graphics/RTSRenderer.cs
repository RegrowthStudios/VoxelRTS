using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSEngine.Data;
using RTSEngine.Controllers;
using BlisterUI.Input;

namespace RTSEngine.Graphics {
    public struct CameraMotionSettings {
        public static CameraMotionSettings Lerp(CameraMotionSettings s1, CameraMotionSettings s2, float r) {
            return new CameraMotionSettings() {
                OrbitSpeed = MathHelper.Lerp(s1.OrbitSpeed, s2.OrbitSpeed, r),
                ScrollSpeed = MathHelper.Lerp(s1.ScrollSpeed, s2.ScrollSpeed, r),
                MinDistance = MathHelper.Lerp(s1.MinDistance, s2.MinDistance, r),
                MaxDistance = MathHelper.Lerp(s1.MaxDistance, s2.MaxDistance, r)
            };
        }

        public float OrbitSpeed;
        public float ScrollSpeed;
        public float MinDistance;
        public float MaxDistance;
    }

    public class RTSRenderer : IDisposable {
        public const Keys KEY_FULLSCREEN = Keys.F11;
        public const char CTRL_CHAR_FULLSCREEN = ControlCharacters.CtrlF;

        public static readonly Vector3 INITIAL_CAMERA_ORIGIN = Vector3.Zero;
        public static readonly CameraMotionSettings INITIAL_LOW_SETTINGS = new CameraMotionSettings() {
            OrbitSpeed = 1f,
            ScrollSpeed = 10f,
            MinDistance = 3f,
            MaxDistance = 10f
        };
        public static readonly CameraMotionSettings INITIAL_HIGH_SETTINGS = new CameraMotionSettings() {
            OrbitSpeed = 2f,
            ScrollSpeed = 100f,
            MinDistance = 10f,
            MaxDistance = 50f
        };
        public const float INITIAL_ZOOM_SPEED = 3f;
        public const float ZOOM_OFFSET = 0.3f;
        public const float INITIAL_CAMERA_YAW = -0.7f;
        public const float INITIAL_CAMERA_PITCH = 0.8f;
        public const float MIN_PITCH = 0.1f;
        public const float MAX_PITCH = 1.5f;
        public const float PITCH_RANGE = MAX_PITCH - MIN_PITCH;
        public const float EYE_HEIGHT_OFFSET = 0.5f;

        // Really Should Not Be Holding This Though
        GraphicsDeviceManager gManager;
        private GraphicsDevice G {
            get { return gManager.GraphicsDevice; }
        }
        private bool toggleFull;

        // Camera Matrices
        Matrix mView, mProj;
        private Matrix View {
            get { return mView; }
            set {
                mView = value;
                fxMap.View = mView;
                fxRTS.VP = mView * mProj;
            }
        }
        private Matrix Projection {
            get { return mProj; }
            set {
                mProj = value;
                fxMap.Projection = mProj;
                fxRTS.VP = mView * mProj;
            }
        }

        // For Panning And Zooming The Camera
        private CameraController camController;
        private Vector3 camOrigin;
        public Vector3 CamOrigin {
            get { return camOrigin; }
        }
        public CameraMotionSettings lowSettings, highSettings;
        public CameraMotionSettings MovementSettings {
            get { return CameraMotionSettings.Lerp(lowSettings, highSettings, PitchRatio); }
        }
        public float PitchRatio {
            get { return (Pitch - MIN_PITCH) / PITCH_RANGE; }
        }
        public float Yaw {
            get;
            private set;
        }
        public float Pitch {
            get;
            private set;
        }
        public float ZoomRatio {
            get;
            private set;
        }
        public float ZoomSpeed {
            get;
            set;
        }
        public bool IsOrthographic {
            get;
            private set;
        }

        // Map To Render
        public HeightmapModel Map {
            get;
            set;
        }

        // All The Unit Models To Render
        public List<RTSUnitModel> UnitModels {
            get;
            private set;
        }

        // Effects
        private BasicEffect fxMap;
        private RTSEffect fxRTS;

        public RTSRenderer(GraphicsDeviceManager gdm, string rtsFXFile, GameWindow w) {
            gManager = gdm;
            toggleFull = false;
            UnitModels = new List<RTSUnitModel>();

            fxMap = new BasicEffect(G);
            fxMap.LightingEnabled = false;
            fxMap.FogEnabled = false;
            fxMap.VertexColorEnabled = false;
            fxMap.TextureEnabled = true;
            fxMap.World = Matrix.Identity;

            fxRTS = new RTSEffect(XNAEffect.Compile(G, rtsFXFile));
            fxRTS.World = Matrix.Identity;
            fxRTS.CPrimary = Vector3.UnitX;
            fxRTS.CSecondary = Vector3.UnitY;
            fxRTS.CTertiary = Vector3.UnitZ;

            camOrigin = INITIAL_CAMERA_ORIGIN;
            Yaw = INITIAL_CAMERA_YAW;
            Pitch = INITIAL_CAMERA_PITCH;
            ZoomRatio = 0.5f;
            lowSettings = INITIAL_LOW_SETTINGS;
            highSettings = INITIAL_HIGH_SETTINGS;
            ZoomSpeed = INITIAL_ZOOM_SPEED;
            RecalculateView(null, MovementSettings.MaxDistance);

            IsOrthographic = false;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, G.Viewport.AspectRatio, 0.01f, 2000f);

            camController = new CameraController(G.Viewport.Width, G.Viewport.Height);
            w.ClientSizeChanged += camController.OnWindowResize;
            camController.Hook();

            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.ReceiveCommand += OnCtrlPress;
        }
        public void Dispose() {
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.ReceiveCommand -= OnCtrlPress;

            camController.Unhook();
            Map.Dispose();

            fxMap.Dispose();
        }

        // View Matrix Recalculation
        public void RecalculateView(Heightmap map, float dist) {
            Matrix rot =
                Matrix.CreateRotationZ(Pitch) *
                Matrix.CreateRotationY(Yaw);

            Vector3 back = Vector3.TransformNormal(Vector3.UnitX, rot);
            back.Normalize();

            Vector3 eye = CamOrigin + back * dist;
            if(map != null) {
                float h = map.HeightAt(eye.X, eye.Z);
                if(eye.Y < h)
                    eye.Y = h + EYE_HEIGHT_OFFSET;
            }
            View = Matrix.CreateLookAt(eye, CamOrigin, Vector3.Up);
        }
        public void UpdateCamera(Heightmap map, float dt) {
            CameraMotionSettings cms = MovementSettings;

            Scroll(camController.ScrollX, camController.ScrollY, cms, dt);
            Orbit(camController.Yaw, camController.Pitch, cms, dt);
            int z;
            camController.GetZoom(out z);
            Zoom(z, dt);
            camOrigin.X = MathHelper.Clamp(camOrigin.X, 0, map.Width);
            camOrigin.Z = MathHelper.Clamp(camOrigin.Z, 0, map.Depth);
            camOrigin.Y = map.HeightAt(camOrigin.X, camOrigin.Z);
            RecalculateView(map, MathHelper.Lerp(cms.MinDistance, cms.MaxDistance, ZoomRatio));
        }
        private void Scroll(int x, int y, CameraMotionSettings cms, float dt) {
            if(x == 0 && y == 0) return;

            Matrix camWorld = Matrix.Invert(mView);

            Vector3 forward = camWorld.Forward;
            forward.Y = 0;
            forward.Normalize();

            Vector3 right = camWorld.Right;
            right.Y = 0;
            right.Normalize();

            camOrigin += forward * y * cms.ScrollSpeed * dt * (ZoomRatio + ZOOM_OFFSET);
            camOrigin += right * x * cms.ScrollSpeed * dt * (ZoomRatio + ZOOM_OFFSET);
        }
        private void Orbit(int x, int y, CameraMotionSettings cms, float dt) {
            Yaw += x * cms.OrbitSpeed * dt;
            Yaw = MathHelper.WrapAngle(Yaw);
            Pitch += y * cms.OrbitSpeed * dt;
            Pitch = MathHelper.Clamp(Pitch, MIN_PITCH, MAX_PITCH);
        }
        private void Zoom(int z, float dt) {
            ZoomRatio += ZoomSpeed * z * dt;
            ZoomRatio = MathHelper.Clamp(ZoomRatio, 0, 1);
        }

        // Hooks To Determine Fullscreen Toggling
        private void OnKeyPress(object s, KeyEventArgs args) {
            switch(args.KeyCode) {
                case KEY_FULLSCREEN:
                    toggleFull = true;
                    break;
            }
        }
        private void OnCtrlPress(object s, CharacterEventArgs args) {
            switch(args.Character) {
                case CTRL_CHAR_FULLSCREEN:
                    toggleFull = true;
                    break;
            }
        }

        public void Draw(GameState s, float dt) {
            if(toggleFull) {
                toggleFull = false;
                // TODO: This Has Problems
                // gManager.ToggleFullScreen();
                return;
            }
            if(G.GraphicsDeviceStatus != GraphicsDeviceStatus.Normal) return;
            G.Clear(Color.Black);

            // Draw The Map
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque; Map.SetPrimaryModel(G);
            // Primary Map Model
            fxMap.Texture = Map.PrimaryTexture;
            G.SamplerStates[0] = SamplerState.LinearClamp;
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawPrimary(G);
            // Secondary Map Model
            fxMap.Texture = Map.SecondaryTexture;
            G.SamplerStates[0] = SamplerState.LinearClamp;
            Map.SetSecondaryModel(G);
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawSecondary(G);

            // Draw All Animated Entities
            foreach(RTSUnitModel unitModel in UnitModels) {
                fxRTS.TexModelMap = unitModel.AnimationTexture;
                fxRTS.TexOverlay = unitModel.ModelTexture;
                fxRTS.TexColor = unitModel.ColorCodeTexture;
                fxRTS.ApplyPassAnimation();
                unitModel.UpdateInstances(G);
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }
        }

        public void GetSelectionBox(Vector2 screenMin, Vector2 screenMax, out OBB? obb, out Frustum? frustum) {
            // TODO: Koshi
            Vector2 ss = new Vector2(G.Viewport.Width, G.Viewport.Height);
            screenMin /= ss;
            screenMax /= ss;
            float miny = 1 - screenMax.Y;
            float maxy = 1 - screenMin.Y;
            screenMin.Y = miny;
            screenMax.Y = maxy;
            screenMin *= 2f; screenMin -= Vector2.One;
            screenMax *= 2f; screenMax -= Vector2.One;
            obb = null;
            frustum = new Frustum(mView, mProj, screenMin, screenMax);
        }
        public Ray GetViewRay(Vector2 screenPos) {
            Ray r = new Ray();
            r.Position = G.Viewport.Unproject(new Vector3(screenPos, 0), mProj, mView, Matrix.Identity);
            r.Direction = G.Viewport.Unproject(new Vector3(screenPos, 1), mProj, mView, Matrix.Identity);
            r.Direction -= r.Position;
            r.Direction.Normalize();
            return r;
        }
    }
}