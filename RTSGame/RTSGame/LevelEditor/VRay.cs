using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTS {
    public class VRay {
        public Vector3 Start;
        public Vector3 Direction;

        public Vector3 CurrentV;
        public Vector3I CurrentCL;

        public VRay(Vector3 start, Vector3 direction) {
            Start = start;
            Direction = direction;
            CurrentV = Start;
            CurrentCL = new Vector3I((int)Start.X, (int)Start.Y, (int)Start.Z);
        }

        public Vector3I GetNextLocation() {
            //Find All Ratios To Next In Each Direction
            double[] next = new double[3];
            double[] r = new double[3];
            if(Direction.X > 0) {
                if(CurrentV.X == (int)CurrentV.X) { next[0] = CurrentV.X + 1; }
                else { next[0] = Math.Ceiling(CurrentV.X); }
                r[0] = (next[0] - CurrentV.X) / Direction.X;
            }
            else if(Direction.X < 0) {
                if(CurrentV.X == (int)CurrentV.X) { next[0] = CurrentV.X - 1; }
                else { next[0] = Math.Floor(CurrentV.X); }
                r[0] = (next[0] - CurrentV.X) / Direction.X;
            }
            else {
                r[0] = double.PositiveInfinity;
            }
            if(Direction.Y > 0) {
                if(CurrentV.Y == (int)CurrentV.Y) { next[1] = CurrentV.Y + 1; }
                else { next[1] = Math.Ceiling(CurrentV.Y); }
                r[1] = (next[1] - CurrentV.Y) / Direction.Y;
            }
            else if(Direction.Y < 0) {
                if(CurrentV.Y == (int)CurrentV.Y) { next[1] = CurrentV.Y - 1; }
                else { next[1] = Math.Floor(CurrentV.Y); }
                r[1] = (next[1] - CurrentV.Y) / Direction.Y;
            }
            else {
                r[1] = double.PositiveInfinity;
            }
            if(Direction.Z > 0) {
                if(CurrentV.Z == (int)CurrentV.Z) { next[2] = CurrentV.Z + 1; }
                else { next[2] = Math.Ceiling(CurrentV.Z); }
                r[2] = (next[2] - CurrentV.Z) / Direction.Z;
            }
            else if(Direction.Z < 0) {
                if(CurrentV.Z == (int)CurrentV.Z) { next[2] = CurrentV.Z - 1; }
                else { next[2] = Math.Floor(CurrentV.Z); }
                r[2] = (next[2] - CurrentV.Z) / Direction.Z;
            }
            else {
                r[2] = double.PositiveInfinity;
            }

            //Get Min
            float rat;
            if(r[0] < r[1] && r[0] < r[2]) {
                //X
                rat = (float)r[0];
                CurrentV += Direction * rat;
                if(Direction.X > 0) { CurrentCL.X++; }
                else if(Direction.X < 0) { CurrentCL.X--; }
            }
            else if(r[1] < r[2]) {
                //Y
                rat = (float)r[1];
                CurrentV += Direction * rat;
                if(Direction.Y > 0) { CurrentCL.Y++; }
                else if(Direction.Y < 0) { CurrentCL.Y--; }
            }
            else {
                //Z
                rat = (float)r[2];
                CurrentV += Direction * rat;
                if(Direction.Z > 0) { CurrentCL.Z++; }
                else if(Direction.Z < 0) { CurrentCL.Z--; }
            }

            return CurrentCL;
        }
    }
}
