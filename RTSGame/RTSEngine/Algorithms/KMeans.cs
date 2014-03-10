using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace RTSEngine.Algorithms {
    public struct KMeansResult {
        public Vector3[] Centroids;
        public int[] ClusterAssigns;

        public KMeansResult(int n, int k) {
            Centroids = new Vector3[k];
            ClusterAssigns = new int[n];
        }
    }
    public static class KMeans {
        public static KMeansResult Compute(int k, Vector3[] data) {
            int n = data.Length;
            KMeansResult res = new KMeansResult(n, k);

            // Find Random Points
            Random r = new Random();
            List<int> rAssign = new List<int>(k);
            int li = 0;
            while(li < k) {
                int rn = r.Next(n);
                if(rAssign.Contains(rn)) continue;
                li++;
                rAssign.Add(rn);
            }

            // Assign Random Centroids
            for(int i = 0; i < k; i++)
                res.Centroids[i] = data[rAssign[i]];

            int[] lastAssign = Enumerable.Repeat<int>(-1, n).ToArray();
            while(Different(lastAssign, res.ClusterAssigns)) {
                // Swap
                var buf = lastAssign;
                lastAssign = res.ClusterAssigns;
                res.ClusterAssigns = buf;

                // Compute
                for(int ni = 0; ni < n; ni++) {
                    int c = 0;
                    float d = (data[ni] - res.Centroids[0]).LengthSquared();
                    for(int ci = 1; ci < k; ci++) {
                        float nd = (data[ni] - res.Centroids[ci]).LengthSquared();
                        if(nd < d) {
                            c = ci;
                            d = nd;
                        }
                    }
                    res.ClusterAssigns[ni] = c;
                }

                // Find New Centroids
                int[] count = new int[k];
                for(int i = 0; i < k; i++)
                    res.Centroids[i] = Vector3.Zero;
                for(int ni = 0; ni < n; ni++) {
                    res.Centroids[res.ClusterAssigns[ni]] += data[ni];
                    count[res.ClusterAssigns[ni]]++;
                }
                for(int i = 0; i < k; i++)
                    res.Centroids[i] /= count[i];
            }
            return res;
        }
        public static void Compute(int k, Vector3[] data, ref KMeansResult res, int stepPause) {
            int n = data.Length;

            // Find Random Points
            Random r = new Random();
            List<int> rAssign = new List<int>(k);
            int li = 0;
            while(li < k) {
                int rn = r.Next(n);
                if(rAssign.Contains(rn)) continue;
                li++;
                rAssign.Add(rn);
            }

            // Assign Random Centroids
            for(int i = 0; i < k; i++)
                res.Centroids[i] = data[rAssign[i]];

            int[] lastAssign = Enumerable.Repeat<int>(-1, n).ToArray();
            while(Different(lastAssign, res.ClusterAssigns)) {
                // Copy Swap
                res.ClusterAssigns.CopyTo(lastAssign, 0);

                // Compute
                for(int ni = 0; ni < n; ni++) {
                    int c = 0;
                    float d = (data[ni] - res.Centroids[0]).LengthSquared();
                    for(int ci = 1; ci < k; ci++) {
                        float nd = (data[ni] - res.Centroids[ci]).LengthSquared();
                        if(nd < d) {
                            c = ci;
                            d = nd;
                        }
                    }
                    res.ClusterAssigns[ni] = c;
                }

                // Find New Centroids
                int[] count = new int[k];
                for(int i = 0; i < k; i++)
                    res.Centroids[i] = Vector3.Zero;
                for(int ni = 0; ni < n; ni++) {
                    res.Centroids[res.ClusterAssigns[ni]] += data[ni];
                    count[res.ClusterAssigns[ni]]++;
                }
                for(int i = 0; i < k; i++)
                    res.Centroids[i] /= count[i];

                // Pause
                Thread.Sleep(stepPause);
            }
        }
        private static bool Different(int[] a, int[] b) {
            for(int i = 0; i < a.Length; i++) {
                if(a[i] != b[i]) return true;
            }
            return false;
        }
    }
}