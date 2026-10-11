using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UNO;

namespace UNO
{
    public partial class Form1 : Form
    {
        private Baraja baraja;
        private List<Carta> cartas_en_pila;
        private Jugador jugador1;
        private Jugador jugador2;
        private Jugador jugador3;
        private int turnoActual = 1; //jug 1 = 1, jug 2 = 2, ....
        private int direccionJuego = 1; // direccion normal 1 y para cuando va en reversa -1
        private ColorCarta colorActualJuego;
        private bool unoCantadoEnTurno = false;
        private bool esJugadaDesdeMazo = false;


        private int idJugador1 = -1; // se inicializan en -1 porque aun no se cargan
        private int idJugador2 = -1;
        private int idJugador3 = -1;
        private int idPartidaActual = 1;
        private Timer timerAviso;
        public Form1(string n1, string n2, string n3)
        {

            InitializeComponent();

            idPartidaActual = ConexionBD.CrearNuevaPartida();

            bool nuevo1, nuevo2, nuevo3;
            idJugador1 = ConexionBD.RegistrarOObtenerJugadorConEstado(n1, out nuevo1);
            idJugador2 = ConexionBD.RegistrarOObtenerJugadorConEstado(n2, out nuevo2);
            idJugador3 = ConexionBD.RegistrarOObtenerJugadorConEstado(n3, out nuevo3);


            ConexionBD.InicializarHistorial(idJugador1);
            ConexionBD.InicializarHistorial(idJugador2);
            ConexionBD.InicializarHistorial(idJugador3);


            baraja = new Baraja();
            baraja.Barajar();
            mazo_cartas.Click += new EventHandler(RobarMazoCarta);
            cartas_en_pila = new List<Carta>();

            //inicializar a los jugadores con los nombres que pusieron en el registro
            jugador1 = new Jugador(n1);
            jugador2 = new Jugador(n2);
            jugador3 = new Jugador(n3);

            //modificamos el Text de los GroupBox para que muestren el nombre de cada usuario
            groupBox1.Text = n1;
            groupBox2.Text = n2;
            groupBox3.Text = n3;

            //creamos el evento del timer
            timerAviso = new Timer();
            timerAviso.Interval = 2500;
            timerAviso.Tick += new EventHandler(timerAviso_Tick);

            //repartimos las cartas en el tablero
            Repartir_Cartas();
            ActualizaTurnoLabel();
            CentrarControles();

            boton_uno.Visible = true;
        }

        private void CentrarControles()
        {
            // CENTRADO DE BOTÓN REINICIO
            this.boton_reinicio.Left = (this.panel_Superior_GroupBox4.ClientSize.Width - this.boton_reinicio.Width) / 2;
            this.boton_reinicio.Top = (this.panel_Superior_GroupBox4.ClientSize.Height - this.boton_reinicio.Height) / 2;

            // CENTRADO DE ETIQUETA TURNO
            this.lblTurno.Left = (this.panel_Superior_GroupBox4.ClientSize.Width - this.lblTurno.Width) / 2;
            this.lblTurno.Top = (this.panel_Superior_GroupBox4.ClientSize.Height - this.boton_reinicio.Height) / 2 - this.lblTurno.Height;

            // CENTRADO DE BOTÓN UNO
            this.boton_uno.Left = (this.panel_Inferior_GroupBox4.ClientSize.Width - this.boton_uno.Width) / 2;
            this.boton_uno.Top = (this.panel_Inferior_GroupBox4.ClientSize.Height - this.boton_uno.Height) / 2;

            // TEXTO EMERGENTE AL COLOCAR EL PUNTERO SOBRE EL MAZO O LA PILA
            this.toolTip1.SetToolTip(this.mazo_cartas, "Mazo de cartas");
            this.toolTip1.SetToolTip(this.pila_cartas, "Pila de cartas");

            // TEXTO DESCRIPTIVO DE JUGADAS
            this.lblAvisoTemp.Left = this.lblAvisoTemp.Width + 50;
            this.lblAvisoTemp.Top = this.lblAvisoTemp.Height + 100;
        }

        private void Repartir_Cartas()
        {
            // Jugador 1
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();
                jugador1.RecibirCarta(carta);

                PictureBox reverso = Obtener_Imagen_Carta(carta);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 70;
                reverso.Height = 100;

                reverso.Tag = carta;
                reverso.Click += new EventHandler(Carta_Click);
                cartas_jugador1.Controls.Add(reverso);
            }

