using System.Collections.Generic;
using System.Threading;
class BufferAcotado
{
    private readonly Queue<int> cola = new Queue<int>();
    private readonly object candado = new object();
    private readonly int capacidad;
    public BufferAcotado(int capacidad) { this.capacidad = capacidad; }
    public void Poner(int item)
    {
        lock (candado)
        {
            while (cola.Count == capacidad) // lleno: el productor espera
                Monitor.Wait(candado);
            cola.Enqueue(item);
            Monitor.PulseAll(candado); // aviso: ¡hay datos!
        }
    }
    public int Sacar()
    {
        lock (candado)
        {
            while (cola.Count == 0) // vacío: el consumidor espera
                Monitor.Wait(candado);
            int item = cola.Dequeue();
            Monitor.PulseAll(candado); // aviso: ¡hay lugar!
            return item;
        }
    }
}