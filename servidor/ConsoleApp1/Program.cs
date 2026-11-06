using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Text.RegularExpressions;
using UNO;

namespace ServidorUNO
{
    class Program
    {
        static Socket servidor;
        static List<Socket> clientes = new List<Socket>();
        static List<string> nombres = new List<string>();
        static object bloqueo = new object();
        static LogicaUNO logica;

        static bool partidaIniciada = false;
        static bool partidaTerminada = false;

        // Una sola API y una sola partida para las tres computadoras.
        static readonly HttpClient clienteApi = new HttpClient();
        static readonly string urlApi = "http://127.0.0.1:8000";
        static int idPartidaActual = -1;
        static int[] idJugadores = new int[3];

        // Estas llamadas se realizan exclusivamente desde el servidor.
        static string PostApi(string ruta, string json)
        {
            try
            {
                using (var contenido = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    var respuesta = clienteApi.PostAsync(urlApi + ruta, contenido)
                        .GetAwaiter().GetResult();
                    string texto = respuesta.Content.ReadAsStringAsync()
                        .GetAwaiter().GetResult();
                    if (!respuesta.IsSuccessStatusCode)
                    {
                        Console.WriteLine("API " + ruta + ": " + respuesta.StatusCode + " " + texto);
                        return "";
                    }
                    return texto;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo consultar la API: " + ex.Message);
                return "";
            }
        }

        static int LeerId(string json, string campo)
        {
            Match encontrado = Regex.Match(json, "\"" + campo + "\"\\s*:\\s*(\\d+)");
            int id;
            return encontrado.Success && int.TryParse(encontrado.Groups[1].Value, out id) ? id : -1;
        }

        static string JsonTexto(string texto)
        {
            // Escapar cadenas para enviar nombres y movimientos como JSON válido.
            return "\"" + texto.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }

        static void CrearRegistroPartida()
        {
            idPartidaActual = -1;
            idJugadores = new int[3];

            for (int i = 0; i < 3; i++)
            {
                string respuesta = PostApi("/jugadores/registrar",
                    "{\"nombre\":" + JsonTexto(nombres[i]) + "}");
                idJugadores[i] = LeerId(respuesta, "id_jugador");
            }
            idPartidaActual = LeerId(PostApi("/partidas/crear", "{}"), "id_partida");
            if (idPartidaActual > 0)
            {
                EnviarATodos("ID_PARTIDA|" + idPartidaActual);
                Console.WriteLine("Partida registrada en MySQL: " + idPartidaActual);
            }
        }

        static void GuardarLog(int numeroJugador, string movimiento)
        {
            if (idPartidaActual <= 0 || numeroJugador < 1 || numeroJugador > 3 ||
                idJugadores[numeroJugador - 1] <= 0) return;
            PostApi("/logs", "{\"id_partida\":" + idPartidaActual +
                ",\"id_jugador\":" + idJugadores[numeroJugador - 1] +
                ",\"movimiento\":" + JsonTexto(movimiento) + "}");
        }

        static void RegistrarGanador(int ganador)
        {
            if (idPartidaActual <= 0) return;
            for (int i = 0; i < 3; i++)
            {
                if (idJugadores[i] <= 0) continue;
                PostApi("/historial/actualizar", "{\"id_jugador\":" + idJugadores[i] +
                    ",\"gano\":" + (i + 1 == ganador ? "true" : "false") + "}");
            }
            PostApi("/partidas/finalizar", "{\"id_partida\":" + idPartidaActual +
                ",\"ganador\":" + JsonTexto(nombres[ganador - 1]) + "}");
        }

        // Convierte COLOR|TIPO|NUMERO en un objeto Carta.
        static Carta TextoACarta(string texto)
        {
            string[] partes = texto.Split('|');

            if (partes.Length != 3) return null;

            int numero;

            if (!int.TryParse(partes[2], out numero))
                return null;

            Carta carta = new Carta(partes[0], partes[1]);
            carta.setNumero(numero);
            return carta;
        }

        // Convierte un objeto Carta en COLOR|TIPO|NUMERO.
        static string CartaATexto(Carta carta)
        {
            if (carta == null)
                return "";

            return carta.getColor().ToString() + "|" +
                carta.getTipo().ToString() + "|" +
                carta.getNumero();
        }

        static bool ObtenerColorElegido(string texto, out ColorCarta color)
        {
            color = ColorCarta.ESPECIAL;

            if (string.IsNullOrWhiteSpace(texto))
                return false;

            if (!Enum.TryParse<ColorCarta>(texto, true, out color))
                return false;

            return color == ColorCarta.ROJO || color == ColorCarta.AMARILLO ||
                color == ColorCarta.VERDE || color == ColorCarta.AZUL;
        }

        static void EnviarCantidades()
        {
            if (logica == null)
                return;

            EnviarATodos("CANTIDADES|" + logica.ObtenerCantidadCartas(1) + "|" +
                logica.ObtenerCantidadCartas(2) + "|" + logica.ObtenerCantidadCartas(3));
        }

        static string ObtenerTextoMano(int numeroJugador)
        {
            List<Carta> mano = logica.ObtenerMano(numeroJugador);
            List<string> cartas = new List<string>();

            foreach (Carta carta in mano)
                cartas.Add(CartaATexto(carta));

            return string.Join(";", cartas);
        }

        static void EnviarManos()
        {
            EnviarJugador(1, "MANO_INICIAL|" + ObtenerTextoMano(1));
            EnviarJugador(2, "MANO_INICIAL|" + ObtenerTextoMano(2));
            EnviarJugador(3, "MANO_INICIAL|" + ObtenerTextoMano(3));
        }

        static void EnviarEstado()
        {
            if (logica == null)
                return;

            EnviarATodos("CARTA_MESA|" + CartaATexto(logica.ObtenerCartaSuperior()));
            EnviarATodos("COLOR_ACTUAL|" + logica.ObtenerColorActual());
            EnviarATodos("SENTIDO|" + logica.ObtenerSentidoJuego());
            EnviarCantidades();
            EnviarATodos("TURNO|" + logica.ObtenerTurnoActual());
        }

        static void IniciarPartida()
        {
            logica = new LogicaUNO(nombres[0], nombres[1], nombres[2]);

            partidaTerminada = false;
            partidaIniciada = true;

            Console.WriteLine("La partida ha comenzado.");
            Console.WriteLine("Carta inicial: " + CartaATexto(logica.ObtenerCartaSuperior()));
            Console.WriteLine("Turno del jugador " + logica.ObtenerTurnoActual());

            EnviarATodos("JUGADORES|" + nombres[0] + "|" + nombres[1] + "|" + nombres[2]);
            EnviarATodos("REINICIO_CONFIRMADO");
            EnviarManos();
            EnviarEstado();
            CrearRegistroPartida();
        }

        static void NotificarJugada(int numeroJugador)
        {
            Carta carta = logica.ObtenerCartaSuperior();

            EnviarATodos("CARTA_JUGADA|" + numeroJugador + "|" + CartaATexto(carta));
            EnviarATodos("COLOR_ACTUAL|" + logica.ObtenerColorActual());
            EnviarCantidades();

            if (logica.PartidaTerminada())
            {
                int ganador = logica.ObtenerGanador();

                partidaTerminada = true;

                EnviarATodos("GANADOR|" + ganador);

                RegistrarGanador(ganador);
                return;
            }

            if (logica.ObtenerCantidadCartas(numeroJugador) == 1)
                EnviarATodos("UNO_PENDIENTE|" + numeroJugador);

            EnviarATodos("SENTIDO|" + logica.ObtenerSentidoJuego());
            EnviarATodos("TURNO|" + logica.ObtenerTurnoActual());
        }

        static void Main(string[] args)
        {
            IniciarServidor();
        }

        static void IniciarServidor()
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint direccion = new IPEndPoint(ip, 11111);
            servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            servidor.Bind(direccion);
            servidor.Listen(3);
            clienteApi.Timeout = TimeSpan.FromSeconds(4);
            Console.WriteLine("Servidor UNO iniciado. Esperando jugadores...");

            while (true)
            {
                Socket cliente = servidor.Accept();
                int numeroJugador;
                lock (bloqueo)
                {
                    if (clientes.Count >= 3 || partidaIniciada)
                    {
                        Enviar(cliente, "SERVIDOR_LLENO");
                        cliente.Close();
                        continue;
                    }
                    clientes.Add(cliente);
                    nombres.Add("");
                    numeroJugador = clientes.Count;
                }
                Console.WriteLine("Jugador " + numeroJugador + " conectado.");
                Enviar(cliente, "JUGADOR|" + numeroJugador);
                Thread hilo = new Thread(() => EscucharCliente(cliente, numeroJugador));
                hilo.IsBackground = true;
                hilo.Start();
            }
        }

