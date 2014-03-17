using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public interface IWorkLoader {
        int WorkAmount { get; }
        int WorkComplete { get; }

        bool IsFinishedWorking { get; }
        bool IsLoadSuccessful { get; }

        ConcurrentQueue<string> Messages { get; }

        void Load();
    }

    public static class IWorkLoaderExt {
        public static float GetPercentComplete(this IWorkLoader o) {
            return (float)o.WorkComplete / (float)o.WorkAmount;
        }
        public static void ReadAllMessages(this IWorkLoader o, LinkedList<string> outMessages) {
            int c = o.Messages.Count;
            string m;
            for(int i = 0; i < c; i++) {
                while(!o.Messages.TryDequeue(out m))
                    continue;
                outMessages.AddLast(m);
                i++;
            }
        }
    }
}