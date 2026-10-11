using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace UNO
{
    internal class apiService
    {
        private static readonly HttpClient client = new HttpClient();

      
        /// Realiza una petición HTTP GET de forma asíncrona a un endpoint determinado.
        public async Task<string> ObtenerDatosAsync(string endpointUrl)
        {
            try
            {
                // Hace la PETICION (REQUEST) GET de forma asíncrona (await) para evitar que la interfaz del juego (la ventana de resumend de partida) se congele mientras el servidor responde.
                HttpResponseMessage response = await client.GetAsync(endpointUrl);

                // Comprueba si la API respondió con éxito .
                if (response.IsSuccessStatusCode)
                {
                    // Lee el cuerpo de la respuesta del servidor como un texto plano (el JSON).
                    string jsonResultado = await response.Content.ReadAsStringAsync();
                    return jsonResultado;
                }
                else
                {
                    // Si el servidor responde con un código de error (como 404 o 500), se interrumpe y se lanza una excepción.
                    throw new Exception($"Error en la API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Captura fallos de conexión por ejemplo, si la API está apagada o no hay red
                throw new Exception($"No se pudo conectar: {ex.Message}");
            }
        }
    }
}