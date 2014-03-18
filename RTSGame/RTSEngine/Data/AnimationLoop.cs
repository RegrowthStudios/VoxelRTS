using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public class AnimationLoop {
        private static readonly Random r = new Random();

        private float curFrame;
        private int frameCount;
        public int StartFrame {
            get;
            private set;
        }
        public int EndFrame {
            get;
            private set;
        }

        public float FrameSpeed {
            get;
            set;
        }
        public int CurrentFrame {
            get {
                int nextFrame = (int)curFrame + StartFrame;
                return nextFrame > EndFrame ? EndFrame : nextFrame;
            }
        }

        public AnimationLoop(int s, int e) {
            StartFrame = s;
            EndFrame = e;
            frameCount = EndFrame - StartFrame + 1;
            curFrame = 0;
            FrameSpeed = 60;
        }

        public void Restart(bool rand = false) {
            if(rand) {
                curFrame = r.Next(0, frameCount);
            }
            else {
                curFrame = 0;
            }
        }

        public void Step(float dt) {
            curFrame += FrameSpeed * dt;
            while(curFrame > frameCount) {
                curFrame -= frameCount;
            }
        }
    }
}