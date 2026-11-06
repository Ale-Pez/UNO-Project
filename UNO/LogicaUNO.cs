using System.Collections.Generic;

namespace UNO
{
    internal class LogicaUNO
    {
        public Baraja baraja;
        public List<Carta> cartas_en_pila;
        public Jugador jugador1;
        public Jugador jugador2;
        public Jugador jugador3;
        public int turno_actual = 1;
        public int sentido_juego = 1;
        public ColorCarta color_actual;
        private HashSet<int> jugadores_con_uno = new HashSet<int>();
        private bool jugada_desde_mazo = false;
        private LlamadaBD llamada;

        // El servidor es quien reparte, valida, avanza turnos y decide el ganador.
        private int numero_jugador_local = 0;
        private int[] cantidad_cartas = new int[3];
        public bool partida_terminada = false;
        public int numero_ganador = -1;

        public LogicaUNO(string nickname1, string nickname2, string nickname3)
        {
            ReiniciarRonda(nickname1, nickname2, nickname3);
        }

        public void ReiniciarRonda(string nickname1, string nickname2, string nickname3, bool repartir = false)
        {
            // No repartir aquí: las cartas llegarán en MANO_INICIAL.
            baraja = null;
            cartas_en_pila = new List<Carta>();
            jugador1 = new Jugador(nickname1);
            jugador2 = new Jugador(nickname2);
            jugador3 = new Jugador(nickname3);
            turno_actual = 1;
            sentido_juego = 1;
            color_actual = ColorCarta.ROJO;
            jugadores_con_uno.Clear();
            jugada_desde_mazo = false;
            cantidad_cartas = new int[3];
            partida_terminada = false;
            numero_ganador = -1;
        }

        public void AsignarJugadorLocal(int numeroJugador)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3)
                numero_jugador_local = numeroJugador;
        }

        private Jugador ObtenerJugador(int numeroJugador)
        {
            if (numeroJugador == 1) return jugador1;
            if (numeroJugador == 2) return jugador2;
            if (numeroJugador == 3) return jugador3;
            return null;
        }

        public void RecibirNombresServidor(string nickname1, string nickname2, string nickname3)
        {
            // Los nombres se reciben del servidor, en orden de conexión.
            jugador1 = new Jugador(nickname1);
            jugador2 = new Jugador(nickname2);
            jugador3 = new Jugador(nickname3);
        }

        public void RecibirManoServidor(int numeroJugador, List<Carta> cartas)
        {
            if (numeroJugador != numero_jugador_local) return;
            Jugador jugador = ObtenerJugador(numeroJugador);
            if (jugador == null) return;

            jugador.GetMano().Clear();
            foreach (Carta carta in cartas)
                if (carta != null) jugador.RecibirCarta(carta);

            cantidad_cartas[numeroJugador - 1] = jugador.ContarCartas();
        }

        public void RecibirCartaMesaServidor(Carta carta)
        {
            if (carta == null) return;
            cartas_en_pila.Clear();
            cartas_en_pila.Add(carta);
            color_actual = carta.getColor();
        }

        public void ActualizarTurnoServidor(int numeroJugador)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3)
                turno_actual = numeroJugador;
        }

        public void ActualizarSentidoServidor(int sentido)
        {
            if (sentido == 1 || sentido == -1)
                sentido_juego = sentido;
        }

        public void ActualizarColorServidor(ColorCarta color)
        {
            color_actual = color;
        }

        public void ActualizarCantidadCartasServidor(int numeroJugador, int cantidad)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3 && cantidad >= 0)
                cantidad_cartas[numeroJugador - 1] = cantidad;
        }

        public int ContarCartasServidor(int numeroJugador)
        {
            if (numeroJugador < 1 || numeroJugador > 3) return 0;
            if (numeroJugador == numero_jugador_local)
            {
                Jugador jugador = ObtenerJugador(numeroJugador);
                return jugador == null ? 0 : jugador.ContarCartas();
            }
            return cantidad_cartas[numeroJugador - 1];
        }

        public bool JugadaValida(Carta carta_jugada, Carta carta_apilada)
        {
            if (carta_jugada == null || carta_apilada == null) return false;

            // El servidor comprobará las restricciones adicionales del +4.
            if (carta_jugada.getTipo() == TipoCarta.COMODIN ||
                carta_jugada.getTipo() == TipoCarta.MASCUATRO)
                return true;

            if (carta_jugada.getColor() == color_actual) return true;

            if (carta_jugada.getTipo() == TipoCarta.NUMERO &&
                carta_apilada.getTipo() == TipoCarta.NUMERO &&
                carta_jugada.getNumero() == carta_apilada.getNumero())
                return true;

            if (carta_jugada.getTipo() != TipoCarta.NUMERO &&
                carta_jugada.getTipo() == carta_apilada.getTipo())
                return true;

            return false;
        }

        public void ConfirmarJugadaServidor(int numeroJugador, Carta carta_jugada)
        {
            if (carta_jugada == null) return;

            if (numeroJugador == numero_jugador_local)
            {
                Jugador jugador = ObtenerJugador(numeroJugador);
                if (jugador != null)
                {
                    List<Carta> mano = jugador.GetMano();
                    for (int i = 0; i < mano.Count; i++)
                    {
                        Carta carta = mano[i];
                        if (carta.getColor() == carta_jugada.getColor() &&
                            carta.getTipo() == carta_jugada.getTipo() &&
                            carta.getNumero() == carta_jugada.getNumero())
                        {
                            mano.RemoveAt(i);
                            break;
                        }
                    }
                    cantidad_cartas[numeroJugador - 1] = mano.Count;
                }
            }

            cartas_en_pila.Add(carta_jugada);
            // No se cambia color_actual aquí: el servidor envía COLOR_ACTUAL.
            // No se avanza el turno aquí: el servidor envía TURNO.
        }

        public void RecibirCartaRobadaServidor(int numeroJugador, Carta carta)
        {
            if (carta == null || numeroJugador != numero_jugador_local) return;
            Jugador jugador = ObtenerJugador(numeroJugador);
            if (jugador == null) return;
            jugador.RecibirCarta(carta);
            cantidad_cartas[numeroJugador - 1] = jugador.ContarCartas();
        }

        public void GritarUNO()
        {
            GritarUNO(turno_actual);
        }

        public void GritarUNO(int numeroJugador)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3)
                jugadores_con_uno.Add(numeroJugador);
        }

        public void CancelarUNO(int numeroJugador)
        {
            jugadores_con_uno.Remove(numeroJugador);
        }

        public void RecibirGanadorServidor(int numeroJugador)
        {
            if (numeroJugador < 1 || numeroJugador > 3) return;
            numero_ganador = numeroJugador;
            partida_terminada = true;
        }

        // Se conservan los métodos anteriores, pero las acciones reales
        // ahora se ejecutan y confirman exclusivamente en ServidorUNO.
        public void RealizarJugada(Carta carta_jugada)
        {
        }

        public void PasarTurno()
        {
        }

        public void RegistrarRoboCarta(Carta carta_robada)
        {
        }

        public void AplicarEfectoCarta(Carta carta_jugada)
        {
        }
    }
}
