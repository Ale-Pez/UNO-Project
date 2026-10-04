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
        public Form3()
        {
            InitializeComponent();
        }



        // 1. El evento cuando abre la ventana
        private async void Form3_Load(object sender, EventArgs e)
        {
            await CargarYTransformarResumenAsync();
        }

        // 2. El método que hace la petición a la API y da formato al texto
        private async Task CargarYTransformarResumenAsync()
        {
            apiService servicioApi = new apiService();
            string urlEndpoint = "http://127.0.0.1:8000/jugadores";

            try
            {
                string jsonRespuesta = await servicioApi.ObtenerDatosAsync(urlEndpoint);
                List<JugadorDto> listaJugadores = JsonConvert.DeserializeObject<List<JugadorDto>>(jsonRespuesta);

                // Buscamos al ganador (el que tenga más partidas ganadas) y al que comió más cartas
                var ganador = listaJugadores.OrderByDescending(j => j.partidas_ganadas).FirstOrDefault();
                var masCartas = listaJugadores.OrderByDescending(j => j.cartas_comidas).FirstOrDefault();

                string resumenFormateado = "========================================\r\n";
                
                if (ganador != null)
                {
                    resumenFormateado += $"🏆 ¡GRAN GANADOR: {ganador.nombre.ToUpper()}! 🏆\r\n";
                    resumenFormateado += $"   Partidas ganadas: {ganador.partidas_ganadas}\r\n\r\n";
                }

                if (masCartas != null)
                {
                    resumenFormateado += $"🃏 El castigado (comió más cartas): {masCartas.nombre}\r\n";
                    resumenFormateado += $"   Total de cartas acumuladas: {masCartas.cartas_comidas}\r\n\r\n";
                }

                resumenFormateado += "\r\n========================================\r\n";
                resumenFormateado += "Estadísticas de todos los jugadores:\r\n\r\n";

                foreach (var jugador in listaJugadores)
                {
                    resumenFormateado += $"  • {jugador.nombre} | Wins: {jugador.partidas_ganadas} | Cartas comidas: {jugador.cartas_comidas}\r\n";
                }

                resumenFormateado += "\r\n========================================\r\n";
                resumenFormateado += "¡Gracias por jugar UNO!";

                label2.Text = resumenFormateado;
            }
            catch (Exception ex)
            {
                label2.Text = "Error al cargar el resumen de la partida:\r\n" + ex.Message;
            }
        }
    }
    public class JugadorDto
    {
        public int id_jugador { get; set; }
        public string nombre { get; set; }
        public int partidas_ganadas { get; set; }
        public int cartas_comidas { get; set; } // O cartas robadas del mazo
    }


}