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
        private List<Carta> cartas_en_pila;

        public Form1()
        {
            InitializeComponent();
            baraja = new Baraja();
            baraja.Barajar();
            cartas_en_pila = new List<Carta>();
            Repartir_Cartas();
        }

        private void Repartir_Cartas()
        {
            // Jugador 1
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();

                PictureBox reverso = Obtener_Imagen_Carta(carta);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 70;
                reverso.Height = 100;

                cartas_jugador1.Controls.Add(reverso);
            }

            // Jugador 2
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();

                PictureBox reverso = Obtener_Imagen_Carta(carta);;
                reverso.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 100;
                reverso.Height = 70;

                cartas_jugador2.Controls.Add(reverso);
            }

            // Jugador 3
            for (int i = 0; i < 7; i++)
            {
                Carta carta = baraja.Robar_Carta();

                PictureBox reverso = Obtener_Imagen_Carta(carta);
                reverso.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                reverso.SizeMode = PictureBoxSizeMode.StretchImage;
                reverso.Width = 100;
                reverso.Height = 70;

                cartas_jugador3.Controls.Add(reverso);
            }

            Carta inicial = baraja.Robar_Carta();

            pila_cartas.Image = Obtener_Imagen_Carta(inicial).Image;
            pila_cartas.SizeMode = PictureBoxSizeMode.StretchImage;
            pila_cartas.Width = 70;
            pila_cartas.Height = 100;

            cartas_en_pila.Add(inicial);
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
    }
}
