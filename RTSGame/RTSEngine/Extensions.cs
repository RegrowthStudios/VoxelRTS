using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using RTSEngine.Controllers;
using RTSEngine.Graphics;
using Microsoft.Win32;
namespace Microsoft.Xna.Framework.Graphics {
    #region Content Pipeline Context Data
    class RTSContentLogger : ContentBuildLogger {
        public override void LogMessage(string message, params object[] messageArgs) { }
        public override void LogImportantMessage(string message, params object[] messageArgs) { }
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs) { }
    }
    class RTSImporterContext : ContentImporterContext {
        public override string IntermediateDirectory { get { return string.Empty; } }
        public override string OutputDirectory { get { return string.Empty; } }

        public override ContentBuildLogger Logger { get { return logger; } }
        ContentBuildLogger logger = new RTSContentLogger();

        public override void AddDependency(string filename) { }
    }
    class RTSProcessorContext : ContentProcessorContext {
        public override TargetPlatform TargetPlatform { get { return TargetPlatform.Windows; } }
        public override GraphicsProfile TargetProfile { get { return GraphicsProfile.HiDef; } }
        public override string BuildConfiguration { get { return string.Empty; } }
        public override string IntermediateDirectory { get { return string.Empty; } }
        public override string OutputDirectory { get { return string.Empty; } }
        public override string OutputFilename { get { return string.Empty; } }

        public override OpaqueDataDictionary Parameters { get { return parameters; } }
        OpaqueDataDictionary parameters = new OpaqueDataDictionary();

        public override ContentBuildLogger Logger { get { return logger; } }
        ContentBuildLogger logger = new RTSContentLogger();

        public override void AddDependency(string filename) { }
        public override void AddOutputFile(string filename) { }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters) { throw new NotImplementedException(); }
        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName) { throw new NotImplementedException(); }
        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName) { throw new NotImplementedException(); }
    }
    #endregion

    #region XNA Content
    public static class XNAEffect {
        public static Effect Compile(GraphicsDevice g, string file) {
            EffectImporter ei = new EffectImporter();
            EffectContent ec = ei.Import(file, new RTSImporterContext());
            EffectProcessor ep = new EffectProcessor();
            var cec = ep.Process(ec, new RTSProcessorContext());
            return new Effect(g, cec.GetEffectCode());
        }
    }

    public static class XNASpriteFont {
        private const string SF_XML_FORMAT =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics=""Microsoft.Xna.Framework.Content.Pipeline.Graphics"">
  <Asset Type=""Graphics:FontDescription"">
    <FontName>{0}</FontName>
    <Size>{1}</Size>
    <Spacing>{2}</Spacing>
    <UseKerning>{3}</UseKerning>
    <Style>{4}</Style>
    <!-- <DefaultCharacter>{5}</DefaultCharacter> -->
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#{6};</Start>
        <End>&#{7};</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
";

        // Reflection Information For Private Fields
        static FieldInfo sfcTexture;
        static FieldInfo sfcGlyphs;
        static FieldInfo sfcCropping;
        static FieldInfo sfcCharMap;
        static FieldInfo sfcLineSpacing;
        static FieldInfo sfcSpacing;
        static FieldInfo sfcKerning;
        static FieldInfo sfcDefaultChar;
        static ConstructorInfo sfConstructor;
        static PropertyInfo txcMipmaps;
        static XNASpriteFont() {
            BindingFlags all = BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;

            Type t = typeof(SpriteFontContent);
            var mems = t.GetFields(all);

            sfcTexture = mems.First((mi) => { return mi.Name.Equals("texture"); });
            sfcGlyphs = mems.First((mi) => { return mi.Name.Equals("glyphs"); });
            sfcCropping = mems.First((mi) => { return mi.Name.Equals("cropping"); });
            sfcCharMap = mems.First((mi) => { return mi.Name.Equals("characterMap"); });
            sfcLineSpacing = mems.First((mi) => { return mi.Name.Equals("lineSpacing"); });
            sfcSpacing = mems.First((mi) => { return mi.Name.Equals("spacing"); });
            sfcKerning = mems.First((mi) => { return mi.Name.Equals("kerning"); });
            sfcDefaultChar = mems.First((mi) => { return mi.Name.Equals("defaultCharacter"); });
            sfConstructor = typeof(SpriteFont).GetConstructor(all, null, new Type[8]{
                typeof(Texture2D),
                typeof(List<Rectangle>),
                typeof(List<Rectangle>),
                typeof(List<char>),
                typeof(int),
                typeof(float),
                typeof(List<Vector3>),
                typeof(char?)
            }, null);
            var props = typeof(Texture2DContent).GetProperties(all);
            txcMipmaps = props.First((mi) => { return mi.Name.Equals("Mipmaps"); });
        }


        public static SpriteFont Compile(GraphicsDevice g, string file, out IDisposable gR) {
            FontDescriptionImporter ei = new FontDescriptionImporter();
            FontDescription ec = ei.Import(file, new RTSImporterContext());
            FontDescriptionProcessor ep = new FontDescriptionProcessor();
            var cec = ep.Process(ec, new RTSProcessorContext());

            // Get Private Texture
            Texture2DContent texC = sfcTexture.GetValue(cec) as Texture2DContent;
            MipmapChain o = txcMipmaps.GetValue(texC, null) as MipmapChain;
            BitmapContent texBMP = o[0];
            SurfaceFormat sf;
            if(!texBMP.TryGetFormat(out sf))
                throw new InvalidContentException("Could Not Obtain The Surface Format Of The SpriteFont");
            Texture2D texture = new Texture2D(g, texBMP.Width, texBMP.Height, false, sf);
            texture.SetData(texBMP.GetPixelData());

            // Get Private Glyph Data
            List<Rectangle> glyphs = sfcGlyphs.GetValue(cec) as List<Rectangle>;
            List<Rectangle> cropping = sfcCropping.GetValue(cec) as List<Rectangle>;
            List<char> charMap = sfcCharMap.GetValue(cec) as List<char>;
            int lineSpacing = (int)sfcLineSpacing.GetValue(cec);
            float spacing = (float)sfcSpacing.GetValue(cec);
            List<Vector3> kerning = sfcKerning.GetValue(cec) as List<Vector3>;
            char? defaultChar = sfcDefaultChar.GetValue(cec) as char?;

            // Invoke Private SpriteFont Constructor
            gR = texture;
            return sfConstructor.Invoke(new object[] { texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultChar }) as SpriteFont;
        }
        public static SpriteFont Compile(GraphicsDevice g,
            string fontName,
            int size,
            out IDisposable gR,
            int spacing = 0,
            bool useKerning = true,
            string style = "Regular",
            char defaultChar = '*',
            int cStart = 32,
            int cEnd = 126
            ) {
            Random r = new Random();
            string ufid = "";
            unchecked {
                ufid += ((ulong)(r.Next() << 32) | (ulong)r.Next()).ToString();
                ufid += ((ulong)(r.Next() << 32) | (ulong)r.Next()).ToString();
                ufid += ((ulong)(r.Next() << 32) | (ulong)r.Next()).ToString();
                ufid += ((ulong)(r.Next() << 32) | (ulong)r.Next()).ToString();
                ufid += ((ulong)(r.Next() << 32) | (ulong)r.Next()).ToString();
            }
            ufid += ".xml";
            using(var s = File.Create(ufid)) {
                StreamWriter sw = new StreamWriter(s);
                sw.Write(SF_XML_FORMAT, fontName, size, spacing, useKerning ? "true" : "false", style, defaultChar, cStart, cEnd);
                sw.Flush();
            }
            SpriteFont sf = Compile(g, ufid, out gR);
            File.Delete(ufid);
            return sf;
        }
    }
    #endregion

    public static class ModelHelper {
        public static void CreateBuffers<T>(GraphicsDevice g, T[] verts, VertexDeclaration vd, int[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = new VertexBuffer(g, vd, verts.Length, bu);
            vb.SetData(verts);
            ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, bu);
            ib.SetData(inds);
        }
        public static void CreateBuffers<T>(RTSRenderer renderer, T[] verts, VertexDeclaration vd, int[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = renderer.CreateVertexBuffer(vd, verts.Length, bu);
            vb.SetData(verts);
            ib = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, inds.Length, bu);
            ib.SetData(inds);
        }
        public static void CreateBuffers<T>(GraphicsDevice g, T[] verts, VertexDeclaration vd, short[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = new VertexBuffer(g, vd, verts.Length, bu);
            vb.SetData(verts);
            ib = new IndexBuffer(g, IndexElementSize.SixteenBits, inds.Length, bu);
            ib.SetData(inds);
        }
        public static void CreateBuffers<T>(RTSRenderer renderer, T[] verts, VertexDeclaration vd, short[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = renderer.CreateVertexBuffer(vd, verts.Length, bu);
            vb.SetData(verts);
            ib = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, inds.Length, bu);
            ib.SetData(inds);
        }
    }

    public static class Installation {
        public static void InstallFont(string name, FileInfo fi) {
        }
    }
}

namespace Microsoft.Xna.Framework {
    public static class SerializationHelper {
        public static void Write(this BinaryWriter s, Vector2 v) {
            s.Write(v.X);
            s.Write(v.Y);
        }
        public static void Write(this BinaryWriter s, Point p) {
            s.Write(p.X);
            s.Write(p.Y);
        }
        public static void Write(this BinaryWriter s, Vector3 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
        }
        public static void Write(this BinaryWriter s, Vector4 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
            s.Write(v.W);
        }
        public static void Write(this BinaryWriter s, Rectangle r) {
            s.Write(r.X);
            s.Write(r.Y);
            s.Write(r.Width);
            s.Write(r.Height);
        }

        public static Vector2 ReadVector2(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            return new Vector2(x, y);
        }
        public static Point ReadPoint(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            return new Point(x, y);
        }
        public static Vector3 ReadVector3(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            return new Vector3(x, y, z);
        }
        public static Vector4 ReadVector4(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            float w = s.ReadSingle();
            return new Vector4(x, y, z, w);
        }
        public static Rectangle ReadRectangle(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            int z = s.ReadInt32();
            int w = s.ReadInt32();
            return new Rectangle(x, y, z, w);
        }
    }
}