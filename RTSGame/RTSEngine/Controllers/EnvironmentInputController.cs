using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Controllers
{
    public class EnvironmentInputController : InputController
    { 
        // Stores The Locations Of The Original Trees On The Map
        private List<Point> treeLocations;

        // Impact At Which Environment Will Begin Spawning Units
        private int spawn_impact = 10;

        // Impact At Which Environment Will No Longer Recover
        private int recover_impact = 20;

        // New Trees Will Spawn Within This Radius Of Original Trees
        //private float spawn_radius = 5;

        // Ore Will Regenerate This Amount During Each Recovery Phase
        //private int regenerate_amount = 5;

       // private int spawn_number = 5; 

        public ImpactGrid Grid { get; private set; }

        public EnvironmentInputController(GameState g, RTSTeam t)
            : base(g, t) {
            treeLocations = new List<Point>();
            Grid = g.IGrid;
        }

        public void Init() {
            //Store where trees first were in the map
        }
        
        private void Recover() {
            foreach (var r in GameState.Regions) {
                if (r.RegionImpact < recover_impact) {
                    //spawn new trees within a certain radius of original trees 
                    //ore restores health




                }
            }
        }

        private void SpawnUnits() {  
            foreach (var r in GameState.Regions) {
                if (r.RegionImpact > spawn_impact) {
                    // Find The Cell With The Largest Impact
                    Point p = r.Cells.First();
                    foreach (var c in r.Cells) {
                        if (Grid.CellImpact[c.X, c.Y] > Grid.CellImpact[p.X, p.Y]){
                            p = c;
                        }
                    }
                    // Spawn Environmental Units At That Cell
                    

                }
            }
        }
        
        public override void Dispose() {
            throw new NotImplementedException();
        }
       
    }
       
}
