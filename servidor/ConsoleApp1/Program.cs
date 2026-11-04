
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

        //baraja y manos de los jugadores
        static List<string> baraja = new List<string>();

        static List<string> mano1 = new List<string>();
        static List<string> mano2 = new List<string>();
        static List<string> mano3 = new List<string>();

        //Estado de la partida
        static string cartaMesa = "";
        static int turnoActual = 1;
        static bool partidaIniciada = false;

        static Random random = new Random();


        static void Main(string[] args)
        {
            IniciarServidor();
        }


        // ---------------------------------------------------------
        // crear las 108 cartas de UNO
        // ---------------------------------------------------------
        static void CrearBaraja()
        {
            baraja.Clear();

            string[] colores = {
                "ROJO", "AMARILLO", "VERDE", "AZUL"
            };

            foreach (string color in colores)
            {
                // un cero de cada color
                baraja.Add(color + "|NUMERO|0");

                // dos cartas de cada numero del 1 al 9
                for (int numero = 1; numero <= 9; numero++)
                {
                    baraja.Add(color + "|NUMERO|" + numero);
                    baraja.Add(color + "|NUMERO|" + numero);
                }

                // dos cartas de cada accion
                for (int i = 0; i < 2; i++)
                {
                    baraja.Add(color + "|BLOQUEA|0");
                    baraja.Add(color + "|REVERSA|0");
                    baraja.Add(color + "|MASDOS|0");
                }
            }

            // cuatro comodines y cuatro +4
            for (int i = 0; i < 4; i++)
            {
                baraja.Add("ESPECIAL|COMODIN|0");
                baraja.Add("ESPECIAL|MASCUATRO|0");
            }
        }


        // ---------------------------------------------------------
        // Mezclar las cartas
        // ---------------------------------------------------------
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


        // ---------------------------------------------------------
        // sacar una carta de la baraja
        // ---------------------------------------------------------
        static string RobarCarta()
        {
            if (baraja.Count == 0)
                return "";

            string carta = baraja[baraja.Count - 1];

            baraja.RemoveAt(baraja.Count - 1);

            return carta;
        }


        // ---------------------------------------------------------
        // iniciar una sola partida para los tres jugadores
        // ---------------------------------------------------------
        static void IniciarPartida()
        {
            CrearBaraja();
            BarajarCartas();

            mano1.Clear();
            mano2.Clear();
            mano3.Clear();

            // repartir siete cartas a cada jugador
            for (int i = 0; i < 7; i++)
            {
                mano1.Add(RobarCarta());
                mano2.Add(RobarCarta());
                mano3.Add(RobarCarta());
            }

            // elegir una carta numerica para iniciar
            // las cartas especiales se regresan a la baraja
            do
            {
                cartaMesa = RobarCarta();

                if (cartaMesa == "")
                    return;

                if (!cartaMesa.Contains("|NUMERO|"))
                    baraja.Insert(0, cartaMesa);

            } while (!cartaMesa.Contains("|NUMERO|"));

            turnoActual = 1;
            partidaIniciada = true;

            Console.WriteLine("La partida ha comenzado.");
            Console.WriteLine("Carta inicial: " + cartaMesa);

            //enviar a cada jugador solamente sus cartas
            Enviar(clientes[0],
                "MANO_INICIAL|" + string.Join(";", mano1));

            Enviar(clientes[1],
                "MANO_INICIAL|" + string.Join(";", mano2));

            Enviar(clientes[2],
                "MANO_INICIAL|" + string.Join(";", mano3));

            // enviar la carta de la mesa y el turno inicial
            EnviarATodos("CARTA_MESA|" + cartaMesa);
            EnviarATodos("TURNO|" + turnoActual);
        }


        // ---------------------------------------------------------
        // iniciar servidor
        // ---------------------------------------------------------
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
                    // no permitir mas jugadores ni nuevas conexiones una vez iniciada la partida
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


        // ---------------------------------------------------------
        // escuchar mensajes de los clientes
        // ---------------------------------------------------------
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
                    // no reasignar numeros a otros jugadores
                    // durante esta partida
                    if (numeroJugador - 1 < clientes.Count &&
                        clientes[numeroJugador - 1] == cliente)
                    {
                        clientes[numeroJugador - 1] = null;
                    }
                }

                cliente.Close();

                Console.WriteLine(
                    "Jugador " + numeroJugador + " desconectado."
                );
            }
        }


        // ---------------------------------------------------------
        // procesar los mensajes de cada jugador
        // ---------------------------------------------------------
        static void ProcesarMensaje(string mensaje, int numeroJugador)
        {
            string[] partes = mensaje.Split('|');

            switch (partes[0])
            {
                case "CONECTAR":

                    if (partes.Length < 2)
                        return;

                    string nombre = partes[1];

                    bool comenzar = false;

                    lock (bloqueo)
                    {
                        nombres[numeroJugador - 1] = nombre;

                        // comprobar que esten los tres jugadores y que todos hayan enviado su nombre
                        if (clientes.Count == 3 &&
                            !partidaIniciada &&
                            nombres.TrueForAll(n => n != "") &&
                            clientes.TrueForAll(c => c != null))
                        {
                            //marcar la partida como iniciad para evitar que comience dos veces
                            partidaIniciada = true;
                            comenzar = true;
                        }
                    }

                    Console.WriteLine(nombre + " se unió al juego.");

                    EnviarATodos(
                        "JUGADOR_CONECTADO|" +
                        numeroJugador + "|" + nombre
                    );

                    if (comenzar)
                    {
                        lock (bloqueo)
                        {
                            IniciarPartida();
                        }
                    }

                    break;


                case "JUGAR_CARTA":

                    Console.WriteLine(
                        "Jugador " + numeroJugador +
                        " solicita jugar: " + mensaje
                    );

                    // todavia no se valida ni se retira la carta,solamente se informa a los clientes.
                    if (partidaIniciada &&
                        turnoActual == numeroJugador)
                    {
                        EnviarATodos(
                            "ACCION_CARTA|" +
                            numeroJugador + "|" + mensaje
                        );
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|NO_ES_TU_TURNO"
                        );
                    }

                    break;


                case "ROBAR_CARTA":

                    Console.WriteLine(
                        "Jugador " + numeroJugador +
                        " solicita robar."
                    );

                  
                    // aun no se modifica la mano del jugador.
                    if (partidaIniciada &&
                        turnoActual == numeroJugador)
                    {
                        EnviarATodos(
                            "ACCION_ROBAR|" + numeroJugador
                        );
                    }
                    else
                    {
                        EnviarJugador(
                            numeroJugador,
                            "ERROR|NO_ES_TU_TURNO"
                        );
                    }

                    break;


                case "UNO":

                    if (partidaIniciada)
                    {
                        EnviarATodos(
                            "JUGADOR_UNO|" + numeroJugador
                        );
                    }

                    break;
            }
        }


        // ---------------------------------------------------------
        // enviar un mensaje a un jugador especifico
        // ---------------------------------------------------------
        static void EnviarJugador(int numeroJugador, string mensaje)
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


        // ---------------------------------------------------------
        // enviar un mensaje a un cliente
        // ---------------------------------------------------------
        static void Enviar(Socket cliente, string mensaje)
        {
            if (cliente == null)
                return;

            try
            {
                byte[] datos = Encoding.UTF8.GetBytes(mensaje + "\n");

                lock (cliente)
                {
                    int enviados = 0;

                    while (enviados < datos.Length)
                    {
                        int cantidad = cliente.Send(
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
            catch (Exception)
            {
                Console.WriteLine("No se pudo enviar el mensaje.");
            }
        }


        // ---------------------------------------------------------
        // enviar un mensaje a todos los clientes
        // ---------------------------------------------------------
        static void EnviarATodos(string mensaje)
        {
            Socket[] conectados;

            lock (bloqueo)
            {
                conectados = clientes.ToArray();
            }

            foreach (Socket cliente in conectados)
            {
                if (cliente != null)
                    Enviar(cliente, mensaje);
            }
        }
    }
}
