using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

class Comanda
{
    public int Numero;
    public int Mesa;
    public string Plato;
    public string Mozo;

    public override string ToString()
    {
        return $"#{Numero} Mesa {Mesa}: {Plato}";
    }
}

class Bandeja
{
    private readonly Queue<Comanda> comandas = new Queue<Comanda>();
    private readonly object candado = new object();
    private readonly int capacidad;
    private bool cerrada = false;

    public Bandeja(int capacidad) { this.capacidad = capacidad; }

    // Productor: la llama un MOZO.
    public void Dejar(Comanda c)
    {
        bool tomado = false;
        try
        {
            Monitor.Enter(candado, ref tomado);

            // WHILE y no IF: al despertar hay que volver a verificar,
            // otro mozo pudo haber ocupado el lugar antes que yo.
            while (comandas.Count == capacidad)
            {
                Program.Log($"{c.Mozo} espera: bandeja llena " + Ocupacion());
                Monitor.Wait(candado);          // libera el lock y duerme
            }

            comandas.Enqueue(c);
            Program.Log($"{c.Mozo} deja {c}  " + Ocupacion());

            // PulseAll y no Pulse: en la cola de espera hay mozos Y
            // cocineros; con Pulse podría despertar al tipo equivocado.
            Monitor.PulseAll(candado);
        }
        finally
        {
            if (tomado) Monitor.Exit(candado);
        }
    }

    // Consumidor: la llama un COCINERO.
    // Devuelve false cuando está cerrada y vacía (no hay más trabajo).
    public bool Tomar(string cocinero, out Comanda c)
    {
        bool tomado = false;
        try
        {
            Monitor.Enter(candado, ref tomado);

            while (comandas.Count == 0 && !cerrada)
                Monitor.Wait(candado);

            if (comandas.Count == 0)            // vacía y cerrada
            {
                c = null;
                return false;
            }

            c = comandas.Dequeue();
            Program.Log($"{cocinero} toma {c}  " + Ocupacion());
            Monitor.PulseAll(candado);          // ahora hay lugar
            return true;
        }
        finally
        {
            if (tomado) Monitor.Exit(candado);
        }
    }

    // La llama Main cuando todos los mozos terminaron.
    public void Cerrar()
    {
        bool tomado = false;
        try
        {
            Monitor.Enter(candado, ref tomado);
            cerrada = true;
            Monitor.PulseAll(candado);          // despierta a cocineros dormidos
        }
        finally
        {
            if (tomado) Monitor.Exit(candado);
        }
    }

    // Se llama siempre con el lock tomado.
    private string Ocupacion()
    {
        return "[" + new string('#', comandas.Count)
                   + new string('.', capacidad - comandas.Count) + "]";
    }
}

class Mozo
{
    private static readonly string[] Platos =
        { "Milanesa", "Ravioles", "Bife", "Ensalada", "Empanadas", "Pizza", "Flan" };
    private static int ultimoNumero = 0;

    private readonly string nombre;
    private readonly Bandeja bandeja;
    private readonly int cantidad;
    private readonly Random rnd;

    public Mozo(string nombre, Bandeja bandeja, int cantidad, int semilla)
    {
        this.nombre = nombre;
        this.bandeja = bandeja;
        this.cantidad = cantidad;
        rnd = new Random(semilla);   // una semilla distinta por mozo
    }

    public void Ejecutar()
    {
        for (int i = 0; i < cantidad; i++)
        {
            Thread.Sleep(rnd.Next(200, 600));   // tomar el pedido en la mesa
            Comanda c = new Comanda
            {
                Numero = Interlocked.Increment(ref ultimoNumero),
                Mesa = rnd.Next(1, 16),
                Plato = Platos[rnd.Next(Platos.Length)],
                Mozo = nombre
            };
            bandeja.Dejar(c);
        }
        Program.Log($"{nombre} terminó su turno");
    }
}

class Cocinero
{
    private readonly string nombre;
    private readonly Bandeja bandeja;
    private readonly Random rnd;
    public int Platos { get; private set; }

    public Cocinero(string nombre, Bandeja bandeja, int semilla)
    {
        this.nombre = nombre;
        this.bandeja = bandeja;
        rnd = new Random(semilla);
    }

    public void Ejecutar()
    {
        Comanda c;
        while (bandeja.Tomar(nombre, out c))
        {
            Thread.Sleep(rnd.Next(500, 1200));  // cocinar FUERA del lock
            Platos++;
            Program.Log($"{nombre} terminó {c} (de {c.Mozo})");
        }
        Program.Log($"{nombre} se va: cocinó {Platos} platos");
    }
}

class Program
{
    private static readonly Stopwatch reloj = Stopwatch.StartNew();
    private static readonly object candadoConsola = new object();

    public static void Log(string mensaje)
    {
        lock (candadoConsola)
            Console.WriteLine($"{reloj.Elapsed.TotalSeconds,5:0.00}s  {mensaje}");
    }

    static void Main()
    {
        const int PEDIDOS_POR_MOZO = 5;
        Bandeja bandeja = new Bandeja(4);

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

        List<Thread> hilosMozos = new List<Thread>();
        List<Thread> hilosCocineros = new List<Thread>();
        foreach (Mozo m in mozos) hilosMozos.Add(new Thread(m.Ejecutar));
        foreach (Cocinero k in cocineros) hilosCocineros.Add(new Thread(k.Ejecutar));

        hilosMozos.ForEach(h => h.Start());
        hilosCocineros.ForEach(h => h.Start());

        hilosMozos.ForEach(h => h.Join());      // esperamos a los mozos...
        bandeja.Cerrar();                       // ...y cerramos la bandeja
        hilosCocineros.ForEach(h => h.Join());

        int total = 0;
        foreach (Cocinero k in cocineros) total += k.Platos;
        int esperado = mozos.Length * PEDIDOS_POR_MOZO;
        Console.WriteLine();
        Console.WriteLine($"Pedidos: {esperado} | Platos cocinados: {total} | " +
                          (total == esperado ? "OK" : "ERROR: se perdieron pedidos"));
    }
}
