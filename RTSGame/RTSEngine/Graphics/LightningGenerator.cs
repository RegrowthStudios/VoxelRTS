using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Graphics {
    public class LightningData {
        public readonly List<Vector2> Lines;
        public readonly Color Color;

        public LightningData(Color c) {
            Lines = new List<Vector2>(40);
            Color = c;
        }
    }

    public static class LightningGenerator {
        #region Lightning Arguments
        public struct BoltArgs {
            public Vector2 Start, End;
            public float JagDisplacement;
            public float LineMinLength, LineMaxLength;
            public Color Color;

            public BoltArgs(Vector2 s, Vector2 e, float jDisp, float minL, float maxL, Color c) {
                Start = s;
                End = e;
                JagDisplacement = jDisp;
                LineMinLength = minL;
                LineMaxLength = maxL;
                Color = c;
            }
        }
        public struct BranchArgs {
            public Vector2 Start, End;
            public float JagDisplacement;
            public float LineMinLength, LineMaxLength;
            public float BranchSlope;
            public Vector2 MinBounds, MaxBounds;
            public Color Color;

            public BranchArgs(Vector2 s, Vector2 e, float jDisp, float minL, float maxL, float slope, Vector2 minB, Vector2 maxB, Color c) {
                Start = s;
                End = e;
                JagDisplacement = jDisp;
                LineMinLength = minL;
                LineMaxLength = maxL;
                BranchSlope = slope;
                MinBounds = minB;
                MaxBounds = maxB;
                Color = c;
            }
            public BranchArgs(BoltArgs a, float slope, Vector2 minB, Vector2 maxB) {
                Start = a.Start;
                End = a.End;
                JagDisplacement = a.JagDisplacement;
                LineMinLength = a.LineMinLength;
                LineMaxLength = a.LineMaxLength;
                BranchSlope = slope;
                MinBounds = minB;
                MaxBounds = maxB;
                Color = a.Color;
            }
        }
        #endregion

        public static void CreateLightning(BoltArgs a, ref LightningData data, Random r) {
            // Find Displacement
            Vector2 d = a.End - a.Start;
            float dist = d.Length();
            d /= dist;
            Vector2 n = new Vector2(-d.Y, d.X);

            // Get Range Of Lightning Displacements
            float range = a.LineMaxLength - a.LineMinLength;

            // Add Startpoint
            data.Lines.Add(a.Start);

            // Add Jags
            while(dist > a.LineMaxLength) {
                float disp = (float)r.NextDouble() * range + a.LineMinLength;
                a.Start += disp * d;
                dist -= disp;
                Vector2 nl = a.Start + ((float)r.NextDouble() * 2 - 1) * a.JagDisplacement * n;
                data.Lines.Add(nl);
                data.Lines.Add(nl);
            }

            // Add Endpoint
            data.Lines.Add(a.End);
        }
        public static void CreateBranch(BranchArgs a, ref LightningData data, Random r) {
            // Find Displacement
            Vector2 d = a.End - a.Start;
            float dist = d.Length();
            d /= dist;
            Vector2 n = new Vector2(-d.Y, d.X);

            // Get Range Of Lightning Displacements
            float range = a.LineMaxLength - a.LineMinLength;

            // Add Startpoint
            data.Lines.Add(a.Start);

            // Add Jags
            while(dist > a.LineMaxLength) {
                float disp = (float)r.NextDouble() * range + a.LineMinLength;
                a.Start += disp * d;
                dist -= disp;
                Vector2 nl = a.Start + ((float)r.NextDouble() * 2 - 1) * a.JagDisplacement * n;
                data.Lines.Add(nl);

                if(nl.X < a.MinBounds.X || nl.Y < a.MinBounds.Y || nl.X > a.MaxBounds.X || nl.Y > a.MaxBounds.Y)
                    return;

                if(r.NextDouble() > 0.95) {
                    Vector2 bd = nl - data.Lines[data.Lines.Count - 2];
                    bd.Normalize();
                    Vector2 bn = new Vector2(-bd.Y, bd.X);

                    // Make Left Branch
                    float dispb = ((float)r.NextDouble() * 2 - 1) * a.BranchSlope * dist;
                    a.End = nl + bd * dist + bn * dispb;
                    CreateBranch(a, ref data, r);

                    // Make Right Branch
                    dispb = ((float)r.NextDouble() * 2 - 1) * a.BranchSlope * dist;
                    a.End = nl + bd * dist + bn * dispb;
                    CreateBranch(a, ref data, r);

                    return;
                }
                else {
                    data.Lines.Add(nl);
                }
            }

            // Add Endpoint
            data.Lines.Add(a.End);
        }
    }
}