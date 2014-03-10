using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RTSCS.Gameplay;
using RTSCS.Graphics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTSCS {
    public class App : BlisterUI.MainGame {
        // The Static Instances
        private static App app;
        private static DataForm form;
        public static Exception FormException {
            get;
            private set;
        }

        public SimScreen SimScreen {
            get;
            private set;
        }

        public App()
            : base() {
            SimScreen = new SimScreen(3);
        }

        protected override void FullInitialize() {
            BlisterUI.Input.WMHookInput.Initialize(Window);
        }
        protected override void FullLoad() {
        }

        protected override void BuildScreenList() {
            screenList = new BlisterUI.ScreenList(this, 0,
                new BlisterUI.FalseFirstScreen(1),
                new RTSEngine.Screens.InduZtryScreen(1),
                new RTSEngine.Screens.KMeansScreen(2),
                SimScreen
                );
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        #region Entry Point
        private static void RunMainInstance(string[] args) {
            using(app = new App()) {
                form = null;
                FormException = null;

                // Create Form Thread
                Thread t = new Thread((a) => {
                    App _app = a as App;
                    try {
                        using(form = new DataForm(_app.SimScreen.Units, _app.SimScreen.Teams, _app.SimScreen.Controllers)) {
                            form.OnUnitSpawn += _app.SimScreen.AddNewUnit;
                            form.ShowDialog();
                        }
                        return;
                    }
                    catch(Exception e) {
                        FormException = e;
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Priority = ThreadPriority.Lowest;
                t.IsBackground = true;
                t.Start(app);

                // Wait For Max 1 Second To Initialize The Form Else Exit
                int trials = 10;
                while(trials > 0) {
                    if(DataForm.Instance != null) {
                        trials = 10;
                        break;
                    }
                    Thread.Sleep(100);
                    trials--;
                }

                // Run The Simulator
                app.Run();

                // Stop The Form Thread
                if(DataForm.Instance != null && form != null) {
                    form.Invoke(form.Closer);
                    while(DataForm.Instance != null) {
                        Thread.Sleep(500);
                        Console.WriteLine("Waiting For Form Thread To Stop");
                    }
                    t.Abort();
                }
            }
        }
        private static bool InputRunNew() {
            Console.WriteLine("Want to try and run a new instance?");
            string input = Console.ReadLine().ToLower().Trim();
            switch(input) {
                case "y":
                case "ye":
                case "yes":
                case "yeah":
                case "sure":
                case "why not":
                case "whatever":
                case "ok":
                case "okay":
                case "cool":
                    return true;
            }
            return false;
        }
        private static void Main(string[] args) {
            bool running = true;
            while(running) {
                Console.WriteLine("A New Instance Will Attempt To Be Run\n\n");
                running = false;
                try {
                    // Close Out Of Any Previous Instances
                    try { app.Exit(); app.Dispose(); }
                    catch(Exception) { }
                    try { form.Invoke(form.Closer); form.Dispose(); }
                    catch(Exception) { }

                    RunMainInstance(args);

                    Console.WriteLine("\n\nHooray, no errors appeared.");
                    running = InputRunNew();
                }
                catch(Exception e) {
                    Console.WriteLine(new string('\n', 10));
                    Console.WriteLine("Oh man bro... looks like an exception was thrown:");
                    Console.WriteLine("Exception Type: {0}", e.GetType().Name);
                    Console.WriteLine("Exception Message:\n{0}\n\n", e.Message);
                    Console.WriteLine("Stack Trace:\n{0}", e.StackTrace);
                    running = InputRunNew();
                }
            }
        }
        #endregion
    }
}