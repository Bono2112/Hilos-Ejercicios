using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufferAcostado
{
    class Consumidor
    {
        private readonly BufferAcotado buffer;
        private readonly string nombre;
        public Consumidor(BufferAcotado buffer, string nombre)
        {
            this.buffer = buffer;
            this.nombre = nombre;
        }
        public void Ejecutar()
        {
            int item;
            while ((item = buffer.Sacar()) != Program.FIN)
            {
                Console.WriteLine($"{nombre} <- sacó {item}");
                Thread.Sleep(400); // consumir es lento
            }
            Console.WriteLine($"{nombre} terminó");
        }
    }
}
