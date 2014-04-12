using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NovaLibrary.Object
{
    public interface INovaObject : ICloneable
    {
        NOVA_TYPE Type { get; }
        Vector2 Center { get; set; }
        float Rotation { get; set; }
        float Radius { get; set; }
        float Mass { get; set; }

        void Spawn();
        void draw(SpriteBatch batch);

        void Update(GameTime time);
        void OnCollision(NovaObject o);
    }
}
