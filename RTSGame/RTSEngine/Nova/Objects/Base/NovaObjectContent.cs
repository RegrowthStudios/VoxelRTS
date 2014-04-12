using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace NovaLibrary.Object {
    public static class NovaObjectContent {
        public static Random RAND = new Random();
        private static Texture2D[] TEXTURES;
        public static bool isInitialized = false;

        public static void Initialize(GraphicsDevice g, string[] textures) {
            TEXTURES = new Texture2D[textures.Length];
            int i = 0;
            foreach(string path in textures) {
                using(var s = File.OpenRead(path)) {
                    TEXTURES[i++] = Texture2D.FromStream(g, s);
                }
            }
            isInitialized = true;
        }

        public static Texture2D Texture(int id) {
            return TEXTURES[id];
        }
    }
}