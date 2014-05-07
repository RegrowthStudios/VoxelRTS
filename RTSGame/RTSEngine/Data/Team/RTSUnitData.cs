using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;
using System.IO;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data.Team {
    public class RTSUnitData {
        public static void Serialize(BinaryWriter s, RTSUnitData data) {
            s.Write(data.FriendlyName);
            s.Write(data.InfoFile);
            s.Write(data.Health);
            s.Write(data.CapitalCost);
            s.Write(data.PopulationCost);
            s.Write(data.MaxCount);
            s.Write(data.MovementSpeed);
            s.Write(data.Impact);
            s.Write(data.BuildTime);
            s.Write(data.CarryingCapacity);
            s.Write(data.IsWorker);
            BaseCombatData.Serialize(s, data.BaseCombatData);
            s.Write((int)data.ICollidableShape.CollisionType);
            s.Write(data.ICollidableShape.Center);
            s.Write(data.ICollidableShape.IsStatic);
            switch(data.ICollidableShape.CollisionType) {
                case CollisionType.Circle:
                    var cc = data.ICollidableShape as CollisionCircle;
                    s.Write(cc.Radius);
                    break;
                case CollisionType.Rectangle:
                    var cr = data.ICollidableShape as CollisionRect;
                    s.Write(cr.Width);
                    s.Write(cr.Depth);
                    break;
            }
            s.Write(data.BBox.Min);
            s.Write(data.BBox.Max);
            s.Write(data.DefaultActionController.TypeName);
            s.Write(data.DefaultCombatController.TypeName);
            s.Write(data.DefaultMoveController.TypeName);
            s.Write(data.DefaultAnimationController.TypeName);
        }
        public static RTSUnitData Deserialize(BinaryReader s, GameState state, int index) {
            RTSUnitData data = new RTSUnitData(index);
            data.FriendlyName = s.ReadString();
            data.InfoFile = s.ReadString();
            data.Health = s.ReadInt32();
            data.CapitalCost = s.ReadInt32();
            data.PopulationCost = s.ReadInt32();
            data.MaxCount = s.ReadInt32();
            data.MovementSpeed = s.ReadSingle();
            data.Impact = s.ReadInt32();
            data.BuildTime = s.ReadInt32();
            data.CarryingCapacity = s.ReadInt32();
            data.IsWorker = s.ReadBoolean();
            BaseCombatData.Deserialize(s, data.BaseCombatData);
            CollisionType ct = (CollisionType)s.ReadInt32();
            Vector2 cc = s.ReadVector2();
            bool cs = s.ReadBoolean();
            switch(ct) {
                case CollisionType.Circle:
                    float cr = s.ReadSingle();
                    data.ICollidableShape = new CollisionCircle(cr, cc, cs);
                    break;
                case CollisionType.Rectangle:
                    float cw = s.ReadSingle();
                    float cd = s.ReadSingle();
                    data.ICollidableShape = new CollisionRect(cw, cd, cc, cs);
                    break;
                default:
                    throw new Exception("Nonexistent Collision Type");
            }
            data.BBox.Min = s.ReadVector3();
            data.BBox.Max = s.ReadVector3();
            data.DefaultActionController = state.Scripts[s.ReadString()];
            data.DefaultCombatController = state.Scripts[s.ReadString()];
            data.DefaultMoveController = state.Scripts[s.ReadString()];
            data.DefaultAnimationController = state.Scripts[s.ReadString()];
            return data;
        }

        // The Friendly Name
        [ZXParse("NAME")]
        public string FriendlyName;
        public string InfoFile;
        public readonly int Index;

        // Health Of The Unit
        [ZXParse("HEALTH")]
        public int Health;

        // The Capital Cost Of The Unit
        [ZXParse("CAPITALCOST")]
        public int CapitalCost;
        // The Population Cost Of The Unit
        [ZXParse("POPULATIONCOST")]
        public int PopulationCost;

        // Maximum Number Of These Units Supported
        [ZXParse("MAXCOUNT")]
        public int MaxCount;
        public int CurrentCount;

        // Speed Of The Unit
        [ZXParse("SPEED")]
        public float MovementSpeed;

        // Environment Impact When Unit Is Produced
        [ZXParse("IMPACT")]
        public int Impact;

        // How Long It Takes For The Building To Make This Unit
        [ZXParse("BUILDTIME")]
        public int BuildTime;

        // Carrying Capacity For This Worker
        [ZXParse("CARRYCAPACITY")]
        public int CarryingCapacity;

        // Flag To Tell This Is A Worker
        [ZXParse("WORKER")]
        public bool IsWorker;

        // BaseCombatData of The Unit
        [ZXParse("BASECOMBAT")]
        public BaseCombatData BaseCombatData;

        // ICollidableShape of The Unit
        public ICollidable ICollidableShape;
        [ZXParse("RADIUS")]
        public float CollisionRadius {
            get { return ICollidableShape.BoundingRadius; }
            set { ICollidableShape = new CollisionCircle(value, Vector2.Zero, false); }
        }

        // Selection Box For Unit
        public BoundingBox BBox;
        [ZXParse("BBOXMIN")]
        public Vector3 BBoxMin {
            get { return BBox.Min; }
            set { BBox.Min = value; }
        }
        [ZXParse("BBOXMAX")]
        public Vector3 BBoxMax {
            get { return BBox.Max; }
            set { BBox.Max = value; }
        }

        public ReflectedScript DefaultActionController;
        public ReflectedScript DefaultCombatController;
        public ReflectedScript DefaultMoveController;
        public ReflectedScript DefaultAnimationController;

        public RTSUnitData(int i) {
            Index = i;
            ICollidableShape = new CollisionCircle(1f, Vector2.Zero, false);
        }

        [ZXParse("SCRCONTROL")]
        public void SetController(Dictionary<string, ReflectedScript> d, string cType, string key) {
            switch(cType.Trim().ToLower()) {
                case "act":
                case "action":
                    d.TryGetValue(key, out DefaultActionController);
                    break;
                case "combat":
                    d.TryGetValue(key, out DefaultCombatController);
                    break;
                case "move":
                case "movement":
                case "motion":
                    d.TryGetValue(key, out DefaultMoveController);
                    break;
                case "anim":
                case "animation":
                    d.TryGetValue(key, out DefaultAnimationController);
                    break;
            }
        }
    }
}