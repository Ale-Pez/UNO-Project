
using System;
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

        // MODIFICACIÓN:
        // Dirección de la única API para las tres computadoras
        private static readonly string url_api = "http://10.103.150.34:8000";

        private int id_partida_actual;
        private int id_jugador1;
        private int id_jugador2;
        private int id_jugador3;

        public LlamadaBD(string nickname1, string nickname2, string nickname3)
        {
            bool nuevo1, nuevo2, nuevo3;

            this.id_partida_actual = -1;

            // Registrar o reconocer a los tres jugadores
            this.id_jugador1 = RegistrarJugadorApi(nickname1, out nuevo1);
            this.id_jugador2 = RegistrarJugadorApi(nickname2, out nuevo2);
            this.id_jugador3 = RegistrarJugadorApi(nickname3, out nuevo3);

            // MODIFICACIÓN:
            // La partida no se crea automáticamente aquí.
            // Su ID debe ser compartido por el servidor UNO.
        }

        // MODIFICACIÓN:
        // Guardar el ID de la partida creada para los tres jugadores
        public void AsignarPartida(int id_partida)
        {
            if (id_partida > 0)
                this.id_partida_actual = id_partida;
        }

        // Crear una nueva partida mediante la API
        // Este método debe ejecutarse una sola vez por partida
        public async Task CrearPartidaApi()
        {
            this.id_partida_actual = -1;

            try
            {
                string url = url_api + "/partidas/crear";

                HttpResponseMessage respuesta =
                    await cliente_http_api.PostAsync(url, null);

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = await respuesta.Content.ReadAsStringAsync();

                    var res = JsonConvert.DeserializeObject<FormatoPartida>(json);

                    if (res != null)
                        this.id_partida_actual = res.id_partida;
                }
                else
                {
                    Console.WriteLine(
                        $"Error al crear partida: {respuesta.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error de conexión con la API: {ex.Message}");
            }
        }

        // MODIFICACIÓN:
        // Consultar el ID para compartirlo con los clientes
        public int ObtenerIdPartida()
        {
            return this.id_partida_actual;
        }

        // Registrar un jugador o recuperar su ID
        private int RegistrarJugadorApi(string nombre, out bool es_nuevo)
        {
            es_nuevo = false;

            try
            {
                string url = url_api + "/jugadores/registrar";

                var datos = new { nombre = nombre };
                string payload = JsonConvert.SerializeObject(datos);

                var contenido = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                HttpResponseMessage respuesta =
                    cliente_http_api.PostAsync(url, contenido)
                    .GetAwaiter().GetResult();

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = respuesta.Content.ReadAsStringAsync()
                        .GetAwaiter().GetResult();

                    var res =
                        JsonConvert.DeserializeObject<RespuestaRegistroDto>(json);

                    if (res != null)
                    {
                        es_nuevo = res.es_nuevo;
                        return res.id_jugador;
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"Error al registrar jugador: {respuesta.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error al registrar jugador: {ex.Message}");
            }

            return -1;
        }

        // Actualizar las victorias y derrotas de un jugador
        public async Task ActualizarHistorialApi(int numero_jugador, bool gano)
        {
            try
            {
                int jugador;

                if (numero_jugador == 1)
                    jugador = id_jugador1;
                else if (numero_jugador == 2)
                    jugador = id_jugador2;
                else if (numero_jugador == 3)
                    jugador = id_jugador3;
                else
                    return;

                if (jugador == -1)
                    return;

                string url = url_api + "/historial/actualizar";

                var datos = new { id_jugador = jugador, gano = gano };
                string payload = JsonConvert.SerializeObject(datos);

                var contenido = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                HttpResponseMessage respuesta =
                    await cliente_http_api.PostAsync(url, contenido);

                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Error al actualizar historial: {respuesta.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error al actualizar historial: {ex.Message}");
            }
        }

        // Finalizar la partida y guardar al ganador
        public async Task FinalizarPartidaApi(string ganador)
        {
            try
            {
                if (this.id_partida_actual == -1)
                    return;

                string url = url_api + "/partidas/finalizar";

                var datos = new
                {
                    id_partida = this.id_partida_actual,
                    ganador = ganador
                };

                string payload = JsonConvert.SerializeObject(datos);

                var contenido = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                HttpResponseMessage respuesta =
                    await cliente_http_api.PostAsync(url, contenido);

                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Error al finalizar partida: {respuesta.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error al finalizar partida: {ex.Message}");
            }
        }

        // Guardar los movimientos realizados durante la partida
        public async Task GuardarLogApi(int numero_jugador, string movimiento)
        {
            try
            {
                int jugador;

                if (numero_jugador == 1)
                    jugador = id_jugador1;
                else if (numero_jugador == 2)
                    jugador = id_jugador2;
                else if (numero_jugador == 3)
                    jugador = id_jugador3;
                else
                    return;

                if (jugador == -1 || this.id_partida_actual == -1)
                    return;

                string url = url_api + "/logs";

                var datos = new
                {
                    id_partida = this.id_partida_actual,
                    id_jugador = jugador,
                    movimiento = movimiento
                };

                string payload = JsonConvert.SerializeObject(datos);

                var contenido = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                HttpResponseMessage respuesta =
                    await cliente_http_api.PostAsync(url, contenido);

                if (!respuesta.IsSuccessStatusCode)
                {
                    Console.WriteLine(
                        $"Error al guardar movimiento: {respuesta.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error al guardar movimiento: {ex.Message}");
            }
        }

        // Clase para interpretar la respuesta JSON de la API
        public class FormatoPartida
        {
            public int id_partida { get; set; }
        }
    }
}
