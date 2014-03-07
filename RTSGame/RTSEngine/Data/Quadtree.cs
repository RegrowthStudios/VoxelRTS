using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {

    public class Quadtree {

        // child[0] is northwest, [1] is northeast, [2] is southwest, [3] is southeast
        private Quadtree[] child = new Quadtree[4];

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
            return child[i];
        }

        // Set the given node to i'th child of this node
        public void setChild(Quadtree node, int i) {
            child[i] = node;
        }

    }
}
