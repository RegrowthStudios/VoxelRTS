using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RTSEngine.Data.Parsers;

namespace RTSCS {
    public partial class ScriptControlForm : Form {
        #region TEMPLATE_ACTION
        private const string TEMPLATE_ACTION =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace Script {
    public class Controller : IActionController {
        // This Controller's Entity
        public IEntity Entity {
            get;
            private set;
        }

        // Empty Constructor
        public Controller() {
            Entity = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException(""Controllers Can Only Have Entities Set Once"");
            Entity = e;
        }

        // Perform Decision For Entity
        public void DecideAction(GameState g, float dt) {
        }

        // Apply Entity's Decision
        public void ApplyAction(GameState g, float dt) {
        }
    }
}
";
        #endregion
        #region TEMPLATE_MOVEMENT
        private const string TEMPLATE_MOVEMENT =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using Microsoft.Xna.Framework;

namespace Script {
    public class Controller : IMovementController {
        // This Controller's Entity
        public IEntity Entity {
            get;
            private set;
        }

        // Waypoint Data
        private Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
        }

        // Empty Constructor
        public Controller() {
            Entity = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException(""Controllers Can Only Have Entities Set Once"");
            Entity = e;
        }

        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
        }

        // Perform Decision For Entity
        public void DecideMove(GameState g, float dt) {
        }

        // Apply Entity's Decision
        public void ApplyMove(GameState g, float dt) {
        }
    }
}
";
        #endregion
        #region TEMPLATE_COMBAT
        private const string TEMPLATE_COMBAT =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace Script {
    public class Controller : ICombatController {
        // This Controller's Entity
        public IEntity Entity {
            get;
            private set;
        }

        // Empty Constructor
        public Controller() {
            Entity = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException(""Controllers Can Only Have Entities Set Once"");
            Entity = e;
        }

        // Perform Attack
        public void Attack(GameState g, float dt) {
        }
    }
}
";
        #endregion
        #region TEMPLATE_TARGETTING
        private const string TEMPLATE_TARGETTING =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace Script {
    public class Controller : ITargettingController {
        // This Controller's Entity
        public IEntity Entity {
            get;
            private set;
        }

        // Empty Constructor
        public Controller() {
            Entity = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException(""Controllers Can Only Have Entities Set Once"");
            Entity = e;
        }

        // Perform Decision For Entity
        public void FindTarget(GameState g, float dt) {
        }

        // Apply Entity's Decision
        public void ChangeTarget(GameState g, float dt) {
        }
    }
}
";
        #endregion

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        private Dictionary<string, ReflectedEntityController> data;

        public ScriptControlForm(Dictionary<string, ReflectedEntityController> d) {
            InitializeComponent();
            Closer = () => { Close(); };
            data = d;
        }

        private void btnGenerate_Click(object sender, EventArgs e) {
            switch(cbTemplates.Text) {
                case "Action":
                    rtbScript.Text = TEMPLATE_ACTION;
                    break;
                case "Combat":
                    rtbScript.Text = TEMPLATE_COMBAT;
                    break;
                case "Movement":
                    rtbScript.Text = TEMPLATE_MOVEMENT;
                    break;
                case "Targetting":
                    rtbScript.Text = TEMPLATE_TARGETTING;
                    break;
            }
        }
        private void btnCompile_Click(object sender, EventArgs e) {
            rtbCompile.Clear();
            rtbCompile.AppendText("Attempting To Build\n");

            CompiledEntityControllers cec;
            string error;
            string[] references = rtbReferences.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < references.Length; i++)
                references[i] = references[i].Trim();
            cec = EntityControllerParser.CompileText(rtbScript.Text, references, out error);

            if(cec == null) {
                rtbCompile.AppendText("Errors Were Found:\n");
                rtbCompile.AppendText(error);
            }
            else {
                foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers) {
                    data.Add(kv.Key, kv.Value);
                    rtbCompile.AppendText("Found Controller: " + kv.Key + "\n");
                }
            }
        }

        [STAThread]
        public static void ThreadedRun(object args) {
            using(var f = new ScriptControlForm(args as Dictionary<string, ReflectedEntityController>)) {
                f.ShowDialog();
            }
        }
    }
}