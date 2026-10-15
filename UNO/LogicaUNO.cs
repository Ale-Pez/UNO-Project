using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UNO
{
    internal class LogicaUNO
    {
        public Baraja baraja;
        public List<Carta> cartas_en_pila;
        public Jugador jugador1;
        public Jugador jugador2;
        public Jugador jugador3;
        public int turno_actual = 1; // Se define por los números de jugadores.
        public int sentido_juego = 1; // Direccion normal: 1; Reversa: -1.
        public ColorCarta color_actual;
        private bool uno_cantado = false;
        private bool jugada_desde_mazo = false;

        private LlamadaBD llamada;

        public LogicaUNO(string nickname1, string nickname2, string nickname3)
        {
            // Registrar partida de UNO
            this.llamada = new LlamadaBD(nickname1, nickname2, nickname3);

            // Crea las cartas necesarias para una baraja de UNO y las reorganiza aleatoriamente.
            this.baraja = new Baraja();
            this.baraja.Barajar();

            // Pila donde se colocaran las cartas jugadas.
            this.cartas_en_pila = new List<Carta>();

            // Inicializar jugadores con los nombres registrados.
            this.jugador1 = new Jugador(nickname1);
            this.jugador2 = new Jugador(nickname2);
            this.jugador3 = new Jugador(nickname3);

            // Repartición de 7 cartas a cada jugador.
            RepartirCartas();
        }

        public void AplicarEfectoCarta(Carta carta_jugada)
        {
            switch (carta_jugada.getTipo())
            {
                case TipoCarta.COMODIN:
                    ColorCarta color_por_comodin = SolicitarEleccionColor(); // Se le solicita escoger un color al jugador actual.
                    carta_jugada.setColor(color_por_comodin);
                    this.color_actual = color_por_comodin;
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
                    DarCartasASiguienteJugador(2); // Al jugador que sigue se le obliga a comer 2 cartas y pierde su turno.
                    AvanzarTurno();
                    AvanzarTurno();
                    break;

                case TipoCarta.MASCUATRO:
                    ColorCarta color_por_mascuatro = SolicitarEleccionColor(); // Se le solicita escoger un color al jugador actual.
                    carta_jugada.setColor(color_por_mascuatro);
                    DarCartasASiguienteJugador(4); // Al jugador que sigue se le obliga a comer 4 cartas y pierde su turno.
                    this.color_actual = color_por_mascuatro;
                    AvanzarTurno();
                    AvanzarTurno();
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
            if (turno_actual < 1) turno_actual = 3;
        }

        // Para casos en los que algún jugador tiene que comer varias cartas del mazo principal (ej. +2 o +4).
        private void DarCartasASiguienteJugador(int cantidad)
        {
            Jugador jugadorDestino = null;
            AvanzarTurno();

            if (this.turno_actual == 1) jugadorDestino = jugador1;
            else if (this.turno_actual == 2) jugadorDestino = jugador2;
            else if (this.turno_actual == 3) jugadorDestino = jugador3;

            for (int i = 0; i < cantidad; i++)
            {
                Carta robada = baraja.RobarCarta();
                jugadorDestino.RecibirCarta(robada);
            }
            AvanzarTurno();
        }

        private void RepartirCartas()
        {
            Carta carta_auxiliar;

            for (int i = 0; i < 7; i++)
            {
                // Jugador 1
                carta_auxiliar = baraja.RobarCarta();
                jugador1.RecibirCarta(carta_auxiliar);

                // Jugador 2
                carta_auxiliar = baraja.RobarCarta();
                jugador2.RecibirCarta(carta_auxiliar);

                // Jugador 3
                carta_auxiliar = baraja.RobarCarta();
                jugador3.RecibirCarta(carta_auxiliar);
            }

            carta_auxiliar = null;
            do
            {
                carta_auxiliar = baraja.RobarCarta();
                if (carta_auxiliar.getTipo() != TipoCarta.NUMERO)
                {
                    baraja.RegresarCarta(carta_auxiliar);
                    carta_auxiliar = null;
                }
                else
                {
                    cartas_en_pila.Add(carta_auxiliar);
                    this.color_actual = carta_auxiliar.getColor();
                }

            } while (carta_auxiliar == null);
        }

        private ColorCarta SolicitarEleccionColor()
        {
            // crea una ventana emergente para la seleccion del color
            ColorCarta colorElegido = ColorCarta.ROJO;

            Form ventanaColor = new Form()
            {
                Width = 350,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Elige un color para el Comodin",
                StartPosition = FormStartPosition.CenterParent
            };

            Label lbl = new Label() { Text = "¿Qué color deseas elegir?", Left = 20, Top = 20, Width = 250 };
            ventanaColor.Controls.Add(lbl);

            Button btnRojo = new Button() { Text = "Rojo", Left = 20, Top = 60, BackColor = Color.Red, ForeColor = Color.White };
            Button btnAmarillo = new Button() { Text = "Amarillo", Left = 90, Top = 60, BackColor = Color.Yellow };
            Button btnVerde = new Button() { Text = "Verde", Left = 160, Top = 60, BackColor = Color.Green, ForeColor = Color.White };
            Button btnAzul = new Button() { Text = "Azul", Left = 230, Top = 60, BackColor = Color.Blue, ForeColor = Color.White };

            // asignamos eventos a los botones
            btnRojo.Click += (s, e) => { colorElegido = ColorCarta.ROJO; ventanaColor.Close(); };
            btnAmarillo.Click += (s, e) => { colorElegido = ColorCarta.AMARILLO; ventanaColor.Close(); };
            btnVerde.Click += (s, e) => { colorElegido = ColorCarta.VERDE; ventanaColor.Close(); };
            btnAzul.Click += (s, e) => { colorElegido = ColorCarta.AZUL; ventanaColor.Close(); };

            ventanaColor.Controls.Add(btnRojo);
            ventanaColor.Controls.Add(btnAmarillo);
            ventanaColor.Controls.Add(btnVerde);
            ventanaColor.Controls.Add(btnAzul);

            ventanaColor.ShowDialog(); // muestra la ventana y espera a que el usuario elija
            return colorElegido;
        }

        public bool JugadaValida(Carta carta_jugada, Carta carta_apilada)
        {
            // Sin importar la carta en pila, los comodines y mas cuatro se pueden tirar siempre.
            if (carta_jugada.getTipo() == TipoCarta.COMODIN || carta_jugada.getTipo() == TipoCarta.MASCUATRO)
                return true;

            // La carta jugada coincide en el color con la que se encuentra en la pila.
            if (carta_jugada.getColor() == carta_apilada.getColor())
                return true;

            // Si son ambas cartas son de tipo NUMERO y coinciden en número.
            if (carta_jugada.getTipo() == TipoCarta.NUMERO && carta_apilada.getTipo() == TipoCarta.NUMERO
                && carta_jugada.getNumero() == carta_apilada.getNumero())
                return true;

            // Si coinciden en ser del tipo ESPECIAL, no importa su color, es válido.
            if (carta_jugada.getTipo() == carta_apilada.getTipo() && carta_jugada.getTipo() != TipoCarta.NUMERO)
                return true;

            return false; // Si no cumple ninguna condición, la jugada es inválida.
        }

        public void RealizarJugada(Carta carta_jugada, bool llamada)
        {
            Jugador jugador_actual = (this.turno_actual == 1) ? jugador1 : (this.turno_actual == 2) ? jugador2 : jugador3;

            if (!jugada_desde_mazo)
            {
                int cartas_en_mano_antes = 0;
                if (this.turno_actual == 1) cartas_en_mano_antes = this.jugador1.ContarCartas();
                else if (this.turno_actual == 2) cartas_en_mano_antes = this.jugador2.ContarCartas();
                else cartas_en_mano_antes = this.jugador3.ContarCartas();

                // Si el jugador tenía EXACTAMENTE 1 carta ANTES de tirar, significa que esta es su última carta.
                if (cartas_en_mano_antes == 1)
                {
                    if (uno_cantado)
                    {
                        uno_cantado = false; // Reinicia la bandera

                        // ¡Cantó UNO a tiempo! Gana la partida limpiamente
                        ContinuarJugadaNormal(carta_jugada, llamada);
                        VerificarGanador();
                        return;
                    }
                    else
                    {
                        // NO cantó UNO: Recibe castigo de 2 cartas y pierde el turno
                        MessageBox.Show($"¡{jugador_actual.GetNombre()} tiró su última carta pero olvidó decir UNO! Recibe una penalización de 2 cartas y pierde su turno.", "Castigo UNO", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        ContinuarJugadaNormal(carta_jugada, llamada);
                        DarCartasAJugadorEspecifico(jugador_actual, 2);
                        AvanzarTurno();
                        return;
                    }
                }
            }
            else
            {
                jugada_desde_mazo = false;
            }

            // Flujo normal para cuando tira una carta y aun le quedan 2 o más cartas en su mano.
            ContinuarJugadaNormal(carta_jugada, llamada);

            // Si al tirar esta carta (que era la penultima) el jugador se queda con EXACTAMENTE 1 carta, habilitamos el botón de UNO para que pueda presionarlo en este mismo turno.
            int cartas_restantes = 0;
            if (this.turno_actual == 1) cartas_restantes = this.jugador1.ContarCartas();
            else if (this.turno_actual == 2) cartas_restantes = this.jugador2.ContarCartas();
            else cartas_restantes = this.jugador3.ContarCartas();

            VerificarGanador();
            AplicarEfectoCarta(carta_jugada);
        }

        private void VerificarGanador()
        {
            string nombre_ganador = "";
            int numero_ganador = -1;
            bool hay_ganador = false;

            //verificamos el conteo de cartas 
            if (this.jugador1.ContarCartas() == 0)
            {
                nombre_ganador = jugador1.GetNombre();
                numero_ganador = 1;
                hay_ganador = true;
            }
            else if (this.jugador2.ContarCartas() == 0)
            {
                nombre_ganador = jugador2.GetNombre();
                numero_ganador = 2;
                hay_ganador = true;
            }
            else if (this.jugador3.ContarCartas() == 0)
            {
                nombre_ganador = jugador3.GetNombre();
                numero_ganador = 3;
                hay_ganador = true;
            }

            if (hay_ganador)
            {
                // Llamada al método que le hace el POST a la API para cerrar la partida
                llamada.FinalizarPartidaApi(nombre_ganador);

                // Actualizamos el historial de victorias/derrotas mediante la API
                llamada.ActualizarHistorialApi(numero_ganador, true);

                // a los otros dos jugadores se les registra la derrota 
                if (numero_ganador != 1) llamada.ActualizarHistorialApi(1, false);
                if (numero_ganador != 2) llamada.ActualizarHistorialApi(2, false);
                if (numero_ganador != 3) llamada.ActualizarHistorialApi(3, false);

                MessageBox.Show($"¡{nombre_ganador} ha ganado la partida!", "¡Victoria!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}