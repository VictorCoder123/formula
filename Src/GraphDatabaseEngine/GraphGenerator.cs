namespace Microsoft.Formula.GraphDatabaseEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GraphGenerator
    {
        public void Randomize<T>(T[] items)
        {
            Random rand = new Random();
            for (int i = 0; i < items.Length - 1; i++)
            {
                int j = rand.Next(i, items.Length);
                T temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }

        public List<Tuple<int, int>> RandomEdges(int vNum, int eNum)
        {
            List<Tuple<int, int>> pairs = new List<Tuple<int, int>>();
            int[] vArray = new int[vNum];
            for (int i = 0; i < vNum; i++) vArray[i] = i;
            eNum = Math.Min(eNum, vNum * vNum);
            decimal stepLen = Math.Ceiling((decimal)eNum / vNum);
            for (int i = 0; i < vNum; i++)
            {
                int num = Math.Min((int)stepLen, eNum - (int)stepLen * i);
                Randomize(vArray);
                for (int j = 0; j < num; j++)
                {
                    pairs.Add(new Tuple<int, int>(i, vArray[j]));
                }
            }
            return pairs;
        }
    }
}
