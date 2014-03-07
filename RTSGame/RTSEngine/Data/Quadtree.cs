using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {

    public class Quadtree {

        // child[0] is northwest, [1] is northeast, [2] is southwest, [3] is southeast
        private Quadtree[] child = new Quadtree[4];

        // List of objects contained in this node
        private List<Object> list = new List<Object>();

        private Vector2 center; // Center of this bounding box
        private float size; // Size of this bounding box

        #region Properties

        public float Size {
            get { return size; }
            set { size = value; }
        }

        public Vector2 Center {
            get { return center; }
            set { center = value; }
        }

        #endregion

        // Constructor
        public Quadtree() {
            for (int i = 0; i < child.Length;i++)
                child[i] = null;
        }

        // Constructor
        public Quadtree(Quadtree northwest, Quadtree northeast, Quadtree southwest, Quadtree southeast) {
            child[0] = northwest;
            child[1] = northeast;
            child[2] = southwest;
            child[3] = southeast;
        }

        // Get i'th child of this node
        public Quadtree getChild(int i) {
            if (i >= 0 && i < child.Length) return child[i];
            else return null;
        }

        // Set the given node to i'th child of this node
        public void setChild(Quadtree node, int i) {
            if (i >= 0 && i < child.Length)
                child[i] = node;
        }

        // Add an object to the list
        public void add(Object obj) {
            list.Add(obj);
        }

        // Get an object in the list
        public Object getObject(int i) {
            if (i >= 0 && i < list.Count) return list[i];
            else return null;
        }
    }
}
