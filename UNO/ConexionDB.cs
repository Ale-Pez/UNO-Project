using MySqlConnector;
using System;
using System.Windows.Forms;

namespace UNO
{
    public static class ConexionBD
    {
        private static string connStr = "Server=127.0.0.1;Port=3306;Database=juego_uno;Uid=root;Pwd=VaRCHAR3006@";
        public static int RegistrarOObtenerJugadorConEstado(string nombreJugador, out bool esNuevo)
        {
            int idJugador = -1;
            esNuevo = false;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    string querySelect = "SELECT id_jugador FROM jugador WHERE nombre = @nombre;";
                    using (MySqlCommand cmdSelect = new MySqlCommand(querySelect, conn))
                    {
                        cmdSelect.Parameters.AddWithValue("@nombre", nombreJugador);
                        object resultado = cmdSelect.ExecuteScalar();

                        if (resultado != null)
                        {
                            //el usuario YA exista
                            idJugador = Convert.ToInt32(resultado);
                            esNuevo = false;
                        }
                        else
                        {
                            //el usuario es NUEVO, lo insertamos
                            string queryInsert = "INSERT INTO jugador (nombre) VALUES (@nombre); SELECT LAST_INSERT_ID();";
                            using (MySqlCommand cmdInsert = new MySqlCommand(queryInsert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@nombre", nombreJugador);
                                idJugador = Convert.ToInt32(cmdInsert.ExecuteScalar());
                                esNuevo = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en BD: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return idJugador;
        }

        public static void GuardarLog(int idPartida, int idJugador, string movimiento)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO log_juego (id_partida, id_jugador, movimiento, fecha_hora) VALUES (@idPartida, @idJugador, @movimiento, NOW());";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idPartida", idPartida);
                        cmd.Parameters.AddWithValue("@idJugador", idJugador);
                        cmd.Parameters.AddWithValue("@movimiento", movimiento);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Error real en GuardarLog: " + ex.Message, "Error de BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    System.Diagnostics.Debug.WriteLine("Error en BD (Log): " + ex.Message);
                }
            }
        }
    

    //inicializa el historial de un jugador recin registrado
        public static void InicializarHistorial(int idJugador)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    // Verificamos si ya tiene un registro en el historial para no duplicarlo
                    string queryCheck = "SELECT COUNT(*) FROM historial_partidas WHERE id_jugador = @id;";
                    using (MySqlCommand cmdCheck = new MySqlCommand(queryCheck, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", idJugador);
                        long existe = (long)cmdCheck.ExecuteScalar();

                        if (existe == 0)
                        {
                            string queryInsert = "INSERT INTO historial_partidas (id_jugador, ganadas, perdidas) VALUES (@id, 0, 0);";
                            using (MySqlCommand cmdInsert = new MySqlCommand(queryInsert, conn))
                            {
                                cmdInsert.Parameters.AddWithValue("@id", idJugador);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error al inicializar historial: " + ex.Message);
                }
            }
        }

        //actualizar las partidas ganadas o perdidas
        public static void RegistrarResultadoPartida(int idJugador, bool gano)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "";

                    if (gano)
                    {
                        query = "UPDATE historial_partidas SET ganadas = ganadas + 1 WHERE id_jugador = @id;";
                    }
                    else
                    {
                        query = "UPDATE historial_partidas SET perdidas = perdidas + 1 WHERE id_jugador = @id;";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idJugador);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar historial: " + ex.Message, "Error BD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}