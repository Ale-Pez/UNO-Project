using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServidorUNO
{
    class Program
    {
        static Socket servidor;
        static List<Socket> clientes = new List<Socket>();
        static List<string> nombres = new List<string>();
        static object bloqueo = new object();

        static void Main(string[] args)
        {
            IniciarServidor();
        }

        static void IniciarServidor()
        {
            IPAddress ip = IPAddress.Any;
            IPEndPoint direccion = new IPEndPoint(ip, 11111);

            servidor = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            servidor.Bind(direccion);
            servidor.Listen(3);

            Console.WriteLine("Servidor UNO iniciado.");
            Console.WriteLine("Esperando jugadores...");

            while (true)
            {
                Socket cliente = servidor.Accept();
                int numeroJugador;

                lock (bloqueo)
                {
                    if (clientes.Count >= 3)
                    {
                        Enviar(cliente, "SERVIDOR_LLENO");
                        cliente.Close();
                        continue;
                    }

                    clientes.Add(cliente);
                    nombres.Add("");
                    numeroJugador = clientes.Count;
                }

                Console.WriteLine(
                    "Jugador " + numeroJugador + " conectado."
                );

                Enviar(cliente, "JUGADOR|" + numeroJugador);

                Thread hilo = new Thread(() =>
                    EscucharCliente(cliente, numeroJugador)
                );

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

                    if (recibidos == 0)
                        break;

                    pendiente.Append(
                        Encoding.UTF8.GetString(buffer, 0, recibidos)
                    );

                    string contenido = pendiente.ToString();
                    int posicion;

                    while ((posicion = contenido.IndexOf('\n')) >= 0)
                    {
                        string mensaje = contenido.Substring(0, posicion)
                            .TrimEnd('\r');

                        contenido = contenido.Substring(posicion + 1);

                        ProcesarMensaje(mensaje, numeroJugador);
                    }

                    pendiente.Clear();
                    pendiente.Append(contenido);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(
                    "Se perdió la conexión del jugador " + numeroJugador
                );
            }
            finally
            {
                lock (bloqueo)
                {
                    // En esta primera versión detenemos la sala
                    // al desconectarse un jugador.
                    clientes.Remove(cliente);
                }

                cliente.Close();
            }
        }

        static void ProcesarMensaje(string mensaje, int numeroJugador)
        {
            string[] partes = mensaje.Split('|');

            switch (partes[0])
            {
                case "CONECTAR":

                    if (partes.Length < 2) return;

                    string nombre = partes[1];

                    lock (bloqueo)
                    {
                        nombres[numeroJugador - 1] = nombre;
                    }

                    Console.WriteLine(nombre + " se unió al juego.");

                    EnviarATodos(
                        "JUGADOR_CONECTADO|" + numeroJugador + "|" + nombre
                    );

                    break;

                case "JUGAR_CARTA":

                    Console.WriteLine(
                        "Jugador " + numeroJugador +
                        " solicita jugar: " + mensaje
                    );

                    // Temporalmente solo retransmitimos.
                    // Después validaremos la jugada aquí.
                    EnviarATodos(
                        "ACCION_CARTA|" + numeroJugador + "|" + mensaje
                    );

                    break;

                case "ROBAR_CARTA":

                    Console.WriteLine(
                        "Jugador " + numeroJugador + " solicita robar."
                    );

                    EnviarATodos(
                        "ACCION_ROBAR|" + numeroJugador
                    );

                    break;

                case "UNO":

                    EnviarATodos(
                        "JUGADOR_UNO|" + numeroJugador
                    );

                    break;
            }
        }

        static void Enviar(Socket cliente, string mensaje)
        {
            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "\n");

                lock (cliente)
                {
                    int enviados = 0;

                    while (enviados < datos.Length)
                    {
                        enviados += cliente.Send(
                            datos,
                            enviados,
                            datos.Length - enviados,
                            SocketFlags.None
                        );
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("No se pudo enviar el mensaje.");
            }
        }

        static void EnviarATodos(string mensaje)
        {
            Socket[] conectados;

            lock (bloqueo)
            {
                conectados = clientes.ToArray();
            }

            foreach (Socket cliente in conectados)
            {
                Enviar(cliente, mensaje);
            }
        }
    }
}
