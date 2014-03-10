using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTSEngine.Screens {
    public class KMeansScreen : GameScreenIndexed {
        private const int KMEANS_CLUSTERS = 8;
        private const int KMEANS_POINTS = 2000;

        public KMeansScreen(int i) : base(i) { }
        public KMeansScreen(int p, int n) : base(p, n) { }

        Texture2D pixel;
        Vector2[] kmeansPos, centPos;
        Color[] kmeansCol, centColors;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;

            Random r = new Random();

            Vector2 vs = ViewSize;
            Vector3[] data = new Vector3[KMEANS_POINTS];
            for(int i = 0; i < data.Length; i++) {
                data[i].X = (float)r.NextDouble() * vs.X;
                data[i].Y = (float)r.NextDouble() * vs.Y;
                data[i].Z = 0;
            }
            var res = RTSEngine.Algorithms.KMeans.Compute(KMEANS_CLUSTERS, data);
            centPos = new Vector2[res.Centroids.Length];
            centColors = new Color[res.Centroids.Length];
            byte[] bc = new byte[3];
            for(int i = 0; i < centColors.Length; i++) {
                centPos[i].X = res.Centroids[i].X;
                centPos[i].Y = res.Centroids[i].Y;
                r.NextBytes(bc);
                centColors[i].R = bc[0];
                centColors[i].G = bc[1];
                centColors[i].B = bc[2];
                centColors[i].A = 128;
            }
            kmeansPos = new Vector2[data.Length];
            kmeansCol = new Color[data.Length];
            for(int i = 0; i < data.Length; i++) {
                kmeansPos[i] = new Vector2(data[i].X, data[i].Y);
                kmeansCol[i] = centColors[res.ClusterAssigns[i]];
                kmeansCol[i].A = 255;
            }
            pixel = new Texture2D(G, 1, 1);
            pixel.SetData(new Color[] { Color.White });
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
            pixel.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            SB.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            for(int i = 0; i < kmeansPos.Length; i++)
                SB.Draw(pixel, kmeansPos[i], null, kmeansCol[i], 0, Vector2.One / 2f, 5f, SpriteEffects.None, 0f);
            for(int i = 0; i < centPos.Length; i++)
                SB.Draw(pixel, centPos[i], null, centColors[i], 0, Vector2.One / 2f, 50f, SpriteEffects.None, 0f);
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