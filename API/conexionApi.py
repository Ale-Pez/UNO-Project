from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import mysql.connector

app = FastAPI()

def obtener_conexion():
    return mysql.connector.connect(
        host="127.0.0.1",
        port=3306,
        database="juego_uno",
        user="root",      
        password="M1234" 
    )

# --- MODELOS DE DATOS (Lo que C# le mandara a la API) ---
class JugadorRequest(BaseModel):
    nombre: str

class LogRequest(BaseModel):
    id_partida: int
    id_jugador: int
    movimiento: str

class ResultadoRequest(BaseModel):
    id_jugador: int
    gano: bool


#NOTA: los metodos POST se utilizan obligatoriamente cuando la accion del servidor va a modificar, insertar o alterar datos en la base de datos
#REGISTRAR O OBTENER JUGADOR (Equivalente a RegistrarOObtenerJugadorConEstado)
@app.post("/jugadores/registrar")
async def registrar_o_obtener_jugador(data: JugadorRequest):
    conexion = obtener_conexion()
    cursor = conexion.cursor()
    
    # Buscar si ya existe
    cursor.execute("SELECT id_jugador FROM jugador WHERE nombre = %s", (data.nombre,))
    resultado = cursor.fetchone()
    
    if resultado:
        id_jugador = resultado[0]
        es_nuevo = False
    else:
        # Insertar si es nuevo
        cursor.execute("INSERT INTO jugador (nombre) VALUES (%s)", (data.nombre,))
        conexion.commit()
        id_jugador = cursor.lastrowid
        es_nuevo = True
        
        # Inicializar historial de una vez
        cursor.execute("INSERT INTO historial_partidas (id_jugador, ganadas, perdidas) VALUES (%s, 0, 0)", (id_jugador,))
        conexion.commit()

    cursor.close()
    conexion.close()
    
    return {"id_jugador": id_jugador, "es_nuevo": es_nuevo}


#GUARDAR LOG DE JUEGO (Equivalente a GuardarLog)
@app.post("/logs")
async def guardar_log(data: LogRequest):
    conexion = obtener_conexion()
    cursor = conexion.cursor()
    
    query = "INSERT INTO log_juego (id_partida, id_jugador, movimiento, fecha_hora) VALUES (%s, %s, %s, NOW())"
    cursor.execute(query, (data.id_partida, data.id_jugador, data.movimiento))
    conexion.commit()
    
    cursor.close()
    conexion.close()
    return {"mensaje": "Log guardado correctamente"}


#CREAR NUEVA PARTIDA (Equivalente a CrearNuevaPartida)
@app.post("/partidas/crear")
async def crear_partida():
    conexion = obtener_conexion()
    cursor = conexion.cursor()
    
    cursor.execute("INSERT INTO partida (estado) VALUES ('En curso')")
    conexion.commit()
    id_partida = cursor.lastrowid
    
    cursor.close()
    conexion.close()
    return {"id_partida": id_partida}


#ACTUALIZAR RESULTADO (Equivalente a RegistrarResultadoPartida)
@app.post("/historial/actualizar")
async def actualizar_historial(data: ResultadoRequest):
    conexion = obtener_conexion()
    cursor = conexion.cursor()
    
    if data.gano:
        query = "UPDATE historial_partidas SET ganadas = ganadas + 1 WHERE id_jugador = %s"
    else:
        query = "UPDATE historial_partidas SET perdidas = perdidas + 1 WHERE id_jugador = %s"
        
    cursor.execute(query, (data.id_jugador,))
    conexion.commit()
    
    cursor.close()
    conexion.close()
    return {"mensaje": "Historial actualizado"}

    # OBTENER LISTA DE JUGADORES Y SU RANKING GLOBAL (Para Form3)
@app.get("/jugadores")
async def obtener_jugadores():
    conexion = obtener_conexion()
    cursor = conexion.cursor(dictionary=True) # dictionary=True para que devuelva formato JSON amigable
    
    query = """
        SELECT j.id_jugador, j.nombre, h.ganadas AS partidas_ganadas 
        FROM jugador j
        JOIN historial_partidas h ON j.id_jugador = h.id_jugador
    """
    cursor.execute(query)
    resultados = cursor.fetchall()
    
    cursor.close()
    conexion.close()
    
    return resultados