        static void EscucharCliente(Socket cliente, int numeroJugador)
        {
            byte[] buffer = new byte[1024];
            StringBuilder pendiente = new StringBuilder();
            try
            {
                while (true)
                {
                    int recibidos = cliente.Receive(buffer);
                    if (recibidos == 0) break;
                    pendiente.Append(Encoding.UTF8.GetString(buffer, 0, recibidos));
                    string contenido = pendiente.ToString();
                    int posicion;
                    while ((posicion = contenido.IndexOf('\n')) >= 0)
                    {
                        string mensaje = contenido.Substring(0, posicion).TrimEnd('\r');
                        contenido = contenido.Substring(posicion + 1);
                        lock (bloqueo) ProcesarMensaje(mensaje, numeroJugador);
                    }
                    pendiente.Clear();
                    pendiente.Append(contenido);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Se perdió la conexión del jugador " +
                    numeroJugador);

                Console.WriteLine(ex.ToString());
            }
            finally
            {
                lock (bloqueo)
                {
                    if (numeroJugador - 1 < clientes.Count && clientes[numeroJugador - 1] == cliente)
                    {
                        clientes[numeroJugador - 1] = null;
                        partidaIniciada = false;
                        partidaTerminada = true;
                        EnviarATodos("JUGADOR_DESCONECTADO|" + numeroJugador);
                    }
                }
                cliente.Close();
                Console.WriteLine("Jugador " + numeroJugador + " desconectado.");
            }
        }

