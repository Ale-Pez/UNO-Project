using System;
using System.Collections.Generic;

namespace UNO
{
    internal class LogicaUNO
    {
        // El servidor administra la baraja.
        // Se conserva la variable por compatibilidad.
        public Baraja baraja;

        public List<Carta> cartas_en_pila;

        public Jugador jugador1;
        public Jugador jugador2;
        public Jugador jugador3;

        public int turno_actual = 1;
        public int sentido_juego = 1;
        public ColorCarta color_actual;

        private HashSet<int> jugadores_con_uno = new HashSet<int>();

        // Datos recibidos del servidor
        public int numero_jugador_local = 0;
        public int[] cantidad_cartas = new int[3];

        public bool partida_terminada = false;
        public int numero_ganador = 0;

        public LogicaUNO(string nickname1, string nickname2, string nickname3)
        {
            ReiniciarRonda(nickname1, nickname2, nickname3);
        }

        // Reiniciar solamente el estado local.
        // El servidor se encarga de crear la partida.
        public void ReiniciarRonda(string nickname1, string nickname2,
                                   string nickname3, bool repartir = false)
        {
            this.baraja = null;

            this.cartas_en_pila = new List<Carta>();

            this.jugador1 = new Jugador(nickname1);
            this.jugador2 = new Jugador(nickname2);
            this.jugador3 = new Jugador(nickname3);

            this.turno_actual = 1;
            this.sentido_juego = 1;

            this.color_actual = ColorCarta.ROJO;

            this.jugadores_con_uno.Clear();

            this.cantidad_cartas = new int[3];

            this.partida_terminada = false;
            this.numero_ganador = 0;
        }

