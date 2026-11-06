using System;
using System.Collections.Generic;

namespace UNO
{
    public class LogicaUNO
    {
        public Baraja baraja;
        public List<Carta> cartas_en_pila;
        public Jugador jugador1;
        public Jugador jugador2;
        public Jugador jugador3;
        public int turno_actual = 1; // Se define por los números de jugadores.
        public int sentido_juego = 1; // Direccion normal: 1; Reversa: -1.
        public ColorCarta color_actual;
        private HashSet<int> jugadores_con_uno = new HashSet<int>();
        private Carta carta_robada_pendiente;
        private int jugador_esperando_decision;
        private int ganador;

        public LogicaUNO(string nickname1, string nickname2, string nickname3)
        {
            ReiniciarRonda(nickname1, nickname2, nickname3);
        }

        private void AplicarEfectoCarta(Carta carta_jugada, ColorCarta color_elegido)
        {
            if (carta_jugada == null)
                return;

            switch (carta_jugada.getTipo())
            {
                case TipoCarta.COMODIN:
                    this.color_actual = color_elegido;
                    AvanzarTurno();
                    break;

                case TipoCarta.REVERSA:
                    sentido_juego *= -1;
                    AvanzarTurno();
                    break;

                case TipoCarta.BLOQUEA:
                    AvanzarTurno();
                    AvanzarTurno();
                    break;

                case TipoCarta.MASDOS:
                    DarCartasASiguienteJugador(2); // Avanza al jugador castigado y le entrega las cartas.
                    AvanzarTurno(); // El jugador castigado pierde el turno.
                    break;

                case TipoCarta.MASCUATRO:
                    this.color_actual = color_elegido;
                    DarCartasASiguienteJugador(4); // Avanza al jugador castigado y le entrega las cartas.
                    AvanzarTurno(); // El jugador castigado pierde el turno.
                    break;

                default:
                    AvanzarTurno();
                    break;
            }
        }

        private void AvanzarTurno()
        {
            // Cambia al siguiente turno según la dirección actual del juego.
            turno_actual += sentido_juego;

            if (turno_actual > 3) turno_actual = 1;
            else if (turno_actual < 1) turno_actual = 3;
        }

        // Para casos en los que algún jugador tiene que comer varias cartas del mazo principal (ej. +2 o +4).
        private void DarCartasASiguienteJugador(int cantidad)
        {
            AvanzarTurno();
            Jugador jugador = ObtenerJugador(this.turno_actual);

            if (jugador != null)
                DarCartasAJugador(jugador, cantidad);
        }

        private void RepartirCartas()
        {
            for (int i = 0; i < 7; i++)
            {
                DarCartaAJugador(jugador1);
                DarCartaAJugador(jugador2);
                DarCartaAJugador(jugador3);
            }
        }

        public int VerificarGanador()
        {
            // Verificamos el conteo de cartas 
            if (jugador1.ContarCartas() == 0)
                this.ganador = 1;
            else if (jugador2.ContarCartas() == 0)
                this.ganador = 2;
            else if (jugador3.ContarCartas() == 0)
                this.ganador = 3;

            return this.ganador;
        }

        // Registra qué jugador anunció UNO. Se conserva su identidad aunque el turno avance.
        public bool GritarUNO()
        {
            return (GritarUNO(this.turno_actual));
        }

        public bool GritarUNO(int numero_jugador)
        {
            if (numero_jugador < 1 || numero_jugador > 3)
                return false;

            Jugador jugador = ObtenerJugador(numero_jugador);

            if (jugador == null || jugador.ContarCartas() != 1)
                return false;

            this.jugadores_con_uno.Add(numero_jugador);
            return true;
        }

        public void CancelarUNO(int numero_jugador)
        {
            jugadores_con_uno.Remove(numero_jugador);
        }


        public bool JugadorAnuncioUNO(int numero_jugador)
        {
            return jugadores_con_uno.Contains(numero_jugador);
        }

        // Se usa cuando el jugador roba y no realiza una jugada.
        public void PasarTurno()
        {
            if (PartidaTerminada())
                return;

            if (this.carta_robada_pendiente != null)
                return;
            
            AvanzarTurno();
        }

