using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

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

        public static SpriteFont Compile(GraphicsDevice g, string file) {
            FontDescriptionImporter ei = new FontDescriptionImporter();
            FontDescription ec = ei.Import(file, new RTSImporterContext());
            FontDescriptionProcessor ep = new FontDescriptionProcessor();
            var cec = ep.Process(ec, new RTSProcessorContext());
            
            // Texture
            Texture2DContent texC = sfcTexture.GetValue(cec) as Texture2DContent;
            MipmapChain o = txcMipmaps.GetValue(texC, null) as MipmapChain;
            Dxt3BitmapContent texBMP = o[0] as Dxt3BitmapContent;
            Texture2D texture = new Texture2D(g, texBMP.Width, texBMP.Height, false, SurfaceFormat.Dxt3);
            texture.SetData(texBMP.GetPixelData());
            
            // Glyph Data
            List<Rectangle> glyphs = sfcGlyphs.GetValue(cec) as List<Rectangle>;
            List<Rectangle> cropping = sfcCropping.GetValue(cec) as List<Rectangle>;
            List<char> charMap = sfcCharMap.GetValue(cec) as List<char>;
            int lineSpacing = (int)sfcLineSpacing.GetValue(cec);
            float spacing = (float)sfcSpacing.GetValue(cec);
            List<Vector3> kerning = sfcKerning.GetValue(cec) as List<Vector3>;
            char? defaultChar = sfcDefaultChar.GetValue(cec) as char?;

            // Invoke Private SpriteFont Constructor
            return sfConstructor.Invoke(new object[] { texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultChar }) as SpriteFont;
        }
    }
}