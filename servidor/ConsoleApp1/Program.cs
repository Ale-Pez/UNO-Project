using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ServidorUNO
{
    class Program
    {
        static Socket servidor;
        static List<Socket> clientes = new List<Socket>();
        static List<string> nombres = new List<string>();
        static object bloqueo = new object();

        static List<string> baraja = new List<string>();
        static List<string> mano1 = new List<string>();
        static List<string> mano2 = new List<string>();
        static List<string> mano3 = new List<string>();
        static List<string> descartes = new List<string>();

        static string cartaMesa = "";
        static string colorActual = "";
        static int turnoActual = 1;
        static int sentidoJuego = 1;
        static bool partidaIniciada = false;
        static bool partidaTerminada = false;
        static int jugadorPendienteUNO = -1;
        static HashSet<int> jugadoresConUNO = new HashSet<int>();
        static int jugadorEsperandoDecision = -1;
        static string cartaRobadaPendiente = "";
        static Random random = new Random();

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

        static void Main(string[] args)
        {
            IniciarServidor();
        }

        static void CrearBaraja()
        {
            baraja.Clear();
            string[] colores = { "ROJO", "AMARILLO", "VERDE", "AZUL" };
            foreach (string color in colores)
            {
                baraja.Add(color + "|NUMERO|0");
                for (int numero = 1; numero <= 9; numero++)
                {
                    baraja.Add(color + "|NUMERO|" + numero);
                    baraja.Add(color + "|NUMERO|" + numero);
                }
                for (int i = 0; i < 2; i++)
                {
                    baraja.Add(color + "|BLOQUEA|-1");
                    baraja.Add(color + "|REVERSA|-1");
                    baraja.Add(color + "|MASDOS|-1");
                }
            }
            for (int i = 0; i < 4; i++)
            {
                baraja.Add("ESPECIAL|COMODIN|-1");
                baraja.Add("ESPECIAL|MASCUATRO|-1");
            }
        }

        static void BarajarCartas()
        {
            for (int i = baraja.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                string temporal = baraja[i];
                baraja[i] = baraja[j];
                baraja[j] = temporal;
            }
        }

        static string RobarCarta()
        {
            if (baraja.Count == 0 && descartes.Count > 0)
            {
                // La carta visible permanece en mesa; se reciclan solo descartes anteriores.
                baraja.AddRange(descartes);
                descartes.Clear();
                BarajarCartas();
            }
            if (baraja.Count == 0) return "";
            string carta = baraja[baraja.Count - 1];
            baraja.RemoveAt(baraja.Count - 1);
            return carta;
        }

        static List<string> ObtenerMano(int numeroJugador)
        {
            if (numeroJugador == 1) return mano1;
            if (numeroJugador == 2) return mano2;
            return mano3;
        }

        static int SiguienteJugador(int numeroJugador)
        {
            int siguiente = numeroJugador + sentidoJuego;
            if (siguiente > 3) siguiente = 1;
            if (siguiente < 1) siguiente = 3;
            return siguiente;
        }

        static string ColorDe(string carta) { return carta.Split('|')[0]; }
        static string TipoDe(string carta) { return carta.Split('|')[1]; }
        static string NumeroDe(string carta) { return carta.Split('|')[2]; }

        static bool ColorElegidoValido(string color)
        {
            return color == "ROJO" || color == "AMARILLO" || color == "VERDE" || color == "AZUL";
        }

        static bool JugadaValida(string carta, List<string> mano)
        {
            string tipo = TipoDe(carta);
            if (tipo == "COMODIN") return true;
            if (tipo == "MASCUATRO")
            {
              
                return true;
            }
            if (ColorDe(carta) == colorActual) return true;
            if (tipo == "NUMERO" && TipoDe(cartaMesa) == "NUMERO" &&
                NumeroDe(carta) == NumeroDe(cartaMesa)) return true;
            if (tipo != "NUMERO" && tipo == TipoDe(cartaMesa)) return true;
            return false;
        }

        static void EnviarCantidades()
        {
            EnviarATodos("CANTIDADES|" + mano1.Count + "|" + mano2.Count + "|" + mano3.Count);
        }

        static void EnviarManos()
        {
            EnviarJugador(1, "MANO_INICIAL|" + string.Join(";", mano1));
            EnviarJugador(2, "MANO_INICIAL|" + string.Join(";", mano2));
            EnviarJugador(3, "MANO_INICIAL|" + string.Join(";", mano3));
        }

        static void IniciarPartida()
        {
            CrearBaraja();
            BarajarCartas();
            mano1.Clear(); mano2.Clear(); mano3.Clear(); descartes.Clear();
            jugadoresConUNO.Clear();
            jugadorPendienteUNO = -1;
            jugadorEsperandoDecision = -1;
            cartaRobadaPendiente = "";
            sentidoJuego = 1;
            turnoActual = 1;
            partidaTerminada = false;

            for (int i = 0; i < 7; i++)
            {
                mano1.Add(RobarCarta());
                mano2.Add(RobarCarta());
                mano3.Add(RobarCarta());
            }
            do
            {
                cartaMesa = RobarCarta();
                if (cartaMesa == "") return;
                if (TipoDe(cartaMesa) != "NUMERO") baraja.Insert(0, cartaMesa);
            } while (TipoDe(cartaMesa) != "NUMERO");

            colorActual = ColorDe(cartaMesa);
            partidaIniciada = true;
            Console.WriteLine("La partida ha comenzado. Carta inicial: " + cartaMesa);
            Console.WriteLine("Turno del jugador " + turnoActual);
            EnviarATodos("JUGADORES|" + nombres[0] + "|" + nombres[1] + "|" + nombres[2]);
            EnviarATodos("REINICIO_CONFIRMADO");
            EnviarManos();
            EnviarATodos("CARTA_MESA|" + cartaMesa);
            EnviarATodos("COLOR_ACTUAL|" + colorActual);
            EnviarATodos("SENTIDO|" + sentidoJuego);
            EnviarCantidades();
            EnviarATodos("TURNO|" + turnoActual);
            CrearRegistroPartida();
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
            catch (Exception) { Console.WriteLine("Se perdió la conexión del jugador " + numeroJugador); }
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

        static void EntregarCartas(int numeroJugador, int cantidad)
        {
            List<string> mano = ObtenerMano(numeroJugador);
            for (int i = 0; i < cantidad; i++)
            {
                string carta = RobarCarta();
                if (carta == "") break;
                mano.Add(carta);
                EnviarJugador(numeroJugador, "CARTA_ROBADA|" + carta);
            }
            jugadoresConUNO.Remove(numeroJugador);
            EnviarCantidades();
        }

        static void TerminarTurno()
        {
            turnoActual = SiguienteJugador(turnoActual);
            Console.WriteLine("Turno del jugador " + turnoActual);
            EnviarATodos("TURNO|" + turnoActual);
        }

        static void JugarCarta(int numeroJugador, string carta, string colorElegido, bool desdeRobo)
        {
            List<string> mano = ObtenerMano(numeroJugador);
            if (!mano.Contains(carta))
            {
                EnviarJugador(numeroJugador, "ERROR|CARTA_NO_DISPONIBLE"); return;
            }
            if (!JugadaValida(carta, mano))
            {
                EnviarJugador(numeroJugador, "ERROR|JUGADA_INVALIDA"); return;
            }
            string tipo = TipoDe(carta);
            if ((tipo == "COMODIN" || tipo == "MASCUATRO") && !ColorElegidoValido(colorElegido))
            {
                EnviarJugador(numeroJugador, "ERROR|COLOR_INVALIDO"); return;
            }

            mano.Remove(carta);
            Console.WriteLine("Jugador " + numeroJugador + " jugó " + carta);
            GuardarLog(numeroJugador, "Jugó " + carta);
            if (cartaMesa != "") descartes.Add(cartaMesa);
            cartaMesa = carta;
            colorActual = tipo == "COMODIN" || tipo == "MASCUATRO" ? colorElegido : ColorDe(carta);
            EnviarATodos("CARTA_JUGADA|" + numeroJugador + "|" + carta);
            EnviarATodos("COLOR_ACTUAL|" + colorActual);
            EnviarCantidades();

            // Conserva la regla original: si juega su última carta sin haber dicho UNO,
            // recibe dos cartas y pierde el turno. Se aplica también a la robada.
            if (mano.Count == 0 && !jugadoresConUNO.Contains(numeroJugador))
            {
                EnviarATodos("CASTIGO_UNO|" + numeroJugador);
                EntregarCartas(numeroJugador, 2);
                jugadorPendienteUNO = -1;
                TerminarTurno();
                return;
            }
            if (mano.Count == 0)
            {
                partidaTerminada = true;
                EnviarATodos("GANADOR|" + numeroJugador);
                RegistrarGanador(numeroJugador);
                return;
            }
            jugadoresConUNO.Remove(numeroJugador);
            jugadorPendienteUNO = mano.Count == 1 ? numeroJugador : -1;
            if (jugadorPendienteUNO != -1) EnviarATodos("UNO_PENDIENTE|" + numeroJugador);

            if (tipo == "REVERSA")
            {
                sentidoJuego *= -1;
                EnviarATodos("SENTIDO|" + sentidoJuego);
            }
            if (tipo == "BLOQUEA")
            {
                turnoActual = SiguienteJugador(turnoActual);
            }
            if (tipo == "MASDOS" || tipo == "MASCUATRO")
            {
                int castigado = SiguienteJugador(turnoActual);
                EntregarCartas(castigado, tipo == "MASDOS" ? 2 : 4);
                turnoActual = castigado;
            }
            TerminarTurno();
        }

        static void ProcesarMensaje(string mensaje, int numeroJugador)
        {
            string[] partes = mensaje.Split('|');
            switch (partes[0])
            {
                case "CONECTAR":
                    if (partes.Length < 2 || string.IsNullOrWhiteSpace(partes[1])) return;
                    if (nombres[numeroJugador - 1] != "") return;
                    nombres[numeroJugador - 1] = partes[1];
                    Console.WriteLine(partes[1] + " se unió al juego.");
                    EnviarATodos("JUGADOR_CONECTADO|" + numeroJugador + "|" + partes[1]);
                    if (clientes.Count == 3 && !partidaIniciada &&
                        nombres.TrueForAll(n => n != "") && clientes.TrueForAll(c => c != null))
                        IniciarPartida();
                    break;

                case "UNO":
                    if (!partidaIniciada || partidaTerminada) return;
                    if (jugadorPendienteUNO == numeroJugador ||
                        (turnoActual == numeroJugador && ObtenerMano(numeroJugador).Count == 1))
                    {
                        jugadoresConUNO.Add(numeroJugador);
                        jugadorPendienteUNO = -1;
                        EnviarATodos("JUGADOR_UNO|" + numeroJugador);
                        GuardarLog(numeroJugador, "Gritó UNO");
                    }
                    else EnviarJugador(numeroJugador, "ERROR|UNO_NO_PERMITIDO");
                    break;

                case "JUGAR_CARTA":
                    if (!partidaIniciada || partidaTerminada) return;
                    if (turnoActual != numeroJugador) { EnviarJugador(numeroJugador, "ERROR|NO_ES_TU_TURNO"); return; }
                    if (jugadorEsperandoDecision != -1) { EnviarJugador(numeroJugador, "ERROR|DECISION_ROBO_PENDIENTE"); return; }
                    if (partes.Length < 4) { EnviarJugador(numeroJugador, "ERROR|FORMATO_CARTA"); return; }
                    // JUGAR_CARTA|COLOR|TIPO|NUMERO|COLOR_ELEGIDO (último solo comodines)
                    string carta = partes[1] + "|" + partes[2] + "|" + partes[3];
                    string color = partes.Length >= 5 ? partes[4] : "";
                    JugarCarta(numeroJugador, carta, color, false);
                    break;

                case "ROBAR_CARTA":
                    if (!partidaIniciada || partidaTerminada) return;
                    if (turnoActual != numeroJugador) { EnviarJugador(numeroJugador, "ERROR|NO_ES_TU_TURNO"); return; }
                    if (jugadorEsperandoDecision != -1) { EnviarJugador(numeroJugador, "ERROR|DECISION_ROBO_PENDIENTE"); return; }
                    string robada = RobarCarta();
                    if (robada == "") { EnviarJugador(numeroJugador, "ERROR|MAZO_VACIO"); return; }
                    ObtenerMano(numeroJugador).Add(robada);
                    Console.WriteLine("Jugador " + numeroJugador + " robó una carta.");
                    GuardarLog(numeroJugador, "Robó una carta");
                    jugadoresConUNO.Remove(numeroJugador);
                    EnviarJugador(numeroJugador, "CARTA_ROBADA|" + robada);
                    EnviarCantidades();
                    if (JugadaValida(robada, ObtenerMano(numeroJugador)))
                    {
                        jugadorEsperandoDecision = numeroJugador;
                        cartaRobadaPendiente = robada;
                        EnviarJugador(numeroJugador, "DECIDIR_ROBO|" + robada);
                    }
                    else TerminarTurno();
                    break;

                case "DECISION_ROBO":
                    if (!partidaIniciada || partidaTerminada || jugadorEsperandoDecision != numeroJugador) return;
                    if (partes.Length < 2) { EnviarJugador(numeroJugador, "ERROR|DECISION_INVALIDA"); return; }
                    if (partes[1] == "CONSERVAR")
                    {
                        jugadorEsperandoDecision = -1;
                        cartaRobadaPendiente = "";
                        TerminarTurno();
                    }
                    else if (partes[1] == "JUGAR")
                    {
                        string cartaPendiente = cartaRobadaPendiente;
                        string elegido = partes.Length >= 3 ? partes[2] : "";
                        if ((TipoDe(cartaPendiente) == "COMODIN" || TipoDe(cartaPendiente) == "MASCUATRO") && !ColorElegidoValido(elegido))
                        { EnviarJugador(numeroJugador, "ERROR|COLOR_INVALIDO"); return; }
                        jugadorEsperandoDecision = -1;
                        cartaRobadaPendiente = "";
                        JugarCarta(numeroJugador, cartaPendiente, elegido, true);
                    }
                    else EnviarJugador(numeroJugador, "ERROR|DECISION_INVALIDA");
                    break;

                case "REINICIAR":
                    if (clientes.Count == 3 && clientes.TrueForAll(c => c != null) &&
                        nombres.TrueForAll(n => n != ""))
                        IniciarPartida();
                    else EnviarJugador(numeroJugador, "ERROR|FALTAN_JUGADORES");
                    break;
            }
        }

        static void EnviarJugador(int numeroJugador, string mensaje)
        {
            Socket cliente = null;
            lock (bloqueo)
            {
                if (numeroJugador >= 1 && numeroJugador <= clientes.Count)
                    cliente = clientes[numeroJugador - 1];
            }
            if (cliente != null) Enviar(cliente, mensaje);
        }

        static void Enviar(Socket cliente, string mensaje)
        {
            if (cliente == null) return;
            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "\n");
                lock (cliente)
                {
                    int enviados = 0;
                    while (enviados < datos.Length)
                    {
                        int cantidad = cliente.Send(datos, enviados, datos.Length - enviados, SocketFlags.None);
                        if (cantidad <= 0) break;
                        enviados += cantidad;
                    }
                }
            }
            catch (Exception) { Console.WriteLine("No se pudo enviar el mensaje."); }
        }

        static void EnviarATodos(string mensaje)
        {
            Socket[] conectados;
            lock (bloqueo) conectados = clientes.ToArray();
            foreach (Socket cliente in conectados)
                if (cliente != null) Enviar(cliente, mensaje);
        }
    }
}