        public bool JugadaValida(Carta carta_jugada)
        {
            if (carta_jugada == null)
                return false;

            TipoCarta tipo = carta_jugada.getTipo();

            // Sin importar la carta en pila, los comodines y mas cuatro se pueden tirar siempre.
            if (tipo == TipoCarta.COMODIN)
                return true;

            if (tipo == TipoCarta.MASCUATRO)
                return PuedeJugarMasCuatro(carta_jugada);

            // La carta jugada coincide en el color con la que se encuentra en la pila.
            if (carta_jugada.getColor() == this.color_actual)
                return true;

            Carta carta_superior = ObtenerCartaSuperior();

            if (carta_superior == null)
                return true;
            
            // Si son ambas cartas son de tipo NUMERO y coinciden en número.
            if (tipo == TipoCarta.NUMERO && carta_superior.getTipo() == TipoCarta.NUMERO &&
                carta_jugada.getNumero() == carta_superior.getNumero())
                return true;

            // Si coinciden en ser del tipo ESPECIAL, no importa su color, es válido.
            if (tipo != TipoCarta.NUMERO && tipo == carta_superior.getTipo())
                return true;

            return false; // Si no cumple ninguna condición, la jugada es inválida.
        }

        public bool RealizarJugada(Carta carta_jugada, ColorCarta color_elegido)
        {
            if (PartidaTerminada())
                return false;

            if (this.carta_robada_pendiente != null)
                return false;

            return (RealizarJugadaInterna(carta_jugada, color_elegido));
        }

        private bool RealizarJugadaInterna(Carta carta_jugada, ColorCarta color_elegido)
        {
            Jugador jugador_actual = ObtenerJugador(this.turno_actual);

            if (jugador_actual == null || carta_jugada == null)
                return false;

            List<Carta> mano = jugador_actual.GetMano();
            bool tiene_carta = false;

            foreach (Carta carta_mano in mano)
            {
                if (CartasIguales(carta_mano, carta_jugada))
                {
                    tiene_carta = true;
                    break;
                }
            }

            if (!tiene_carta) return false;
            
            if (!JugadaValida(carta_jugada)) return false;

            bool es_comodin = (carta_jugada.getTipo() == TipoCarta.COMODIN || 
                carta_jugada.getTipo() == TipoCarta.MASCUATRO);

            if (es_comodin && !ColorElegidoValido(color_elegido))
                return false;

            int cartas_antes = jugador_actual.ContarCartas();

            if (!jugador_actual.JugarCarta(carta_jugada))
                return false;

            this.cartas_en_pila.Add(carta_jugada);

            if (es_comodin)
                this.color_actual = color_elegido;
            else
                this.color_actual = carta_jugada.getColor();

            // Si el jugador tenía EXACTAMENTE 1 carta ANTES de tirar, significa que esta es su última carta.
            if (cartas_antes == 1)
            {
                if (!this.jugadores_con_uno.Contains(this.turno_actual))
                {
                    // NO cantó UNO: Recibe castigo de 2 cartas y pierde el turno.
                    DarCartasAJugador(jugador_actual, 2);
                    this.jugadores_con_uno.Remove(this.turno_actual);
                    AvanzarTurno();
                    return true;
                }

                // ¡Cantó UNO a tiempo! Gana la partida limpiamente
                this.jugadores_con_uno.Remove(this.turno_actual);
                VerificarGanador();
                if (PartidaTerminada()) 
                    return true;
            }
            else
            {
                if (jugador_actual.ContarCartas() == 1)
                    this.jugadores_con_uno.Remove(this.turno_actual);
            }

            AplicarEfectoCarta(carta_jugada, color_elegido);
            return true;
        }

        public void ReiniciarRonda(string nickname1, string nickname2, string nickname3)
        {
            // Crea las cartas necesarias para una baraja de UNO y las reorganiza aleatoriamente.
            this.baraja = new Baraja();
            this.baraja.Barajar();

            // Pila donde se colocaran las cartas jugadas.
            this.cartas_en_pila = new List<Carta>();

            // Inicializar jugadores con los nombres registrados.
            this.jugador1 = new Jugador(nickname1);
            this.jugador2 = new Jugador(nickname2);
            this.jugador3 = new Jugador(nickname3);

            // Configuraciones básicas de juego.
            this.turno_actual = 1;
            this.sentido_juego = 1;
            this.jugadores_con_uno = new HashSet<int>();
            this.carta_robada_pendiente = null;
            this.jugador_esperando_decision = 0;
            this.ganador = 0;

            // Repartición de 7 cartas a cada jugador.
            RepartirCartas();

            Carta carta_inicial = RobarDelMazo();
            while (carta_inicial != null && carta_inicial.getTipo() != TipoCarta.NUMERO)
            {
                this.baraja.RegresarCarta(carta_inicial);
                carta_inicial = RobarDelMazo();
            }

            if (carta_inicial == null)
                throw new InvalidOperationException("No se pudo obtener la carta inicial.");

            this.cartas_en_pila.Add(carta_inicial);
            this.color_actual = carta_inicial.getColor();
        }

        public Jugador ObtenerJugador(int numero_jugador)
        {
            switch (numero_jugador)
            {
                case 1:
                    return jugador1;

                case 2:
                    return jugador2;

                case 3:
                    return jugador3;

                default:
                    return null;
            }
        }

