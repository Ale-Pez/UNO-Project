
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UNO
{
    public partial class Registro : Form
    {
        // Propiedad que lee Form2 para extraer el nombre ingresado
        public string UsuarioRegistrado { get; private set; }

        // Cliente HTTP para comunicarse con la API
        private static readonly HttpClient client = new HttpClient();

        public Registro()
        {
            InitializeComponent();

            // Ventana fija de registro
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void boton_registrar_Click(object sender, EventArgs e)
        {
            string usuario = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(usuario))
            {
                MessageBox.Show(
                    "Por favor, escribe un nombre.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            UsuarioRegistrado = usuario;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Clase para interpretar la respuesta JSON de la API
        public class RespuestaRegistroDto
        {
            public int id_jugador { get; set; }
            public bool es_nuevo { get; set; }
        }

        private void Registro_Load(object sender, EventArgs e)
        {

        }
    }
}
