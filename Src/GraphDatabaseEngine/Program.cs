namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Program
    {
        public static void Main(string[] args)
        {
            List<TimeSpan> testSelfLoop = new List<TimeSpan>();
            List<TimeSpan> testSelfLoopWithDepth = new List<TimeSpan>();
            List<TimeSpan> testMatchSimplePattern = new List<TimeSpan>();

            int[] nums = new int[] { 5, 10, 50, 100, 200, 500, 1000, 1500, 2000 };
            foreach (int vNum in nums)
            {
                int eNum = vNum * vNum / 800;
                GraphDatabaseTest test = new GraphDatabaseTest(vNum, eNum);

                TimeSpan t1 = test.TestSelfLoop(false);
                testSelfLoop.Add(t1);

                TimeSpan t2 = test.TestSelfLoopWithDepth(false);
                testSelfLoopWithDepth.Add(t2);

                TimeSpan t3 = test.TestMatchSimplePattern(false);
                testMatchSimplePattern.Add(t3);
            }

            Console.WriteLine("Self-loop Test result");
            testSelfLoop.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            Console.WriteLine("Self-loop with depth Test result");
            testSelfLoopWithDepth.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            Console.WriteLine("Match pattern Test result");
            testMatchSimplePattern.ForEach(el => {
                Console.WriteLine(el.ToString());
            });

            string line = Console.ReadLine();
        }
    }
}
