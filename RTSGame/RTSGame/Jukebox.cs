using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace RTS {
    public class Jukebox : IDisposable {
        private List<SoundEffect> seFX;
        SoundEffectInstance currentSound;
        int si;

        public Jukebox() {
            seFX = new List<SoundEffect>();
            currentSound = null;
            si = 0;
        }
        public void Dispose() {
            var l = System.Threading.Interlocked.Exchange(ref seFX, new List<SoundEffect>());
            foreach(var se in l) se.Dispose();
        }

        public void LoadFromDirectory(DirectoryInfo dir) {
            var files = dir.GetFiles();
            foreach(var f in files) {
                using(var s = File.OpenRead(f.FullName)) {
                    seFX.Add(SoundEffect.FromStream(s));
                }
            }
        }

        public void Update() {
            if(currentSound == null) {
                if(seFX.Count < 1) return;
                currentSound = seFX[si].CreateInstance();
                currentSound.IsLooped = false;
                currentSound.Play();

                si++;
                si %= seFX.Count;
            }
            else {
                if(currentSound.State == SoundState.Stopped) {
                    currentSound.Dispose();
                    currentSound = null;
                }
            }
        }
    }
}