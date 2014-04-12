using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NovaLibrary.Object {
    public class Asteroid : NovaMovingObject, IBoundable {
        public static List<Asteroid> ALL = new List<Asteroid>();
        static Asteroid last;

        protected bool hasEntered;
        protected bool shouldDestroy;

        public static void AttemptSpawn() {
            if(ALL.Count < 40 && NovaObjectContent.RAND.Next(70) > 60) {
                ObjectProcessor.addObject<Asteroid>(out last);
            }
        }

        public override void Spawn() {
            hasEntered = false;
            shouldDestroy = false;
            type = NOVA_TYPE.ASTEROID;
            setTexture(NovaObjectContent.Texture(1));
            centerOffset = Vector2.One * 160f;
            color = Color.Chartreuse;
            Radius = 12f;
            Mass = 200f;
            ALL.Add(this);
            center = new Vector2(
                NovaObjectContent.RAND.Next(300) * -1f,
                NovaObjectContent.RAND.Next(300) * -1f
                );
            Point gameCenter = GameArea.GameBounds.Center;
            Vector2 dest = new Vector2(
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Width - 10) + GameArea.GameBounds.Left + 5,
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Height - 10) + GameArea.GameBounds.Top + 5
                );
            velocity = Vector2.Normalize(dest - center);
            velocity *= 25f + NovaObjectContent.RAND.Next(80);
        }

        public override void Update(GameTime time) {
            if(shouldDestroy) {
                ObjectProcessor.removeObject<Asteroid>(this);
                ALL.Remove(this);
            }
        }

        public override void OnCollision(NovaObject o) {

        }

        public void CheckBound(Rectangle bounds) {
            if(!hasEntered) {
                if(bounds.Contains(
                    (int)center.X,
                    (int)center.Y
                    )) {
                    hasEntered = true;
                }
            }
            else {
                if(!bounds.Contains(
                    (int)center.X,
                    (int)center.Y
                    )) {
                    shouldDestroy = true;
                }
            }
        }

        public override void Move(float dt) {
            center += velocity * dt;
            CheckBound(GameArea.GameBounds);
        }
    }
}