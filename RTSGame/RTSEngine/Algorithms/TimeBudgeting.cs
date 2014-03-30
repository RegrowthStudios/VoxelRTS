using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Algorithms {
    public abstract class ACBudgetedTask {
        // This Should Not Be Modified By The Task
        public int CurrentBin {
            get;
            set;
        }

        // This Is How Tasks Will Be Sorted Into Bins
        public int WorkAmount {
            get;
            private set;
        }

        public ACBudgetedTask(int workAmount) {
            CurrentBin = -1;
            WorkAmount = workAmount;
        }

        // The Task's Work Function
        public abstract void DoWork(float dt);
    }

    public class TimeBudget {
        // Bins Of Tasks
        private readonly List<ACBudgetedTask>[] taskBins;
        private int curBin;
        // For Sorting By Relative Work
        private readonly int[] totalWork;

        public int Bins {
            get { return taskBins.Length; }
        }
        public int TotalTasks {
            get;
            private set;
        }

        public TimeBudget(int bins) {
            taskBins = new List<ACBudgetedTask>[bins];
            totalWork = new int[bins];
            curBin = 0;
            ClearTasks();
        }

        private int FindEasiestBin() {
            int w = totalWork[0];
            int bin = 0;
            for(int i = 1; i < totalWork.Length; i++) {
                if(totalWork[i] < w) {
                    bin = i;
                    w = totalWork[i];
                }
            }
            return bin;
        }

        public void AddTask(ACBudgetedTask t) {
            // Make Sure Task Hasn't Already Been Added
            if(t.CurrentBin >= 0) RemoveTask(t);

            int bin = FindEasiestBin();
            t.CurrentBin = bin;
            taskBins[bin].Add(t);
            totalWork[bin] += t.WorkAmount;
            TotalTasks++;
        }
        public void RemoveTask(ACBudgetedTask t) {
            // Make Sure Task Has Been Added
            if(t.CurrentBin < 0) return;

            TotalTasks--;
            totalWork[t.CurrentBin] -= t.WorkAmount;
            taskBins[t.CurrentBin].Remove(t);
            t.CurrentBin = -1;
        }

        public void ClearTasks() {
            TotalTasks = 0;
            for(int i = 0; i < taskBins.Length; i++)
                taskBins[i] = new List<ACBudgetedTask>();
            Array.Clear(totalWork, 0, totalWork.Length);
        }
        public void ResortBins() {
            ACBudgetedTask[] tasks = new ACBudgetedTask[TotalTasks];
            int i = 0;
            for(int bin = 0; bin < taskBins.Length; bin++)
                for(int ti = 0; ti < taskBins[bin].Count; ti++)
                    tasks[i++] = taskBins[bin][ti];

            // Re-Add Tasks
            ClearTasks();
            for(i = 0; i < tasks.Length; i++) AddTask(tasks[i]);
        }

        public void DoTasks(float dt) {
            var tasks = taskBins[curBin];
            for(int i = 0; i < tasks.Count; i++)
                tasks[i].DoWork(dt);
            curBin = (curBin + 1) % taskBins.Length;
        }
    }
}