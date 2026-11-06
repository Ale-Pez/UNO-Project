
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UNO
{
    public partial class MesaUNO : Form
    {
        private Timer timerAviso;
        private ClienteJuego cliente;
        private List<Carta> manoLocal = new List<Carta>();
        private Carta cartaMesa;

        private string[] nombresJugadores = new string[3];
        private int[] cantidadesCartas = new int[3];

        private int numeroJugadorLocal = 0;
        private int turnoActual = 0;
        private int jugadorPendienteUNO = -1;
        private ColorCarta colorActual;

        private bool partidaIniciada = false;
        private bool partidaTerminada = false;
        private bool accionPendiente = false;
        private bool esperandoReinicio = false;

        public MesaUNO(string n1, string n2, string n3)
        {
            InitializeComponent();

            nombresJugadores[0] = n1;
            nombresJugadores[1] = n2;
            nombresJugadores[2] = n3;

            groupBox1.Text = n1;
            groupBox2.Text = n2;
            groupBox3.Text = n3;

            timerAviso = new Timer();
            timerAviso.Interval = 2500;
            timerAviso.Tick += timerAviso_Tick;

            cliente = new ClienteJuego();

            FormClosed += MesaUNO_FormClosed;

            IniciarControles();
            ActualizarInterfaz();

            cliente.MensajeRecibido += (mensaje) =>
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (!IsDisposed)
                            ProcesarMensajeServidor(mensaje);
                    }));
                }
            };

            var identificador = Handle;

            // El nombre enviado debe corresponder al jugador de esta computadora.
            cliente.ConectarAlServidor("127.0.0.1", 11111, n1);
        }

        private void MesaUNO_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerAviso.Stop();

            if (cliente != null)
                cliente.Desconectar();
        }

        private void IniciarControles()
        {
            toolTip1.SetToolTip(mazo_cartas, "Mazo de cartas");
            toolTip1.SetToolTip(pila_cartas, "Pila de cartas");

            boton_uno.Visible = false;

            lblAvisoTemp.Left = lblAvisoTemp.Width + 50;
            lblAvisoTemp.Top = lblAvisoTemp.Height + 100;

            mazo_cartas.Click -= RobarMazoCarta;
            mazo_cartas.Click += RobarMazoCarta;

            boton_uno.Click -= boton_uno_Click;
            boton_uno.Click += boton_uno_Click;

            boton_reinicio.Click -= button1_Click;
            boton_reinicio.Click += button1_Click;

            AcomodarControles();
        }

        private void AcomodarControles()
        {
            boton_reinicio.Left =
                (panel_Superior_GroupBox4.ClientSize.Width - boton_reinicio.Width) / 2;

            boton_reinicio.Top = 2;

            lblTurno.Left =
                (panel_Superior_GroupBox4.ClientSize.Width - lblTurno.Width) / 2;

            lblTurno.Top = boton_reinicio.Bottom + 2;

            boton_uno.Left =
                (panel_Inferior_GroupBox4.ClientSize.Width - boton_uno.Width) / 2;

            boton_uno.Top =
                (panel_Inferior_GroupBox4.ClientSize.Height - boton_uno.Height) / 2;
        }

        // ---------------------------------------------------------
        // ACTUALIZACIÓN DE LA INTERFAZ
        // ---------------------------------------------------------

        private void ActualizarInterfaz()
        {
            DibujarMano(manoLocal, cartas_jugador1, 1);
            DibujarMano(manoLocal, cartas_jugador2, 2);
            DibujarMano(manoLocal, cartas_jugador3, 3);

            if (cartaMesa != null)
            {
                PictureBox sprite = ObtenerSpriteCarta(cartaMesa);

                Image imagen = sprite.Image;
                sprite.Image = null;
                sprite.Dispose();

                Image anterior = pila_cartas.Image;
                pila_cartas.Image = imagen;

                if (anterior != null)
                    anterior.Dispose();

                pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
                pila_cartas.Width = 70;
                pila_cartas.Height = 100;
            }

            ActualizarIndicadorColor();
            ActualizarTurnoEtiqueta();

            boton_uno.Visible =
                partidaIniciada &&
                jugadorPendienteUNO == numeroJugadorLocal &&
                !partidaTerminada;
        }

        private void DibujarMano(List<Carta> mano, FlowLayoutPanel panel, int numeroJugador)
        {
            LimpiarPanelCartas(panel);

            bool esMiMano = numeroJugador == numeroJugadorLocal;

            int cantidad = esMiMano
                ? mano.Count
                : cantidadesCartas[numeroJugador - 1];

            for (int i = 0; i < cantidad; i++)
            {
                PictureBox sprite;

                if (esMiMano)
                {
                    if (i >= mano.Count)
                        break;

                    sprite = ObtenerSpriteCarta(mano[i]);
                    sprite.Tag = mano[i];
                    sprite.Click += Carta_Click;
                }
                else
                {
                    sprite = new PictureBox();
                    sprite.Image = (Image)Properties.Resources.back_uno.Clone();
                    sprite.Tag = null;
                }

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
                        RotateFlipType rotacion =
                            numeroJugador == 2
                            ? RotateFlipType.Rotate270FlipNone
                            : RotateFlipType.Rotate90FlipNone;

                        sprite.Image.RotateFlip(rotacion);
                    }
                }

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
            Image nuevoIcono = null;

            switch (colorActual)
            {
                case ColorCarta.ROJO:
                    nuevoIcono = Properties.Resources.red;
                    break;

                case ColorCarta.AZUL:
                    nuevoIcono = Properties.Resources.blue;
                    break;

                case ColorCarta.AMARILLO:
                    nuevoIcono = Properties.Resources.yellow;
                    break;

                case ColorCarta.VERDE:
                    nuevoIcono = Properties.Resources.green;
                    break;
            }

            if (nuevoIcono != null)
            {
                Image anterior = icono_color_juego.Image;
                icono_color_juego.Image = (Image)nuevoIcono.Clone();

                if (anterior != null)
                    anterior.Dispose();
            }

            icono_color_juego.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void ActualizarTurnoEtiqueta()
        {
            if (!partidaIniciada || turnoActual < 1 || turnoActual > 3)
            {
                lblTurno.Text = "Esperando jugadores...";
                AcomodarControles();
                return;
            }

            lblTurno.Text = "Turno de: " + nombresJugadores[turnoActual - 1];

            AcomodarControles();
        }

        // ---------------------------------------------------------
        // AVISOS
        // ---------------------------------------------------------

        private void MostrarAvisoTemporal(string mensaje)
        {
            lblAvisoTemp.Text = mensaje;

            timerAviso.Stop();
            timerAviso.Start();
        }

        private void timerAviso_Tick(object sender, EventArgs e)
        {
            timerAviso.Stop();
            lblAvisoTemp.Text = "";
        }

        // ---------------------------------------------------------
        // ACCIONES DEL JUGADOR
        // ---------------------------------------------------------

        private void Carta_Click(object sender, EventArgs e)
        {
            if (!partidaIniciada)
            {
                MessageBox.Show("Esperando a los tres jugadores.");
                return;
            }

            if (partidaTerminada || accionPendiente)
                return;

            if (turnoActual != numeroJugadorLocal)
            {
                MessageBox.Show("¡No es tu turno!");
                return;
            }

            PictureBox picture = sender as PictureBox;

            if (picture == null)
                return;

            Carta cartaUsada = picture.Tag as Carta;

            if (cartaUsada == null)
                return;

            string mensaje =
                $"JUGAR_CARTA|{cartaUsada.getColor()}|" +
                $"{cartaUsada.getTipo()}|{cartaUsada.getNumero()}";

            // El color elegido se envía junto con la carta.
            if (cartaUsada.getTipo() == TipoCarta.COMODIN ||
                cartaUsada.getTipo() == TipoCarta.MASCUATRO)
            {
                ColorCarta colorElegido;

                if (!SolicitarEleccionColor(out colorElegido))
                    return;

                mensaje += "|" + colorElegido.ToString();
            }

            accionPendiente = true;
            cliente.EnviarAccion(mensaje);
        }

        private void RobarMazoCarta(object sender, EventArgs e)
        {
            if (!partidaIniciada)
            {
                MessageBox.Show("Esperando a los tres jugadores.");
                return;
            }

            if (partidaTerminada || accionPendiente)
                return;

            if (turnoActual != numeroJugadorLocal)
            {
                MessageBox.Show("¡No es tu turno!");
                return;
            }

            accionPendiente = true;
            cliente.EnviarAccion("ROBAR_CARTA");
        }

        private void boton_uno_Click(object sender, EventArgs e)
        {
            if (!partidaIniciada || partidaTerminada || accionPendiente)
                return;

            if (jugadorPendienteUNO != numeroJugadorLocal)
                return;

            cliente.EnviarAccion("UNO");
        }

        // ---------------------------------------------------------
        // VENTANAS DE DECISIÓN
        // ---------------------------------------------------------

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

        // Devuelve false si se cierra la ventana sin elegir un color.

        private bool SolicitarEleccionColor(out ColorCarta colorElegido)
        {
            ColorCarta colorSeleccionado = ColorCarta.ROJO;

            using (Form ventanaColor = new Form())
            {
                ventanaColor.Width = 350;
                ventanaColor.Height = 150;
                ventanaColor.FormBorderStyle = FormBorderStyle.FixedDialog;
                ventanaColor.Text = "Elige un color para el Comodín";
                ventanaColor.StartPosition = FormStartPosition.CenterParent;
                ventanaColor.MinimizeBox = false;
                ventanaColor.MaximizeBox = false;

                Label lbl = new Label
                {
                    Text = "¿Qué color deseas elegir?",
                    Left = 20,
                    Top = 20,
                    Width = 250
                };

                Button btnRojo = new Button
                {
                    Text = "Rojo",
                    Left = 20,
                    Top = 60,
                    BackColor = Color.Red,
                    ForeColor = Color.White
                };

                Button btnAmarillo = new Button
                {
                    Text = "Amarillo",
                    Left = 90,
                    Top = 60,
                    BackColor = Color.Yellow
                };

                Button btnVerde = new Button
                {
                    Text = "Verde",
                    Left = 160,
                    Top = 60,
                    BackColor = Color.Green,
                    ForeColor = Color.White
                };

                Button btnAzul = new Button
                {
                    Text = "Azul",
                    Left = 230,
                    Top = 60,
                    BackColor = Color.Blue,
                    ForeColor = Color.White
                };

                btnRojo.Click += (s, e) =>
                {
                    colorSeleccionado = ColorCarta.ROJO;
                    ventanaColor.DialogResult = DialogResult.OK;
                    ventanaColor.Close();
                };

                btnAmarillo.Click += (s, e) =>
                {
                    colorSeleccionado = ColorCarta.AMARILLO;
                    ventanaColor.DialogResult = DialogResult.OK;
                    ventanaColor.Close();
                };

                btnVerde.Click += (s, e) =>
                {
                    colorSeleccionado = ColorCarta.VERDE;
                    ventanaColor.DialogResult = DialogResult.OK;
                    ventanaColor.Close();
                };

                btnAzul.Click += (s, e) =>
                {
                    colorSeleccionado = ColorCarta.AZUL;
                    ventanaColor.DialogResult = DialogResult.OK;
                    ventanaColor.Close();
                };

                ventanaColor.Controls.Add(lbl);
                ventanaColor.Controls.Add(btnRojo);
                ventanaColor.Controls.Add(btnAmarillo);
                ventanaColor.Controls.Add(btnVerde);
                ventanaColor.Controls.Add(btnAzul);

                DialogResult resultado = ventanaColor.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    colorElegido = colorSeleccionado;
                    return true;
                }
            }

            colorElegido = ColorCarta.ROJO;
            return false;
        }


        // ---------------------------------------------------------
        // REPRESENTACIÓN VISUAL DE LAS CARTAS
        // ---------------------------------------------------------

        private PictureBox ObtenerSpriteCarta(Carta carta)
        {
            PictureBox picture = new PictureBox();
            Image imagen = Properties.Resources.back_uno;

            if (carta.getTipo() == TipoCarta.COMODIN)
            {
                imagen = Properties.Resources.wildcard_uno;
            }
            else if (carta.getTipo() == TipoCarta.MASCUATRO)
            {
                imagen = Properties.Resources.wildcard_plus4;
            }
            else
            {
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

                string nombreRecurso = null;

                if (carta.getTipo() == TipoCarta.NUMERO)
                    nombreRecurso = color + "_" + carta.getNumero();

                else if (carta.getTipo() == TipoCarta.BLOQUEA)
                    nombreRecurso = color + "_block";

                else if (carta.getTipo() == TipoCarta.REVERSA)
                    nombreRecurso = color + "_switch";

                else if (carta.getTipo() == TipoCarta.MASDOS)
                    nombreRecurso = color + "_plus";

                if (!string.IsNullOrEmpty(nombreRecurso))
                {
                    Image recurso =
                        Properties.Resources.ResourceManager.GetObject(nombreRecurso) as Image;

                    if (recurso != null)
                        imagen = recurso;
                }
            }

            picture.Image = (Image)imagen.Clone();
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            return picture;
        }

        // ---------------------------------------------------------
        // REINICIO
        // ---------------------------------------------------------

        public void ReiniciarJuego()
        {
            ReiniciarJuego(true);
        }

        private void ReiniciarJuego(bool mostrarMensaje)
        {
            if (esperandoReinicio)
                return;

            esperandoReinicio = true;
            cliente.EnviarAccion("REINICIAR");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (partidaTerminada)
            {
                ReiniciarJuego(true);
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Estás seguro de que deseas reiniciar la partida actual?",
                "Confirmar reinicio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
                ReiniciarJuego(true);
        }

        // ---------------------------------------------------------
        // RESULTADOS
        // ---------------------------------------------------------

        private void ImprimirResultados(string nombreGanador)
        {
            int jugadorConMasCartas = 1;
            int maximoCartas = cantidadesCartas[0];

            for (int i = 1; i < 3; i++)
            {
                if (cantidadesCartas[i] > maximoCartas)
                {
                    maximoCartas = cantidadesCartas[i];
                    jugadorConMasCartas = i + 1;
                }
            }

            string nombreCastigado = nombresJugadores[jugadorConMasCartas - 1];

            Form3 ventanaResumen = new Form3(nombreGanador, nombreCastigado);
            DialogResult resultado = ventanaResumen.ShowDialog(this);

            if (resultado == DialogResult.Retry)
                ReiniciarJuego(false);
            else
                Close();
        }

        // ---------------------------------------------------------
        // CONVERSIÓN DE MENSAJES
        // ---------------------------------------------------------

        private Carta ConvertirCarta(string datos)
        {
            string[] partes = datos.Split('|');

            if (partes.Length < 3)
                throw new FormatException("Datos de carta incompletos: " + datos);

            ColorCarta color =
                (ColorCarta)Enum.Parse(typeof(ColorCarta), partes[0]);

            TipoCarta tipo =
                (TipoCarta)Enum.Parse(typeof(TipoCarta), partes[1]);

            int numero = int.Parse(partes[2]);

            if (tipo == TipoCarta.NUMERO)
                return new Carta(color, tipo, numero);

            return new Carta(color, tipo);
        }

        private void ProcesarMensajeServidor(string mensaje)
        {
            string[] partes = mensaje.Split('|');

            try
            {
                switch (partes[0])
                {
                    case "JUGADOR":
                        numeroJugadorLocal = int.Parse(partes[1]);
                        ActualizarInterfaz();
                        break;

                    case "JUGADORES":
                        if (partes.Length < 4)
                            break;

                        nombresJugadores[0] = partes[1];
                        nombresJugadores[1] = partes[2];
                        nombresJugadores[2] = partes[3];

                        groupBox1.Text = partes[1];
                        groupBox2.Text = partes[2];
                        groupBox3.Text = partes[3];

                        ActualizarInterfaz();
                        break;

                    case "REINICIO_CONFIRMADO":
                        partidaIniciada = false;
                        partidaTerminada = false;
                        accionPendiente = false;
                        esperandoReinicio = false;

                        jugadorPendienteUNO = -1;
                        turnoActual = 0;
                        cartaMesa = null;
                        manoLocal.Clear();

                        for (int i = 0; i < 3; i++)
                            cantidadesCartas[i] = 0;

                        LimpiarPanelCartas(cartas_jugador1);
                        LimpiarPanelCartas(cartas_jugador2);
                        LimpiarPanelCartas(cartas_jugador3);

                        if (pila_cartas.Image != null)
                        {
                            pila_cartas.Image.Dispose();
                            pila_cartas.Image = null;
                        }

                        timerAviso.Stop();
                        lblAvisoTemp.Text = "";

                        ActualizarInterfaz();
                        break;

                    case "MANO_INICIAL":
                        manoLocal.Clear();

                        if (partes.Length >= 2)
                        {
                            string datosMano =
                                mensaje.Substring("MANO_INICIAL|".Length);

                            foreach (string datos in datosMano.Split(';'))
                            {
                                if (!string.IsNullOrWhiteSpace(datos))
                                    manoLocal.Add(ConvertirCarta(datos));
                            }
                        }

                        partidaIniciada = true;
                        cantidadesCartas[numeroJugadorLocal - 1] = manoLocal.Count;

                        ActualizarInterfaz();
                        break;

                    case "CARTA_MESA":
                        cartaMesa = ConvertirCarta(
                            mensaje.Substring("CARTA_MESA|".Length));

                        ActualizarInterfaz();
                        break;

                    case "TURNO":
                        turnoActual = int.Parse(partes[1]);
                        accionPendiente = false;

                        ActualizarInterfaz();
                        break;

                    case "COLOR_ACTUAL":
                        colorActual =
                            (ColorCarta)Enum.Parse(typeof(ColorCarta), partes[1]);

                        ActualizarInterfaz();
                        break;

                    case "CANTIDADES":
                        if (partes.Length < 4)
                            break;

                        for (int i = 0; i < 3; i++)
                            cantidadesCartas[i] = int.Parse(partes[i + 1]);

                        ActualizarInterfaz();
                        break;

                    case "CARTA_JUGADA":
                        if (partes.Length < 5)
                            break;

                        int jugadorQueJugo = int.Parse(partes[1]);

                        Carta jugada = ConvertirCarta(
                            partes[2] + "|" + partes[3] + "|" + partes[4]);

                        if (jugadorQueJugo == numeroJugadorLocal)
                        {
                            int indice = manoLocal.FindIndex(c =>
                                c.getColor() == jugada.getColor() &&
                                c.getTipo() == jugada.getTipo() &&
                                c.getNumero() == jugada.getNumero());

                            if (indice >= 0)
                                manoLocal.RemoveAt(indice);
                        }

                        cartaMesa = jugada;
                        accionPendiente = false;

                        ActualizarInterfaz();
                        break;

                    case "CARTA_ROBADA":
                        if (partes.Length < 4)
                            break;

                        Carta robada = ConvertirCarta(
                            partes[1] + "|" + partes[2] + "|" + partes[3]);

                        // La carta robada se envía únicamente al jugador correspondiente.
                        manoLocal.Add(robada);
                        cantidadesCartas[numeroJugadorLocal - 1] = manoLocal.Count;

                        ActualizarInterfaz();
                        break;

                    case "DECIDIR_ROBO":
                        if (partes.Length < 4)
                            break;

                        Carta cartaRobada = ConvertirCarta(
                            partes[1] + "|" + partes[2] + "|" + partes[3]);

                        bool jugar = JugadaEstrategica(cartaRobada);

                        if (jugar)
                        {
                            string decision = "DECISION_ROBO|JUGAR";

                            if (cartaRobada.getTipo() == TipoCarta.COMODIN ||
                                cartaRobada.getTipo() == TipoCarta.MASCUATRO)
                            {
                                ColorCarta colorElegido;

                                if (!SolicitarEleccionColor(out colorElegido))
                                {
                                    cliente.EnviarAccion("DECISION_ROBO|CONSERVAR");
                                    break;
                                }

                                decision += "|" + colorElegido.ToString();
                            }

                            cliente.EnviarAccion(decision);
                        }
                        else
                        {
                            cliente.EnviarAccion("DECISION_ROBO|CONSERVAR");
                        }

                        break;

                    case "UNO_PENDIENTE":
                        jugadorPendienteUNO = int.Parse(partes[1]);
                        ActualizarInterfaz();
                        break;

                    case "JUGADOR_UNO":
                        int numeroUNO = int.Parse(partes[1]);

                        jugadorPendienteUNO = -1;

                        MostrarAvisoTemporal(
                            "¡" + nombresJugadores[numeroUNO - 1] + " gritó UNO!");

                        ActualizarInterfaz();
                        break;

                    case "CASTIGO_UNO":
                        int jugadorCastigado = int.Parse(partes[1]);

                        jugadorPendienteUNO = -1;

                        MostrarAvisoTemporal(
                            "¡" + nombresJugadores[jugadorCastigado - 1] +
                            " olvidó decir UNO! Recibe 2 cartas.");

                        ActualizarInterfaz();
                        break;

                    case "GANADOR":
                        int numeroGanador = int.Parse(partes[1]);

                        partidaTerminada = true;
                        accionPendiente = false;
                        jugadorPendienteUNO = -1;

                        ActualizarInterfaz();

                        ImprimirResultados(
                            nombresJugadores[numeroGanador - 1]);

                        break;

                    case "ERROR":
                        accionPendiente = false;
                        esperandoReinicio = false;

                        MostrarAvisoTemporal(
                            "Error del servidor: " +
                            mensaje.Substring("ERROR|".Length));

                        break;

                    case "JUGADOR_DESCONECTADO":
                        partidaIniciada = false;
                        partidaTerminada = true;
                        accionPendiente = false;

                        MessageBox.Show(
                            "Se desconectó un jugador. La partida se ha detenido.");

                        break;

                    case "DESCONECTADO":
                        partidaIniciada = false;
                        partidaTerminada = true;
                        accionPendiente = false;

                        MessageBox.Show(
                            "Se perdió la conexión con el servidor.");

                        break;

                    case "SERVIDOR_LLENO":
                        partidaTerminada = true;

                        MessageBox.Show(
                            "El servidor ya tiene tres jugadores.");

                        break;
                }
            }
            catch (Exception ex)
            {
                accionPendiente = false;

                MessageBox.Show(
                    "No se pudo procesar el mensaje del servidor:\n" +
                    mensaje + "\n\n" + ex.Message,
                    "Error de comunicación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void panel_Superior_GroupBox4_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
