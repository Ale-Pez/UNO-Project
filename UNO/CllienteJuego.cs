
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UNO
{
    public class ClienteJuego
    {
        private Socket _socketCliente;
        private byte[] _bufferRecibo = new byte[1024];

        public string NombreJugador { get; private set; }
        public int NumeroJugador { get; private set; }

        // Datos de la partida recibidos del servidor
        public string[] ManoInicial { get; private set; }
        public string CartaMesa { get; private set; }
        public int TurnoActual { get; private set; }

        // Evento para avisar a MesaUNO que llegaron datos
        public event Action<string> MensajeRecibido;

        public void ConectarAlServidor(string ip, int puerto, string nombre)
        {
            NombreJugador = nombre;

            try
            {
                IPAddress ipAddr = IPAddress.Parse(ip);

                IPEndPoint remoteEndPoint =
                    new IPEndPoint(ipAddr, puerto);

                _socketCliente = new Socket(
                    ipAddr.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                _socketCliente.Connect(remoteEndPoint);

                Console.WriteLine(
                    $"[CONECTADO] Conectado como {NombreJugador}"
                );

                // MODIFICACIÓN:
                // Iniciar primero el hilo que recibe los mensajes
                Thread hiloEscucha = new Thread(EscucharAlServidor);
                hiloEscucha.IsBackground = true;
                hiloEscucha.Start();

                // Identificarse ante el servidor
                EnviarAccion($"CONECTAR|{NombreJugador}");
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"No se pudo conectar al servidor: {e.Message}"
                );

                // Avisar a MesaUNO del error de conexión
                MensajeRecibido?.Invoke("DESCONECTADO");
            }
        }

        public void EnviarAccion(string mensajeProtocolo)
        {
            try
            {
                if (_socketCliente != null &&
                    _socketCliente.Connected)
                {
                    byte[] datos = Encoding.UTF8.GetBytes(
                        mensajeProtocolo + "\n"
                    );

                    int enviados = 0;

                    lock (_socketCliente)
                    {
                        while (enviados < datos.Length)
                        {
                            int cantidad = _socketCliente.Send(
                                datos,
                                enviados,
                                datos.Length - enviados,
                                SocketFlags.None
                            );

                            if (cantidad <= 0)
                                throw new SocketException();

                            enviados += cantidad;
                        }
                    }
                }
                else
                {
                    Console.WriteLine(
                        "No hay conexión con el servidor."
                    );

                    MensajeRecibido?.Invoke("DESCONECTADO");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Error al enviar datos: {e.Message}"
                );

                MensajeRecibido?.Invoke("DESCONECTADO");
            }
        }

        private void EscucharAlServidor()
        {
            // Acumular los datos recibidos por TCP
            StringBuilder pendiente = new StringBuilder();

            try
            {
                while (true)
                {
                    int bytesRecibidos =
                        _socketCliente.Receive(_bufferRecibo);

                    // El servidor cerró la conexión
                    if (bytesRecibidos == 0)
                        break;

                    pendiente.Append(
                        Encoding.UTF8.GetString(
                            _bufferRecibo,
                            0,
                            bytesRecibidos
                        )
                    );

                    string contenido = pendiente.ToString();
                    int posicion;

                    // MODIFICACIÓN:
                    // Procesar cada mensaje completo separado por \n
                    while ((posicion = contenido.IndexOf('\n')) >= 0)
                    {
                        string mensaje = contenido
                            .Substring(0, posicion)
                            .TrimEnd('\r');

                        contenido = contenido.Substring(posicion + 1);

                        if (!string.IsNullOrWhiteSpace(mensaje))
                        {
                            ProcesarOrdenDelServidor(mensaje);
                        }
                    }

                    pendiente.Clear();
                    pendiente.Append(contenido);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine(
                    "[ERROR] Se perdió la conexión con el servidor."
                );
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine(
                    "La conexión con el servidor se cerró."
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Error al recibir datos: {e.Message}"
                );
            }
            finally
            {
                // MODIFICACIÓN:
                // Informar a MesaUNO cuando se pierde la conexión
                MensajeRecibido?.Invoke("DESCONECTADO");
            }
        }

        private void ProcesarOrdenDelServidor(string mensajeRecibido)
        {
            string[] partes = mensajeRecibido.Split('|');
            string comando = partes[0];

            try
            {
                switch (comando)
                {
                    case "JUGADOR":

                        if (partes.Length >= 2)
                        {
                            NumeroJugador = int.Parse(partes[1]);

                            Console.WriteLine(
                                $"Soy el jugador {NumeroJugador}"
                            );
                        }
                        break;

                    case "JUGADOR_CONECTADO":

                        if (partes.Length >= 3)
                        {
                            Console.WriteLine(
                                $"Se conectó el jugador {partes[1]}: {partes[2]}"
                            );
                        }
                        break;

                    case "JUGADORES":

                        Console.WriteLine(
                            "Se recibieron los nombres de los jugadores."
                        );
                        break;

                    case "REINICIO_CONFIRMADO":

                        // Limpiar los datos de la ronda anterior
                        ManoInicial = null;
                        CartaMesa = null;
                        TurnoActual = 0;

                        Console.WriteLine(
                            "El servidor reinició la partida."
                        );
                        break;

                    case "MANO_INICIAL":

                        if (mensajeRecibido.StartsWith("MANO_INICIAL|"))
                        {
                            string cartas = mensajeRecibido.Substring(
                                "MANO_INICIAL|".Length
                            );

                            ManoInicial = cartas.Split(';');

                            Console.WriteLine("Mis cartas iniciales:");

                            foreach (string carta in ManoInicial)
                            {
                                if (!string.IsNullOrWhiteSpace(carta))
                                    Console.WriteLine(carta);
                            }
                        }
                        break;

                    case "CARTA_MESA":

                        if (mensajeRecibido.StartsWith("CARTA_MESA|"))
                        {
                            CartaMesa = mensajeRecibido.Substring(
                                "CARTA_MESA|".Length
                            );

                            Console.WriteLine(
                                $"Carta de la mesa: {CartaMesa}"
                            );
                        }
                        break;

                    case "TURNO":

                        if (partes.Length >= 2)
                        {
                            TurnoActual = int.Parse(partes[1]);

                            Console.WriteLine(
                                $"Turno del jugador {TurnoActual}"
                            );
                        }
                        break;

                    case "CARTA_JUGADA":

                        if (partes.Length >= 5)
                        {
                            Console.WriteLine(
                                $"El jugador {partes[1]} jugó " +
                                $"{partes[2]}|{partes[3]}|{partes[4]}"
                            );
                        }
                        break;

                    case "CARTA_ROBADA":

                        Console.WriteLine(
                            "Se recibió una carta del servidor."
                        );
                        break;

                    case "DECIDIR_ROBO":

                        Console.WriteLine(
                            "El servidor solicita decidir si jugar " +
                            "o conservar la carta robada."
                        );
                        break;

                    case "COLOR_ACTUAL":

                        if (partes.Length >= 2)
                        {
                            Console.WriteLine(
                                $"Color actual: {partes[1]}"
                            );
                        }
                        break;

                    case "SENTIDO":

                        if (partes.Length >= 2)
                        {
                            Console.WriteLine(
                                $"Sentido del juego: {partes[1]}"
                            );
                        }
                        break;

                    case "CANTIDADES":

                        if (partes.Length >= 4)
                        {
                            Console.WriteLine(
                                $"Cartas restantes: " +
                                $"{partes[1]}, {partes[2]}, {partes[3]}"
                            );
                        }
                        break;

                    case "UNO_PENDIENTE":

                        Console.WriteLine(
                            "Un jugador debe anunciar UNO."
                        );
                        break;

                    case "JUGADOR_UNO":

                        if (partes.Length >= 2)
                        {
                            Console.WriteLine(
                                $"El jugador {partes[1]} gritó UNO."
                            );
                        }
                        break;

                    case "CASTIGO_UNO":

                        if (partes.Length >= 2)
                        {
                            Console.WriteLine(
                                $"El jugador {partes[1]} recibió " +
                                "un castigo por no decir UNO."
                            );
                        }
                        break;

                    case "GANADOR":

                        if (partes.Length >= 2)
                        {
                            Console.WriteLine(
                                $"Ganó el jugador {partes[1]}"
                            );
                        }
                        break;

                    case "JUGADOR_DESCONECTADO":

                        Console.WriteLine(
                            "Se desconectó un jugador."
                        );
                        break;

                    case "SERVIDOR_LLENO":

                        Console.WriteLine(
                            "El servidor ya tiene tres jugadores."
                        );
                        break;

                    case "ERROR":

                        Console.WriteLine(
                            $"Error del servidor: {mensajeRecibido}"
                        );
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Error al interpretar mensaje: {e.Message}"
                );

                return;
            }

            // Avisar a MesaUNO de cada mensaje recibido
            MensajeRecibido?.Invoke(mensajeRecibido);
        }

        // MODIFICACIÓN:
        // Cerrar la conexión al terminar de utilizar el cliente
        public void Desconectar()
        {
            try
            {
                if (_socketCliente != null)
                {
                    _socketCliente.Shutdown(SocketShutdown.Both);
                    _socketCliente.Close();
                }
            }
            catch (SocketException)
            {
                // La conexión ya estaba cerrada
            }
            catch (ObjectDisposedException)
            {
                // El socket ya fue liberado
            }
        }
    }
}
