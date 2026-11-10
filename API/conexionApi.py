
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


# --- MODELOS DE DATOS (Lo que C# le mandará a la API) ---

class JugadorRequest(BaseModel):
    nombre: str


class LogRequest(BaseModel):
    id_partida: int
    id_jugador: int
    movimiento: str


class ResultadoRequest(BaseModel):
    id_jugador: int
    gano: bool


# MODIFICACIÓN:
# Modelo para finalizar una partida
class FinalizarPartidaRequest(BaseModel):
    id_partida: int
    ganador: str


# NOTA:
# Los métodos POST se utilizan cuando la acción
# modifica, inserta o altera datos en la base de datos.


# REGISTRAR O RECUPERAR UN JUGADOR
@app.post("/jugadores/registrar")
async def registrar_o_obtener_jugador(data: JugadorRequest):

    conexion = obtener_conexion()
    cursor = conexion.cursor()

    try:
        # Buscar si el jugador ya existe
        cursor.execute(
            "SELECT id_jugador FROM jugador WHERE nombre = %s",
            (data.nombre,)
        )

        resultado = cursor.fetchone()

        if resultado:
            id_jugador = resultado[0]
            es_nuevo = False

        else:
            # Insertar jugador nuevo
            cursor.execute(
                "INSERT INTO jugador (nombre) VALUES (%s)",
                (data.nombre,)
            )

            id_jugador = cursor.lastrowid
            es_nuevo = True

            # Inicializar su historial
            cursor.execute(
                """
                INSERT INTO historial_partidas
                (id_jugador, ganadas, perdidas)
                VALUES (%s, 0, 0)
                """,
                (id_jugador,)
            )

            conexion.commit()

        return {
            "id_jugador": id_jugador,
            "es_nuevo": es_nuevo
        }

    except mysql.connector.Error as error:
        conexion.rollback()
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()


# GUARDAR MOVIMIENTOS DEL JUEGO
@app.post("/logs")
async def guardar_log(data: LogRequest):

    conexion = obtener_conexion()
    cursor = conexion.cursor()

    try:
        query = """
            INSERT INTO log_juego
            (id_partida, id_jugador, movimiento, fecha_hora)
            VALUES (%s, %s, %s, NOW())
        """

        cursor.execute(
            query,
            (
                data.id_partida,
                data.id_jugador,
                data.movimiento
            )
        )

        conexion.commit()

        return {
            "mensaje": "Log guardado correctamente"
        }

    except mysql.connector.Error as error:
        conexion.rollback()
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()


# CREAR UNA NUEVA PARTIDA
@app.post("/partidas/crear")
async def crear_partida():

    conexion = obtener_conexion()
    cursor = conexion.cursor()

    try:
        cursor.execute(
            "INSERT INTO partida (estado) VALUES ('En curso')"
        )

        conexion.commit()

        id_partida = cursor.lastrowid

        return {
            "id_partida": id_partida
        }

    except mysql.connector.Error as error:
        conexion.rollback()
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()


# MODIFICACIÓN:
# FINALIZAR UNA PARTIDA
@app.post("/partidas/finalizar")
async def finalizar_partida(data: FinalizarPartidaRequest):

    conexion = obtener_conexion()
    cursor = conexion.cursor()

    try:
        # Cambiar el estado de la partida
        cursor.execute(
            """
            UPDATE partida
            SET estado = %s
            WHERE id_partida = %s
            """,
            ("Finalizada", data.id_partida)
        )

        if cursor.rowcount == 0:
            raise HTTPException(
                status_code=404,
                detail="No se encontró la partida"
            )

        conexion.commit()

        return {
            "mensaje": "Partida finalizada correctamente",
            "ganador": data.ganador
        }

    except mysql.connector.Error as error:
        conexion.rollback()
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()


# ACTUALIZAR VICTORIAS Y DERROTAS
@app.post("/historial/actualizar")
async def actualizar_historial(data: ResultadoRequest):

    conexion = obtener_conexion()
    cursor = conexion.cursor()

    try:
        if data.gano:
            query = """
                UPDATE historial_partidas
                SET ganadas = ganadas + 1
                WHERE id_jugador = %s
            """

        else:
            query = """
                UPDATE historial_partidas
                SET perdidas = perdidas + 1
                WHERE id_jugador = %s
            """

        cursor.execute(query, (data.id_jugador,))

        conexion.commit()

        return {
            "mensaje": "Historial actualizado"
        }

    except mysql.connector.Error as error:
        conexion.rollback()
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()


# OBTENER LISTA DE JUGADORES Y RANKING GLOBAL
# Utilizado por Form3
@app.get("/jugadores")
async def obtener_jugadores():

    conexion = obtener_conexion()
    cursor = conexion.cursor(dictionary=True)

    try:
        query = """
            SELECT
                j.id_jugador,
                j.nombre,
                h.ganadas AS partidas_ganadas
            FROM jugador j
            JOIN historial_partidas h
            ON j.id_jugador = h.id_jugador
        """

        cursor.execute(query)

        resultados = cursor.fetchall()

        return resultados

    except mysql.connector.Error as error:
        raise HTTPException(
            status_code=500,
            detail=str(error)
        )

    finally:
        cursor.close()
        conexion.close()
