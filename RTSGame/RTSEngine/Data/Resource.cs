using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum ResourceType : byte {
        Flora = 0x01,
        Ore = 0x01 << 1
    }

    public class ResourceCollection {
        private readonly List<IResource> resources;

        public IResource this[int id] {
            get { return resources[id]; }
        }

        public ResourceCollection() {
            resources = new List<IResource>();
        }
    }
}