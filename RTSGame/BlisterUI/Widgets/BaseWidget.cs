using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public static class Alignment {
        public const int MIN = 0;
        public const int MID = 1;
        public const int MAX = 2;

        public const int LEFT = MIN;
        public const int RIGHT = MAX;
        public const int TOP = MIN;
        public const int BOTTOM = MAX;
    }

    public abstract class BaseWidget : IDisposable {
        public const float LAYER_DELTA = -0.01f;
        public const float LAYER_DEFAULT = 1f;

        // Parent Hierarchy (Should Not Have Cycles)
        private BaseWidget parent;
        public BaseWidget Parent {
            get { return parent; }
            set {
                if(parent == value)
                    return;
                if(parent != null)
                    parent.OnRecompute -= OnParentRecompute;
                parent = value;
                if(parent != null)
                    parent.OnRecompute += OnParentRecompute;
                Recompute();
            }
        }
        public event Action<BaseWidget> OnRecompute;

        // Offset From Parent Calculation Point
        protected Point offset;
        public Point Offset {
            get { return offset; }
            set {
                offset = value;
                Recompute();
            }
        }
        protected float offLayer;
        public float LayerOffset {
            get { return offLayer; }
            set {
                if(offLayer != value) {
                    offLayer = value;
                    Recompute();
                }
            }
        }

        // Another Way To Calculate Anchoring
        protected Point offAlign;
        public int OffsetAlignX {
            get { return offAlign.X; }
            set {
                offAlign.X = value;
                Recompute();
            }
        }
        public int OffsetAlignY {
            get { return offAlign.Y; }
            set {
                offAlign.Y = value;
                Recompute();
            }
        }

        // Anchor Point
        protected Point anchor;
        public Point Anchor {
            get { return anchor; }
            set {
                anchor = value;
                Recompute();
            }
        }

        // Alignment From Anchor (0/1/2)
        protected Point align;
        public int AlignX {
            get { return align.X; }
            set {
                align.X = value;
                Recompute();
            }
        }
        public int AlignY {
            get { return align.Y; }
            set {
                align.Y = value;
                Recompute();
            }
        }

        // Where To Draw To Screen
        private WidgetRenderer renderer;
        protected Rectangle widgetRect;
        public virtual int X {
            get { return widgetRect.X; }
            set {
                if(X == value) return;
                widgetRect.X = value;
                Recompute();
            }
        }
        public virtual int Y {
            get { return widgetRect.Y; }
            set {
                if(Y == value) return;
                widgetRect.Y = value;
                Recompute();
            }
        }
        public virtual int Width {
            get { return widgetRect.Width; }
            set {
                if(Width == value) return;
                widgetRect.Width = value;
                Recompute();
            }
        }
        public virtual int Height {
            get { return widgetRect.Height; }
            set {
                if(Height == value) return;
                widgetRect.Height = value;
                Recompute();
            }
        }
        protected float layer;
        public virtual float LayerDepth {
            get { return layer; }
            set {
                if(LayerDepth == value) return;
                layer = value;
                Recompute();
            }
        }

        public BaseWidget(WidgetRenderer r) {
            offLayer = LAYER_DELTA;

            renderer = r;
            anchor = new Point(0, 0);
            align = new Point(Alignment.LEFT, Alignment.TOP);
            PreInit();
            LayerDepth = LAYER_DEFAULT;
            Recompute();
            AddAllDrawables(renderer);
        }
        public void Dispose() {
            OnRecompute = null;
            RemoveAllDrawables(renderer);
            DisposeOther();
        }
        protected abstract void DisposeOther();

        public abstract void PreInit();

        public abstract void AddAllDrawables(WidgetRenderer r);
        public abstract void RemoveAllDrawables(WidgetRenderer r);

        public Point GetOffset(int x, int y) {
            return new Point(x - X, y - Y);
        }
        public bool Inside(int x, int y) {
            Point p = GetOffset(x, y);
            return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
        }
        public bool Inside(int x, int y, out Vector2 ratio) {
            Point p = GetOffset(x, y);
            ratio = new Vector2((float)p.X / (float)Width, (float)p.Y / (float)Height);
            return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
        }

        protected virtual void Recompute() {
            if(parent != null) {
                // Get Anchor Via The Parent
                anchor.X = parent.X + ((offAlign.X * parent.Width) / 2) + offset.X;
                anchor.Y = parent.Y + ((offAlign.Y * parent.Height) / 2) + offset.Y;
                LayerDepth = parent.LayerDepth + LayerOffset;
            }

            // Use Alignment For Computation
            X = anchor.X - ((align.X * Width) / 2);
            Y = anchor.Y - ((align.Y * Height) / 2);

            if(OnRecompute != null)
                OnRecompute(this);
        }
        protected virtual void OnParentRecompute(BaseWidget w) {
            Recompute();
        }
    }
}