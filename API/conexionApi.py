import mysql.connector
from fastapi import FastAPI

app = FastAPI()

def obtener_conexion():
    return mysql.connector.connect(
        host="127.0.0.1",
        port=3306,
        database="juego_uno",
        user="root",      
        password="M1234" 
    )

@app.get("/jugadores")
async def usuarios():
    conexion = obtener_conexion()
    cursor = conexion.cursor()
    
    # Hacemos un JOIN para traer la información del jugador junto con su historial de partidas
    query = """
        SELECT j.id_jugador, j.nombre, COALESCE(h.ganadas, 0), COALESCE(h.perdidas, 0)
        FROM jugador j
        LEFT JOIN historial_partidas h ON j.id_jugador = h.id_jugador
    """
    cursor.execute(query)
    datos = cursor.fetchall()
    
    cursor.close()
    conexion.close()

    lista_jugadores = []
    for fila in datos:
        jugador = {
            "id_jugador": fila[0],
            "nombre": fila[1],
            "partidas_ganadas": fila[2],
            "cartas_comidas": fila[3] 
        }
        lista_jugadores.append(jugador)
    
    return lista_jugadores