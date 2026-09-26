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
    public partial class Form1 : Form
    {
        private Baraja baraja;

        public Form1()
        {
            InitializeComponent();
            baraja = new Baraja();
            baraja.Barajar();
            Repartir_Cartas();
        }

        private void Repartir_Cartas()
        {
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();

                PictureBox reverso = new PictureBox();
                reverso.Image = Properties.Resources.back_uno;
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 80;
                reverso.Height = 120;

                cartas_jugador1.Controls.Add(reverso);
            }
        }
    }
}
