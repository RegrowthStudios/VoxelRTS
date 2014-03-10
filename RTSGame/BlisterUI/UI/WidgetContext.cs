using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI {
    /// <summary>
    /// <para>This Class Houses Widget Operations</para>
    /// <para>It Also Keeps Track Of Various States And Settings</para>
    /// </summary>
    public static class WidgetContext {
        // Log Stream
        private static TextWriter logger;

        private static readonly LinkedList<WidgetBase> widgets;
        public static IEnumerable<WidgetBase> Registered {
            get { return widgets; }
        }

        private static UIEffect fx;
        private static UITexture gMap;
        private static Vector2 screenSize;

        private static DrawBatch batch;
        private static bool shouldRepaint;

        /// <summary>
        /// Initialized A Default Logger
        /// </summary>
        static WidgetContext() {
            // Error Log As Default Out
            setLogger(Console.Error);

            widgets = new LinkedList<WidgetBase>();
        }

        /// <summary>
        /// Create The Context's Resources
        /// </summary>
        /// <param name="g">Graphics Device</param>
        /// <param name="_fx">UI Shader</param>
        /// <param name="_gMap">Glyph Map</param>
        public static void init(GraphicsDevice g, Effect _fx, UITexture _gMap) {
            fx = new UIEffect(_fx);
            gMap = _gMap;
            batch = new DrawBatch(g);
        }

        /// <summary>
        /// Called When An Application Is Exiting To Flush The Log
        /// </summary>
        public static void onExitFlush(object sender, EventArgs args) {
            log("Application Sent Exit Signal");
            if(logger != null) logger.Flush();
            logger = null;
        }
        /// <summary>
        /// Lets The Context Know The Correct Screen Size
        /// </summary>
        /// <param name="w">View Width</param>
        /// <param name="h">View Height</param>
        public static void onScreenResize(float w, float h) {
            screenSize.X = w;
            screenSize.Y = h;
        }

        /// <summary>
        /// Set The Default Logging Stream
        /// </summary>
        /// <param name="l">New Log</param>
        public static void setLogger(TextWriter l, bool close = true) {
            // Check For A Good Logger
            if(l == null) return;

            // Flush And Dispose The Old Log If Necessary
            if(logger != null) {
                logger.Flush();
                if(close) {
                    logger.Close();
                    logger.Dispose();
                }
            }
            logger = l;
#if VERBOSE
            log("A Logger Has Been Set - " + logger.GetType().Name);
#endif
        }
        /// <summary>
        /// Set The Default Logging Stream
        /// </summary>
        /// <param name="s">New Stream</param>
        public static void setLogger(Stream s, bool close = true) {
            setLogger(new StreamWriter(s), close);
        }
        /// <summary>
        /// Create Or Overwrite A Logging File
        /// </summary>
        /// <param name="file">The File Path And Name</param>
        public static void setLogger(string file, bool close = true) {
            try {
                // Try To Create The File
                FileStream f = File.Create(file);
                setLogger(f, close);
            }
            catch(Exception e) {
                // State Cause Of The Error
                log("Could Not Open Log File <{0}>", file);
                log("Error:\n{0}", e.Message);
            }
        }

        /// <summary>
        /// Log A Message To The Default Logging File
        /// </summary>
        /// <param name="msg">Message String</param>
        public static void log(string msg) {
            if(logger != null) logger.WriteLine(msg);
        }
        /// <summary>
        /// Log A Formatted Message To The Default Logging File
        /// </summary>
        /// <param name="msgFormat">The Format Of The Message</param>
        /// <param name="args">Message Parameters</param>
        public static void log(string msgFormat, params object[] args) {
            log(string.Format(msgFormat, args));
        }
        /// <summary>
        /// Log A Message To The Default Logging File Without Appending A New Line
        /// </summary>
        /// <param name="msg">Message String</param>
        public static void logRNL(string msg) {
            if(logger != null) logger.Write(msg);
        }
        /// <summary>
        /// Log A Formatted Message To The Default Logging File Without Appending A New Line
        /// </summary>
        /// <param name="msgFormat">The Format Of The Message</param>
        /// <param name="args">Message Parameters</param>
        public static void logRNL(string msgFormat, params object[] args) {
            logRNL(string.Format(msgFormat, args));
        }

        /// <summary>
        /// Registers A Widget To This Context
        /// </summary>
        /// <param name="w">Widget</param>
        public static void register(WidgetBase w) {
            if(w == null) return;
            w.register(widgets);
            repaint();
        }
        /// <summary>
        /// Unregisters A Widget From This Context
        /// </summary>
        /// <param name="w"></param>
        public static void unregister(WidgetBase w) {
            if(w == null) return;
            w.unregister(widgets);
            repaint();
        }

        public static void repaint() {
            shouldRepaint = true;
        }
        private static void rebuild(GraphicsDevice g) {
            shouldRepaint = false;
            batch.begin();
            foreach(WidgetBase w in widgets) {
                w.draw(batch, gMap);
            }
            batch.end(g);
        }
        public static void draw(GraphicsDevice g) {
            if(shouldRepaint) rebuild(g);
            g.BlendState = BlendState.NonPremultiplied;
            g.DepthStencilState = DepthStencilState.None;

            fx.ScreenSize = screenSize;
            fx.GlyphMap = gMap.Texture;
            fx.apply();
            batch.draw(g);

            g.SetVertexBuffer(null);
            g.Indices = null;
        }
    }
}
