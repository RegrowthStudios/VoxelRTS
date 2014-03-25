using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

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

    public abstract class BaseWidget {
        // Parent Hierarchy (Should Not Have Cycles)
        private BaseWidget parent;
        public BaseWidget Parent {
            get { return parent; }
            set {
                if(parent != null)
                    parent.OnRecompute -= OnParentRecompute;
                parent = value;
                if(parent != null)
                    parent.OnRecompute += OnParentRecompute;
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
        protected Rectangle drawRect;
        public Rectangle DrawRectangle {
            get { return drawRect; }
        }
        public int X {
            get { return drawRect.X; }
        }
        public int Y {
            get { return drawRect.Y; }
        }
        public int Width {
            get { return drawRect.Width; }
            set {
                drawRect.Width = value;
                Recompute();
            }
        }
        public int Height {
            get { return drawRect.Height; }
            set {
                drawRect.Height = value;
                Recompute();
            }
        }

        public BaseWidget() {
            anchor = new Point(0, 0);
            align = new Point(Alignment.LEFT, Alignment.TOP);
            drawRect.Width = 1;
            drawRect.Height = 1;
            Recompute();
        }

        protected virtual void Recompute() {
            if(parent != null) {
                // Get Anchor Via The Parent
                anchor.X = parent.X + ((offAlign.X * parent.Width) / 2) + offset.X;
                anchor.Y = parent.Y + ((offAlign.Y * parent.Height) / 2) + offset.Y;
            }

            // Use Alignment For Computation
            drawRect.X = anchor.X - ((align.X * Width) / 2);
            drawRect.Y = anchor.Y - ((align.Y * Height) / 2);

            if(OnRecompute != null)
                OnRecompute(this);
        }
        protected virtual void OnParentRecompute(BaseWidget w) {
            Recompute();
        }
    }
}