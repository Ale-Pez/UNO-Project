using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace mysql
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connStr = "Server=127.0.0.1;Port=3306;Database=juego_uno;Uid=root;Pwd=M1234";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    Console.WriteLine("se conecto bien");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de conexión: " + ex.Message);
            }
        }
    }
}