        static void ProcesarMensaje(string mensaje, int numeroJugador)
        {
            string[] partes = mensaje.Split('|');

            switch (partes[0])
            {
                case "CONECTAR":

                    if (partes.Length < 2 ||
                        string.IsNullOrWhiteSpace(partes[1]))
                        return;

                    if (nombres[numeroJugador - 1] != "")
                        return;

                    nombres[numeroJugador - 1] = partes[1];

                    Console.WriteLine(partes[1] + " se unió al juego.");

                    EnviarATodos("JUGADOR_CONECTADO|" + numeroJugador + "|" + partes[1]);

                    if (clientes.Count == 3 && !partidaIniciada && nombres.TrueForAll(n => n != "") && clientes.TrueForAll(c => c != null))
                        IniciarPartida();

                    break;

                case "UNO":

                    if (!partidaIniciada ||
                        partidaTerminada)
                        return;

                    if (logica.GritarUNO(numeroJugador))
                    {
                        EnviarATodos(
                            "JUGADOR_UNO|" + numeroJugador);

                        GuardarLog(
                            numeroJugador, "Gritó UNO");
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|UNO_NO_PERMITIDO");
                    }

                    break;

                case "JUGAR_CARTA":

                    if (!partidaIniciada ||
                        partidaTerminada)
                        return;

                    if (logica.ObtenerTurnoActual() != numeroJugador)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|NO_ES_TU_TURNO");
                        return;
                    }

                    if (partes.Length < 4)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|FORMATO_CARTA");
                        return;
                    }

                    // JUGAR_CARTA|COLOR|TIPO|NUMERO|COLOR_ELEGIDO

                    string textoCarta =
                        partes[1] + "|" +
                        partes[2] + "|" +
                        partes[3];

                    Carta cartaJugada = TextoACarta(textoCarta);

                    if (cartaJugada == null)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|FORMATO_CARTA");
                        return;
                    }

                    ColorCarta colorElegido = ColorCarta.ESPECIAL;

                    if (cartaJugada.getTipo() == TipoCarta.COMODIN ||
                        cartaJugada.getTipo() == TipoCarta.MASCUATRO)
                    {
                        string textoColor =
                            partes.Length >= 5 ? partes[4] : "";

                        if (!ObtenerColorElegido(
                            textoColor, out colorElegido))
                        {
                            EnviarJugador(
                                numeroJugador,
                                "ERROR|COLOR_INVALIDO");
                            return;
                        }
                    }

                    if (logica.RealizarJugada(
                        cartaJugada, colorElegido))
                    {
                        GuardarLog(
                            numeroJugador,
                            "Jugó " + textoCarta);

                        NotificarJugada(numeroJugador);
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|JUGADA_INVALIDA");
                    }

                    break;

                case "ROBAR_CARTA":

                    if (!partidaIniciada ||
                        partidaTerminada)
                        return;

