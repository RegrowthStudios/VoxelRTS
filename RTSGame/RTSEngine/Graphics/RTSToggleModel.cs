using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Interfaces;

namespace RTSEngine.Graphics {
    public class RTSToggleModel : IToggleableGraphics {
        // The Location Of The Model
        private string file;

        // How This Model Draws
        public VertexBuffer VBuffer { get; private set; }
        public IndexBuffer IBuffer { get; private set; }
        public Texture2D ModelTexture { get; private set; }

        public bool IsActive { get; private set; }
        public ParsingFlags ParseFlags { get; set; }

        public RTSToggleModel(FileInfo fi, ParsingFlags ps = ParsingFlags.ConversionOpenGL) {
            // Perform Check
            if(fi == null || !fi.Exists) throw new ArgumentException("No Valid File");

            // Grab The File Name And Parsing Flags
            file = fi.FullName;
            ParseFlags = ps;

            // Not Yet Active
            IsActive = false;
            VBuffer = null;
            IBuffer = null;
        }
        ~RTSToggleModel() {
            Dispose();
        }

        private void Activate(GraphicsDevice g) {
            VertexPositionNormalTexture[] verts;
            int[] inds;
            using(var fs = File.OpenRead(file)) {
                ObjParser.TryParse(fs, out verts, out inds, ParseFlags);
            }
            if(inds == null || verts == null) throw new ArgumentException("Cannot Parse Model");

            VBuffer = new VertexBuffer(g, VertexPositionNormalTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            VBuffer.SetData(verts);
            IBuffer = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            IBuffer.SetData(inds);
        }
        public bool SetActive(GraphicsDevice g) {
            if(IsActive) return true;
            try {
                Activate(g);
                IsActive = true;
                return true;
            }
            catch(Exception) {
                SetInactive(g);
                return false;
            }
        }

        public void SetInactive(GraphicsDevice g) {
            if(!IsActive) return;

            if(VBuffer != null && !VBuffer.IsDisposed) {
                VBuffer.Dispose();
                VBuffer = null;
            }
            if(IBuffer != null && !IBuffer.IsDisposed) {
                IBuffer.Dispose();
                IBuffer = null;
            }
        }

        public void Dispose() {
            SetInactive(null);
        }
    }
}