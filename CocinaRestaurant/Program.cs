using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CocinaRestaurant
{
    internal class Program
    {
        private static readonly Stopwatch reloj = Stopwatch.StartNew();
        private static readonly object candadoConsola = new object();

        // Método estático de logueo sincronizado (evita superposición de impresiones en consola)
        public static void Log(string mensaje)
        {
            lock (candadoConsola)
            {
                Console.WriteLine($"{reloj.Elapsed.TotalSeconds,5:0.00}s  {mensaje}");
            }
        }

        static void Main(string[] args)
        {
            const int PEDIDOS_POR_MOZO = 5;
            Bandeja bandeja = new Bandeja(4);

            // 1. Instanciación de productores (Mozos) y consumidores (Cocineros)
            Mozo[] mozos =
            {
                new Mozo("Mozo Ana  ", bandeja, PEDIDOS_POR_MOZO, 1),
                new Mozo("Mozo Beto ", bandeja, PEDIDOS_POR_MOZO, 2),
                new Mozo("Mozo Carla", bandeja, PEDIDOS_POR_MOZO, 3)
            };

            Cocinero[] cocineros =
            {
                new Cocinero("Cocinero Dani", bandeja, 4),
                new Cocinero("Cocinero Eli ", bandeja, 5)
            };

            // 2. Creación e inicialización de los hilos
            List<Thread> hilosMozos = new List<Thread>();
            List<Thread> hilosCocineros = new List<Thread>();

            foreach (Mozo m in mozos)
                hilosMozos.Add(new Thread(m.Ejecutar));

            foreach (Cocinero k in cocineros)
                hilosCocineros.Add(new Thread(k.Ejecutar));

            // 3. Arrancar la ejecución en paralelo
            hilosMozos.ForEach(h => h.Start());
            hilosCocineros.ForEach(h => h.Start());

            // 4. Coordinación de cierre ordenado
            hilosMozos.ForEach(h => h.Join());      // Espera a que los 3 mozos generen sus 15 comandas
            bandeja.Cerrar();                       // Notifica el fin de jornada y despierta a los cocineros en Wait()
            hilosCocineros.ForEach(h => h.Join());  // Espera a que los cocineros vacíen la cola y finalicen

            // 5. Verificación final de integridad (15 = 15)
            int totalCocinados = 0;
            foreach (Cocinero k in cocineros)
                totalCocinados += k.Platos;

            int esperado = mozos.Length * PEDIDOS_POR_MOZO; // 3 * 5 = 15

            Console.WriteLine();
            Console.WriteLine($"Pedidos: {esperado} | Platos cocinados: {totalCocinados} | " +
                              (totalCocinados == esperado ? "OK" : "ERROR: se perdieron pedidos"));

            Console.ReadKey();
        }
    }
}