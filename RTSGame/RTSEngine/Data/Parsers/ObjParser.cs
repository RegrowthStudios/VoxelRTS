using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Graphics {
    // Parsing Options
    public enum ParsingFlags : byte {
        /// <summary>
        /// Assume Correct V TexCoord And 
        /// Triangles Ordered Clockwise
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Flips The V (Y) Texture Coordinate
        /// </summary>
        FlipTexCoordV = 0x01,
        /// <summary>
        /// Culls And Forms Triangles In The Opposite Order
        /// </summary>
        FlipTriangleOrder = 0x02,
        ConversionOpenGL = FlipTexCoordV | FlipTriangleOrder,

        /// <summary>
        /// Load An Effect As Byte Code Instead
        /// Of From The ContentManager
        /// </summary>
        LoadEffectByteCode = 0x04,
        /// <summary>
        /// Load A 2D Texture From A File Stream
        /// Instead Of From The ContentManager
        /// </summary>
        LoadTextureStream = 0x08,

        WriteUV = 0x10,
        WriteNorms = 0x20,
        WriteColor = 0x40,
        WriteAll = WriteUV | WriteNorms | WriteColor
    }

    // This Is Drawn With Triangles Arranged In Clockwise Order
    public struct ObjTriangle {
        public VertexPositionNormalTexture V1, V2, V3;
        public Vector3 Normal;

        public ObjTriangle(VertexPositionNormalTexture v1, VertexPositionNormalTexture v2, VertexPositionNormalTexture v3) {
            // Set Vertices
            V1 = v1; V2 = v2; V3 = v3;

            // Calculate Face Normal
            Normal = Vector3.Cross(
                V3.Position - V1.Position,
                V2.Position - V1.Position
                );
            Normal.Normalize();
        }
    }

    public static class ObjParser {
        // For Reading
        #region Intermediate Structs
        struct VInds {
            public int PosInd, UVInd, NormInd;

            public bool Viable {
                get { return PosInd >= 0; }
            }

            public VInds(string s) {
                // Trim And Split
                String[] inds = s.Trim().Split(new string[] { "/", @"\" }, StringSplitOptions.None);
                if(inds.Length < 1) { PosInd = -1; UVInd = -1; NormInd = -1; return; }

                // Get Position
                if(!int.TryParse(inds[0], out PosInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }

                // Get UV
                if(inds.Length < 2) {
                    UVInd = 0; NormInd = 0;
                }
                else {
                    if(string.IsNullOrEmpty(inds[1])) UVInd = 0;
                    else if(!int.TryParse(inds[1], out UVInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }
                }
                // Get Normal
                if(inds.Length < 3) {
                    NormInd = 0;
                }
                else {
                    if(string.IsNullOrEmpty(inds[2])) NormInd = 0;
                    else if(!int.TryParse(inds[2], out NormInd)) { PosInd = -1; UVInd = -1; NormInd = -1; return; }
                }
                // Make Indices Zero-based
                PosInd--;
                UVInd--;
                NormInd--;
            }
        }
        struct Tri {
            public VInds V1, V2, V3;

            public Tri(string v1, string v2, string v3) {
                V1 = new VInds(v1);
                V2 = new VInds(v2);
                V3 = new VInds(v3);
            }
        }
        class VertDict : IEnumerable<VertDict.Key> {
            public struct Key {
                public int Index;
                public VInds Vertex;

                public Key(int i, VInds v) {
                    Index = i;
                    Vertex = v;
                }
            }

            List<Key>[] verts;
            public int Count { get; private set; }

            public VertDict() {
                Count = 0;
                verts = new List<Key>[256];
                for(int i = 0; i < verts.Length; i++) {
                    verts[i] = new List<Key>(8);
                }
            }
            ~VertDict() {
                foreach(var l in verts) {
                    l.Clear();
                }
                verts = null;
                Count = 0;
            }

            public int get(VInds v) {
                int h = v.GetHashCode() & 0xff;
                for(int i = 0; i < verts[h].Count; i++) {
                    if(verts[h][i].Vertex.Equals(v)) return verts[h][i].Index;
                }
                verts[h].Add(new Key(Count, v));
                Count++;
                return Count - 1;
            }

            public IEnumerator<Key> GetEnumerator() {
                for(int h = 0; h < verts.Length; h++) {
                    for(int i = 0; i < verts[h].Count; i++) {
                        yield return verts[h][i];
                    }
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
        #endregion

        public static bool tryParse(Stream s, out VertexPositionNormalTexture[] verts, out int[] inds, ParsingFlags ps = ParsingFlags.None) {
            // Default Values
            verts = null; inds = null;

            // Encapsulate Stream To A Buffered Stream Reader
            BufferedStream bs = new BufferedStream(s);
            StreamReader f = new StreamReader(bs);

            // List Of Components
            List<Vector3> pos = new List<Vector3>(100);
            List<Vector2> uv = new List<Vector2>(100);
            List<Vector3> norms = new List<Vector3>(100);
            List<Tri> tris = new List<Tri>(200);

            // Buffer Vectors
            Vector3 v3 = Vector3.Zero; Vector2 v2 = Vector2.Zero;

            // Get All The Information From The Stream
            string line; string[] spl;
            while(!f.EndOfStream) {
                line = f.ReadLine();
                spl = Regex.Split(line, @"\s+", RegexOptions.IgnorePatternWhitespace);
                switch(spl[0].ToLower()) {
                    case "v": // Vertex Position
                        if(spl.Length != 4) return false;
                        if(!float.TryParse(spl[1], out v3.X)) return false;
                        if(!float.TryParse(spl[2], out v3.Y)) return false;
                        if(!float.TryParse(spl[3], out v3.Z)) return false;
                        pos.Add(v3);
                        break;
                    case "vt": // Vertex Texture Coordinate
                        if(spl.Length != 3) return false;
                        if(!float.TryParse(spl[1], out v2.X)) return false;
                        if(!float.TryParse(spl[2], out v2.Y)) return false;
                        // Possibly Flip Tex Coords
                        if(ps.HasFlag(ParsingFlags.FlipTexCoordV)) v2.Y = 1 - v2.Y;
                        uv.Add(v2);
                        break;
                    case "vn": // Vertex Normal
                        if(spl.Length != 4) return false;
                        if(!float.TryParse(spl[1], out v3.X)) return false;
                        if(!float.TryParse(spl[2], out v3.Y)) return false;
                        if(!float.TryParse(spl[3], out v3.Z)) return false;
                        norms.Add(v3);
                        break;
                    case "f": // Mesh Triangle
                        if(spl.Length != 4) return false;
                        try {
                            // Add In Correct Triangle Ordering
                            if(ps.HasFlag(ParsingFlags.FlipTriangleOrder))
                                tris.Add(new Tri(spl[1], spl[3], spl[2]));
                            else
                                tris.Add(new Tri(spl[1], spl[2], spl[3]));
                        }
                        catch(Exception) {
                            return false;
                        }
                        break;
                }
            }




            // Create Indices
            VertDict vd = new VertDict();
            inds = new int[tris.Count * 3];
            int ii = 0;
            foreach(Tri tri in tris) {
                inds[ii++] = vd.get(tri.V1);
                inds[ii++] = vd.get(tri.V2);
                inds[ii++] = vd.get(tri.V3);
            }

            // Create Vertices
            verts = new VertexPositionNormalTexture[vd.Count];
            foreach(VertDict.Key v in vd) {
                verts[v.Index].Position = pos[v.Vertex.PosInd];

                if(v.Vertex.UVInd < 0) verts[v.Index].TextureCoordinate = Vector2.Zero;
                else verts[v.Index].TextureCoordinate = uv[v.Vertex.UVInd];

                if(v.Vertex.NormInd < 0) verts[v.Index].Normal = Vector3.Zero;
                else verts[v.Index].Normal = norms[v.Vertex.NormInd];
            }

            return true;
        }
        public static bool tryParse(Stream s, out ObjTriangle[] tris, ParsingFlags ps = ParsingFlags.None) {
            VertexPositionNormalTexture[] verts;
            int[] inds;
            if(tryParse(s, out verts, out inds, ps)) {
                tris = new ObjTriangle[inds.Length / 3];
                for(int ti = 0, i = 0; ti < tris.Length; ti++) {
                    tris[ti] = new ObjTriangle(
                        verts[inds[i]],
                        verts[inds[i + 1]],
                        verts[inds[i + 2]]
                        );
                    i += 3;
                }
                return true;
            }
            tris = null;
            return false;
        }
        //public static bool tryParse(GraphicsDevice g, Stream s, out ModelBuffer mb, ParsingFlags ps = ParsingFlags.None, BufferUsage bu = BufferUsage.None) {
        //    VertexPositionNormalTexture[] verts;
        //    int[] inds;
        //    mb = ModelBuffer.Null;
        //    if(!tryParse(s, out verts, out inds, ps)) return false;
        //    mb.Vertex = new VertexBuffer(g, VertexPositionNormalTexture.VertexDeclaration, verts.Length, bu);
        //    mb.Vertex.SetData(verts);
        //    mb.Index = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, bu);
        //    mb.Index.SetData(inds);
        //    return true;
        //}
        public static bool tryParse(Stream s, GraphicsDevice g, ContentManager c, out Effect fx, ref List<object> refs, ParsingFlags ps = ParsingFlags.None) {
            fx = null;
            StreamReader f = new StreamReader(new BufferedStream(s));

            // Get The Arguments From The Material File
            List<string[]> args = new List<string[]>();
            while(!f.EndOfStream) {
                string line = f.ReadLine();
                if(string.IsNullOrWhiteSpace(line)) continue;
                string[] split = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if(split.Length < 1) continue;
                split[0] = split[0].Trim().ToLower();
                switch(split[0]) {
                    case "fx": if(split.Length >= 2) args.Add(split); break;
                    case "fxpt": if(split.Length >= 3) args.Add(split); break;
                    case "fxptc": if(split.Length >= 3) args.Add(split); break;
                    case "fxpf": if(split.Length >= 4) args.Add(split); break;
                }
            }

            // Get The Effect For This Material
            Predicate<string[]> fxMatch = (a) => { return a[0].Equals("fx"); };
            string[] fxArg = args.Find(fxMatch);
            if(fxArg == null) return false;
            args.RemoveAll(fxMatch);


            // Try To Create The Effect
            if(ps.HasFlag(ParsingFlags.LoadEffectByteCode)) {
                try {
                    byte[] code = null;
                    using(FileStream fxs = File.OpenRead(fxArg[1].Trim())) {
                        code = new byte[fxs.Length];
                        fxs.Read(code, 0, code.Length);
                    }
                    fx = new Effect(g, code);
                }
                catch(Exception) { fx = null; return false; }
            }
            else {
                try { fx = c.Load<Effect>(fxArg[1].Trim()); }
                catch(Exception) { fx = null; return false; }
            }

            // Will Attempt To Set As Many Uniforms As Possible Without Raising Errors
            foreach(string[] arg in args) {
                switch(arg[0]) {
                    case "fxpt":
                        EffectParameter fxpt = fx.Parameters[arg[1].Trim()];
                        if(fxpt == null) continue;
                        try {
                            Texture2D t = null;
                            if(ps.HasFlag(ParsingFlags.LoadTextureStream)) {
                                using(FileStream ts = File.OpenRead(arg[2].Trim())) {
                                    t = Texture2D.FromStream(g, ts);
                                }
                            }
                            else t = c.Load<Texture2D>(arg[2].Trim());
                            if(t != null) {
                                refs.Add(t);
                                fxpt.SetValue(t);
                            }
                        }
                        catch(Exception) { continue; }
                        break;
                    case "fxptc": // Texture Cube Parameter
                        EffectParameter fxptc = fx.Parameters[arg[1].Trim()];
                        if(fxptc == null) continue;
                        try {
                            TextureCube tc = c.Load<TextureCube>(arg[2].Trim());
                            if(tc != null) {
                                refs.Add(tc);
                                fxptc.SetValue(tc);
                            }
                        }
                        catch(Exception) { continue; }
                        break;
                    case "fxpf": // Vector Parameter
                        EffectParameter fxptv = fx.Parameters[arg[1].Trim()];
                        int comps;
                        if(fxptv == null || !int.TryParse(arg[2], out comps)) continue;
                        string[] sc = arg[3].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if(sc.Length != comps) continue;
                        switch(comps) {
                            case 1:
                                float v1;
                                if(float.TryParse(sc[0], out v1)) fxptv.SetValue(v1);
                                break;
                            case 2:
                                Vector2 v2 = Vector2.Zero;
                                if(float.TryParse(sc[0], out v2.X) &&
                                   float.TryParse(sc[1], out v2.Y)
                                    ) fxptv.SetValue(v2);
                                break;
                            case 3:
                                Vector3 v3 = Vector3.Zero;
                                if(float.TryParse(sc[0], out v3.X) &&
                                   float.TryParse(sc[1], out v3.Y) &&
                                   float.TryParse(sc[2], out v3.Z)
                                    ) fxptv.SetValue(v3);
                                break;
                            case 4:
                                Vector4 v4 = Vector4.Zero;
                                if(float.TryParse(sc[0], out v4.X) &&
                                   float.TryParse(sc[1], out v4.Y) &&
                                   float.TryParse(sc[2], out v4.Z) &&
                                   float.TryParse(sc[3], out v4.W)
                                    ) fxptv.SetValue(v4);
                                break;
                            default:
                                if(comps > 4) {
                                    float[] vn = new float[comps];
                                    bool vnc = true;
                                    for(int i = 0; i < sc.Length && i < vn.Length && vnc; i++) {
                                        if(!float.TryParse(sc[i], out vn[i])) vnc = false;
                                    }
                                    if(vnc) fxptv.SetValue(vn);
                                }
                                break;
                        }
                        break;
                }
            }
            return true;
        }


        // For Writing
        #region Intermediate Structs
        const BindingFlags PrivateFieldBF = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
        static readonly FieldInfo fxRefField = typeof(EffectMaterial).GetField("pParentEffect", PrivateFieldBF);
        static readonly FieldInfo codeField = typeof(Effect).GetField("pCachedEffectData", PrivateFieldBF);

        class MMComp : IEqualityComparer<ModelMeshPart> {
            public bool Equals(ModelMeshPart x, ModelMeshPart y) {
                return
                    x.VertexBuffer == y.VertexBuffer &&
                    x.IndexBuffer == y.IndexBuffer &&
                    x.StartIndex == y.StartIndex &&
                    x.PrimitiveCount == y.PrimitiveCount
                    ;
            }
            public int GetHashCode(ModelMeshPart obj) {
                return obj.GetHashCode();
            }
        }
        public struct MeshPart {
            public VertexBuffer VBuffer;
            public int VertexOffset, NumVertices;
            public IndexBuffer IBuffer;
            public int StartIndex, Primitives;

            public int VertexStride {
                get { return VBuffer.VertexDeclaration.VertexStride; }
            }
            public int NumIndices {
                get { return Primitives * 3; }
            }

            public MeshPart(ModelMeshPart mp) {
                VBuffer = mp.VertexBuffer;
                VertexOffset = mp.VertexOffset;
                NumVertices = mp.NumVertices;
                IBuffer = mp.IndexBuffer;
                StartIndex = mp.StartIndex;
                Primitives = mp.PrimitiveCount;
            }
            public MeshPart(VertexBuffer vb, int vo, int vc, IndexBuffer ib, int si, int pc) {
                VBuffer = vb;
                VertexOffset = vo;
                NumVertices = vc;
                IBuffer = ib;
                StartIndex = si;
                Primitives = pc;
            }
            public MeshPart(VertexBuffer vb, IndexBuffer ib) {
                VBuffer = vb;
                VertexOffset = 0;
                NumVertices = VBuffer.VertexCount;
                IBuffer = ib;
                StartIndex = 0;
                Primitives = IBuffer.IndexCount / 3;
            }
        }
        #endregion

        public static void write(Model model, string name, ParsingFlags ps = ParsingFlags.WriteAll) {
            int mi = 0, ei = 0;
            foreach(ModelMesh mesh in model.Meshes) {
                // Get All The Distinct Meshes
                foreach(ModelMeshPart mp in mesh.MeshParts.Distinct(new MMComp())) {
                    string nfObj = name + "." + mi + ".obj";
                    StreamWriter fObj = new StreamWriter(new BufferedStream(File.Create(nfObj)));
                    writeObj(new MeshPart(mp), fObj, ps);
                    fObj.Flush(); fObj.Close();
                    mi++;
                }
                // Get All The Distinct Effects
                foreach(EffectMaterial e in mesh.Effects) {
                    string nfEff = name + "." + ei + ".fxc";
                    StreamWriter fEff = new StreamWriter(new BufferedStream(File.Create(nfEff)));
                    writeEffect(e, fEff);
                    fEff.Flush(); fEff.Close();
                    ei++;
                }
            }
        }
        public static void writeObj(MeshPart mp, StreamWriter writer, ParsingFlags ps = ParsingFlags.WriteAll) {
            ParsingFlags capable = ParsingFlags.None;

            // Loop Through All Vertex Elements
            VertexElement[] elements = mp.VBuffer.VertexDeclaration.GetVertexElements();
            foreach(VertexElement ve in elements) {
                // Choose Formatting Functions By Type
                switch(ve.VertexElementFormat) {
                    case VertexElementFormat.Vector2: writeElement<Vector2>(writer, ve, mp, format, ref capable); break;
                    case VertexElementFormat.Vector3: writeElement<Vector3>(writer, ve, mp, format, ref capable); break;
                    case VertexElementFormat.Vector4: writeElement<Vector4>(writer, ve, mp, format, ref capable); break;
                    case VertexElementFormat.Single: writeElement<float>(writer, ve, mp, format, ref capable); break;
                    case VertexElementFormat.Color: writeElement<Color>(writer, ve, mp, format, ref capable); break;
                    case VertexElementFormat.Byte4: writeElement<Color>(writer, ve, mp, format, ref capable); break;
                    default:
                        // Those Are The Basic Types
                        break;
                }
            }
            // Only Write What Is Capable
            ps &= capable;

            // Write Indices
            switch(mp.IBuffer.IndexElementSize) {
                case IndexElementSize.SixteenBits: writeIndices<short>(writer, mp, format, ps); break;
                case IndexElementSize.ThirtyTwoBits: writeIndices<int>(writer, mp, format, ps); break;
            }
        }
        #region Formatting Functions
        static int format(short i) {
            return i;
        }
        static int format(int i) {
            return i;
        }
        static string format(float f) {
            return f.ToString();
        }
        static string format(Vector2 v) {
            return v.X + " " + v.Y;
        }
        static string formatFV(Vector2 v) {
            return v.X + " " + (1f - v.Y);
        }
        static string format(Vector3 v) {
            return v.X + " " + v.Y + " " + v.Z;
        }
        static string format(Vector4 v) {
            return v.X + " " + v.Y + " " + v.Z + " " + v.W;
        }
        static string format(Color c) {
            return c.R + " " + c.G + " " + c.B + " " + c.A;
        }
        #endregion
        static void writeElement<T>(StreamWriter writer, VertexElement ve, MeshPart mp, Func<T, string> formatter, ref ParsingFlags ps) where T : struct {
            T[] data = new T[mp.NumVertices];
            mp.VBuffer.GetData(ve.Offset + mp.VertexOffset * mp.VertexStride, data, 0, data.Length, mp.VertexStride);
            writeElementByUsage(writer, data, ve, formatter, ref ps);
        }
        static void writeElementByUsage<T>(StreamWriter writer, T[] data, VertexElement ve, Func<T, string> formatter, ref ParsingFlags ps) {
            switch(ve.VertexElementUsage) {
                case VertexElementUsage.Position:
                    if(ve.UsageIndex == 0) writeAllElements(writer, data, "v", formatter);
                    else writeAllElements(writer, data, "v" + ve.UsageIndex, formatter);
                    break;
                case VertexElementUsage.Normal:
                    ps |= ParsingFlags.WriteNorms;
                    if(ve.UsageIndex == 0) writeAllElements(writer, data, "vn", formatter);
                    else writeAllElements(writer, data, "vn" + ve.UsageIndex, formatter);
                    break;
                case VertexElementUsage.TextureCoordinate:
                    ps |= ParsingFlags.WriteUV;
                    if(ps.HasFlag(ParsingFlags.FlipTexCoordV)) {
                        Vector2[] vt = data as Vector2[];
                        if(vt != null) {
                            for(int i = 0; i < vt.Length; i++) vt[i].Y = 1 - vt[i].Y;
                            data = vt as T[];
                        }
                    }
                    if(ve.UsageIndex == 0) writeAllElements(writer, data, "vt", formatter);
                    else writeAllElements(writer, data, "vt" + ve.UsageIndex, formatter);
                    break;
                case VertexElementUsage.Color:
                    ps |= ParsingFlags.WriteColor;
                    if(ve.UsageIndex == 0) writeAllElements(writer, data, "vc", formatter);
                    else writeAllElements(writer, data, "vc" + ve.UsageIndex, formatter);
                    break;
            }
        }
        static void writeAllElements<T>(StreamWriter writer, T[] data, string identifier, Func<T, string> formatter) {
            foreach(T o in data) {
                writer.Write(identifier + " ");
                writer.WriteLine(formatter(o));
            }
        }
        static void writeIndices<T>(StreamWriter writer, MeshPart mp, Func<T, int> formatter, ParsingFlags ps) where T : struct {
            IndexBuffer iBuffer = mp.IBuffer;
            T[] data = new T[mp.NumIndices];
            int iSize = mp.IBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
            mp.IBuffer.GetData(mp.StartIndex * iSize, data, 0, data.Length);
            int i = 0;
            while(i < data.Length) {
                writer.Write("f");
                for(int vi = 0; vi < 3 && i < data.Length; vi++) {
                    int vert;
                    if(ps.HasFlag(ParsingFlags.FlipTriangleOrder)) vert = formatter(data[i + (2 - vi)]) + 1;
                    else vert = formatter(data[i + vi]) + 1;

                    writer.Write(" " + vert);
                    writer.Write("/");
                    if(ps.HasFlag(ParsingFlags.WriteUV)) writer.Write(vert);
                    writer.Write("/");
                    if(ps.HasFlag(ParsingFlags.WriteNorms)) writer.Write(vert);
                }
                i += 3;
                writer.WriteLine("");
            }
        }
        public static void writeEffect(EffectMaterial fx, StreamWriter writer) {
            // Get Reference To Actual Effect
            WeakReference effectRef = fxRefField.GetValue(fx) as WeakReference;

            // Get The Bytecode (Super-Indirection)
            byte[] code = codeField.GetValue(effectRef.Target as Effect) as byte[];

            // Write The Bytecode
            writer.BaseStream.Write(code, 0, code.Length);
        }
    }
}
