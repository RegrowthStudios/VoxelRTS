using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Vox {
    public interface IRegionWorker {
        void DoWork();
    }

    public class VoxWorkPool : ACThreadPool<IRegionWorker> {
        protected override void DoWork(IRegionWorker o) {
            o.DoWork();
        }
    }
}