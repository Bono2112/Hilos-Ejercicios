using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferAcostado
{
    class Productor
    {
        private readonly BufferAcotado buffer;
        private readonly int cantidad;

        public Productor(BufferAcotado buffer, int cantidad)
        {
            this.buffer = buffer;
            this.cantidad = cantidad;
        }
        public void Ejecutar()
        {
            for(int i = 1; i <= cantidad; i++)
            {
                Console.WriteLine($"Productor -> produce {i}");
                buffer.Poner(i);    // espera si está lleno
                Thread.Sleep(100);      // producir es rápido
            }
        }
    }
}
