
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

        //datos de la partida recibidos del servidor
        public string[] ManoInicial { get; private set; }
        public string CartaMesa { get; private set; }
        public int TurnoActual { get; private set; }

        //evento para avisar a MesaUNO que llegaron datos
        public event Action<string> MensajeRecibido;

        public void ConectarAlServidor(string ip, int puerto, string nombre)
        {
            NombreJugador = nombre;

            IPAddress ipAddr = IPAddress.Parse(ip);
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddr, puerto);

            _socketCliente = new Socket(ipAddr.AddressFamily,SocketType.Stream,ProtocolType.Tcp);

            try
            {
                _socketCliente.Connect(remoteEndPoint);

                Console.WriteLine(
                    $"[CONECTADO] Conectado como {NombreJugador}"
                );

                EnviarAccion($"CONECTAR|{NombreJugador}");

                Thread hiloEscucha = new Thread(EscucharAlServidor);
                hiloEscucha.IsBackground = true;
                hiloEscucha.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"No se pudo conectar al servidor: {e.Message}");
            }
        }

        public void EnviarAccion(string mensajeProtocolo)
        {
            try
            {
                if (_socketCliente != null && _socketCliente.Connected)
                {
                    byte[] datos = Encoding.UTF8.GetBytes(mensajeProtocolo + "\n");

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
                                break;

                            enviados += cantidad;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Error al enviar datos: {e.Message}"
                );
            }
        }

        private void EscucharAlServidor()
        {
            StringBuilder pendiente = new StringBuilder();

            try
            {
                while (true)
                {
                    int bytesRecibidos = _socketCliente.Receive(_bufferRecibo);

                    if (bytesRecibidos == 0)
                        break;

                    pendiente.Append(
                        Encoding.UTF8.GetString(
                            _bufferRecibo, 0, bytesRecibidos
                        )
                    );

                    string contenido = pendiente.ToString();
                    int posicion;

                    while ((posicion = contenido.IndexOf('\n')) >= 0)
                    {
                        string mensaje = contenido.Substring(0, posicion)
                            .TrimEnd('\r');

                        contenido = contenido.Substring(posicion + 1);

                        ProcesarOrdenDelServidor(mensaje);
                    }

                    pendiente.Clear();
                    pendiente.Append(contenido);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine(
                    "[ERROR] Se perdio la conexion con el servidor."
                );
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine(
                    "La conexion con el servidor se cerro."
                );
            }
        }

        private void ProcesarOrdenDelServidor(string mensajeRecibido)
        {
            string[] partes = mensajeRecibido.Split('|');
            string comando = partes[0];

            switch (comando)
            {
                case "JUGADOR":

                    //guardar el numero asignado por el servidor
                    NumeroJugador = int.Parse(partes[1]);

                    Console.WriteLine(
                        $"Soy el jugador {NumeroJugador}"
                    );
                    break;


                case "JUGADOR_CONECTADO":

                    Console.WriteLine(
                        $"Se conectó el jugador {partes[1]}: {partes[2]}"
                    );
                    break;


                //recibir las siete cartas iniciales
                case "MANO_INICIAL":

                    string cartas = mensajeRecibido.Substring(
                        "MANO_INICIAL|".Length
                    );

                    ManoInicial = cartas.Split(';');

                    Console.WriteLine("Mis cartas iniciales:");

                    foreach (string carta in ManoInicial)
                    {
                        Console.WriteLine(carta);
                    }
                    break;


                //recibir la carta de la mesa
                case "CARTA_MESA":

                    CartaMesa = mensajeRecibido.Substring(
                        "CARTA_MESA|".Length
                    );

                    Console.WriteLine(
                        $"Carta de la mesa: {CartaMesa}"
                    );
                    break;


                //recibir el turno actual
                case "TURNO":

                    TurnoActual = int.Parse(partes[1]);

                    Console.WriteLine($"Turno del jugador {TurnoActual}");
                    break;


                case "ACCION_CARTA":

                    Console.WriteLine($"El jugador {partes[1]} solicito jugar una carta.");
                    break;


                case "ACCION_ROBAR":

                    Console.WriteLine($"El jugador {partes[1]} solicito robar una carta.");
                    break;


                case "JUGADOR_UNO":

                    Console.WriteLine($"El jugador {partes[1]} grito UNO.");
                    break;


                case "SERVIDOR_LLENO":

                   Console.WriteLine("El servidor ya tiene tres jugadores.");
                    break;


                //recibir errores del servidor
                case "ERROR":

                    Console.WriteLine(
                        $"Error del servidor: {mensajeRecibido}");
                    break;
            }

            //avisar a MesaUNO que se recibio un mensaje
            MensajeRecibido?.Invoke(mensajeRecibido);
        }
    }
}
