using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;


namespace StadiumLockInterlocked
{
    internal class Program
    {       
        static void BoardThread(Stadium stadium, ref bool running)
        {
            while (running)
            {                
                int current = Interlocked.CompareExchange(ref stadium.insideAtomic, 0, 0);
                Console.WriteLine($"\n\t===== Табло зрителей: {current} =====\n");
                Thread.Sleep(200);
            }
        }

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
           
            const int turnstiles = 4;
            const int eventsPerTurnstile = 250000;

            var stadium = new Stadium();
            var sw = new Stopwatch();
           
            stadium.Reset();
            Thread[] threads = new Thread[turnstiles];

            for (int t = 0; t < turnstiles; t++)
            {
                int seed = t * 1000 + 42; 
                threads[t] = new Thread(() => 
                stadium.ProcessUnsync(eventsPerTurnstile, seed));
            }

            sw.Restart();
            foreach (var th in threads) 
            { 
                th.Start(); 
            }
            foreach (var th in threads) 
            { 
                th.Join(); 
            }
            sw.Stop();

            long unsyncInside = stadium.inside;
            long unsyncEntered = stadium.totalEntered;
            long expectedInside1 = stadium.ExpectedInside;
            long expectedEntered1 = stadium.ExpectedTotalEntered;
            long unsyncTime = sw.ElapsedMilliseconds;

            //--------------------------               
            stadium.Reset();

            bool running = true;
            for (int t = 0; t < turnstiles; t++)
            {
                int seed = t * 1000 + 42; 
                threads[t] = new Thread(() => 
                stadium.ProcessAtomic(eventsPerTurnstile, seed));
            }
            
            var scoreboard = new Thread(() => 
            BoardThread(stadium, ref running));

            sw.Restart();
            foreach (var th in threads) 
            { 
                th.Start(); 
            }

            scoreboard.Start();
            foreach (var th in threads) 
            { 
                th.Join(); 
            }

            running = false;    
            scoreboard.Join();
            sw.Stop();

            long atomicInside = stadium.InsideAtomic;
            long atomicEntered = stadium.TotalEnteredAtomic;
            long expectedInside2 = stadium.ExpectedInside;
            long expectedEntered2 = stadium.ExpectedTotalEntered;
            long atomicTime = sw.ElapsedMilliseconds;
            
            Console.WriteLine("\t====== Таблица результатов ======");
            Console.WriteLine($"{"Версия",-22} {"inside",12} {"totalEntered",14} {"ожидание",12} {"время (мс)",10}");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"Без синхронизации",-22} {unsyncInside,12} {unsyncEntered,14} {expectedInside1,12} {unsyncTime,10}");
            Console.WriteLine($"{"Interlocked",-22} {atomicInside,12} {atomicEntered,14} {expectedInside2,12} {atomicTime,10}");
            Console.WriteLine(new string('-', 80));
          
            Console.WriteLine($"\nВсего вошло (totalEntered): {expectedEntered2}");
            Console.WriteLine($"Внутри (inside):      {expectedInside2}");
            
            Console.WriteLine();
            Console.WriteLine(unsyncEntered == expectedEntered1
                ? "Без синхронизации: totalEntered \"YES\")"
                : "Без синхронизации: totalEntered \"NON\"");
            Console.WriteLine(atomicEntered == expectedEntered2
                ? "Interlocked: totalEntered \"YES\" (атомарно)"
                : "Interlocked: totalEntered \"NON\"");
        }
    }
}
