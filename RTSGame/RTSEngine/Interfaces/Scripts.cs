using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Algorithms;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;

namespace RTSEngine.Interfaces {
    // Way To Reflect If It Is A Script
    public abstract class ACScript {
    }

    #region Unit
    // Base Controller Functionality
    public abstract class ACUnitController : ACScript {
        // The Unit That Is Being Controlled
        protected RTSUnit unit;
        public RTSUnit Unit {
            get { return Unit; }
        }

        // This Called By The GameplayController (After Load/Deserialization)
        public abstract void Init(GameState s, GameplayController c);

        public virtual void SetUnit(RTSUnit u) {
            if(u == null) return;
            if(unit != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Unit");
            unit = u;
            return;
        }
        public T SetUnit<T>(RTSUnit u) where T : ACUnitController {
            SetUnit(u);
            return this as T;
        }

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s);
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACUnitActionController : ACUnitController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // Steps Animations And May Send Particle Events
    public abstract class ACUnitAnimationController : ACUnitController {
        // This Is The Value Read By The Renderer
        public float AnimationFrame {
            get;
            protected set;
        }

        // Local List Of Particles For The Controller
        private List<Particle> particles = new List<Particle>();
        public bool HasParticles {
            get { return particles.Count > 0; }
        }

        protected void AddParticle(Particle p) {
            particles.Add(p);
        }
        public void GetParticles(List<Particle> lOut) {
            if(particles.Count > 0) {
                lOut.AddRange(particles);
                particles.Clear();
            }
        }

        // Scripted Animation
        public abstract void Update(GameState s, float dt);
    }

    // Reasons About How Combat Damage Should Be Performed To A Unit's Target
    public abstract class ACUnitCombatController : ACUnitController {
        // Scripted Logic For Attacking
        public abstract void Attack(GameState g, float dt);
    }

    // Special Movement Mechanics
    public abstract class ACUnitMovementController : ACUnitController {
        // Pathfinder To Be Run On A Separate Thread         
        public Pathfinder Pathfinder { get; set; }

        // Squad Waypoints
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // Index Of Squad Waypoint That This Controller's Unit Is Currently Attempting To Reach
        public int CurrentWaypointIndex { get; set; }

        // Whether This Unit Has Actually Decided To Move
        public bool doMove { get; set; }

        // The Net Force On This Unit
        public Vector2 NetForce {get; set; }

        // Does The Provided Index Point To A Valid Squad Waypoint?
        public bool IsValid(int idx) {
            return Waypoints != null && idx >= 0 && idx < Waypoints.Count;
        }

        // Scripted Logic For Movement
        public abstract void DecideMove(GameState g, float dt);
        public abstract void ApplyMove(GameState g, float dt);
    }
    #endregion

    #region Building
    // Base Controller Functionality
    public abstract class ACBuildingController : ACScript {
        // The Building That Is Being Controlled
        protected RTSBuilding building;
        public RTSBuilding Building {
            get { return building; }
        }

        // This Called By The GameplayController
        public abstract void Init(GameState s, GameplayController c);

        public virtual void SetBuilding(RTSBuilding b) {
            if(b == null) return;
            if(building != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Building");
            building = b;
            return;
        }
        public T SetBuilding<T>(RTSBuilding b) where T : ACUnitController {
            SetBuilding(b);
            return this as T;
        }

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s);
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACBuildingActionController : ACBuildingController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // One Of Numerous Buttons Held By The Building
    public abstract class ACBuildingButtonController : ACBuildingController {
        private int enqueueCount;

        public abstract int QueueTime {
            get;
        }

        // Scripted Button Logic
        public abstract void OnQueueFinished(GameState s);

        public abstract void OnClick();

        protected void Enqueue() {
            System.Threading.Interlocked.Increment(ref enqueueCount);
        }
        protected int GetEnqueueCount() {
            int c = System.Threading.Interlocked.Exchange(ref enqueueCount, 0);
            return c;
        }

        // For Cool-downs, Etc.
        public abstract void DecideAction(GameState s, float dt);
        public abstract void ApplyAction(GameState s, float dt);
    }
    #endregion

    #region Button
    // A Button That Will Be Shown
    public abstract class ACRTSButton : ACScript {
        public abstract void Apply(GameState s);
    }
    #endregion

    #region Squad
    // Base Controller Functionality
    public abstract class ACSquadController : ACScript {
        // The Squad That Is Being Controlled
        protected RTSSquad squad;
        public RTSSquad Squad {
            get { return squad; }
        }

        // This Called By The GameplayController
        public abstract void Init(GameState s, GameplayController c);

        // Will Set Once And Then Fail On Later Occurrences
        public virtual void SetSquad(RTSSquad s) {
            if(s == null) return;
            if(squad != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Squad");
            squad = s;
            return;
        }
        public T SetSquad<T>(RTSSquad s) where T : ACSquadController {
            SetSquad(s);
            return this as T;
        }

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s);
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACSquadActionController : ACSquadController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // The Movement Controller That Dictates The General Movement Behavior Of Units In The Squad
    public abstract class ACSquadMovementController : ACSquadController {
        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // The Whole Squad Will Move At The Min Default Movespeed
        public float MinDefaultMoveSpeed { get; set; } 

        // Used For Movement Halting Logic
        public float SquadRadiusSquared { get; set; }

        // Pathfinder To Be Run On A Separate Thread         
        protected Pathfinder pathfinder;

        // This Squad Movement Controller's Current PathQuery
        public PathQuery Query { get; set; }

        // Scripted Logic For Movement
        public abstract void DecideMove(GameState g, float dt);
        public abstract void ApplyMove(GameState g, float dt);
    }

    // Controls The Targeting That A Squad Performs
    public abstract class ACSquadTargetingController : ACSquadController {
        // A Unit Target
        protected IEntity target;
        public IEntity Target {
            get { return target; }
            set { target = value; }
        }
        public RTSUnit TargetUnit {
            get { return target as RTSUnit; }
            set { target = value; }
        }
        public RTSBuilding TargetBuilding {
            get { return target as RTSBuilding; }
            set { target = value; }
        }

        // Find And Setting A Target For This Squad
        public abstract void DecideTarget(GameState g, float dt);
        public abstract void ApplyTarget(GameState g, float dt);
    }
    #endregion

    #region Game Type
    public abstract class ACGameTypeController : ACScript, IDisposable {
        private GameState state;
        private Thread t;
        private bool running;

        public int? VictoriousTeam {
            get;
            private set;
        }

        // The Same File As The Map File
        public abstract void Load(GameState s, FileInfo mapFile);

        public abstract int? GetVictoriousTeam(GameState s);

        public void Start(GameState s) {
            state = s;
            running = true;
            t = new Thread(UpdateThread);
            t.IsBackground = true;
            t.Start();
        }
        public void Dispose() {
            running = false;
            t.Join();
        }

        public void UpdateThread() {
            while(running) {
                Tick(state);
                VictoriousTeam = GetVictoriousTeam(state);
                Thread.Sleep(100);
            }
        }
        public abstract void Tick(GameState s);

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s, GameState state);
    }

    #endregion

    public interface IVisualInputController {
        Camera Camera { get; set; }

        void Build(RTSRenderer renderer);

        void Update(RTSRenderer renderer, GameState s);
        void Draw(RTSRenderer renderer, SpriteBatch batch);
    }
}