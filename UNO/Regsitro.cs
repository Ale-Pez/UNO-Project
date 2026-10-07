using MySqlConnector;
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
    public partial class Regitsro : Form
    {
        //popiedad que lee Form2 para extraer el nombre ingresado
        public string UsuarioRegistrado { get; private set; }

        public Regitsro()
        {
            InitializeComponent();

            // Ventana fija de registro
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private async void boton_registrar_Click(object sender, EventArgs e)
        {
            string usuario = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(usuario))
            {
                MessageBox.Show("Por favor, escribe un nombre.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //llamar al meetodo que nos dice si es nuevo o ya existia
            bool esNuevo;
            int idJugador = ConexionBD.RegistrarOObtenerJugadorConEstado(usuario, out esNuevo);

            if (idJugador != -1)
            {
                //inicializa su historial por si es nuevo (si ya existia, la BD lo ignora con seguridad)
                ConexionBD.InicializarHistorial(idJugador);

                // Mensaje personalizado según el estado del usuario
                if (esNuevo)
                {
                    // Pausamos 1 segundo (1000 ms) para que el usuario alcance a leer el texto
                    label5.Text= "Te registramos como nuevo usuario!";
                    await Task.Delay(1000);
                }
                else
                {
                    label5.Text = "Bienvenido de nuevo!";
                    await Task.Delay(1000);
                }

                // Forzamos a que Windows dibuje el cambio en pantalla inmediatamente
                label5.Refresh();

                //guardamos el nombre y cerramos para continuar con el siguiente jugador
                UsuarioRegistrado = usuario;
                this.DialogResult = DialogResult.OK;
             
                this.Close();
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Regsitro_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}