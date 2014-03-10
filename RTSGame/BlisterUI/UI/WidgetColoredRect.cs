using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlisterUI {
    public class WidgetColoredRect : WidgetBase {
        // Generate The Type ID For The Widget Class
        private static ulong tid = getTID();

        /// <summary>
        /// The Color Of This Widget
        /// </summary>
        public Vector4 Color { get; set; }

        public WidgetColoredRect()
            : base(tid) {
            Color = Vector4.One;
        }

        public override void drawSelf(DrawBatch db, UITexture tex) {
            UIGlyph gPix = tex.getGlyphPixel();
            if(!gPix.Created) return;
            db.addLayer(new DrawLayer(height)
            .addVerts(
            new VertexUI(BoundTL, gPix.TopLeft, Color),
            new VertexUI(BoundTR, gPix.TopRight, Color),
            new VertexUI(BoundBL, gPix.BottomLeft, Color),
            new VertexUI(BoundBR, gPix.BottomRight, Color)
            )
            .addInds(0, 1, 2, 2, 1, 3)
            );
        }
    }
}
