using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Graphics {
    public static class XNAEffect {
        class MyLogger : ContentBuildLogger {
            public override void LogMessage(string message, params object[] messageArgs) { }
            public override void LogImportantMessage(string message, params object[] messageArgs) { }
            public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs) { }
        }
        class MyImporterContext : ContentImporterContext {
            public override string IntermediateDirectory { get { return string.Empty; } }
            public override string OutputDirectory { get { return string.Empty; } }

            public override ContentBuildLogger Logger { get { return logger; } }
            ContentBuildLogger logger =  new MyLogger();

            public override void AddDependency(string filename) { }
        }
        class MyProcessorContext : ContentProcessorContext {
            public override TargetPlatform TargetPlatform { get { return TargetPlatform.Windows; } }
            public override GraphicsProfile TargetProfile { get { return GraphicsProfile.HiDef; } }
            public override string BuildConfiguration { get { return string.Empty; } }
            public override string IntermediateDirectory { get { return string.Empty; } }
            public override string OutputDirectory { get { return string.Empty; } }
            public override string OutputFilename { get { return string.Empty; } }

            public override OpaqueDataDictionary Parameters { get { return parameters; } }
            OpaqueDataDictionary parameters = new OpaqueDataDictionary();

            public override ContentBuildLogger Logger { get { return logger; } }
            ContentBuildLogger logger = new MyLogger();

            public override void AddDependency(string filename) { }
            public override void AddOutputFile(string filename) { }

            public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters) { throw new NotImplementedException(); }
            public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName) { throw new NotImplementedException(); }
            public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName) { throw new NotImplementedException(); }
        }

        public static Effect Compile(GraphicsDevice g, string file) {
            EffectImporter ei = new EffectImporter();
            EffectContent ec = ei.Import(file, new MyImporterContext());
            EffectProcessor ep = new EffectProcessor();
            var cec = ep.Process(ec, new MyProcessorContext());
            return new Effect(g, cec.GetEffectCode());
        }
    }
}
