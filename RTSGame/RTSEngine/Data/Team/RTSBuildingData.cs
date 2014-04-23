using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSBuildingData {
        public static void Serialize(BinaryWriter s, RTSBuildingData data) {
            s.Write(data.FriendlyName);
            s.Write(data.InfoFile);
            s.Write(data.Health);
            s.Write(data.CapitalCost);
            s.Write(data.PopCapChange);
            s.Write(data.MaxCount);
            s.Write(data.IsResource);
            s.Write(data.Impact);
            s.Write(data.BuildAmount);
            s.Write(data.Depositable);
            s.Write(data.SightRadius);
            s.Write(data.GridSize);
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
        }
        public static RTSBuildingData Deserialize(BinaryReader s, GameState state, int index) {
            RTSBuildingData data = new RTSBuildingData(index);
            data.FriendlyName = s.ReadString();
            data.InfoFile = s.ReadString();
            data.Health = s.ReadInt32();
            data.CapitalCost = s.ReadInt32();
            data.PopCapChange = s.ReadInt32();
            data.MaxCount = s.ReadInt32();
            data.IsResource = s.ReadBoolean();
            data.Impact = s.ReadInt32();
            data.BuildAmount = s.ReadInt32();
            data.Depositable = s.ReadBoolean();
            data.SightRadius = s.ReadInt32();
            data.GridSize = s.ReadPoint();
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
            return data;
        }

        // The Friendly Name
        public string FriendlyName;
        public string InfoFile;
        public readonly int Index;

        // Health Of The Building
        public int Health;

        // The Capital Cost Of The Building
        public int CapitalCost;
        public int PopCapChange;

        // Maximum Number Of These Buildings Supported
        public int MaxCount, CurrentCount;

        // Checking For If It Is A Resource
        public bool IsResource;

        // Environment Impact
        public int Impact;

        // Time To Build The Building
        public int BuildAmount;

        // Can deposit resources
        public bool Depositable;

        // View Radius
        public int SightRadius;

        // ICollidableShape of The Building
        public Point GridSize;
        public ICollidable ICollidableShape;
        public BoundingBox BBox;

        public ReflectedScript DefaultActionController;
        public readonly List<ReflectedScript> DefaultButtonControllers;

        public RTSBuildingData(int i) {
            Index = i;
            DefaultButtonControllers = new List<ReflectedScript>();
        }
    }
}