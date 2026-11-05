using System.Collections.Generic;
using System.Threading;

namespace CocinaRestaurant
{
    public class Bandeja
    {
        private readonly Queue<Comanda> comandas = new Queue<Comanda>();
        private readonly object candado = new object();
        private readonly int capacidad;
        private bool cerrada = false;

        public Bandeja(int capacidad)
        {
            this.capacidad = capacidad;
        }

        // MOZO (Productor): deja una comanda en la bandeja
        public void Dejar(Comanda c)
        {
            bool tomado = false;
            try
            {
                // TODO 1: Tomar lock explícito
                Monitor.Enter(candado, ref tomado);

                // TODO 2: Mientras la bandeja esté llena, el mozo espera
                while (comandas.Count == capacidad)
                {
                    Program.Log($"{c.Mozo} espera: bandeja llena " + Ocupacion());
                    Monitor.Wait(candado); // Suelta el lock y se duerme
                }

                // TODO 3: Encolar la comanda y avisar a los cocineros
                comandas.Enqueue(c);
                Program.Log($"{c.Mozo} deja {c} " + Ocupacion());
                Monitor.PulseAll(candado); // Avisa a los hilos en espera
            }
            finally
            {
                // Liberar el lock siempre
                if (tomado) Monitor.Exit(candado);
            }
        }

        // COCINERO (Consumidor): toma una comanda de la bandeja
        public bool Tomar(string cocinero, out Comanda c)
        {
            bool tomado = false;
            try
            {
                Monitor.Enter(candado, ref tomado);

                // TODO 4: Esperar mientras la bandeja esté vacía Y la cocina siga abierta
                while (comandas.Count == 0 && !cerrada)
                {
                    Monitor.Wait(candado);
                }

                // TODO 5: Si la bandeja está vacía y la cocina cerró, no hay más trabajo
                if (comandas.Count == 0 && cerrada)
                {
                    c = null;
                    return false;
                }

                // TODO 6: Desencolar pedido, avisar que hay espacio disponible y retornar true
                c = comandas.Dequeue();
                Program.Log($"{cocinero} toma {c} " + Ocupacion());
                Monitor.PulseAll(candado); // Notifica a los mozos que hay lugar
                return true;
            }
            finally
            {
                if (tomado) Monitor.Exit(candado);
            }
        }

        // MAIN (Orquestador): se llama cuando todos los mozos terminaron de pedir
        public void Cerrar()
        {
            bool tomado = false;
            try
            {
                Monitor.Enter(candado, ref tomado);

                // TODO 7: Marcar cerrada = true y despertar a los cocineros dormidos
                cerrada = true;
                Monitor.PulseAll(candado);
            }
            finally
            {
                if (tomado) Monitor.Exit(candado);
            }
        }

        // Método auxiliar para dibujar el nivel de ocupación [##..]
        private string Ocupacion()
        {
            return "[" + new string('#', comandas.Count)
                       + new string('.', capacidad - comandas.Count) + "]";
        }
    }

}
