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

            // Mantiene la ventana de menú fija como me pediste antes
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nombreJugador1 = "";
            string nombreJugador2 = "";
            string nombreJugador3 = "";

            // --- JUGADOR 1 ---
            Regitsro reg1 = new Regitsro();
            reg1.Text = "Registro - Jugador 1";
            if (reg1.ShowDialog() == DialogResult.OK)
            {
                nombreJugador1 = reg1.UsuarioRegistrado;
            }
            else
            {
                return; // Si cancela, se detiene
            }

            // --- JUGADOR 2 ---
            Regitsro reg2 = new Regitsro();
            reg2.Text = "Registro - Jugador 2";
            if (reg2.ShowDialog() == DialogResult.OK)
            {
                nombreJugador2 = reg2.UsuarioRegistrado;

                // Validación por si repiten el mismo nombre en la misma partida
                if (nombreJugador2 == nombreJugador1)
                {
                    MessageBox.Show("Este jugador ya fue registrado. Ingresa un nombre diferente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                return;
            }

            // --- JUGADOR 3 ---
            Regitsro reg3 = new Regitsro();
            reg3.Text = "Registro - Jugador 3";
            if (reg3.ShowDialog() == DialogResult.OK)
            {
                nombreJugador3 = reg3.UsuarioRegistrado;

                if (nombreJugador3 == nombreJugador1 || nombreJugador3 == nombreJugador2)
                {
                    MessageBox.Show("Este jugador ya fue registrado. Ingresa un nombre diferente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                return;
            }

            // Se abre el tablero de juego (Form1) pasándole los 3 nombres limpios
            Form1 tableroJuego = new Form1(nombreJugador1, nombreJugador2, nombreJugador3);
            tableroJuego.Show();

            // Oculta el menú principal
            this.Hide();
        }
    }
}