        public List<Carta> ObtenerMano(int numero_jugador)
        {
            Jugador jugador = ObtenerJugador(numero_jugador);

            if (jugador == null)
                return new List<Carta>();

            return new List<Carta>(jugador.GetMano());
        }

        public int ObtenerCantidadCartas(int numero_jugador)
        {
            Jugador jugador = ObtenerJugador(numero_jugador);

            if (jugador == null)
                return 0;

            return jugador.ContarCartas();
        }

        public Carta ObtenerCartaSuperior()
        {
            if (this.cartas_en_pila.Count == 0)
                return null;

            return this.cartas_en_pila[this.cartas_en_pila.Count - 1];
        }

        public ColorCarta ObtenerColorActual()
        {
            return this.color_actual;
        }


        public int ObtenerTurnoActual()
        {
            return this.turno_actual;
        }


        public int ObtenerSentidoJuego()
        {
            return this.sentido_juego;
        }


        public int ObtenerGanador()
        {
            return this.ganador;
        }


        public bool PartidaTerminada()
        {
            return this.ganador != 0;
        }

        public int ObtenerJugadorSiguiente()
        {
            int siguiente = turno_actual + sentido_juego;

            if (siguiente > 3)
                siguiente = 1;
            else if (siguiente < 1)
                siguiente = 3;

            return siguiente;
        }

        private Carta RobarDelMazo()
        {
            Carta carta = this.baraja.RobarCarta();

            // Si el mazo está vacío, recuperamos los descartes.
            if (carta == null && this.cartas_en_pila.Count > 1)
            {
                baraja.RecargarDesdePila(this.cartas_en_pila);
                carta = this.baraja.RobarCarta();
            }

            return carta;
        }

        private void DarCartaAJugador(Jugador jugador)
        {
            Carta carta = RobarDelMazo();

            if (carta != null)
                jugador.RecibirCarta(carta);
        }

        private void DarCartasAJugador(Jugador jugador, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
                DarCartaAJugador(jugador);
        }

        private bool ColorElegidoValido(ColorCarta color)
        {
            return color == ColorCarta.ROJO || color == ColorCarta.AMARILLO ||
                   color == ColorCarta.VERDE || color == ColorCarta.AZUL;
        }

        public Carta RobarCarta()
        {
            if (PartidaTerminada())
                return null;

            if (carta_robada_pendiente != null)
                return null;

            Jugador jugador = ObtenerJugador(this.turno_actual);

            if (jugador == null)
                return null;

            Carta carta = RobarDelMazo();

            if (carta == null)
                return null;

            jugador.RecibirCarta(carta);

            this.carta_robada_pendiente = carta;
            this.jugador_esperando_decision = this.turno_actual;

            return carta;
        }

        public bool PuedeCartaJugadaRobada()
        {
            if (carta_robada_pendiente == null)
                return false;
            
            return JugadaValida(this.carta_robada_pendiente);
        }

        public bool DecidirCartaRobada(bool carta_jugada, ColorCarta color_elegido)
        {
            if (this.carta_robada_pendiente == null)
                return false;

            if (this.jugador_esperando_decision != this.turno_actual)
                return false;

            Carta carta = this.carta_robada_pendiente;

            if (carta_jugada && (carta.getTipo() == TipoCarta.COMODIN ||
                carta.getTipo() == TipoCarta.MASCUATRO) &&
                !ColorElegidoValido(color_elegido))
                return false;

            this.carta_robada_pendiente = null;
            this.jugador_esperando_decision = 0;

            if (!carta_jugada)
            {
                AvanzarTurno();
                return true;
            }

            if (!JugadaValida(carta))
            {
                // Se conserva la carta en la mano.
                AvanzarTurno();
                return false;
            }

            return RealizarJugadaInterna(carta, color_elegido);
        }

        private bool PuedeJugarMasCuatro(Carta carta)
        {
            Jugador jugador = ObtenerJugador(this.turno_actual);

            if (jugador == null) return false;

            List<Carta> mano = jugador.GetMano();

            bool carta_omitida = false;

            foreach (Carta carta_mano in mano)
            {
                if (!carta_omitida && CartasIguales(carta_mano, carta))
                {
                    carta_omitida = true;
                    continue;
                }

                if (carta_mano.getColor() == this.color_actual)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CartasIguales(Carta carta1, Carta carta2)
        {
            if (carta1 == null || carta2 == null)
            {
                return false;
            }

            return carta1.getColor() == carta2.getColor() &&
                   carta1.getTipo() == carta2.getTipo() &&
                   carta1.getNumero() == carta2.getNumero();
        }
    }
}