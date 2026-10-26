using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static UNO.Registro;

namespace UNO
{
    internal class LlamadaBD
    {
        private static readonly HttpClient cliente_http_api = new HttpClient();

        private int id_partida_actual;
        private int id_jugador1;
        private int id_jugador2;
        private int id_jugador3;

        public LlamadaBD(string nickname1, string nickname2, string nickname3)
        {
            bool nuevo1, nuevo2, nuevo3;

            // Registrar partida en la base de datos.
            CrearPartidaApi();

            // Registrar o reconocer a los jugadores de la base de datos.
            this.id_jugador1 = RegistrarJugadorApi(nickname1, out nuevo1);
            this.id_jugador2 = RegistrarJugadorApi(nickname1, out nuevo2);
            this.id_jugador3 = RegistrarJugadorApi(nickname1, out nuevo3);
        }

        // MÉTODOS QUE UTILIZA LA API PARA GUARDAR LOS DATOS EN LA BD:
        private void CrearPartidaApi()
        {
            try
            {
                string url = "http://127.0.0.1:8000/partidas/crear";
                var respuesta = Task.Run(() => cliente_http_api.PostAsync(url, null)).Result;
                if (respuesta.IsSuccessStatusCode)
                {
                    string json = Task.Run(() => respuesta.Content.ReadAsStringAsync()).Result;
                    var res = JsonConvert.DeserializeObject<FormatoPartida>(json);
                    this.id_partida_actual = res.id_partida;
                }
            }
            catch { }
            this.id_partida_actual = -1;
        }

        private int RegistrarJugadorApi(string nombre, out bool es_nuevo)
        {
            es_nuevo = false;
            try
            {
                string url = "http://127.0.0.1:8000/jugadores/registrar";
                var datos = new { nombre = nombre };
                string payload = JsonConvert.SerializeObject(datos);
                var contenido = new StringContent(payload, Encoding.UTF8, "application/json");

                var respuesta = Task.Run(() => cliente_http_api.PostAsync(url, contenido)).Result;
                if (respuesta.IsSuccessStatusCode)
                {
                    string json = Task.Run(() => respuesta.Content.ReadAsStringAsync()).Result;
                    var res = JsonConvert.DeserializeObject<RespuestaRegistroDto>(json);
                    es_nuevo = res.es_nuevo;
                    return res.id_jugador;
                }
            }
            catch { }
            return -1;
        }

        public void ActualizarHistorialApi(int numero_jugador, bool gano)
        {
            try
            {
                int jugador;
                if (numero_jugador == 1) jugador = id_jugador1;
                else if (numero_jugador == 1) jugador = id_jugador2;
                else jugador = id_jugador3;

                string url = "http://127.0.0.1:8000/historial/actualizar";
                var datos = new { id_jugador = jugador, gano = gano };
                string payload = JsonConvert.SerializeObject(datos);
                var contenido = new StringContent(payload, Encoding.UTF8, "application/json");

                Task.Run(() => cliente_http_api.PostAsync(url, contenido));
            }
            catch { }
        }

        public void FinalizarPartidaApi(string ganador)
        {
            try
            {
                string url = "http://127.0.0.1:8000/partidas/finalizar";
                var datos = new { id_partida = this.id_partida_actual, ganador = ganador };
                string payload = JsonConvert.SerializeObject(datos);
                var contenido = new StringContent(payload, Encoding.UTF8, "application/json");

                Task.Run(() => cliente_http_api.PostAsync(url, contenido));
            }
            catch { }
        }

        public void GuardarLogApi(int numero_jugador, string movimiento)
        {
            try
            {
                int jugador;
                if (numero_jugador == 1) jugador = id_jugador1;
                else if (numero_jugador == 1) jugador = id_jugador2;
                else jugador = id_jugador3;

                string url = "http://127.0.0.1:8000/logs";
                var datos = new { id_partida = this.id_partida_actual, id_jugador = jugador, movimiento = movimiento };
                string payload = JsonConvert.SerializeObject(datos);
                var contenido = new StringContent(payload, Encoding.UTF8, "application/json");

                Task.Run(() => cliente_http_api.PostAsync(url, contenido));
            }
            catch { }
        }

        public class FormatoPartida  // Un molde que le dice a C# como leer la respuesta en formato JSON que nos manda la API cuando se crea una nueva partida 
        {
            public int id_partida { get; set; }
        }
    }
}
