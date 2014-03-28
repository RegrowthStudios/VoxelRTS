using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTS {
    public class OrbitingCamera {
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
    }
}