using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTS {
    public static class Program {
        public static void Main(string[] args) {
            using(App app = new App()) {
                app.Run();
            }
        }
    }
}
