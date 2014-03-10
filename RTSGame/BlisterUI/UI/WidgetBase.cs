using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlisterUI {
    /// <summary>
    /// <para>The Most Basic Abstract Class Of Widgets</para>
    /// <para>All Widgets Inherit From This Class</para>
    /// </summary>
    public abstract class WidgetBase : IDisposable {
        #region Static
        // A Way To Generate Unique IDs For The Widgets
        private static ulong ctrUUID, ctrTID;
        // Thread Locks
        private static object lckUUID, lckTID;
        static WidgetBase() {
            lckUUID = new object();
            // 1 Will Be The First UUID Created
            ctrUUID = 0;

            lckTID = new object();
            // 1 Will Be The First Type ID Created
            ctrTID = 0;
        }
        // A Thread-Safe Way To Get The UUIDs
        private static ulong getUUID() {
            ulong i;
            lock(lckUUID) {
                i = ++ctrUUID;
            }
            return i;
        }
        // A Thread-Safe Way To Get The TIDs
        protected static ulong getTID() {
            ulong i;
            lock(lckTID) {
                i = ++ctrTID;
            }
            return i;
        }
        #endregion

        // Simple Alignment Values
        public const float ALIGN_LEFT = 0f;
        public const float ALIGN_TOP = ALIGN_LEFT;
        public const float ALIGN_CENTER = 0.5f;
        public const float ALIGN_RIGHT = 1f;
        public const float ALIGN_BOTTOM = ALIGN_RIGHT;

        // Simple Offset Values
        public const float OFFSET_LEFT = ALIGN_LEFT;
        public const float OFFSET_TOP = OFFSET_LEFT;
        public const float OFFSET_CENTER = ALIGN_CENTER;
        public const float OFFSET_RIGHT = ALIGN_RIGHT;
        public const float OFFSET_BOTTOM = OFFSET_RIGHT;

        /// <summary>
        /// The Unique Identifier Of This Widget Used For
        /// Logging Operations
        /// </summary>
        public ulong UUID { get; private set; }
        /// <summary>
        /// The Unique Type Identifier Of This Widget
        /// </summary>
        public ulong TypeID { get; private set; }

        protected Vector2 anchor;
        public Vector2 Anchor {
            get { return anchor; }
            set {
                anchor = value;
                recalculateBounds();
            }
        }
        protected float height;
        public float Height {
            get { return height; }
            set {
                height = value;
                notifyRepaint();
            }
        }

        protected bool offsetRatio;
        public bool IsRatioOffset {
            get { return offsetRatio; }
            set {
                offsetRatio = value;
                if(parent != null) onParentChange(parent);
            }
        }
        protected Vector2 offset;
        public Vector2 Offset {
            get { return offset; }
            set {
                offset = value;
                if(parent != null) onParentChange(parent);
            }
        }
        protected float heightOffset;
        public float HeightOffset {
            get { return heightOffset; }
            set {
                heightOffset = value;
                if(parent != null) onParentChange(parent);
            }
        }

        protected bool alignRatio;
        public bool IsRatioAlignment {
            get { return alignRatio; }
            set {
                alignRatio = value;
                recalculateBounds();
            }
        }
        protected Vector2 align;
        /// <summary>
        /// The Alignment Of This Widget To Its Anchor
        /// </summary>
        public Vector2 Alignment {
            get { return align; }
            set {
                align = value;
                recalculateBounds();
            }
        }
        protected Vector4 bounds;
        /// <summary>
        /// The Size Of This Widget
        /// </summary>
        public Vector2 Size {
            get { return new Vector2(bounds.Z, bounds.W); }
            set {
                bounds.Z = value.X;
                bounds.W = value.Y;
                recalculateBounds();
            }
        }
        /// <summary>
        /// The Top Left Corner Of This Widget
        /// </summary>
        public Vector2 BoundTL {
            get { return new Vector2(bounds.X, bounds.Y); }
        }
        /// <summary>
        /// The Top Right Corner Of This Widget
        /// </summary>
        public Vector2 BoundTR {
            get { return new Vector2(bounds.X + bounds.Z, bounds.Y); }
        }
        /// <summary>
        /// The Bottom Left Corner Of This Widget
        /// </summary>
        public Vector2 BoundBL {
            get { return new Vector2(bounds.X, bounds.Y + bounds.W); }
        }
        /// <summary>
        /// The Bottom Right Corner Of This Widget
        /// </summary>
        public Vector2 BoundBR {
            get { return new Vector2(bounds.X + bounds.Z, bounds.Y + bounds.W); }
        }

        private WidgetBase parent;
        /// <summary>
        /// The Parent Widget
        /// </summary>
        public WidgetBase Parent {
            get { return parent; }
            set {
                if(parent != null) {
                    parent.OnChange -= onParentChange;
                    parent.children.Remove(this);
                }
                parent = value;
                if(parent != null) {
                    parent.OnChange += onParentChange;
                    parent.children.AddLast(this);
                }
            }
        }

        // Children Widgets
        protected readonly LinkedList<WidgetBase> children;
        /// <summary>
        /// Widgets That Have This As Their Parent
        /// </summary>
        public IEnumerable<WidgetBase> Children {
            get { return children; }
        }

        private bool isRegistered;
        /// <summary>
        /// Value Indicating If The Widget Context Is Aware Of The Widget
        /// <para>False By Default</para>
        /// </summary>
        public bool IsRegistered {
            get {
                return parent == null ? isRegistered : parent.IsRegistered;
            }
            private set {
                isRegistered = parent == null ? value : false;
            }
        }

        /// <summary>
        /// True If This Widget Has Already Been Disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Event Fired As This Is Being Disposed
        /// </summary>
        public event Action<WidgetBase> OnDisposal;
        /// <summary>
        /// Event Fired If This Is Visibly Changed
        /// </summary>
        public event Action<WidgetBase> OnChange;

        /// <summary>
        /// Default Constructor That Must Be Used In Overriding Classes
        /// </summary>
        public WidgetBase(ulong tID) {
            UUID = getUUID();
            TypeID = tID;
            IsDisposed = false;

            parent = null;
            children = new LinkedList<WidgetBase>();

#if VERBOSE
            log("Initializer {0}", TypeID);
#endif

            // Set Default Visuals
            anchor = Vector2.Zero;
            offsetRatio = false;
            offset = new Vector2(OFFSET_LEFT, OFFSET_TOP);
            alignRatio = true;
            align = new Vector2(ALIGN_LEFT, ALIGN_TOP);
            height = 0;
            heightOffset = 0;
            Size = Vector2.One;
        }
        /// <summary>
        /// The Disposing Destructor Of This Widget
        /// </summary>
        ~WidgetBase() {
#if VERBOSE
            log("Destructor");
#endif
            Dispose();
        }

        /// <summary>
        /// Disposes Of This Widget And All Its Resources
        /// </summary>
        public void Dispose() {
            // Make Sure The Widget Wasn't Already Disposed
            if(IsDisposed) return;

#if VERBOSE
            log("Disposing");
#endif

            // Set Disposal To Be True
            IsDisposed = true;

            // Call Disposal Event
            if(OnDisposal != null) OnDisposal(this);
        }

        /// <summary>
        /// Log A Message Prefixed By UUID
        /// </summary>
        /// <param name="msg">Message String</param>
        public void log(string msg) {
            WidgetContext.log("WID {0:X6} - {1}", UUID, msg);
        }
        /// <summary>
        /// Log A Formatted Message Prefixed By UUID
        /// </summary>
        /// <param name="msgFormat">The Format Of The Message</param>
        /// <param name="args">Message Parameters</param>
        public void log(string msgFormat, params object[] args) {
            log(string.Format(msgFormat, args));
        }

        /// <summary>
        /// Registers The Widget To A Single List
        /// </summary>
        /// <param name="l">Registration List</param>
        public void register(LinkedList<WidgetBase> l) {
            if(l == null || IsRegistered) return;
            IsRegistered = true;
#if VERBOSE
            log("Registration   @{0:X8}", l.GetHashCode());
#endif
            // Make Sure To Unregister The Children
            foreach(WidgetBase c in children) c.unregister(l);
            l.AddLast(this);
        }
        /// <summary>
        /// Unregisters The Widget From A List, If It Was Contained In It
        /// </summary>
        /// <param name="l">Registration List</param>
        public void unregister(LinkedList<WidgetBase> l) {
            if(l == null) return;
            IsRegistered = !l.Remove(this);
#if VERBOSE
            if(!IsRegistered) log("Unregistration @{0:X8}", l.GetHashCode());
#endif
            // Make Sure To Unregister The Children
            foreach(WidgetBase c in children) c.unregister(l);
        }

        public void onParentChange(WidgetBase w) {
            // Make Sure We Get The Correct Widget
            if(w == null || w != parent) return;

            // Set From Offset Values
            if(IsRatioOffset) {
                Anchor = w.BoundTL + w.Size * offset;
            }
            else {
                Anchor = w.BoundTL + offset;
            }
            Height = w.height + heightOffset;
        }

        public void recalculateBounds() {
            // Use Alignment Factor
            if(IsRatioAlignment) {
                bounds.X = Anchor.X - align.X * bounds.Z;
                bounds.Y = Anchor.Y - align.Y * bounds.W;
            }
            else {
                bounds.X = Anchor.X - align.X;
                bounds.Y = Anchor.Y - align.Y;
            }

            // Signal Children And Other Listeners
            if(OnChange != null) OnChange(this);
            notifyRepaint();
        }

        /// <summary>
        /// Draws This Widget And All Of Its Children
        /// </summary>
        /// <param name="dl">Draw Batch</param>
        public void draw(DrawBatch dl, UITexture tex) {
            drawSelf(dl, tex);
            foreach(WidgetBase c in children) {
                c.draw(dl, tex);
            }
        }
        /// <summary>
        /// This Where This Widget Will Draw Itself
        /// </summary>
        /// <param name="db">Draw Batch</param>
        public abstract void drawSelf(DrawBatch db, UITexture tex);

        public void notifyRepaint() {
            if(IsRegistered) WidgetContext.repaint();
        }
    }
}
