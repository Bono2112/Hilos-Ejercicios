using System;
using System.Threading;

namespace CocinaRestaurant
{
    class Cocinero
    {
        private readonly string nombre;
        private readonly Bandeja bandeja;
        private readonly Random rnd;

        // Propiedad requerida por Program.cs para la verificación final (15 = 15)[cite: 13, 14]
        public int Platos { get; private set; } = 0;

        public Cocinero(string nombre, Bandeja bandeja, int semilla)
        {
            this.nombre = nombre;
            this.bandeja = bandeja;
            this.rnd = new Random(semilla);
        }

        public void Ejecutar()
        {
            Comanda? c;
            // Procesa mientras la bandeja devuelva true (hasta estar cerrada y vacía)[cite: 14, 23]
            while (bandeja.Tomar(nombre, out c))
            {
                // Cocinar tarda entre 500 y 1200 ms
                Thread.Sleep(rnd.Next(500, 1201));
                Platos++;
            }

            Program.Log($"<-- {nombre.Trim()} finalizó su turno. Cocinó {Platos} platos");
        }
    }
}