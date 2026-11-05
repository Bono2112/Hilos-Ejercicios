using System;
using System.Threading;

class Program
{
    static int contador = 0;
    static readonly object candado = new object();

    static void Main()
    {
        Thread a = new Thread(Sumar);
        Thread b = new Thread(Sumar);
        a.Start(); b.Start();
        a.Join(); b.Join();

        Console.WriteLine("Obtenido: " + contador); //Siempre 2.000.000
    }
    static void Sumar()
    {
        for(int i = 0; i < 1000000; i++)
        { 
            bool tomado = false;
            try
            {
                Monitor.Enter(candado, ref tomado);
                contador++;
            }
            finally
            {
                if (tomado) Monitor.Exit(candado);
            }
        }
    }
}