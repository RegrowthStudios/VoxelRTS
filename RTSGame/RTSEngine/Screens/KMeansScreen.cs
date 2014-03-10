using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTSEngine.Screens {
    public class KMeansScreen : GameScreenIndexed {
        private const int KMEANS_CLUSTERS = 40;
        private const int KMEANS_POINTS = 6000;

        public KMeansScreen(int i) : base(i) { }
        public KMeansScreen(int p, int n) : base(p, n) { }

        private Texture2D tPixel;
        private Vector2[] dataPos, centPos;
        private Color[] dataCol, centCol;
        private RTSEngine.Algorithms.KMeansResult res;
        Thread t;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;

            // Run K-Means On Random Data
            Random r = new Random();
            Vector2 vs = ViewSize;
            Vector3[] data = new Vector3[KMEANS_POINTS];
            for(int i = 0; i < data.Length; i++) {
                data[i].X = (float)r.NextDouble() * vs.X;
                data[i].Y = (float)r.NextDouble() * vs.Y;
                data[i].Z = 0;
            }

            res = new Algorithms.KMeansResult(KMEANS_POINTS, KMEANS_CLUSTERS);
            t = new Thread(() => {
                RTSEngine.Algorithms.KMeans.Compute(KMEANS_CLUSTERS, data, ref res, 100);
            });

            // Build Centroid View
            centPos = new Vector2[res.Centroids.Length];
            centCol = new Color[res.Centroids.Length];
            byte[] bc = new byte[3];
            for(int i = 0; i < centCol.Length; i++) {
                r.NextBytes(bc);
                centCol[i].R = bc[0];
                centCol[i].G = bc[1];
                centCol[i].B = bc[2];
                centCol[i].A = 128;
            }

            // Build Data View
            dataPos = new Vector2[data.Length];
            dataCol = new Color[data.Length];
            for(int i = 0; i < data.Length; i++) {
                dataPos[i] = new Vector2(data[i].X, data[i].Y);
            }

            // Create Quick Texture
            tPixel = new Texture2D(G, 1, 1);
            tPixel.SetData(new Color[] { Color.White });

            t.Start();
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
            tPixel.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            for(int i = 0; i < centCol.Length; i++) {
                centPos[i].X = res.Centroids[i].X;
                centPos[i].Y = res.Centroids[i].Y;
            }
            for(int i = 0; i < dataCol.Length; i++) {
                dataCol[i] = centCol[res.ClusterAssigns[i]];
                dataCol[i].A = 255;
            }

            SB.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            for(int i = 0; i < dataPos.Length; i++)
                SB.Draw(tPixel, dataPos[i], null, dataCol[i], 0, Vector2.One / 2f, 5f, SpriteEffects.None, 0f);
            for(int i = 0; i < centPos.Length; i++)
                SB.Draw(tPixel, centPos[i], null, centCol[i], 0, Vector2.One / 2f, 50f, SpriteEffects.None, 0f);
            SB.End();
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Escape:
                    State = ScreenState.ChangeNext;
                    break;
            }
        }
    }
}