using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNO
{

    public class Jugador
    {
        private string nombre;
        private List<Carta> mano;

        public Jugador(string nombre)
        {
            this.nombre = nombre;
            this.mano = new List<Carta>();
        }
        public string GetNombre()
        {
            return nombre;
        }

        public List<Carta> GetMano()
        {
            return mano;
        }

        public void RecibirCarta(Carta carta)
        {
            mano.Add(carta);
        }

        //  remover una carta de la mano cuando la juega
        public void JugarCarta(Carta carta)
        {
            mano.Remove(carta);
        }

        // para saber cartas le quedan al jugador
        public int ContarCartas()
        {
            return mano.Count;
        }
    }
}




