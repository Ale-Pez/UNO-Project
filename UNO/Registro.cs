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

        // Cliente HTTP estatio para comunicarse  con la API
        private static readonly HttpClient client = new HttpClient();

        public Registro()
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

            try
            {
                // Preparamos los datos en formato JSON para mandárselos a la API
                var datosJugador = new { nombre = usuario };
                string jsonPayload = JsonConvert.SerializeObject(datosJugador);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Hacemos la petición POST a la API de FastAPI
                string urlApi = "http://127.0.0.1:8000/jugadores/registrar";
                HttpResponseMessage response = await client.PostAsync(urlApi, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonRespuesta = await response.Content.ReadAsStringAsync();

                    // Deserializamos la respuesta de la API que nos regresa el ID y si es nuevo
                    var resultadoApi = JsonConvert.DeserializeObject<RespuestaRegistroDto>(jsonRespuesta);

                    int idJugador = resultadoApi.id_jugador;
                    bool esNuevo = resultadoApi.es_nuevo;

                    if (idJugador != -1)
                    {
                        // Mensaje personalizado según el estado del usuario
                        if (esNuevo)
                        {
                            label5.Text = "Te registramos como nuevo usuario!";
                            await Task.Delay(1000);
                        }
                        else
                        {
                            label5.Text = "Bienvenido de nuevo!";
                            await Task.Delay(1000);
                        }

                        // Forzamos a que Windows dibuje el cambio en pantalla inmediatamente
                        label5.Refresh();

                        // Guardamos el nombre y cerramos para continuar con el siguiente jugador
                        UsuarioRegistrado = usuario;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Error al conectar con la API de registro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar con la API: " + ex.Message, "Error de red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // clase para leer la respuesta JSON que nos mandar la API
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