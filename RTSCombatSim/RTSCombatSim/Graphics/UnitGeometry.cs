using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Team;

namespace RTSCS.Graphics {
    #region Instancing Vertex Type
    public struct VertexUnitInstance : IVertexType {
        #region Declaration
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.Position, 4),
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Color, VertexElementUsage.Color, 1)
            );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }
        #endregion

        public Matrix World;
        public Color Color;

        public VertexUnitInstance(Matrix w, Color c) {
            World = w;
            Color = c;
        }
        public VertexUnitInstance(Vector3 scale, float r, Vector3 pos, Color c) {
            World = Matrix.CreateRotationZ(r);
            World.Right *= scale.X;
            World.Up *= scale.Y;
            World.Backward *= scale.Z;
            World.Translation = pos;
            Color = c;
        }
    }
    public struct UnitRenderData {
        public RTSUnitInstance Unit;
        public Color Color;
    }
    #endregion

    public class UnitGeometry : IDisposable {
        // Defines The Model Geometry
        private VertexBuffer vbModel;
        private IndexBuffer ibModel;

        // Defines Location And Color Of Geometry Instances
        private DynamicVertexBuffer vbInst;

        // The Texture To Be Used By The Unit
        private Texture2D texture;

        // A Way To Know If A Unit Belongs In The Batch
        public RTSUnit UnitData {
            get;
            private set;
        }

        // List Of Units To Render
        private List<UnitRenderData> units;

        // Holds All Instance Information
        private VertexUnitInstance[] instances;
        public int UnitCount {
            get { return instances.Length; }
        }
        public VertexUnitInstance this[int i] {
            get { return instances[i]; }
        }

        public UnitGeometry(GraphicsDevice g, string texFile, float scale, int count, RTSUnit data) {
            IsDisposed = false;
            UnitData = data;
            units = new List<UnitRenderData>();

            // Load The Texture
            using(var s = System.IO.File.OpenRead(texFile)) {
                texture = Texture2D.FromStream(g, s);
            }

            // Create Model
            VertexPositionTexture[] verts = {
                new VertexPositionTexture(new Vector3(-scale, scale, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(scale, scale, 0), Vector2.UnitX),
                new VertexPositionTexture(new Vector3(-scale, -scale, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(scale, -scale, 0), Vector2.One)
            };
            int[] inds = { 0, 1, 2, 2, 1, 3 };


            // Create Model Buffers
            vbModel = new VertexBuffer(g, VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbModel.SetData(verts);
            ibModel = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ibModel.SetData(inds);

            // Create Instancing Buffer
            instances = new VertexUnitInstance[count];
            for(int i = 0; i < UnitCount; i++) {
                instances[i].World = Matrix.Identity;
                instances[i].Color = Color.White;
            }
            vbInst = new DynamicVertexBuffer(g, VertexUnitInstance.Declaration, UnitCount, BufferUsage.WriteOnly);
            vbInst.SetData(instances);
        }
        #region Disposing Destructor
        ~UnitGeometry() {
            if(!IsDisposed) Dispose();
        }
        public event Action<object> OnDisposal;
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            if(IsDisposed)
                throw new ObjectDisposedException("Unit Geometry #" + GetHashCode());

            IsDisposed = true;
            if(OnDisposal != null) OnDisposal(this);

            if(vbModel != null) {
                vbModel.Dispose();
                vbModel = null;
            }
            if(vbInst != null) {
                vbInst.Dispose();
                vbInst = null;
            }
            if(ibModel != null) {
                ibModel.Dispose();
                ibModel = null;
            }
        }
        #endregion

        // Instance Modification Methods
        public void SetInstanceMatrix(int i, Matrix w) {
            instances[i].World = w;
        }
        public void SetInstanceColor(int i, Color c) {
            instances[i].Color = c;
        }
        // Apply Count Of Instance Data To The Buffer
        public void ApplyInstancing(int c) {
            if(c == 0) return;
            vbInst.SetData(instances, 0, c);
        }
        public void ApplyInstancing() {
            ApplyInstancing(UnitCount);
        }

        public void AddUnit(RTSUnitInstance u, Color c) {
            units.Add(new UnitRenderData() { Unit = u, Color = c });
            SetInstanceMatrix(units.Count - 1, Matrix.CreateTranslation(u.WorldPosition));
            SetInstanceColor(units.Count - 1, c);
        }
        public void RemoveAll(Predicate<RTSUnitInstance> f) {
            units.RemoveAll((d) => { return f(d.Unit); });
        }
        public void InstanceUnits() {
            for(int i = 0; i < units.Count; i++) {
                float x = units[i].Unit.ViewDirection.X;
                float y = units[i].Unit.ViewDirection.Y;
                SetInstanceMatrix(i, new Matrix(
                    x, y, 0, 0,
                    y, -x, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                    ) * Matrix.CreateTranslation(units[i].Unit.WorldPosition));
                SetInstanceColor(i, units[i].Color);
            }
            ApplyInstancing(units.Count);
        }

        // Activate Buffers On The GPU
        public void SetBuffers(GraphicsDevice g) {
            g.Textures[0] = texture;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            g.SetVertexBuffers(
                new VertexBufferBinding(vbModel),
                new VertexBufferBinding(vbInst, 0, 1)
                );
            g.Indices = ibModel;
        }
        // Issue Draw Call
        public void Draw(GraphicsDevice g, int c) {
            if(c == 0) return;
            g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vbModel.VertexCount, 0, ibModel.IndexCount / 3, c);
        }
        public void Draw(GraphicsDevice g) {
            Draw(g, UnitCount);
        }
        public void DrawUnits(GraphicsDevice g) {
            Draw(g, units.Count);
        }
    }
}