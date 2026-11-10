
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UNO
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            //mantiene la ventana de menu fija
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nombreJugador1 = "";

            //registro del jugador de esta computadora
            Registro reg1 = new Registro();
            reg1.Text = "Registro de jugador";

            if (reg1.ShowDialog() == DialogResult.OK)
            {
                nombreJugador1 = reg1.UsuarioRegistrado;
            }
            else
            {
                return;
            }

            // Abre el tablero con el jugador registrado
            // Los nombres de los demás jugadores llegarán del servidor
            MesaUNO tableroJuego = new MesaUNO(nombreJugador1, "", "");

            // MODIFICACIÓN:
            // Cuando se cierre el tablero, vuelve a mostrar el menú
            tableroJuego.FormClosed += (s, args) =>
            {
                this.Show();
            };

            tableroJuego.Show();

            //oculta el menu principal
            this.Hide();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void boton_historial_Click(object sender, EventArgs e)
        {
            HistorialPartidas ventanaResumen = new HistorialPartidas("", "");
            DialogResult resultado = ventanaResumen.ShowDialog(this);
        }
    }
}
