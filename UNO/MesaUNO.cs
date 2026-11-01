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

        private CllienteJuego cliente;

        // Conserva al jugador que acaba de quedarse con una carta.
        // La oportunidad de cantar UNO termina cuando comienza la siguiente acción válida.
        private int jugador_pendiente_uno = -1;
        private bool partida_terminada = false;


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
            ActualizarInterfaz();

            //conexion al servidor
            cliente = new CllienteJuego();
            cliente.ConectarAlServidor("127.0.0.1", 11111, n1);
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
            this.mazo_cartas.Click += new EventHandler(RobarMazoCarta);

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
                if (anterior != null)
                    anterior.Dispose();

                this.pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
                this.pila_cartas.Width = 70;
                this.pila_cartas.Height = 100;
            }

            ActualizarIndicadorColor();
            ActualizarTurnoEtiqueta();
            this.boton_uno.Visible = this.jugador_pendiente_uno != -1 && !this.partida_terminada;
        }

        private void DibujarMano(List<Carta> mano, FlowLayoutPanel panel, int numeroJugador)
        {
            LimpiarPanelCartas(panel);

            foreach (Carta carta in mano)
            {
                PictureBox sprite = ObtenerSpriteCarta(carta);
                sprite.Tag = carta;
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

                sprite.Click += Carta_Click;
                panel.Controls.Add(sprite);
            }
        }

        private void LimpiarPanelCartas(FlowLayoutPanel panel)
        {
            foreach (Control control in panel.Controls)
            {
                PictureBox carta = control as PictureBox;
                if (carta != null && carta.Image != null)
                {
                    carta.Image.Dispose();
                    carta.Image = null;
                }

                control.Dispose();
            }

            panel.Controls.Clear();
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
            if (this.partida_terminada) return;

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

            if (this.director.cartas_en_pila.Count == 0) return;

            Carta carta_apilada = this.director.cartas_en_pila.Last();
            if (!this.director.JugadaValida(carta_usada, carta_apilada))
            {
                MessageBox.Show("¡Jugada inválida! La carta no coincide en color, número o tipo.", "Reglas del UNO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int jugador_que_jugo = this.director.turno_actual;
            //Enviar la carta jugada al servidor
            cliente.EnviarAccion(
                $"JUGAR_CARTA|{carta_usada.getColor()}|{carta_usada.getTipo()}|{carta_usada.getNumero()}"
                );
            CerrarVentanaUNO();
            this.director.RealizarJugada(carta_usada);
            TerminarAccion(jugador_que_jugo);
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
            string ganador = ObtenerNombreGanador();

            if (ganador != null)
            {
                this.partida_terminada = true;
                this.jugador_pendiente_uno = -1;
                ActualizarInterfaz();
                ImprimirResultados(ganador);
                return;
            }

            // Si la acción dejó al jugador con una sola carta, se habilita UNO.
            if (ContarCartas(jugador_que_jugo) == 1)
                jugador_pendiente_uno = jugador_que_jugo;
            else
                jugador_pendiente_uno = -1;

            ActualizarInterfaz();
        }

        private int ContarCartas(int numero_jugador)
        {
            if (numero_jugador == 1) return this.director.jugador1.ContarCartas();
            if (numero_jugador == 2) return this.director.jugador2.ContarCartas();
            return this.director.jugador3.ContarCartas();
        }

        private string ObtenerNombreGanador()
        {
            if (this.director.jugador1.ContarCartas() == 0) return this.director.jugador1.GetNombre();
            if (this.director.jugador2.ContarCartas() == 0) return this.director.jugador2.GetNombre();
            if (this.director.jugador3.ContarCartas() == 0) return this.director.jugador3.GetNombre();

            return null;
        }

        private void CerrarVentanaUNO()
        {
            this.jugador_pendiente_uno = -1;
            this.boton_uno.Visible = false;
        }

        private void boton_uno_Click(object sender, EventArgs e)
        {
            if (this.jugador_pendiente_uno == -1 || this.partida_terminada) return;

            //únicamente el jugador que tiene pendiente anunciar UNO podrá hacerlo.
            if (this.jugador_pendiente_uno != cliente.NumeroJugador)
                return;

            this.director.GritarUNO(this.jugador_pendiente_uno);

            cliente.EnviarAccion("UNO");
            string nombre = ObtenerJugador(this.jugador_pendiente_uno).GetNombre();

            this.jugador_pendiente_uno = -1;
            this.boton_uno.Visible = false;
            MostrarAvisoTemporal($"¡{nombre} gritó UNO!");
        }

        private Jugador ObtenerJugador(int numero_jugador)
        {
            if (numero_jugador == 1) return this.director.jugador1;
            if (numero_jugador == 2) return this.director.jugador2;
            return this.director.jugador3;
        }

        private void RobarMazoCarta(object sender, EventArgs e)
        {
            if (this.partida_terminada)
                return;

            //verifica que sea el turno de este jugador

            if (this.director.turno_actual != cliente.NumeroJugador)
            {
                MessageBox.Show("¡No es tu turno!");
                return;
            }

            int jugador_actual = this.director.turno_actual;
            cliente.EnviarAccion("ROBAR_CARTA");
            CerrarVentanaUNO();

            Carta robada = RobarCartaDisponible();
            if (robada == null)
            {
                MessageBox.Show("No quedan cartas disponibles en el mazo ni en la pila de descarte.", "Mazo vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ObtenerJugador(jugador_actual).RecibirCarta(robada);
            this.director.CancelarUNO(jugador_actual);
            this.director.RegistrarRoboCarta();

            bool es_valida = this.director.JugadaValida(robada, this.director.cartas_en_pila.Last());
            if (es_valida)
            {
                if (JugadaEstrategica(robada))
                {
                    this.director.RealizarJugada(robada);
                    TerminarAccion(jugador_actual);
                    return;
                }

                MostrarAvisoTemporal("Decidiste conservar la carta.");
            }
            else
            {
                string detalle = robada.getTipo() == TipoCarta.NUMERO ? $"Robaste una carta {robada.getColor()} {robada.getNumero()}. No puedes jugarla, así que pierdes el turno." : $"Robaste una carta {robada.getTipo()} de color {robada.getColor()}. No puedes jugarla, así que pierdes el turno.";
                MostrarAvisoTemporal(detalle);
            }

            this.director.PasarTurno();
            ActualizarInterfaz();
        }

        private Carta RobarCartaDisponible()
        {
            Carta carta = this.director.baraja.RobarCarta();
            if (carta != null) return carta;

            // Conserva la carta superior de la pila y devuelve el resto a la baraja.
            if (this.director.cartas_en_pila.Count <= 1) return null;

            Carta superior = this.director.cartas_en_pila.Last();

            for (int i = 0; i < this.director.cartas_en_pila.Count - 1; i++)
                this.director.baraja.RegresarCarta(this.director.cartas_en_pila[i]);

            this.director.cartas_en_pila.Clear();
            this.director.cartas_en_pila.Add(superior);
            this.director.baraja.Barajar();

            return this.director.baraja.RobarCarta();
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
            string nombre1 = this.director.jugador1.GetNombre();
            string nombre2 = this.director.jugador2.GetNombre();
            string nombre3 = this.director.jugador3.GetNombre();

            this.timerAviso.Stop();
            this.lblAvisoTemp.Text = "";
            this.lblAvisoTemp.Visible = false;

            this.jugador_pendiente_uno = -1;
            this.partida_terminada = false;

            this.director.ReiniciarRonda(nombre1, nombre2, nombre3);
            ActualizarInterfaz();

            if (mostrarMensaje)
                MessageBox.Show("Se ha iniciado una nueva partida.", "Reiniciar", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            int maximo_cartas = this.director.jugador1.ContarCartas();

            if (this.director.jugador2.ContarCartas() > maximo_cartas)
            {
                maximo_cartas = this.director.jugador2.ContarCartas();
                jugador_con_mas_cartas = 2;
            }

            if (this.director.jugador3.ContarCartas() > maximo_cartas)
                jugador_con_mas_cartas = 3;

            string nombre_castigado = ObtenerJugador(jugador_con_mas_cartas).GetNombre();
            Form3 ventanaResumen = new Form3(nombre_ganador, nombre_castigado);
            DialogResult resultado = ventanaResumen.ShowDialog(this);

            if (resultado == DialogResult.Retry)
                ReiniciarJuego(false);
            else
                Close();
        }

        private void panel_Superior_GroupBox4_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}