        // Número asignado por el servidor a esta computadora.
        public void AsignarJugadorLocal(int numeroJugador)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3)
                this.numero_jugador_local = numeroJugador;
        }

        // Obtener un jugador según su número.
        private Jugador ObtenerJugador(int numeroJugador)
        {
            if (numeroJugador == 1)
                return jugador1;

            if (numeroJugador == 2)
                return jugador2;

            if (numeroJugador == 3)
                return jugador3;

            return null;
        }

        // Recibir los nombres de los tres jugadores.
        public void RecibirNombresServidor(string nombre1,string nombre2,string nombre3)
        {
            this.jugador1 = new Jugador(nombre1);
            this.jugador2 = new Jugador(nombre2);
            this.jugador3 = new Jugador(nombre3);
        }

        // Recibir las siete cartas iniciales.
        // Solamente se guarda la mano de esta computadora.
        public void RecibirManoServidor(int numeroJugador,List<Carta> cartas)
        {
            if (numeroJugador != numero_jugador_local)
                return;

            Jugador jugador = ObtenerJugador(numeroJugador);

            if (jugador == null || cartas == null)
                return;

            jugador.GetMano().Clear();

            foreach (Carta carta in cartas)
            {
                jugador.RecibirCarta(carta);
            }

            cantidad_cartas[numeroJugador - 1] =
                jugador.ContarCartas();
        }

        // Recibir la carta inicial de la mesa.
        public void RecibirCartaMesaServidor(Carta carta)
        {
            if (carta == null)
                return;

            this.cartas_en_pila.Clear();
            this.cartas_en_pila.Add(carta);

            this.color_actual = carta.getColor();
        }

        // Actualizar el turno enviado por el servidor.
        public void ActualizarTurnoServidor(int turno)
        {
            if (turno >= 1 && turno <= 3)
                this.turno_actual = turno;
        }

        // Actualizar el sentido del juego.
        public void ActualizarSentidoServidor(int sentido)
        {
            if (sentido == 1 || sentido == -1)
                this.sentido_juego = sentido;
        }

        // Actualizar el color elegido en un comodín.
        public void ActualizarColorServidor(ColorCarta color)
        {
            this.color_actual = color;
        }

        // Actualizar la cantidad de cartas de cada jugador.
        public void ActualizarCantidadCartasServidor(int numeroJugador,int cantidad)
        {
            if (numeroJugador < 1 || numeroJugador > 3)
                return;

            if (cantidad < 0)
                return;

            this.cantidad_cartas[numeroJugador - 1] = cantidad;
        }

        public int ContarCartasServidor(int numeroJugador)
        {
            if (numeroJugador < 1 || numeroJugador > 3)
                return 0;

            return this.cantidad_cartas[numeroJugador - 1];
        }

        // Comprobación preliminar.
        // La validación definitiva la realiza el servidor.
        public bool JugadaValida(Carta carta_jugada,Carta carta_apilada)
        {
            if (carta_jugada == null || carta_apilada == null)
                return false;

            if (carta_jugada.getTipo() == TipoCarta.COMODIN ||
                carta_jugada.getTipo() == TipoCarta.MASCUATRO)
                return true;

            if (carta_jugada.getColor() == this.color_actual)
                return true;

            if (carta_jugada.getTipo() == TipoCarta.NUMERO &&
                carta_apilada.getTipo() == TipoCarta.NUMERO &&
                carta_jugada.getNumero() == carta_apilada.getNumero())
                return true;

            if (carta_jugada.getTipo() == carta_apilada.getTipo() &&
                carta_jugada.getTipo() != TipoCarta.NUMERO)
                return true;

            return false;
        }

        // El servidor confirmó que una carta fue jugada.
        public void ConfirmarJugadaServidor(int numeroJugador,Carta carta_jugada)
        {
            if (numeroJugador < 1 || numeroJugador > 3 ||
                carta_jugada == null)
                return;

            // Solo conocemos las cartas de nuestra propia mano.
            if (numeroJugador == numero_jugador_local)
            {
                Jugador jugador = ObtenerJugador(numeroJugador);

                Carta carta_encontrada = null;

                foreach (Carta carta in jugador.GetMano())
                {
                    if (carta.getColor() == carta_jugada.getColor() &&
                        carta.getTipo() == carta_jugada.getTipo() &&
                        carta.getNumero() == carta_jugada.getNumero())
                    {
                        carta_encontrada = carta;
                        break;
                    }
                }

                if (carta_encontrada != null)
                    jugador.JugarCarta(carta_encontrada);
            }

            this.cartas_en_pila.Add(carta_jugada);

            // El color definitivo se recibe en COLOR_ACTUAL.
            if (carta_jugada.getTipo() != TipoCarta.COMODIN &&
                carta_jugada.getTipo() != TipoCarta.MASCUATRO)
            {
                this.color_actual = carta_jugada.getColor();
            }
        }

        // Recibir una carta robada del servidor.
        public void RecibirCartaRobadaServidor(int numeroJugador,Carta carta)
        {
            if (numeroJugador != numero_jugador_local ||
                carta == null)
                return;

            Jugador jugador = ObtenerJugador(numeroJugador);

            if (jugador == null)
                return;

            jugador.RecibirCarta(carta);
        }

        // Registrar que un jugador anunció UNO.
        public void GritarUNO()
        {
            GritarUNO(this.numero_jugador_local);
        }

        public void GritarUNO(int numeroJugador)
        {
            if (numeroJugador >= 1 && numeroJugador <= 3)
                this.jugadores_con_uno.Add(numeroJugador);
        }

        public void CancelarUNO(int numeroJugador)
        {
            this.jugadores_con_uno.Remove(numeroJugador);
        }

        // El servidor informa quién ganó.
        public void RecibirGanadorServidor(int numeroJugador)
        {
            if (numeroJugador < 1 || numeroJugador > 3)
                return;

            this.numero_ganador = numeroJugador;
            this.partida_terminada = true;
        }

        // Estos métodos se conservan temporalmente para
        // mantener compatibilidad con MesaUNO.
        // No deben modificar el juego localmente.

        public void RealizarJugada(Carta carta_jugada)
        {
            // La jugada se ejecuta en ServidorUNO.
        }

        public void PasarTurno()
        {
            // El servidor envía TURNO.
        }

        public void RegistrarRoboCarta()
        {
            // El servidor registra el robo.
        }

        public void AplicarEfectoCarta(Carta carta_jugada)
        {
            // El servidor aplica los efectos especiales.
        }
    }
}