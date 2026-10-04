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
        private int cartasComidasPartida;

        //datos de la partida actual que concluy
        public Form3(string nombreGanador, string nombreCastigado, int totalCartasComidas)
        {
            InitializeComponent();
            ganadorPartida = nombreGanador;
            castigadoPartida = nombreCastigado;
            cartasComidasPartida = totalCartasComidas;
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

                // Si la API no responde o regresa vacio,lanza error
                if (string.IsNullOrEmpty(jsonRespuesta))
                {
                    throw new Exception("La API no devolvió datos.");
                }

                List<JugadorDto> listaJugadores = JsonConvert.DeserializeObject<List<JugadorDto>>(jsonRespuesta);

                string resumenFormateado = "========================================\r\n";
              
                // Datos de la partida actual
                resumenFormateado += $"🏆 ¡GRAN GANADOR: {ganadorPartida.ToUpper()}! 🏆\r\n\r\n";
                resumenFormateado += $"🃏 El castigado de esta ronda: {castigadoPartida}\r\n";
                resumenFormateado += $"   Cartas acumuladas hoy: {cartasComidasPartida}\r\n\r\n";

                resumenFormateado += "----------------------------------------\r\n";
                resumenFormateado += "Estadísticas globales:\r\n\r\n";

                foreach (var jugador in listaJugadores)
                {
                    resumenFormateado += $"  • {jugador.nombre} | Wins: {jugador.partidas_ganadas} | Historial comidas: {jugador.cartas_comidas}\r\n";
                }

                resumenFormateado += "\r\n========================================\r\n";
                resumenFormateado += "¡Gracias por jugar UNO!";

                label2.Text = resumenFormateado;
            }
            catch (Exception ex)
            {
                // Si falla la API, mostramos explícitamente el error de conexión para la evaluación
                label2.Text = "❌ ERROR: No se pudo conectar con la API.\r\n\r\n" +
                              "Verifique que el servidor FastAPI esté encendido.\r\n\r\n" +
                              $"Detalles técnicos:\r\n{ex.Message}";
            }
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