using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Controllers;

namespace RTSEngine.Data {
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

    public class Camera {
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
        public const float INITIAL_ZOOM_RATIO = 0.5f;
        public const float MIN_PITCH = 0.1f;
        public const float MAX_PITCH = 1.5f;
        public const float PITCH_RANGE = MAX_PITCH - MIN_PITCH;
        public const float EYE_HEIGHT_OFFSET = 0.5f;

        // Camera Matrices
        Matrix mView, mProj;
        public Matrix View {
            get { return mView; }
            private set {
                mView = value;
            }
        }
        public Matrix Projection {
            get { return mProj; }
            private set {
                mProj = value;
            }
        }

        // For Panning And Zooming The Camera
        private CameraController camController;
        public CameraController Controller {
            get { return camController; }
        }
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

        public Camera(Viewport v) {
            camOrigin = INITIAL_CAMERA_ORIGIN;
            Yaw = INITIAL_CAMERA_YAW;
            Pitch = INITIAL_CAMERA_PITCH;
            ZoomRatio = INITIAL_ZOOM_RATIO;
            lowSettings = INITIAL_LOW_SETTINGS;
            highSettings = INITIAL_HIGH_SETTINGS;
            ZoomSpeed = INITIAL_ZOOM_SPEED;
            RecalculateView(null, MovementSettings.MaxDistance);

            IsOrthographic = false;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, v.AspectRatio, 0.01f, 2000f);
            camController = new CameraController(v.Width, v.Height);
        }

        // View Matrix Recalculation
        public void RecalculateView(Heightmap map, float dist) {
            Matrix rot =
                Matrix.CreateRotationZ(Pitch) *
                Matrix.CreateRotationY(Yaw);

            Vector3 back = Vector3.TransformNormal(Vector3.UnitX, rot);
            back.Normalize();

            Vector3 eye = CamOrigin + back * dist;
            //if(map != null) {
            //    float h = map.HeightAt(eye.X, eye.Z);
            //    if(eye.Y < h)
            //        eye.Y = h + EYE_HEIGHT_OFFSET;
            //}
            View = Matrix.CreateLookAt(eye, CamOrigin, Vector3.Up);
        }

        public void Update(Heightmap map, float dt) {
            CameraMotionSettings cms = MovementSettings;

            Scroll(camController.ScrollX, camController.ScrollZ, cms, dt);
            Orbit(camController.Yaw, camController.Pitch, cms, dt);
            int z;
            camController.GetZoom(out z);
            Zoom(z, dt);
            camOrigin.X = MathHelper.Clamp(camOrigin.X, 0, map.Width);
            camOrigin.Z = MathHelper.Clamp(camOrigin.Z, 0, map.Depth);
            //camOrigin.Y = Grey.Vox.Region.HEIGHT * 0.5f; // MathHelper.Clamp(camOrigin.Y, 0, Grey.Vox.Region.HEIGHT);
            // camOrigin.Y = map.SmoothHeightAt(camOrigin.X, camOrigin.Z);

            bool reset;
            camController.GetResetDefault(out reset);
            if(reset) {
                Yaw = INITIAL_CAMERA_YAW;
                Pitch = INITIAL_CAMERA_PITCH;
                ZoomRatio = INITIAL_ZOOM_RATIO;
            }

            RecalculateView(map, MathHelper.Lerp(cms.MinDistance, cms.MaxDistance, ZoomRatio));
        }
        private void Scroll(int x, int y, CameraMotionSettings cms, float dt) {
            camOrigin.Y += camController.ScrollY * cms.ScrollSpeed * dt * (ZoomRatio + ZOOM_OFFSET);

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
        
        public void MoveTo(float x, float z) {
            camOrigin.X = x;
            camOrigin.Z = z;
        }

        public BoundingFrustum GetSelectionBox(Vector2 screenMin, Vector2 screenMax) {
            Vector2 ss = new Vector2(camController.WindowWidth, camController.WindowHeight);
            screenMin /= ss;
            screenMax /= ss;
            float miny = 1 - screenMax.Y;
            float maxy = 1 - screenMin.Y;
            screenMin.Y = miny;
            screenMax.Y = maxy;
            screenMin *= 2f; screenMin -= Vector2.One;
            screenMax *= 2f; screenMax -= Vector2.One;

            return SelectionDetection.FromSelectionRect(mView, mProj, Vector2.Min(screenMin, screenMax), Vector2.Max(screenMin, screenMax));
        }
        public Ray GetViewRay(Vector2 screenPos) {
            Ray r = new Ray();

            // Inverse Screen
            screenPos.X /= camController.WindowWidth;
            screenPos.Y /= camController.WindowHeight;
            screenPos.Y = 1 - screenPos.Y;
            screenPos *= 2f;
            screenPos -= Vector2.One;

            // Inverse Project
            Matrix mProjInv = Matrix.Invert(mProj);
            Vector4 near = new Vector4(screenPos, 0, 1);
            near = Vector4.Transform(near, mProjInv);
            near /= near.W;
            Vector4 far = new Vector4(screenPos, 1, 1);
            far = Vector4.Transform(far, mProjInv);
            far /= far.W;

            // Inverse View
            Matrix mViewInv = Matrix.Invert(mView);
            r.Position = Vector3.Transform(new Vector3(near.X, near.Y, near.Z), mViewInv);
            r.Direction = Vector3.Transform(new Vector3(far.X, far.Y, far.Z), mViewInv);
            r.Direction -= r.Position;
            r.Direction.Normalize();
            return r;
        }
    }
}