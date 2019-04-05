using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Formula.MuzEngine
{
    class Program
    {
        public static void Main(string[] args)
        {
            TestSuite testSuite = new TestSuite();
            testSuite.TestRelationFixpoint();

            return;
        }
    }
}