            // Jugador 2
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();
                jugador2.RecibirCarta(carta);

                PictureBox reverso = Obtener_Imagen_Carta(carta); ;
                reverso.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 100;
                reverso.Height = 70;

                reverso.Tag = carta;
                reverso.Click += new EventHandler(Carta_Click);

                cartas_jugador2.Controls.Add(reverso);
            }

            // Jugador 3
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();
                jugador3.RecibirCarta(carta);

                PictureBox reverso = Obtener_Imagen_Carta(carta);
                reverso.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 100;
                reverso.Height = 70;

                reverso.Tag = carta;
                reverso.Click += new EventHandler(Carta_Click);
                cartas_jugador3.Controls.Add(reverso);
            }

            Carta carta_inicial = null;
            do
            {
                carta_inicial = baraja.Robar_Carta();
                if (carta_inicial.getTipo() != TipoCarta.NUMERO)
                {
                    baraja.Regresar_Carta(carta_inicial);
                    carta_inicial = null;
                }
                else
                {
                    pila_cartas.Image = Obtener_Imagen_Carta(carta_inicial).Image;
                    pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
                    pila_cartas.Width = 70;
                    pila_cartas.Height = 100;

                    cartas_en_pila.Add(carta_inicial);
                    Actualizar_Color_Actual(carta_inicial.getColor());
                }

            } while (carta_inicial == null);
        }

        // logica de los turnos

        private void ActualizaTurnoLabel()
        {
            if (turnoActual == 1) lblTurno.Text = $"Turno de: {jugador1.GetNombre()}";
            else if (turnoActual == 2) lblTurno.Text = $"Turno de: {jugador2.GetNombre()}";
            else if (turnoActual == 3) lblTurno.Text = $"Turno de: {jugador3.GetNombre()}";
        }

        private void AvanzarTurno()
        {
            // Cambiamos al siguiente turno segun la dirección del juego
            turnoActual += direccionJuego;

            if (turnoActual > 3) turnoActual = 1;
            if (turnoActual < 1) turnoActual = 3;

            ActualizaTurnoLabel();
        }

        private bool EsJugadaValida(Carta cartaJugada, Carta cartaEnPila)
        {
            //los comodines y mas cuatro siempre se pueden tirar
            if (cartaJugada.getTipo() == TipoCarta.COMODIN || cartaJugada.getTipo() == TipoCarta.MASCUATRO)
            {
                return true;
            }

            //coincide el color
            if (cartaJugada.getColor() == cartaEnPila.getColor())
            {
                return true;
            }

            // si son de tipo numero y coincide con la ultima tirada
            if (cartaJugada.getTipo() == TipoCarta.NUMERO && cartaEnPila.getTipo() == TipoCarta.NUMERO && cartaJugada.getNumero() == cartaEnPila.getNumero())
            {
                return true;
            }

            // si coincide el tipo de carta especial 
            if (cartaJugada.getTipo() == cartaEnPila.getTipo() && cartaJugada.getTipo() != TipoCarta.NUMERO)
            {
                return true;
            }

            return false; // si no cumple ninguna es invalida
        }

        //evento Aviso timer y se ejecuta cuando termina para limpiar el mensaje
        private void timerAviso_Tick(object sender, EventArgs e)
        {
            timerAviso.Stop();
            lblAvisoTemp.Text = "";
        }
        // muestra el mensaje del label
        private void MostrarAvisoTemporal(string mensaje)
        {
            lblAvisoTemp.Text = mensaje;
            timerAviso.Stop(); // si ya habiera uno corriendo lo reinicmos
            timerAviso.Start();
        }

        private void AplicarEfectoCarta(Carta cartaJugada)
        {
            switch (cartaJugada.getTipo())
            {
                case TipoCarta.COMODIN:
                    ColorCarta colorElegidoComodin = SolicitarEleccionColor(); //el jugaedor selecciona el nuevo color
                    cartaJugada.setColor(colorElegidoComodin);
                    Actualizar_Color_Actual(colorElegidoComodin);
                    AvanzarTurno();
                    break;

                case TipoCarta.REVERSA:
                    direccionJuego *= -1;
                    AvanzarTurno();
                    break;

                case TipoCarta.BLOQUEA:
                    AvanzarTurno();
                    if (turnoActual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} has sido bloqueado");
                    else if (turnoActual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} has sido bloqueado");
                    else if (turnoActual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} has sido bloqueado");
                    AvanzarTurno();
                    break;

                case TipoCarta.MASDOS:
                    DarCartasASiguienteJugador(2); //cuando un jugador come por un +2 pierde su turno
                    AvanzarTurno();
                    if (turnoActual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} comes 2 y pierdes tu turno");
                    else if (turnoActual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} comes 2 y pierdes tu turno");
                    else if (turnoActual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} comes 2 y pierdes tu turno");
                    AvanzarTurno();
                    break;

                case TipoCarta.MASCUATRO:
                    ColorCarta colorElegidoMas4 = SolicitarEleccionColor(); //jugador elige el nuevo color
                    cartaJugada.setColor(colorElegidoMas4);
                    DarCartasASiguienteJugador(4); //cuando un jugador come por un +4 pierde su turno
                    AvanzarTurno();
                    if (turnoActual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} comes 4 y pierdes tu turno");
                    else if (turnoActual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} comes 4 y pierdes tu turno");
                    else if (turnoActual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} comes 4 y pierdes tu turno");
                    Actualizar_Color_Actual(colorElegidoMas4);
                    AvanzarTurno();
                    break;

                default:
                    AvanzarTurno();
                    break;
            }

        }

        //para cuando algun jugador tiene que comer cartas del mazo (con +2 o +4)
        private void DarCartasASiguienteJugador(int cantidad)
        {
            Jugador jugadorDestino = null;
            int siguienteTurno = turnoActual + direccionJuego;

            if (siguienteTurno > 3) siguienteTurno = 1;
            if (siguienteTurno < 1) siguienteTurno = 3;

            if (siguienteTurno == 1) jugadorDestino = jugador1;
            else if (siguienteTurno == 2) jugadorDestino = jugador2;
            else if (siguienteTurno == 3) jugadorDestino = jugador3;

            for (int i = 0; i < cantidad; i++)
            {
                Carta robada = baraja.Robar_Carta();
                jugadorDestino.RecibirCarta(robada);

                if (siguienteTurno == 1)
                {
                    PictureBox card = Obtener_Imagen_Carta(robada);
                    card.SizeMode = PictureBoxSizeMode.StretchImage;
                    card.Width = 70;
                    card.Height = 100;

                    card.Tag = robada;
                    card.Click += new EventHandler(Carta_Click);
                    cartas_jugador1.Controls.Add(card);
                }
                else if (siguienteTurno == 2)
                {
                    PictureBox card = Obtener_Imagen_Carta(robada); ;
                    card.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    card.SizeMode = PictureBoxSizeMode.StretchImage;
                    card.Width = 100;
                    card.Height = 70;

                    card.Tag = robada;
                    card.Click += new EventHandler(Carta_Click);
                    cartas_jugador2.Controls.Add(card);
                }
                else if (siguienteTurno == 3)
                {
                    PictureBox card = Obtener_Imagen_Carta(robada);
                    card.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    card.SizeMode = PictureBoxSizeMode.StretchImage;
                    card.Width = 100;
                    card.Height = 70;

                    card.Tag = robada;
                    card.Click += new EventHandler(Carta_Click);
                    cartas_jugador3.Controls.Add(card);
                }
            }
        }

        //evento que controla el juego cuando se hace click en una carta
        private void Carta_Click(object sender, EventArgs e)
        {
            //checamos en que picture box hizo click
            PictureBox pictureBoxClickeado = sender as PictureBox;
            if (pictureBoxClickeado == null) return;

            //recuperamos la pura carta
            Carta cartaSeleccionada = pictureBoxClickeado.Tag as Carta;
            if (cartaSeleccionada == null) return;


            if (!validaTurno(pictureBoxClickeado))
            {
                MessageBox.Show("¡No es tu turno!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            Carta cartaEnPila = cartas_en_pila.Last();
            if (!EsJugadaValida(cartaSeleccionada, cartaEnPila))
            {
                MessageBox.Show("¡Jugada inválida! La carta no coincide en color, número o tipo.", "Reglas del UNO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RealizaJugada(cartaSeleccionada, pictureBoxClickeado, true);
        }

        private bool validaTurno(PictureBox pb)
        {

            if (turnoActual == 1 && cartas_jugador1.Controls.Contains(pb)) return true;
            if (turnoActual == 2 && cartas_jugador2.Controls.Contains(pb)) return true;
            if (turnoActual == 3 && cartas_jugador3.Controls.Contains(pb)) return true;

            return false;
        }

        private void RealizaJugada(Carta cartaJugada, PictureBox pictureBoxCarta, bool llamada)
        {

            Jugador jugadorActualObj = (turnoActual == 1) ? jugador1 : (turnoActual == 2) ? jugador2 : jugador3;

            if (!esJugadaDesdeMazo)
            {
                int cartasEnManoAntesDeJugar = 0;
                if (turnoActual == 1) cartasEnManoAntesDeJugar = cartas_jugador1.Controls.Count;
                else if (turnoActual == 2) cartasEnManoAntesDeJugar = cartas_jugador2.Controls.Count;
                else if (turnoActual == 3) cartasEnManoAntesDeJugar = cartas_jugador3.Controls.Count;

                // Si el jugador tenía EXACTAMENTE 1 carta ANTES de tirar, significa que esta es su última carta.
                if (cartasEnManoAntesDeJugar == 1)
                {
                    if (unoCantadoEnTurno)
                    {
                        // ¡Cantó UNO a tiempo! Gana la partida limpiamente
                        ContinuarJugadaNormal(cartaJugada, pictureBoxCarta, llamada);
                        VerificarGanador();
                        unoCantadoEnTurno = false; // Reseteamos la bandera
                        return;
                    }
                    else
                    {
                        // NO cantó UNO: Recibe castigo de 2 cartas y pierde el turno
                        MessageBox.Show($"¡{jugadorActualObj.GetNombre()} tiró su última carta pero olvidó decir UNO! Recibe una penalización de 2 cartas y pierde su turno.", "Castigo UNO", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        ContinuarJugadaNormal(cartaJugada, pictureBoxCarta, llamada);
                        DarCartasAJugadorEspecifico(jugadorActualObj, 2);
                        unoCantadoEnTurno = false; // Reseteamos la bandera
                        AvanzarTurno();
                        return;
                    }
                }
            }
            else
            {
                esJugadaDesdeMazo = false;
            }

            //flujo normal para cuando tira una carta y aun le quedan 2 o mas en la mano
            ContinuarJugadaNormal(cartaJugada, pictureBoxCarta, llamada);

            //Si al tirar esta carta (que era la pen+ultima) el jugador se queda con EXACTAMENTE 1 carta,habilitamos el botón de UNO para que pueda presionarlo en este mismo turno.
            int cartasRestantesDespuesDeJugar = 0;
            if (turnoActual == 1) cartasRestantesDespuesDeJugar = cartas_jugador1.Controls.Count;
            else if (turnoActual == 2) cartasRestantesDespuesDeJugar = cartas_jugador2.Controls.Count;
            else if (turnoActual == 3) cartasRestantesDespuesDeJugar = cartas_jugador3.Controls.Count;

            if (cartasRestantesDespuesDeJugar == 1)
            {
                boton_uno.Visible = true; // Aseguramos que el botón este visible para gritar UNO

            }

            VerificarGanador();
            AplicarEfectoCarta(cartaJugada);
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

        private bool JugadaEstrategica(Carta cartaRobada)
        {

            bool opcion = false;

            Form ventanaJugada = new Form()
            {
                Width = 280,
                Height = 230,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Jugada Estrategica",
                StartPosition = FormStartPosition.CenterParent
            };

            Label lbl = new Label() { Text = "¿Deseas jugar esta carta?", Left = 20, Top = 15, Width = 230 };
            ventanaJugada.Controls.Add(lbl);

            PictureBox pbCarta = Obtener_Imagen_Carta(cartaRobada);
            pbCarta.SizeMode = PictureBoxSizeMode.StretchImage;
            pbCarta.Width = 70;
            pbCarta.Height = 100;
            pbCarta.Left = (ventanaJugada.ClientSize.Width - pbCarta.Width) / 2;
            pbCarta.Top = 45;
            ventanaJugada.Controls.Add(pbCarta);

            Button si = new Button() { Text = "Si", Left = 50, Top = 155, Width = 70, BackColor = Color.Green, ForeColor = Color.White };
            Button no = new Button() { Text = "No", Left = 140, Top = 155, Width = 70, BackColor = Color.Red, ForeColor = Color.White };


            // asignamos eventos a los botones
            si.Click += (s, e) => { opcion = true; ventanaJugada.Close(); };
            no.Click += (s, e) => { opcion = false; ventanaJugada.Close(); };


            ventanaJugada.Controls.Add(si);
            ventanaJugada.Controls.Add(no);

            ventanaJugada.ShowDialog();
            return opcion;
        }

        private void RobarMazoCarta(object sender, EventArgs e)
        {

            Carta robada = baraja.Robar_Carta();


            if (robada == null)
            {
                if (cartas_en_pila.Count > 1)
                {

                    baraja.RecargarDesdePila(cartas_en_pila);
                    MessageBox.Show("¡Se acabaron las cartas del mazo! Se han reciclado las cartas de la pila.", "Baraja Renovada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    robada = baraja.Robar_Carta();
                }
                else
                {
                    MessageBox.Show("¡Ya no hay cartas disponibles ni en el mazo ni en la pila!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int idJugadorActual = -1;
            if (turnoActual == 1) idJugadorActual = idJugador1;
            else if (turnoActual == 2) idJugadorActual = idJugador2;
            else if (turnoActual == 3) idJugadorActual = idJugador3;

            ConexionBD.GuardarLog(idPartidaActual, idJugadorActual, "Robó una carta del mazo");

            PictureBox imagenCarta = Obtener_Imagen_Carta(robada);
            if (turnoActual == 1) jugador1.RecibirCarta(robada);
            else if (turnoActual == 2) jugador2.RecibirCarta(robada);
            else if (turnoActual == 3) jugador3.RecibirCarta(robada);

            if (EsJugadaValida(robada, cartas_en_pila.Last()))
            {
                if (JugadaEstrategica(robada))
                {
                    esJugadaDesdeMazo = true;
                    RealizaJugada(robada, imagenCarta, false);
                    return;

                }
                else
                {
                    MessageBox.Show("Decidiste conservar la carta", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (robada.getTipo() == TipoCarta.NUMERO)
                {
                    MessageBox.Show($"La carta que robaste es {robada.getColor()} y numero {robada.getNumero()} por lo tanto no es valida. Haz perdido tu turno", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"La carta que robaste es  {robada.getColor()} y de tipo {robada.getTipo()} por lo tanto no es valida. Haz perdido tu turno", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (turnoActual == 1)
            {
                imagenCarta.SizeMode = PictureBoxSizeMode.StretchImage;
                imagenCarta.Width = 70;
                imagenCarta.Height = 100;

                imagenCarta.Tag = robada;
                imagenCarta.Click += new EventHandler(Carta_Click);
                cartas_jugador1.Controls.Add(imagenCarta);
            }
            else if (turnoActual == 2)
            {
                imagenCarta.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                imagenCarta.SizeMode = PictureBoxSizeMode.StretchImage;
                imagenCarta.Width = 100;
                imagenCarta.Height = 70;

                imagenCarta.Tag = robada;
                imagenCarta.Click += new EventHandler(Carta_Click);
                cartas_jugador2.Controls.Add(imagenCarta);
            }
            else if (turnoActual == 3)
            {
                imagenCarta.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                imagenCarta.SizeMode = PictureBoxSizeMode.StretchImage;
                imagenCarta.Width = 100;
                imagenCarta.Height = 70;

                imagenCarta.Tag = robada;
                imagenCarta.Click += new EventHandler(Carta_Click);
                cartas_jugador3.Controls.Add(imagenCarta);
            }
            AvanzarTurno();



        }


        private PictureBox Obtener_Imagen_Carta(Carta carta)
        {
            PictureBox imagen_carta = new PictureBox();
            imagen_carta.Image = Properties.Resources.back_uno;

            // COMODIN
            if (carta.getTipo() == TipoCarta.COMODIN)
                imagen_carta.Image = Properties.Resources.wildcard_uno;

            // +4
            else if (carta.getTipo() == TipoCarta.MASCUATRO)
                imagen_carta.Image = Properties.Resources.wildcard_plus4;

            // IDENTIFICACIÓN DE COLOR
            string color = "";

            if (carta.getColor() == ColorCarta.ROJO)
                color = "red";
            else if (carta.getColor() == ColorCarta.AMARILLO)
                color = "yellow";
            else if (carta.getColor() == ColorCarta.VERDE)
                color = "green";
            else if (carta.getColor() == ColorCarta.AZUL)
                color = "blue";

            // NUMÉRICAS
            if (carta.getTipo() == TipoCarta.NUMERO)
            {
                string nombre = color + "_" + carta.getNumero();
                imagen_carta.Image = (Image)Properties.Resources.ResourceManager.GetObject(nombre);
            }

            // BLOQUEA
            else if (carta.getTipo() == TipoCarta.BLOQUEA)
            {
                string nombre = color + "_block";
                imagen_carta.Image = (Image)Properties.Resources.ResourceManager.GetObject(nombre);
            }

            // REVERSA
            else if (carta.getTipo() == TipoCarta.REVERSA)
            {
                string nombre = color + "_switch";
                imagen_carta.Image = (Image)Properties.Resources.ResourceManager.GetObject(nombre);
            }

            // +2
            else if (carta.getTipo() == TipoCarta.MASDOS)
            {
                string nombre = color + "_plus";
                imagen_carta.Image = (Image)Properties.Resources.ResourceManager.GetObject(nombre);
            }

            return imagen_carta;
        }


        private void VerificarGanador()
        {
            string nombreGanador = "";
            int idGanador = -1;
            bool hayGanador = false;

            //verificamos el conteo de cartas 
            if (cartas_jugador1.Controls.Count == 0)
            {
                nombreGanador = jugador1.GetNombre();
                idGanador = idJugador1;
                hayGanador = true;
            }
            else if (cartas_jugador2.Controls.Count == 0)
            {
                nombreGanador = jugador2.GetNombre();
                idGanador = idJugador2;
                hayGanador = true;
            }
            else if (cartas_jugador3.Controls.Count == 0)
            {
                nombreGanador = jugador3.GetNombre();
                idGanador = idJugador3;
                hayGanador = true;
            }

            if (hayGanador)
            {
                ConexionBD.FinalizarPartida(idPartidaActual, nombreGanador);

                ConexionBD.RegistrarResultadoPartida(idGanador, true);

                // a los otros dos jugadores se les registra la derrota 
                if (idGanador != idJugador1) ConexionBD.RegistrarResultadoPartida(idJugador1, false);
                if (idGanador != idJugador2) ConexionBD.RegistrarResultadoPartida(idJugador2, false);
                if (idGanador != idJugador3) ConexionBD.RegistrarResultadoPartida(idJugador3, false);

                MessageBox.Show($"¡{nombreGanador} ha ganado la partida!", "¡Victoria!", MessageBoxButtons.OK, MessageBoxIcon.Information);


                // ventana de resumen de partda

                string nombreCastigado = jugador2.GetNombre(); // O el perdedor correspondiente
                Form3 ventanaResumen = new Form3(nombreGanador, nombreCastigado);

                DialogResult resultadoResumen = ventanaResumen.ShowDialog(this);

                if (resultadoResumen == DialogResult.Retry)
                {
                    ReiniciarJuego();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void boton_uno_Click(object sender, EventArgs e)
        {

            unoCantadoEnTurno = true;
            MessageBox.Show("¡UNO!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        //metodo auxiliar que se encargar exclusivamente de jugar la carta robada del mazo de forma limpia y sin activar jamas la penalizacion (el jugador no dice UNO)
        private void RealizaJugadaDesdeMazo(Carta cartaJugada, PictureBox pictureBoxCarta)
        {
            int idJugadorActual = -1;
            if (turnoActual == 1) idJugadorActual = idJugador1;
            else if (turnoActual == 2) idJugadorActual = idJugador2;
            else if (turnoActual == 3) idJugadorActual = idJugador3;

            ConexionBD.GuardarLog(idPartidaActual, idJugadorActual, $"Tiró (desde mazo) la carta {cartaJugada.getTipo()} de color {cartaJugada.getColor()}");

            // NOTA: Como la carta se acaba de robar, NUNCA se añadió formalmente a la mano del jugador (solo se evaluó). 
            // Por lo tanto, no hay que quitarla del objeto Jugador, solo colocarla directamente en la pila.

            pila_cartas.Image = pictureBoxCarta.Image;
            cartas_en_pila.Add(cartaJugada);

            // Verificamos si con esto gana (por si acaso se quedó sin cartas antes, aunque robó) y aplicamos efectos
            VerificarGanador();
            AplicarEfectoCarta(cartaJugada);
        }



        // Método auxiliar para dar cartas a un jugador en específico (por castigos de UNO, etc.)
        private void DarCartasAJugadorEspecifico(Jugador jugadorDestino, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                Carta robada = baraja.Robar_Carta();
                if (robada == null) break; // Si la baraja se queda sin cartas

                jugadorDestino.RecibirCarta(robada);
                PictureBox card = Obtener_Imagen_Carta(robada);
                card.SizeMode = PictureBoxSizeMode.StretchImage;

                // Asignamos el panel y rotación según el jugador destino exacto
                if (jugadorDestino == jugador1)
                {
                    card.Width = 70; card.Height = 100;
                    card.Tag = robada; card.Click += new EventHandler(Carta_Click);
                    cartas_jugador1.Controls.Add(card);
                }
                else if (jugadorDestino == jugador2)
                {
                    card.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    card.Width = 100; card.Height = 70;
                    card.Tag = robada; card.Click += new EventHandler(Carta_Click);
                    cartas_jugador2.Controls.Add(card);
                }
                else if (jugadorDestino == jugador3)
                {
                    card.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    card.Width = 100; card.Height = 70;
                    card.Tag = robada; card.Click += new EventHandler(Carta_Click);
                    cartas_jugador3.Controls.Add(card);
                }
            }
        }

        private void ContinuarJugadaNormal(Carta cartaJugada, PictureBox pictureBoxCarta, bool llamada)
        {
            int idJugadorActual = -1;
            if (turnoActual == 1) idJugadorActual = idJugador1;
            else if (turnoActual == 2) idJugadorActual = idJugador2;
            else if (turnoActual == 3) idJugadorActual = idJugador3;

            ConexionBD.GuardarLog(idPartidaActual, idJugadorActual, $"Tiró la carta {cartaJugada.getTipo()} de color {cartaJugada.getColor()}");

            // Quitamos la carta de la mano lógica del jugador actual
            if (turnoActual == 1) jugador1.JugarCarta(cartaJugada);
            else if (turnoActual == 2) jugador2.JugarCarta(cartaJugada);
            else if (turnoActual == 3) jugador3.JugarCarta(cartaJugada);

            // Quitamos el pictureBox visualmente de su panel actual
            if (pictureBoxCarta.Parent != null)
            {
                pictureBoxCarta.Parent.Controls.Remove(pictureBoxCarta);
            }

            if (llamada)
            {
                if (turnoActual == 2) pictureBoxCarta.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                else if (turnoActual == 3) pictureBoxCarta.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }

            pila_cartas.Image = pictureBoxCarta.Image;
            cartas_en_pila.Add(cartaJugada);
        }

        public void ReiniciarJuego()
        {
            cartas_jugador1.Controls.Clear();
            cartas_jugador2.Controls.Clear();
            cartas_jugador3.Controls.Clear();
            pila_cartas.Image = null;

            string nombre1 = jugador1.GetNombre();
            string nombre2 = jugador2.GetNombre();
            string nombre3 = jugador3.GetNombre();

            jugador1 = new Jugador(nombre1);
            jugador2 = new Jugador(nombre2);
            jugador3 = new Jugador(nombre3);

            baraja = new Baraja();
            baraja.Barajar();
            cartas_en_pila.Clear();

            turnoActual = 1;
            direccionJuego = 1;
            unoCantadoEnTurno = false;
            esJugadaDesdeMazo = false;


            idPartidaActual = ConexionBD.CrearNuevaPartida();

            Repartir_Cartas();
            ActualizaTurnoLabel();

            MessageBox.Show("¡Se ha iniciado una nueva partida!", "Reiniciar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //boton para reinicio de juego
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show("¿Estás seguro de que deseas reiniciar la partida actual?", "Confirmar Reinicio", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                ReiniciarJuego();
            }
        }

        private void Actualizar_Color_Actual(ColorCarta color)
        {
            this.colorActualJuego = color;

            switch (colorActualJuego)
            {
                case ColorCarta.ROJO:
                    this.icono_color_juego.Image = Properties.Resources.red;
                    break;

                case ColorCarta.AZUL:
                    this.icono_color_juego.Image = Properties.Resources.blue;
                    break;

                case ColorCarta.AMARILLO:
                    this.icono_color_juego.Image = Properties.Resources.yellow;
                    break;

                case ColorCarta.VERDE:
                    this.icono_color_juego.Image = Properties.Resources.green;
                    break;
            }

            this.icono_color_juego.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}