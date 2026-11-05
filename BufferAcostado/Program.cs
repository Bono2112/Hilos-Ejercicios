using BufferAcostado;
using System;
using System.Threading;
class Program
{
    public const int FIN = -1; // "píldora venenosa"
    static void Main()
    {

        BufferAcotado buffer = new BufferAcotado(3);
        Thread p = new Thread(new Productor(buffer, 10).Ejecutar);
        Thread c1 = new Thread(new Consumidor(buffer, "Consumidor 1").Ejecutar);
        Thread c2 = new Thread(new Consumidor(buffer, "Consumidor 2").Ejecutar);
        p.Start(); c1.Start(); c2.Start();
        
        p.Join();           // el productor terminó...
        buffer.Poner(FIN);                  // ...aviso a cada consumidor
        buffer.Poner(FIN);
        
        c1.Join(); c2.Join();
        Console.WriteLine("Fin del programa");

    }
}

