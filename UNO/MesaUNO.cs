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

        private ClienteJuego cliente;
        private bool partida_iniciada = false;

        // Conserva al jugador que acaba de quedarse con una carta.
        // La oportunidad de cantar UNO termina cuando comienza la siguiente acción válida.
        private int jugador_pendiente_uno = -1;
        private bool partida_terminada = false;
        private bool accion_pendiente = false; // Evita enviar acciones repetidas
        private bool esperando_reinicio = false;


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

            //conexion al servidor
            cliente = new ClienteJuego();

            IniciarControles();
            ActualizarInterfaz();



            cliente.MensajeRecibido += (mensaje) =>
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                            ProcesarMensajeServidor(mensaje);
                    }));
                }
            };
            var identificador = this.Handle;

            cliente.ConectarAlServidor("10.103.150.202", 11111, n1);
        }

        private void IniciarControles()
        {
            // TEXTO EMERGENTE AL COLOCAR EL PUNTERO SOBRE EL MAZO O LA PILA
            this.toolTip1.SetToolTip(this.mazo_cartas, "Mazo de cartas");
            this.toolTip1.SetToolTip(this.pila_cartas, "Pila de cartas");

            // VISIBILIDAD DEL BOTON UNO
            this.boton_uno.Visible = false;

            // TEXTO DESCRIPTIVO DE MOVIMIENTOS
            this.lblAvisoTemp.Left = this.lblAvisoTemp.Width + 50;
            this.lblAvisoTemp.Top = this.lblAvisoTemp.Height + 100;

            // EVENTOS
            // El evento se registra una sola vez abajo.

            this.mazo_cartas.Click -= RobarMazoCarta;
            this.mazo_cartas.Click += RobarMazoCarta;

            this.boton_uno.Click -= boton_uno_Click;
            this.boton_uno.Click += boton_uno_Click;

            this.boton_reinicio.Click -= button1_Click;
            this.boton_reinicio.Click += button1_Click;

            AcomodarControles();
        }

        private void AcomodarControles()
        {
            // CENTRADO DE BOTÓN REINICIO
            this.boton_reinicio.Left = (this.panel_Superior_GroupBox4.ClientSize.Width - this.boton_reinicio.Width) / 2;
            this.boton_reinicio.Top = 2;

            // CENTRADO DE ETIQUETA TURNO
            this.lblTurno.Left = (this.panel_Superior_GroupBox4.ClientSize.Width - this.lblTurno.Width) / 2;
            this.lblTurno.Top = this.boton_reinicio.Bottom + 2;

            // CENTRADO DE BOTÓN UNO
            this.boton_uno.Left = (this.panel_Inferior_GroupBox4.ClientSize.Width - this.boton_uno.Width) / 2;
            this.boton_uno.Top = (this.panel_Inferior_GroupBox4.ClientSize.Height - this.boton_uno.Height) / 2;
        }

        private void ActualizarInterfaz()
        {
            DibujarMano(this.director.jugador1.GetMano(), this.cartas_jugador1, 1);
            DibujarMano(this.director.jugador2.GetMano(), this.cartas_jugador2, 2);
            DibujarMano(this.director.jugador3.GetMano(), this.cartas_jugador3, 3);

            if (this.director.cartas_en_pila.Count > 0)
            {
                PictureBox sprite = ObtenerSpriteCarta(this.director.cartas_en_pila.Last());
                Image imagen = sprite.Image;
                sprite.Image = null;
                sprite.Dispose();

                Image anterior = this.pila_cartas.Image;
                this.pila_cartas.Image = imagen;
                if (anterior != null) anterior.Dispose();

                this.pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
                this.pila_cartas.Width = 70;
                this.pila_cartas.Height = 100;
            }

            ActualizarIndicadorColor();
            ActualizarTurnoEtiqueta();
            this.boton_uno.Visible = this.jugador_pendiente_uno == cliente?.NumeroJugador && !this.partida_terminada;
        }

        private void DibujarMano(List<Carta> mano, FlowLayoutPanel panel, int numeroJugador)
        {
            LimpiarPanelCartas(panel);

            bool esMiMano = cliente != null && numeroJugador == cliente.NumeroJugador;
            int cantidad = esMiMano ? mano.Count : this.director.ContarCartasServidor(numeroJugador);

            for (int i = 0; i < cantidad; i++)
            {
                PictureBox sprite = esMiMano ? ObtenerSpriteCarta(mano[i]) : new PictureBox();
                if (!esMiMano) sprite.Image = (Image)Properties.Resources.back_uno.Clone();
                sprite.Tag = esMiMano ? (object)mano[i] : null;
                sprite.SizeMode = PictureBoxSizeMode.StretchImage;

                if (numeroJugador == 1)
                {
                    sprite.Width = 70;
                    sprite.Height = 100;
                }
                else
                {
                    sprite.Width = 100;
                    sprite.Height = 70;
                    if (sprite.Image != null)
                    {
                        RotateFlipType rotacion = numeroJugador == 2 ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone;
                        sprite.Image.RotateFlip(rotacion);
                    }
                }
                if (esMiMano) sprite.Click += Carta_Click;
                panel.Controls.Add(sprite);
            }
        }

        private void LimpiarPanelCartas(FlowLayoutPanel panel)
        {
            while (panel.Controls.Count > 0)
            {
                Control control = panel.Controls[0];
                panel.Controls.RemoveAt(0);
                PictureBox carta = control as PictureBox;
                if (carta != null && carta.Image != null)
                {
                    carta.Image.Dispose();
                    carta.Image = null;
                }
                control.Dispose();
            }
        }

        private void ActualizarIndicadorColor()
        {
            Image nuevo_icono = null;

            switch (this.director.color_actual)
            {
                case ColorCarta.ROJO:
                    nuevo_icono = Properties.Resources.red;
                    break;

                case ColorCarta.AZUL:
                    nuevo_icono = Properties.Resources.blue;
                    break;

                case ColorCarta.AMARILLO:
                    nuevo_icono = Properties.Resources.yellow;
                    break;

                case ColorCarta.VERDE:
                    nuevo_icono = Properties.Resources.green;
                    break;
            }

            if (nuevo_icono != null)
                icono_color_juego.Image = (Image)nuevo_icono.Clone();

            icono_color_juego.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void ActualizarTurnoEtiqueta()
        {
            string nombre;

            if (this.director.turno_actual == 1) nombre = this.director.jugador1.GetNombre();
            else if (this.director.turno_actual == 2) nombre = this.director.jugador2.GetNombre();
            else nombre = this.director.jugador3.GetNombre();

            this.lblTurno.Text = $"Turno de: {nombre}";
            AcomodarControles();
        }


        // Muestra el mensaje de la etiqueta de avisos temporales.
        private void MostrarAvisoTemporal(string mensaje)
        {
            lblAvisoTemp.Text = mensaje;
            AcomodarControles();

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
            if (!partida_iniciada)
            {
                MessageBox.Show("Esperando a que se conecten los tres jugadores.");
                return;
            }

            if (this.partida_terminada || this.accion_pendiente) return;
            PictureBox pic_box_click = sender as PictureBox;
            if (pic_box_click == null) return;
            Carta carta_usada = pic_box_click.Tag as Carta;
            if (carta_usada == null) return;

            if (!EstabaEnMazo(pic_box_click))
            {
                MessageBox.Show("¡No es tu turno!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (this.director.cartas_en_pila.Count == 0) return;

            Carta carta_apilada = this.director.cartas_en_pila.Last();
            // El servidor decide finalmente si una carta es válida (especialmente el +4).
            if (!this.director.JugadaValida(carta_usada, carta_apilada))
            {
                MessageBox.Show("¡Jugada inválida! La carta no coincide en color, número o tipo.", "Reglas del UNO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string mensaje = $"JUGAR_CARTA|{carta_usada.getColor()}|{carta_usada.getTipo()}|{carta_usada.getNumero()}";
            if (carta_usada.getTipo() == TipoCarta.COMODIN || carta_usada.getTipo() == TipoCarta.MASCUATRO)
                mensaje += "|" + SolicitarEleccionColor().ToString();

            this.accion_pendiente = true;
            cliente.EnviarAccion(mensaje);
            // NO se modifica la mano, la pila ni el turno aquí.
        }

        //Ahora cada computadora solamente podrá seleccionar las cartas de su jugador, siempre que sea su turno
        private bool EstabaEnMazo(PictureBox pb)
        {
            // Verificar que sea el turno de este cliente
            if (this.director.turno_actual != cliente.NumeroJugador)
                return false;

            if (cliente.NumeroJugador == 1 &&
                this.cartas_jugador1.Controls.Contains(pb))
                return true;

            if (cliente.NumeroJugador == 2 &&
                this.cartas_jugador2.Controls.Contains(pb))
                return true;

            if (cliente.NumeroJugador == 3 &&
                this.cartas_jugador3.Controls.Contains(pb))
                return true;

            return false;
        }

        private void TerminarAccion(int jugador_que_jugo)
        {
            // La jugada ya fue confirmada por el servidor.
            // El ganador solo se determina cuando llega GANADOR.
            ActualizarInterfaz();
        }

        private int ContarCartas(int numero_jugador)
        {
            return this.director.ContarCartasServidor(numero_jugador);
        }

        private string ObtenerNombreGanador()
        {
            if (!this.director.partida_terminada) return null;
            return ObtenerJugador(this.director.numero_ganador).GetNombre();
        }

        private void CerrarVentanaUNO()
        {
            this.jugador_pendiente_uno = -1;
            this.boton_uno.Visible = false;
        }

        private void boton_uno_Click(object sender, EventArgs e)
        {
            if (!this.partida_iniciada || this.jugador_pendiente_uno == -1 || this.partida_terminada) return; //solo se puede gritar UNO cuando la partida está iniciada y corresponde a ese jugador
            if (this.jugador_pendiente_uno != cliente.NumeroJugador) return;
            cliente.EnviarAccion("UNO");
            // Esperar JUGADOR_UNO para ocultar el botón.
        }

        private Jugador ObtenerJugador(int numero_jugador)
        {
            if (numero_jugador == 1) return this.director.jugador1;
            if (numero_jugador == 2) return this.director.jugador2;
            return this.director.jugador3;
        }

        private void RobarMazoCarta(object sender, EventArgs e)
        {
            if (!partida_iniciada)
            {
                MessageBox.Show("Esperando a que se conecten los tres jugadores.");
                return;
            }

            if (this.partida_terminada || this.accion_pendiente) return;
            if (this.director.turno_actual != cliente.NumeroJugador)
            {
                MessageBox.Show("¡No es tu turno!");
                return;
            }
            this.accion_pendiente = true;
            cliente.EnviarAccion("ROBAR_CARTA");
            // El servidor enviará CARTA_ROBADA y, si es válida, DECIDIR_ROBO.
        }

        private Carta RobarCartaDisponible()
        {
            // El servidor es el único que puede sacar cartas del mazo.
            return null;
        }

        private bool JugadaEstrategica(Carta robada)
        {
            using (Form ventana = new Form())
            {
                ventana.Width = 280;
                ventana.Height = 230;
                ventana.FormBorderStyle = FormBorderStyle.FixedDialog;
                ventana.Text = "Jugada estratégica";
                ventana.StartPosition = FormStartPosition.CenterParent;
                ventana.MinimizeBox = false;
                ventana.MaximizeBox = false;

                Label etiqueta = new Label
                {
                    Text = "¿Deseas jugar esta carta?",
                    Left = 20,
                    Top = 15,
                    Width = 230
                };

                PictureBox carta = ObtenerSpriteCarta(robada);
                carta.SizeMode = PictureBoxSizeMode.StretchImage;
                carta.Width = 70;
                carta.Height = 100;
                carta.Left = (ventana.ClientSize.Width - carta.Width) / 2;
                carta.Top = 45;

                Button si = new Button
                {
                    Text = "Sí",
                    Left = 50,
                    Top = 155,
                    Width = 70,
                    DialogResult = DialogResult.Yes,
                    BackColor = Color.Green,
                    ForeColor = Color.White
                };

                Button no = new Button
                {
                    Text = "No",
                    Left = 140,
                    Top = 155,
                    Width = 70,
                    DialogResult = DialogResult.No,
                    BackColor = Color.Firebrick,
                    ForeColor = Color.White
                };

                ventana.Controls.Add(etiqueta);
                ventana.Controls.Add(carta);
                ventana.Controls.Add(si);
                ventana.Controls.Add(no);
                ventana.AcceptButton = si;
                ventana.CancelButton = no;

                return ventana.ShowDialog(this) == DialogResult.Yes;
            }
        }


        // Ventana de selección de color trasladada de LogicaUNO a la interfaz.
        private ColorCarta SolicitarEleccionColor()
        {
            ColorCarta colorElegido = ColorCarta.ROJO;
            using (Form ventanaColor = new Form())
            {
                ventanaColor.Width = 350;
                ventanaColor.Height = 150;
                ventanaColor.FormBorderStyle = FormBorderStyle.FixedDialog;
                ventanaColor.Text = "Elige un color para el Comodín";
                ventanaColor.StartPosition = FormStartPosition.CenterParent;
                Label lbl = new Label() { Text = "¿Qué color deseas elegir?", Left = 20, Top = 20, Width = 250 };
                ventanaColor.Controls.Add(lbl);
                Button btnRojo = new Button() { Text = "Rojo", Left = 20, Top = 60, BackColor = Color.Red, ForeColor = Color.White };
                Button btnAmarillo = new Button() { Text = "Amarillo", Left = 90, Top = 60, BackColor = Color.Yellow };
                Button btnVerde = new Button() { Text = "Verde", Left = 160, Top = 60, BackColor = Color.Green, ForeColor = Color.White };
                Button btnAzul = new Button() { Text = "Azul", Left = 230, Top = 60, BackColor = Color.Blue, ForeColor = Color.White };
                btnRojo.Click += (s, e) => { colorElegido = ColorCarta.ROJO; ventanaColor.Close(); };
                btnAmarillo.Click += (s, e) => { colorElegido = ColorCarta.AMARILLO; ventanaColor.Close(); };
                btnVerde.Click += (s, e) => { colorElegido = ColorCarta.VERDE; ventanaColor.Close(); };
                btnAzul.Click += (s, e) => { colorElegido = ColorCarta.AZUL; ventanaColor.Close(); };
                ventanaColor.Controls.Add(btnRojo);
                ventanaColor.Controls.Add(btnAmarillo);
                ventanaColor.Controls.Add(btnVerde);
                ventanaColor.Controls.Add(btnAzul);
                ventanaColor.ShowDialog(this);
            }
            return colorElegido;
        }

        private PictureBox ObtenerSpriteCarta(Carta carta)
        {
            PictureBox pic_box = new PictureBox();
            Image imagen = Properties.Resources.back_uno;

            // COMODIN
            if (carta.getTipo() == TipoCarta.COMODIN)
                imagen = Properties.Resources.wildcard_uno;

            // +4
            else if (carta.getTipo() == TipoCarta.MASCUATRO)
                imagen = Properties.Resources.wildcard_plus4;

            else
            {
                // IDENTIFICACIÓN DE COLOR
                string color = "";

                switch (carta.getColor())
                {
                    case ColorCarta.ROJO:
                        color = "red";
                        break;
                    case ColorCarta.AMARILLO:
                        color = "yellow";
                        break;
                    case ColorCarta.VERDE:
                        color = "green";
                        break;
                    case ColorCarta.AZUL:
                        color = "blue";
                        break;
                }

                string nombre_recurso = null;

                // NUMÉRICAS
                if (carta.getTipo() == TipoCarta.NUMERO)
                    nombre_recurso = color + "_" + carta.getNumero();

                // BLOQUEA
                else if (carta.getTipo() == TipoCarta.BLOQUEA)
                    nombre_recurso = color + "_block";

                // REVERSA
                else if (carta.getTipo() == TipoCarta.REVERSA)
                    nombre_recurso = color + "_switch";

                // +2
                else if (carta.getTipo() == TipoCarta.MASDOS)
                    nombre_recurso = color + "_plus";

                if (!string.IsNullOrEmpty(nombre_recurso))
                {
                    Image recurso = Properties.Resources.ResourceManager.GetObject(nombre_recurso) as Image;
                    if (recurso != null)
                        imagen = recurso;
                }
            }
            pic_box.Image = (Image)imagen.Clone();
            pic_box.SizeMode = PictureBoxSizeMode.StretchImage;
            return pic_box;
        }

        public void ReiniciarJuego()
        {
            ReiniciarJuego(true);
        }

        private void ReiniciarJuego(bool mostrarMensaje)
        {
            // Solo el servidor puede iniciar una ronda para los tres jugadores.
            if (this.esperando_reinicio) return;
            this.esperando_reinicio = true;
            cliente.EnviarAccion("REINICIAR");
        }

        // Botón para reinicio de juego
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.partida_terminada)
            {
                ReiniciarJuego(true);
                return;
            }

            DialogResult respuesta = MessageBox.Show("¿Estás seguro de que deseas reiniciar la partida actual?", "Confirmar reinicio", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
                ReiniciarJuego(true);
        }

        private void ImprimirResultados(string nombre_ganador)
        {
            int jugador_con_mas_cartas = 1;
            int maximo_cartas = ContarCartas(1);

            if (ContarCartas(2) > maximo_cartas)
            {
                maximo_cartas = ContarCartas(2);
                jugador_con_mas_cartas = 2;
            }
            if (ContarCartas(3) > maximo_cartas)
                jugador_con_mas_cartas = 3;

            string nombre_castigado = ObtenerJugador(jugador_con_mas_cartas).GetNombre();
            Form3 ventanaResumen = new Form3(nombre_ganador, nombre_castigado);
            DialogResult resultado = ventanaResumen.ShowDialog(this);
            if (resultado == DialogResult.Retry) ReiniciarJuego(false);
            else Close();
        }

        private void panel_Superior_GroupBox4_Paint(object sender, PaintEventArgs e)
        {

        }
        //convierte una cadena recibida del servidor, como ROJO|NUMERO|5, en un objeto Carta compatible con tu juego.
        private Carta ConvertirCarta(string datos)
        {
            string[] partes = datos.Split('|');

            ColorCarta color = (ColorCarta)Enum.Parse(typeof(ColorCarta), partes[0]);

            TipoCarta tipo = (TipoCarta)Enum.Parse(typeof(TipoCarta), partes[1]);

            int numero = int.Parse(partes[2]);

            if (tipo == TipoCarta.NUMERO)
                return new Carta(color, tipo, numero);

            return new Carta(color, tipo);
        }

        //cuando el servidor envie MANO_INICIAL, se reemplazarán las cartas locales del jugador correspondiente por las siete cartas recibidas.
        private void ProcesarMensajeServidor(string mensaje)
        {
            string[] partes = mensaje.Split('|');
            switch (partes[0])
            {
                case "JUGADOR":
                    this.director.AsignarJugadorLocal(cliente.NumeroJugador);
                    ActualizarInterfaz();
                    break;

                case "JUGADORES":
                    if (partes.Length < 4) break;
                    this.director.RecibirNombresServidor(partes[1], partes[2], partes[3]);
                    this.groupBox1.Text = partes[1];
                    this.groupBox2.Text = partes[2];
                    this.groupBox3.Text = partes[3];
                    ActualizarInterfaz();
                    break;

                case "REINICIO_CONFIRMADO":
                    this.timerAviso.Stop();
                    this.lblAvisoTemp.Text = "";
                    this.jugador_pendiente_uno = -1;
                    this.partida_terminada = false;
                    this.partida_iniciada = false; //evita que se puedan jugar cartas mientras el servidor reparte una nueva mano
                    this.accion_pendiente = false;
                    this.esperando_reinicio = false;
                    this.director.ReiniciarRonda(this.director.jugador1.GetNombre(),this.director.jugador2.GetNombre(),this.director.jugador3.GetNombre());
                    ActualizarInterfaz();
                    break;

                case "MANO_INICIAL":
                    List<Carta> cartas = new List<Carta>();
                    if (cliente.ManoInicial != null)
                    {
                        foreach (string datos in cliente.ManoInicial)
                            if (!string.IsNullOrWhiteSpace(datos)) cartas.Add(ConvertirCarta(datos));
                    }
                    this.director.RecibirManoServidor(cliente.NumeroJugador, cartas);
                    this.partida_iniciada = true;
                    ActualizarInterfaz();
                    break;

                case "CARTA_MESA":
                    this.director.RecibirCartaMesaServidor(ConvertirCarta(cliente.CartaMesa));
                    ActualizarInterfaz();
                    break;

                case "TURNO":
                    this.director.ActualizarTurnoServidor(cliente.TurnoActual);
                    this.accion_pendiente = false;
                    ActualizarInterfaz();
                    break;

                case "SENTIDO":
                    if (partes.Length >= 2)
                        this.director.ActualizarSentidoServidor(int.Parse(partes[1]));
                    break;

                case "COLOR_ACTUAL":
                    if (partes.Length >= 2)
                        this.director.ActualizarColorServidor((ColorCarta)Enum.Parse(typeof(ColorCarta), partes[1]));
                    ActualizarInterfaz();
                    break;

                case "CANTIDADES":
                    if (partes.Length >= 4)
                        for (int i = 1; i <= 3; i++)
                            this.director.ActualizarCantidadCartasServidor(i, int.Parse(partes[i]));
                    ActualizarInterfaz();
                    break;

                case "CARTA_JUGADA":
                    if (partes.Length < 5) break;
                    Carta jugada = ConvertirCarta(partes[2] + "|" + partes[3] + "|" + partes[4]);
                    this.director.ConfirmarJugadaServidor(int.Parse(partes[1]), jugada);
                    this.accion_pendiente = false;
                    ActualizarInterfaz();
                    break;

                case "CARTA_ROBADA":
                    if (partes.Length < 4) break;
                    Carta robada = ConvertirCarta(partes[1] + "|" + partes[2] + "|" + partes[3]);
                    this.director.RecibirCartaRobadaServidor(cliente.NumeroJugador, robada);
                    ActualizarInterfaz();
                    break;

                case "DECIDIR_ROBO":
                    if (partes.Length < 4) break;
                    Carta cartaRobada = ConvertirCarta(partes[1] + "|" + partes[2] + "|" + partes[3]);
                    bool jugar = JugadaEstrategica(cartaRobada);
                    if (jugar)
                    {
                        string decision = "DECISION_ROBO|JUGAR";
                        if (cartaRobada.getTipo() == TipoCarta.COMODIN || cartaRobada.getTipo() == TipoCarta.MASCUATRO)
                            decision += "|" + SolicitarEleccionColor().ToString();
                        cliente.EnviarAccion(decision);
                    }
                    else cliente.EnviarAccion("DECISION_ROBO|CONSERVAR");
                    break;

                case "UNO_PENDIENTE":
                    if (partes.Length >= 2) this.jugador_pendiente_uno = int.Parse(partes[1]);
                    ActualizarInterfaz();
                    break;

                case "JUGADOR_UNO":
                    if (partes.Length < 2) break;
                    int numeroUNO = int.Parse(partes[1]);
                    this.director.GritarUNO(numeroUNO);
                    this.jugador_pendiente_uno = -1;
                    MostrarAvisoTemporal("¡" + ObtenerJugador(numeroUNO).GetNombre() + " gritó UNO!");
                    ActualizarInterfaz();
                    break;

                case "CASTIGO_UNO":
                    if (partes.Length >= 2)
                    {
                        this.jugador_pendiente_uno = -1;
                        MostrarAvisoTemporal("¡" + ObtenerJugador(int.Parse(partes[1])).GetNombre() + " olvidó decir UNO! Recibe 2 cartas.");
                    }
                    ActualizarInterfaz();
                    break;

                case "GANADOR":
                    if (partes.Length < 2) break;
                    this.director.RecibirGanadorServidor(int.Parse(partes[1]));
                    this.partida_terminada = true;
                    this.accion_pendiente = false;
                    this.jugador_pendiente_uno = -1;
                    ActualizarInterfaz();
                    // Se espera a que lleguen las cantidades finales antes de mostrar Form3.
                    ImprimirResultados(ObtenerNombreGanador());
                    break;

                case "ERROR":
                    this.accion_pendiente = false;
                    this.esperando_reinicio = false;
                    MostrarAvisoTemporal("Error del servidor: " + mensaje.Substring("ERROR|".Length));
                    break;

                case "JUGADOR_DESCONECTADO":
                    this.partida_terminada = true;
                    this.accion_pendiente = false;
                    MessageBox.Show("Se desconectó un jugador. La partida se ha detenido.");
                    break;

                case "SERVIDOR_LLENO":
                    this.partida_terminada = true;
                    MessageBox.Show("El servidor ya tiene tres jugadores.");
                    break;
            }
        }
    }
}