                    if (logica.ObtenerTurnoActual() != numeroJugador)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|NO_ES_TU_TURNO");
                        return;
                    }

                    Carta robada = logica.RobarCarta();

                    if (robada == null)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|MAZO_VACIO");
                        return;
                    }

                    string textoRobada = CartaATexto(robada);

                    Console.WriteLine(
                        "Jugador " + numeroJugador +
                        " robó una carta.");

                    GuardarLog(
                        numeroJugador, "Robó una carta");

                    EnviarJugador(
                        numeroJugador,
                        "CARTA_ROBADA|" + textoRobada);

                    EnviarCantidades();

                    if (logica.PuedeCartaJugadaRobada())
                    {
                        EnviarJugador(
                            numeroJugador,
                            "DECIDIR_ROBO|" + textoRobada);
                    }
                    else
                    {
                        logica.DecidirCartaRobada(
                            false, ColorCarta.ESPECIAL);

                        EnviarATodos(
                            "TURNO|" +
                            logica.ObtenerTurnoActual());
                    }

                    break;

                case "DECISION_ROBO":

                    if (!partidaIniciada ||
                        partidaTerminada)
                        return;

                    if (logica.ObtenerTurnoActual() != numeroJugador)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|NO_ES_TU_TURNO");
                        return;
                    }

                    if (partes.Length < 2)
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|DECISION_INVALIDA");
                        return;
                    }

                    if (partes[1] == "CONSERVAR")
                    {
                        if (logica.DecidirCartaRobada(
                            false, ColorCarta.ESPECIAL))
                        {
                            EnviarATodos(
                                "TURNO|" +
                                logica.ObtenerTurnoActual());
                        }
                        else
                        {
                            EnviarJugador(
                                numeroJugador,
                                "ERROR|DECISION_INVALIDA");
                        }
                    }
                    else if (partes[1] == "JUGAR")
                    {
                        ColorCarta colorRobada =
                            ColorCarta.ESPECIAL;

                        // El color elegido solo es necesario
                        // cuando la carta robada es comodín.
                        Carta cartaSuperior =
                            logica.ObtenerCartaSuperior();

                        // El mensaje del cliente conserva el
                        // formato DECISION_ROBO|JUGAR|COLOR.
                        string elegido =
                            partes.Length >= 3 ? partes[2] : "";

                        // La carta pendiente se encuentra dentro
                        // de LogicaUNO. La lógica valida su tipo.
                        if (!string.IsNullOrWhiteSpace(elegido))
                        {
                            if (!ObtenerColorElegido(
                                elegido, out colorRobada))
                            {
                                EnviarJugador(
                                    numeroJugador,
                                    "ERROR|COLOR_INVALIDO");
                                return;
                            }
                        }

                        if (logica.DecidirCartaRobada(
                            true, colorRobada))
                        {
                            GuardarLog(
                                numeroJugador,
                                "Jugó una carta robada");

                            NotificarJugada(numeroJugador);
                        }
                        else
                        {
                            EnviarJugador(
                                numeroJugador,
                                "ERROR|JUGADA_INVALIDA");
                        }
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|DECISION_INVALIDA");
                    }

                    break;

                case "REINICIAR":

                    if (clientes.Count == 3 &&
                        clientes.TrueForAll(c => c != null) &&
                        nombres.TrueForAll(n => n != ""))
                    {
                        IniciarPartida();
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|FALTAN_JUGADORES");
                    }

                    break;
            }
        }

        static void EnviarJugador(
            int numeroJugador, string mensaje)
        {
            Socket cliente = null;

            lock (bloqueo)
            {
                if (numeroJugador >= 1 &&
                    numeroJugador <= clientes.Count)
                {
                    cliente = clientes[numeroJugador - 1];
                }
            }

            if (cliente != null)
                Enviar(cliente, mensaje);
        }

        static void Enviar(Socket cliente, string mensaje)
        {
            if (cliente == null)
                return;

            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(
                    mensaje + "\n");

                lock (cliente)
                {
                    int enviados = 0;

                    while (enviados < datos.Length)
                    {
                        int cantidad = cliente.Send(
                            datos,
                            enviados,
                            datos.Length - enviados,
                            SocketFlags.None);

                        if (cantidad <= 0)
                            break;

                        enviados += cantidad;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(
                    "No se pudo enviar el mensaje.");
            }
        }

        static void EnviarATodos(string mensaje)
        {
            Socket[] conectados;

            lock (bloqueo)
                conectados = clientes.ToArray();

            foreach (Socket cliente in conectados)
            {
                if (cliente != null)
                    Enviar(cliente, mensaje);
            }
        }
    }
}
