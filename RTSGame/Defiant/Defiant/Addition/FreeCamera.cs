using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS {
    public class FreeCamera {
        private const float YAW_SPEED = 0.25f;
        private const float PITCH_SPEED = 0.25f;
        private const float FOV_ZOOM_SPEED = 0.5f;
        private const float MOVE_SPEED = 1f;

        public const float MIN_FOV = 0.001f;
        public const float MAX_FOV = MathHelper.PiOver2;
        public const float PROJ_NEAR = 0.001f;
        public const float PROJ_FAR = 1000f;
        public const float DEFAULT_MIN_PITCH = 0.01f - MathHelper.PiOver2;
        public const float DEFAULT_MAX_PITCH = MathHelper.PiOver2 - 0.01f;
        public const float DEFAULT_FOV = MathHelper.PiOver4;
        public const float DEFAULT_ASPECT = 4f / 3f;

        // View Information
        public Vector3 Eye {
            get;
            private set;
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

        // World Matrix
        private Matrix mWorld;
        public Matrix World {
            get { return mWorld; }
        }

        public FreeCamera(Vector3 c, float yaw, float pitch, float aspect = DEFAULT_ASPECT) {
            FOV = DEFAULT_FOV;
            PitchMin = DEFAULT_MIN_PITCH;
            PitchMax = DEFAULT_MAX_PITCH;
            Pitch = pitch;
            Yaw = yaw;
            mWorld = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0) * Matrix.CreateTranslation(c);
            UpdateView();
            UpdateProjection(aspect);
        }

        public void UpdateView() {
            mView = Matrix.Invert(mWorld);
            Eye = mWorld.Translation;
        }
        public void UpdateProjection(float aspect, float near = PROJ_NEAR, float far = PROJ_FAR) {
            mProj = Matrix.CreatePerspectiveFieldOfView(FOV, aspect, near, far);
        }

        public void ControlCamera(float dt, InputManager input, Viewport vp) {
            if(input.Keyboard.IsKeyJustPressed(Keys.LeftControl) || input.Keyboard.IsKeyJustPressed(Keys.RightControl)) {
                if(input.Mouse.IsBound)
                    input.Mouse.Unbind();
                else
                    input.Mouse.Bind(vp.Width / 2, vp.Height / 2);
                return;
            }

            if(!input.Mouse.IsBound)
                return;

            Point p = input.Mouse.Displacement;
            Vector3I moves = Vector3I.Zero;
            moves.X += input.Keyboard.Current.IsKeyDown(Keys.D) ? 1 : 0;
            moves.X -= input.Keyboard.Current.IsKeyDown(Keys.A) ? 1 : 0;
            moves.Z += input.Keyboard.Current.IsKeyDown(Keys.W) ? 1 : 0;
            moves.Z -= input.Keyboard.Current.IsKeyDown(Keys.S) ? 1 : 0;
            moves.Y += input.Keyboard.Current.IsKeyDown(Keys.Space) ? 1 : 0;
            moves.Y -= input.Keyboard.Current.IsKeyDown(Keys.LeftShift) ? 1 : 0;

            Yaw += -p.X * YAW_SPEED * dt;
            Pitch += -p.Y * PITCH_SPEED * dt;

            mWorld =
                Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0) *
                Matrix.CreateTranslation(
                    moves.X * mWorld.Right +
                    moves.Y * mWorld.Up +
                    moves.Z * mWorld.Forward +
                    mWorld.Translation
                    );
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