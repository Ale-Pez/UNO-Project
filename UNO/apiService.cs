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

      
        //realiza una peticion HTTP GET de forma asíncrona a un endpoint determinado.
        public async Task<string> ObtenerDatosAsync(string endpointUrl)
        {
            try
            {
                //hace la PETICION (REQUEST) GET de forma asíncrona (await) para evitar que la interfaz del juego (la ventana de resumend de partida) se congele mientras el servidor responde.
                HttpResponseMessage response = await client.GetAsync(endpointUrl);

                //comprueba si la API respondió con exito .
                if (response.IsSuccessStatusCode)
                {
                    //lee el cuerpo de la respuesta del servidor como un texto plano (el JSON).
                    string jsonResultado = await response.Content.ReadAsStringAsync();
                    return jsonResultado;
                }
                else
                {
                    //si el servidor responde con un codigo de error (como 404 o 500), se interrumpe y se lanza una excepción.
                    throw new Exception($"Error en la API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                //captura fallos de conexion por ejemplo, si la API está apagada o no hay red
                throw new Exception($"No se pudo conectar: {ex.Message}");
            }
        }
    }
}