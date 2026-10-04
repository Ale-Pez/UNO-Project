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

        
        public async Task<string> ObtenerDatosAsync(string endpointUrl)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(endpointUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResultado = await response.Content.ReadAsStringAsync();
                    return jsonResultado;
                }
                else
                {
                    throw new Exception($"Error en la API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo conectar: {ex.Message}");
            }
        }
    }
}

