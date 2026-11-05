using System;
using System.Threading;

namespace CocinaRestaurant
{
    class Mozo
    {
        private static int contadorComandas = 0; // Incremento atómico para la numeración[cite: 19]

        private readonly string nombre;
        private readonly Bandeja bandeja;
        private readonly int cantidadPedidos;
        private readonly Random rnd;

        public Mozo(string nombre, Bandeja bandeja, int cantidadPedidos, int semilla)
        {
            this.nombre = nombre;
            this.bandeja = bandeja;
            this.cantidadPedidos = cantidadPedidos;
            this.rnd = new Random(semilla);
        }

        public void Ejecutar()
        {
            string[] platosDisponibles = { "Milanesa", "Pizza", "Empanadas", "Pasta", "Ensalada" };

            for (int i = 1; i <= cantidadPedidos; i++)
            {
                // Tarda entre 200 y 600 ms por pedido
                Thread.Sleep(rnd.Next(200, 601));

                Comanda c = new Comanda
                {
                    Numero = Interlocked.Increment(ref contadorComandas), // Garantiza números únicos[cite: 19]
                    Mesa = rnd.Next(1, 15),
                    Plato = platosDisponibles[rnd.Next(platosDisponibles.Length)],
                    Mozo = nombre.Trim()
                };

                bandeja.Dejar(c); // Deposita en la bandeja[cite: 14, 23]
            }

            Program.Log($"--> {nombre.Trim()} terminó de tomar sus 5 pedidos");
        }
    }
}
