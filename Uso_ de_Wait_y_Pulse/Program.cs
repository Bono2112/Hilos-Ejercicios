using System;
using System.Data;
using System.Threading;

class Program
{
    static readonly object candado = new object();
    static bool listo = false;

    static void Main()
    {
        new Thread(Esperar).Start();
        Thread.Sleep(2000); //Simulamos Trabajo

        lock (candado)
        {
            listo = true;           //1. Cambiamos el estado
            Console.WriteLine("Main: aviso con Pulse");
            Monitor.Pulse(candado);
            Console.WriteLine("¿Qué pasa?");
            // 2) despierto al que espera
            Console.WriteLine("Main: todavía tengo el lock");
            // 3) recién acá lo libero
        }
    }
    static void Esperar()
    {
        lock (candado)
        {
            while (!listo)      // siempre while, nunca if
            {
                Console.WriteLine("Hilo: no está listo, espero...");
                Monitor.Wait(candado); // libera el lock y se duerme
            }
            Console.WriteLine("Hilo: ¡me despertaron y está listo!");
        }
    }
}
