using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS {
    public class OrbitingCamera {
        private const float YAW_SPEED = 1f;
        private const float PITCH_SPEED = 1f;
        private const float FOV_ZOOM_SPEED = 0.5f;
        private const float DIST_ZOOM_SPEED = 2f;

        public const float MIN_FOV = 0.001f;
        public const float MAX_FOV = MathHelper.PiOver2;
        public const float PROJ_NEAR = 0.001f;
        public const float PROJ_FAR = 1000f;
        public const float DEFAULT_MIN_PITCH = 0.01f;
        public const float DEFAULT_MAX_PITCH = MathHelper.PiOver2 - 0.01f;
        public const float DEFAULT_FOV = MathHelper.PiOver4;
        public const float DEFAULT_ASPECT = 4f / 3f;

        // View Information
        public Vector3 Eye {
            get {
                return Vector3.Normalize(new Vector3(
                    (float)(Math.Sin(Yaw) * Math.Cos(Pitch)),
                    (float)(Math.Sin(Pitch)),
                    (float)(Math.Cos(Yaw) * Math.Cos(Pitch))
                    )) *
                    Distance +
                    Center;
            }
        }
        public Vector3 Center {
            get;
            set;
        }
        public float Distance {
            get;
            set;
        }

        // Rotational Information
        Vector3 rotation;
        public float Yaw {
            get { return rotation.Y; }
            set { rotation.Y = MathHelper.WrapAngle(value); }
        }
        public float Pitch {
            get { return rotation.X; }
            set { rotation.X = MathHelper.Clamp(value, PitchMin, PitchMax); }
        }
        public float PitchMin {
            get;
            set;
        }
        public float PitchMax {
            get;
            set;
        }

        // Another Way To Zoom
        private float fov;
        public float FOV {
            get { return fov; }
            set { fov = MathHelper.Clamp(value, MIN_FOV, MAX_FOV); }
        }

        // Camera Matrices
        private Matrix mView, mProj;
        public Matrix View {
            get { return mView; }
        }
        public Matrix Projection {
            get { return mProj; }
        }

        public OrbitingCamera(Vector3 c, float d, float aspect = DEFAULT_ASPECT) {
            Center = c;
            Distance = d;

            PitchMin = DEFAULT_MIN_PITCH;
            PitchMax = DEFAULT_MAX_PITCH;
            rotation = new Vector3((PitchMin + PitchMax) * 0.5f, 0, 0);
            fov = DEFAULT_FOV;

            UpdateView();
            UpdateProjection(aspect);
        }

        public void UpdateView() {
            mView = Matrix.CreateLookAt(Eye, Center, Vector3.Up);
        }
        public void UpdateProjection(float aspect, float near = PROJ_NEAR, float far = PROJ_FAR) {
            mProj = Matrix.CreatePerspectiveFieldOfView(FOV, aspect, near, far);
        }

        public void ControlCamera(float dt, InputManager input, Viewport vp) {
            Point p = input.Mouse.Displacement;
            if(input.Keyboard.Current.IsKeyDown(Keys.LeftControl)) {
                if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                    // Zoom By FOV
                    if(p.Y != 0) FOV *= (float)Math.Pow(FOV_ZOOM_SPEED, dt * p.Y);
                }
            }
            else if(input.Keyboard.Current.IsKeyDown(Keys.LeftShift)) {
                if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                    // Pan
                    Vector3 eye = Eye;
                    Ray r1 = new Ray(eye, vp.Unproject(
                        new Vector3(input.Mouse.Previous.X, input.Mouse.Previous.Y, 1),
                        Projection,
                        View,
                        Matrix.Identity
                        ));
                    Ray r2 = new Ray(eye, vp.Unproject(
                        new Vector3(input.Mouse.Current.X, input.Mouse.Current.Y, 1),
                        Projection,
                        View,
                        Matrix.Identity
                        ));

                    Matrix mVI = Matrix.Invert(mView);
                     Plane hit = new Plane(Center, Center + mVI.Up, Center + mVI.Right);

                   //new Plane(Vector3.Normalize(eye - Center), 0f);

                    r1.Direction -= r1.Position; r1.Direction.Normalize();
                    float d1 = r1.Intersects(hit).Value;
                    Vector3 p1 = r1.Position + d1 * r1.Direction;

                    r2.Direction -= r2.Position; r2.Direction.Normalize();
                    float d2 = r2.Intersects(hit).Value;
                    Vector3 p2 = r2.Position + d2 * r2.Direction;

                    Center += p1 - p2;
                }
            }
            else if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                // Yaw
                if(p.X != 0) Yaw -= YAW_SPEED * (p.X * dt);
                // Pitch
                if(p.Y != 0) Pitch += PITCH_SPEED * (p.Y * dt);
            }

            // Zoom By Distance
            if(input.Mouse.ScrollDisplacement != 0) {
                float amt = (input.Mouse.ScrollDisplacement / 60) * dt;
                Distance = MathHelper.Clamp(Distance * (float)Math.Pow(DIST_ZOOM_SPEED, amt), 0.1f, 1000f);
            }

            if(input.Keyboard.IsKeyJustPressed(Keys.R)) {
                // Reset The Camera
                Center = Vector3.Zero;
                Distance = 4f;
                Pitch = MathHelper.PiOver4;
                Yaw = 0;
                FOV = MathHelper.PiOver4;
            }
        }
        public Ray GetViewRay(Vector2 screenPos, int ww, int wh) {
            Ray r = new Ray();

            // Inverse Screen
            screenPos.X /= ww;
            screenPos.Y /= wh;
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