using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTSEngine.Data.Parsers;

namespace RTSEngineTests {

    public class ZXPTestObj {
        [ZXParse("MyValue")]
        public int Value;

        [ZXParse("Inner")]
        public ZXPTestObj Rec;

        public ZXPTestObj() {
            Value = -1;
        }
    }

    class Program {
        static void Main(string[] args) {
            ZXPTestObj o = new ZXPTestObj();
            ZXParser.Parse(
@"
MyValue [10] Inner [RTSEngineTests.ZXPTestObj] {
    MyValue [12]
    Inner [RTSEngineTests.ZXPTestObj] {
        MyValue [14]
    }
}
",
                o);
            Console.WriteLine(o.Value);
            Console.WriteLine(o.Rec.Value);
            Console.WriteLine(o.Rec.Rec.Value);
            var sw = new StreamWriter("Test.txt");
            ZXParser.Write(sw, o);
            sw.Flush();
            sw.Dispose();
            Console.Read();
        }
    }
}
