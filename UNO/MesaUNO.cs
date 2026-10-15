using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UNO
{
    public partial class MesaUNO : Form
    {
        private LogicaUNO director;
        private Timer timerAviso;

        public MesaUNO(string n1, string n2, string n3)
        {
            InitializeComponent();

            // Modifica el Text de los GroupBox para que muestren el nombre de cada usuario
            this.groupBox1.Text = n1;
            this.groupBox2.Text = n2;
            this.groupBox3.Text = n3;

            // Inicializar partida de UNO
            this.director = new LogicaUNO(n1, n2, n3);

            // Creación del Timer (para mensajes emergentes).
            this.timerAviso = new Timer();
            this.timerAviso.Interval = 2500;
            
            IniciarControles();
        }

        private void IniciarControles()
        {
            // DIBUJAR CARTAS
            DibujarCartas();

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

            // PROPIEDADES
            this.boton_uno.Visible = false;

            // EVENTOS
            this.mazo_cartas.Click += new EventHandler(RobarMazoCarta);
            this.timerAviso.Tick += new EventHandler(timerAviso_Tick);
        }

        private void DibujarCartas()
        {
            List<Carta> mano_jugador1 = this.director.jugador1.GetMano();
            List<Carta> mano_jugador2 = this.director.jugador2.GetMano();
            List<Carta> mano_jugador3 = this.director.jugador3.GetMano();

            Carta auxiliar = null;
            PictureBox sprite = null;

            for (int i = 0; i < 7; i++)
            {
                // Jugador 1
                auxiliar = mano_jugador1[i];
                sprite = ObtenerSpriteCarta(auxiliar);
                sprite.SizeMode = PictureBoxSizeMode.StretchImage;
                sprite.Width = 70;
                sprite.Height = 100;
                sprite.Tag = auxiliar;
                sprite.Click += new EventHandler(Carta_Click);
                cartas_jugador1.Controls.Add(sprite);

                // Jugador 2
                auxiliar = mano_jugador2[i];
                sprite = ObtenerSpriteCarta(auxiliar);
                sprite.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                sprite.SizeMode = PictureBoxSizeMode.StretchImage;

                sprite.Width = 100;
                sprite.Height = 70;

                sprite.Tag = auxiliar;
                sprite.Click += new EventHandler(Carta_Click);
                cartas_jugador2.Controls.Add(sprite);

                // Jugador 3
                auxiliar = mano_jugador3[i];
                sprite = ObtenerSpriteCarta(auxiliar);
                sprite.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                sprite.SizeMode = PictureBoxSizeMode.StretchImage;
                sprite.Width = 100;
                sprite.Height = 70;

                sprite.Tag = auxiliar;
                sprite.Click += new EventHandler(Carta_Click);
                cartas_jugador3.Controls.Add(sprite);
            }

            // Pila
            auxiliar = this.director.cartas_en_pila.Last();
            this.pila_cartas.Image = ObtenerSpriteCarta(auxiliar).Image;
            this.pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pila_cartas.Width = 70;
            this.pila_cartas.Height = 100;
            ActualizarIndicadorColor();
        }

        private void RedibujarCartas(int turno, PictureBox picture_carta)
        {
            
        }

        private void ActualizarIndicadorColor()
        {
            switch (this.director.color_actual)
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

        // Lógica de los turnos
        private void ActualizarTurnoEtiqueta()
        {
            if (this.director.turno_actual == 1) lblTurno.Text = $"Turno de: {this.director.jugador1.GetNombre()}";
            else if (this.director.turno_actual == 2) lblTurno.Text = $"Turno de: {this.director.jugador2.GetNombre()}";
            else lblTurno.Text = $"Turno de: {this.director.jugador1.GetNombre()}";
        }


        // Muestra el mensaje de la etiqueta de avisos temporales.
        private void MostrarAvisoTemporal(string mensaje)
        {
            lblAvisoTemp.Text = mensaje;
            timerAviso.Stop(); // Si ya había uno ejecutandose lo reinicia.
            timerAviso.Start();
        }

        // Evento del timer Aviso que se ejecuta cuando termina para limpiar el mensaje.
        private void timerAviso_Tick(object sender, EventArgs e)
        {
            timerAviso.Stop();
            lblAvisoTemp.Text = "";
        }

        // Evento que controla el juego cuando se hace click en una carta
        private void Carta_Click(object sender, EventArgs e)
        {
            PictureBox pic_box_click = sender as PictureBox;
            if (pic_box_click == null) return;

            // Recupera la carta lógica
            Carta carta_usada = pic_box_click.Tag as Carta;
            if (carta_usada == null) return;


            if (!EstabaEnMazo(pic_box_click))
            {
                MessageBox.Show("¡No es tu turno!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            Carta carta_apilada = this.director.cartas_en_pila.Last();
            if (!this.director.JugadaValida(carta_usada, carta_apilada))
            {
                MessageBox.Show("¡Jugada inválida! La carta no coincide en color, número o tipo.", "Reglas del UNO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.director.RealizarJugada(carta_usada, true);
            RedibujarCartas(this.director.turno_actual, pic_box_click);
        }

        public void RealizarJugada(Carta carta_jugada, PictureBox picture_carta, bool llamada)
        {
            Jugador jugadorActualObj = (this.turno_actual == 1) ? jugador1 : (this.turno_actual == 2) ? jugador2 : jugador3;

            if (!JugadaDesdeMazo)
            {
                int cartas_en_mano_antes = 0;
                if (this.turno_actual == 1) cartas_en_mano_antes = this.jugador1.Controls.Count;
                else if (this.turno_actual == 2) cartas_en_mano_antes = cartas_jugador2.Controls.Count;
                else if (this.turno_actual == 3) cartas_en_mano_antes = cartas_jugador3.Controls.Count;

                // Si el jugador tenía EXACTAMENTE 1 carta ANTES de tirar, significa que esta es su última carta.
                if (cartas_en_mano_antes == 1)
                {
                    if (unoCantadoEnTurno)
                    {
                        // ¡Cantó UNO a tiempo! Gana la partida limpiamente
                        ContinuarJugadaNormal(carta_jugada, picture_carta, llamada);
                        VerificarGanador();
                        unoCantadoEnTurno = false; // Reseteamos la bandera
                        return;
                    }
                    else
                    {
                        // NO cantó UNO: Recibe castigo de 2 cartas y pierde el turno
                        MessageBox.Show($"¡{jugadorActualObj.GetNombre()} tiró su última carta pero olvidó decir UNO! Recibe una penalización de 2 cartas y pierde su turno.", "Castigo UNO", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        ContinuarJugadaNormal(carta_jugada, picture_carta, llamada);
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
            ContinuarJugadaNormal(carta_jugada, picture_carta, llamada);

            //Si al tirar esta carta (que era la pen+ultima) el jugador se queda con EXACTAMENTE 1 carta,habilitamos el botón de UNO para que pueda presionarlo en este mismo turno.
            int cartasRestantesDespuesDeJugar = 0;
            if (this.turno_actual == 1) cartasRestantesDespuesDeJugar = cartas_jugador1.Controls.Count;
            else if (this.turno_actual == 2) cartasRestantesDespuesDeJugar = cartas_jugador2.Controls.Count;
            else if (this.turno_actual == 3) cartasRestantesDespuesDeJugar = cartas_jugador3.Controls.Count;

            if (cartasRestantesDespuesDeJugar == 1)
            {
                boton_uno.Visible = true; // Aseguramos que el botón este visible para gritar UNO

            }

            VerificarGanador();
            AplicarEfectoCarta(carta_jugada);
        }

        private bool EstabaEnMazo(PictureBox pb)
        {
            if (this.director.turno_actual == 1 && this.cartas_jugador1.Controls.Contains(pb)) return true;
            if (this.director.turno_actual == 2 && this.cartas_jugador2.Controls.Contains(pb)) return true;
            if (this.director.turno_actual == 3 && this.cartas_jugador3.Controls.Contains(pb)) return true;

            return false;
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
            if (this.director.turno_actual == 1) idJugadorActual = idJugador1;
            else if (this.director.turno_actual == 2) idJugadorActual = idJugador2;
            else if (this.director.turno_actual == 3) idJugadorActual = idJugador3;

            GuardarLogApi(idPartidaActual, idJugadorActual, "Robó una carta del mazo");

            PictureBox imagenCarta = Obtener_Imagen_Carta(robada);
            if (this.director.turno_actual == 1) jugador1.RecibirCarta(robada);
            else if (this.director.turno_actual == 2) jugador2.RecibirCarta(robada);
            else if (this.director.turno_actual == 3) jugador3.RecibirCarta(robada);

            if (this.director.EsJugadaValida(robada, cartas_en_pila.Last()))
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

            if (this.director.turno_actual == 1)
            {
                imagenCarta.SizeMode = PictureBoxSizeMode.StretchImage;
                imagenCarta.Width = 70;
                imagenCarta.Height = 100;

                imagenCarta.Tag = robada;
                imagenCarta.Click += new EventHandler(Carta_Click);
                cartas_jugador1.Controls.Add(imagenCarta);
            }
            else if (this.director.turno_actual == 2)
            {
                imagenCarta.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                imagenCarta.SizeMode = PictureBoxSizeMode.StretchImage;
                imagenCarta.Width = 100;
                imagenCarta.Height = 70;

                imagenCarta.Tag = robada;
                imagenCarta.Click += new EventHandler(Carta_Click);
                cartas_jugador2.Controls.Add(imagenCarta);
            }
            else if (this.director.turno_actual == 3)
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


        private PictureBox ObtenerSpriteCarta(Carta carta)
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

        

        private void boton_uno_Click(object sender, EventArgs e)
        {

            unoCantadoEnTurno = true;
            MessageBox.Show("¡UNO!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private void ContinuarJugadaNormal(Carta carta_jugada, PictureBox picture_carta, bool llamada)
        {
            int idJugadorActual = -1;
            if (this.director.turno_actual == 1) idJugadorActual = idJugador1;
            else if (this.director.turno_actual == 2) idJugadorActual = idJugador2;
            else if (this.director.turno_actual == 3) idJugadorActual = idJugador3;

            GuardarLogApi(idPartidaActual, idJugadorActual, $"Tiró la carta {carta_jugada.getTipo()} de color {carta_jugada.getColor()}");

            // Quitamos la carta de la mano lógica del jugador actual
            if (this.director.turno_actual == 1) jugador1.JugarCarta(carta_jugada);
            else if (this.director.turno_actual == 2) jugador2.JugarCarta(carta_jugada);
            else if (this.director.turno_actual == 3) jugador3.JugarCarta(carta_jugada);

            // Quitamos el pictureBox visualmente de su panel actual
            if (picture_carta.Parent != null)
            {
                picture_carta.Parent.Controls.Remove(picture_carta);
            }

            if (llamada)
            {
                if (this.director.turno_actual == 2) picture_carta.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                else if (this.director.turno_actual == 3) picture_carta.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }

            pila_cartas.Image = picture_carta.Image;
            cartas_en_pila.Add(carta_jugada);
            Actualizar_Color_Actual(carta_jugada.getColor());
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

            this.director.turno_actual = 1;
            direccionJuego = 1;
            unoCantadoEnTurno = false;
            esJugadaDesdeMazo = false;


            idPartidaActual = CrearPartidaApi();

            Repartir_Cartas();
            ActualizaTurnoEtiqueta();

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

        /*

        public void AplicarEfectoCarta(Carta carta_jugada)
        {
            switch (carta_jugada.getTipo())
            {
                case TipoCarta.COMODIN:
                    ColorCarta color_por_comodin = SolicitarEleccionColor(); // Se le solicita escoger un color al jugador actual.
                    carta_jugada.setColor(color_por_comodin);
                    ActualizarColorActual(color_por_comodin);
                    AvanzarTurno();
                    break;

                case TipoCarta.REVERSA:
                    sentido_juego *= -1;
                    AvanzarTurno();
                    break;

                case TipoCarta.BLOQUEA:
                    AvanzarTurno();
                    if (this.director.turno_actual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} has sido bloqueado");
                    else if (this.director.turno_actual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} has sido bloqueado");
                    else if (this.director.turno_actual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} has sido bloqueado");
                    AvanzarTurno();
                    break;

                case TipoCarta.MASDOS:
                    DarCartasASiguienteJugador(2); //cuando un jugador come por un +2 pierde su turno
                    AvanzarTurno();
                    if (this.director.turno_actual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} comes 2 y pierdes tu turno");
                    else if (this.director.turno_actual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} comes 2 y pierdes tu turno");
                    else if (this.director.turno_actual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} comes 2 y pierdes tu turno");
                    AvanzarTurno();
                    break;

                case TipoCarta.MASCUATRO:
                    ColorCarta color_por_mascuatro = SolicitarEleccionColor(); //jugador elige el nuevo color
                    carta_jugada.setColor(color_por_mascuatro);
                    DarCartasASiguienteJugador(4); //cuando un jugador come por un +4 pierde su turno
                    AvanzarTurno();
                    if (this.director.turno_actual == 1) MostrarAvisoTemporal($"{jugador1.GetNombre()} comes 4 y pierdes tu turno");
                    else if (this.director.turno_actual == 2) MostrarAvisoTemporal($"{jugador2.GetNombre()} comes 4 y pierdes tu turno");
                    else if (this.director.turno_actual == 3) MostrarAvisoTemporal($"{jugador3.GetNombre()} comes 4 y pierdes tu turno");
                    Actualizar_Color_Actual(color_por_mascuatro);
                    AvanzarTurno();
                    break;

                default:
                    AvanzarTurno();
                    break;
            }
        }

        private void AvanzarTurno()
        {
            // Cambiamos al siguiente turno segun la dirección del juego
            this.director.turno_actual += sentido_juego;

            if (this.director.turno_actual > 3) this.director.turno_actual = 1;
            if (this.director.turno_actual < 1) this.director.turno_actual = 3;

            ActualizaTurnoEtiqueta();
        }

        // Para casos en los que algún jugador tiene que comer varias cartas del mazo principal (ej. +2 o +4).
        private void DarCartasASiguienteJugador(int cantidad)
        {
            Jugador jugadorDestino = null;
            AvanzarTurno();

            if (this.this.director.turno_actual == 1) jugadorDestino = jugador1;
            else if (this.this.director.turno_actual == 2) jugadorDestino = jugador2;
            else if (this.this.director.turno_actual == 3) jugadorDestino = jugador3;

            for (int i = 0; i < cantidad; i++)
            {
                Carta robada = baraja.Robar_Carta();
                jugadorDestino.RecibirCarta(robada);

                if (this.this.director.turno_actual == 1)
                {
                    PictureBox card = Obtener_Imagen_Carta(robada);
                    card.SizeMode = PictureBoxSizeMode.StretchImage;
                    card.Width = 70;
                    card.Height = 100;

                    card.Tag = robada;
                    card.Click += new EventHandler(Carta_Click);
                    cartas_jugador1.Controls.Add(card);
                }
                else if (this.this.director.turno_actual == 2)
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
                else if (this.this.director.turno_actual == 3)
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
            AvanzarTurno();
        }

        private void RepartirCartas()
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
        */

        private void ImprimirResultados(string nombre_ganador)
        {
            // ventana de resumen de partda

            string nombre_castigado = this.director.jugador2.GetNombre(); // O el perdedor correspondiente
            Form3 ventanaResumen = new Form3(nombre_ganador, nombre_castigado);

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
}