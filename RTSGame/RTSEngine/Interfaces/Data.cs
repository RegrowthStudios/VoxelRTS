using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;

namespace RTSEngine.Interfaces {
    public interface IToggleableGraphics : IDisposable {
        bool IsActive { get; }

        bool SetActive(GraphicsDevice g);
        void SetInactive(GraphicsDevice g);
    }

    public interface IResource {
        ushort ID { get; }
        ResourceType Type { get; }
        string TypeName { get; }
        string FriendlyName { get; }
    }
}