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

        [ZXParse("Add")]
        public void Add(int i) {
            Value += i;
        }
    }

    class Program {
        static void Main(string[] args) {
            string data =
@"    MyValue [10]
    Add({10})
    Add({10})
    Add({10})
    Inner <RTSEngineTests.ZXPTestObj> {
        MyValue [12]
        Add({10})
        Add({10})
        Add({10})
        Inner <RTSEngineTests.ZXPTestObj> {
            MyValue [14]
            Add({10})
            Add({10})
            Add({10})
        }
    }"
;
            ZXPTestObj o = new ZXPTestObj();
            ZXParser.ParseInto(data, o);
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
