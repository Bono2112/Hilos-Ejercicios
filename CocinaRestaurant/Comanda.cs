using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocinaRestaurant
{
    public class Comanda
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
}
