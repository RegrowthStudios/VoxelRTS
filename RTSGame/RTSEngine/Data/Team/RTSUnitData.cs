using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;
using System.IO;

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
            BaseCombatData.Serialize(s, ref data.BaseCombatData);
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
            BaseCombatData.Deserialize(s, ref data.BaseCombatData);
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
        public string FriendlyName;
        public string InfoFile;
        public readonly int Index;

        // Health Of The Unit
        public int Health;

        // The Capital Cost Of The Unit
        public int CapitalCost;
        // The Population Cost Of The Unit
        public int PopulationCost;

        // Maximum Number Of These Units Supported
        public int MaxCount, CurrentCount;

        // Speed Of The Unit
        public float MovementSpeed;

        // Environment Impact When Unit Is Produced
        public int Impact;

        // How Long It Takes For The Building To Make This Unit
        public int BuildTime;

        // BaseCombatData of The Unit
        public BaseCombatData BaseCombatData;

        // ICollidableShape of The Unit
        public ICollidable ICollidableShape;
        public BoundingBox BBox;

        public ReflectedScript DefaultActionController;
        public ReflectedScript DefaultCombatController;
        public ReflectedScript DefaultMoveController;
        public ReflectedScript DefaultAnimationController;

        public RTSUnitData(int i) {
            Index = i;
        }
    }
}