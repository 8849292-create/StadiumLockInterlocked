using System;
using System.Threading;

namespace StadiumLockInterlocked
{
    public class Stadium
    {        
        public int inside;    
        public int totalEntered; 
        
        public int insideAtomic;
        private int totalEnteredAtomic;
        //public int InsideAtomic => insideAtomic;
        public int InsideAtomic
        {
            get
            {
                return insideAtomic;
            }
        }
        public int TotalEnteredAtomic
        {
            get
            {
                return totalEnteredAtomic;
            }
        }     
                        
        private long expectedInside;
        private long expectedTotalEntered;
        public long ExpectedInside
        {
            get { return expectedInside; }
        }            
        public long ExpectedTotalEntered {
            get { return expectedTotalEntered; }
        }
       
        public void Reset()
        {
            inside = 0;
            totalEntered = 0;
            insideAtomic = 0;
            totalEnteredAtomic = 0;
            expectedInside = 0;
            expectedTotalEntered = 0;
        }
        private static (int deltaInside, int deltaEntered) GenerateEvent(Random rdn)
        {
            if (rdn.NextDouble() < 0.7)  
            {
                int group = rdn.NextDouble() < 0.05 ? 5 : 1; 
                return (group, group); 
            }
            else                             
            {
                return (-1, 0);
            }
        }
       
        public void ProcessUnsync(int events, int seed)
        {
            long localInside = 0; 
            long localEntered = 0;
            var rdn = new Random(seed);

            for (int i = 0; i < events; i++)
            {
                var (dInside, dEntered) = GenerateEvent(rdn);
                inside += dInside; 
                totalEntered += dEntered;
                localInside += dInside; 
                localEntered += dEntered;
            }
            Interlocked.Add(ref expectedInside, localInside);
            Interlocked.Add(ref expectedTotalEntered, localEntered);
        }
       
        public void ProcessAtomic(int events, int seed)
        {
            //Console.WriteLine("--- Версия с Interlocked ---");
            long localInside = 0;
            long localEntered = 0;

            var rdn = new Random(seed);

            for (int i = 0; i < events; i++)
            {
                var (dInside, dEntered) = GenerateEvent(rdn);

                if (dInside > 0)
                {
                    if (dInside == 1)
                    {                        
                        Interlocked.Increment(ref insideAtomic);
                        Interlocked.Increment(ref totalEnteredAtomic);
                    }
                    else
                    {                        
                        Interlocked.Add(ref insideAtomic, dInside);
                        Interlocked.Add(ref totalEnteredAtomic, dEntered);
                    }
                }
                else
                {                   
                    Interlocked.Decrement(ref insideAtomic);
                }
                
                localInside += dInside;
                localEntered += dEntered;
            }            
            Interlocked.Add(ref expectedInside, localInside);
            Interlocked.Add(ref expectedTotalEntered, localEntered);




            //long localInside = 0;
            //long localEntered = 0;

            //var rdn = new Random(seed); 
            //for (int i = 0; i < events; i++)
            //{
            //    var (dInside, dEntered) = GenerateEvent(rdn);

            //    if (dInside > 0)
            //    {                    
            //        Interlocked.Add(ref insideAtomic, dInside);
            //        Interlocked.Add(ref totalEnteredAtomic, dEntered);
            //    }
            //    else
            //    {                    
            //        Interlocked.Decrement(ref insideAtomic);
            //    }

            //    localInside += dInside;
            //    localEntered += dEntered;
            //}

            //Interlocked.Add(ref expectedInside, localInside);
            //Interlocked.Add(ref expectedTotalEntered, localEntered);
        }

    }
}
