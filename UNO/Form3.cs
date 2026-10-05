using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UNO
{
    public partial class Form3 : Form
    {
        private string ganadorPartida;
        private string castigadoPartida;

        // Constructor que recibe únicamente al ganador y al castigado
        public Form3(string nombreGanador, string nombreCastigado)
        {
            InitializeComponent();
            ganadorPartida = nombreGanador;
            castigadoPartida = nombreCastigado;
        }

        private async void Form3_Load(object sender, EventArgs e)
        {
            await CargarResumenDesdeApiAsync();
        }

        private async Task CargarResumenDesdeApiAsync()
        {
            apiService servicioApi = new apiService();
            string urlEndpoint = "http://127.0.0.1:8000/jugadores";

            try
            {
                string jsonRespuesta = await servicioApi.ObtenerDatosAsync(urlEndpoint);

                if (string.IsNullOrEmpty(jsonRespuesta))
                {
                    throw new Exception("La API no devolvio datos.");
                }

                List<JugadorDto> listaJugadores = JsonConvert.DeserializeObject<List<JugadorDto>>(jsonRespuesta);

                // ORDENAMIENTO: Coloca al jugador con más partidas ganadas en primer lugar
                listaJugadores = listaJugadores.OrderByDescending(j => j.partidas_ganadas).ToList();

                string resumenFormateado = "========================================\r\n";

                // Resultados de la partida actual
                resumenFormateado += $"🏆 GANADOR DE LA PARTIDA:\r\n {ganadorPartida.ToUpper()}\r\n";
           
                resumenFormateado += "========================================\r\n";
                resumenFormateado += "Ranking Global de Ganadores:\r\n\r\n";

                // Lista de todos los jugadores ordenados por victorias
                foreach (var jugador in listaJugadores)
                {
                    resumenFormateado += $"  • {jugador.nombre} | Partidas ganadas:{jugador.partidas_ganadas}\r\n";
                }

                resumenFormateado += "\r\n========================================\r\n";

                label2.Text = resumenFormateado;
            }
            catch (Exception ex)
            {
                label2.Text = "ERROR: No se pudo conectar con la API.\r\n\r\n" +
                              $"Detalles técnicos:\r\n{ex.Message}";
            }
        }

        private void bttnReinicio_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry; 
            this.Close();
        }

        private void bttnSalir_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; 
            this.Close();
        }
    }

    public class JugadorDto
    {
        public int id_jugador { get; set; }
        public string nombre { get; set; }
        public int partidas_ganadas { get; set; }
        public int cartas_comidas { get; set; }
    }
}