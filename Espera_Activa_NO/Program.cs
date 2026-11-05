using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static Queue<int> cola = new Queue<int>();
    static readonly object candado = new object();

    static void Main()
    {
        new Thread(Consumir).Start();
        Thread.Sleep(3000); //El productor tarda 3 seg
        lock (candado) cola.Enqueue(42);
    }
    static void Consumir()
    {
        long vueltas = 0;
        int dato;
        while (true)
        {
            lock (candado)
            {
                if (cola.Count > 0) {dato = cola.Dequeue(); break; }
            }
            vueltas++;
        }
        Console.WriteLine($"Recibi {dato} tras {vueltas:N0} vueltas inutiles");
    }
}