DROP DATABASE IF EXISTS juego_uno;
CREATE DATABASE juego_uno;
USE juego_uno;

CREATE TABLE jugador (
    id_jugador INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL
);


CREATE TABLE historial_partidas (
    id_historial INT AUTO_INCREMENT PRIMARY KEY,
    id_jugador INT NOT NULL,
    ganadas INT DEFAULT 0,
    perdidas INT DEFAULT 0,
    FOREIGN KEY (id_jugador) REFERENCES jugador(id_jugador) ON DELETE CASCADE ON UPDATE CASCADE
);


CREATE TABLE partida (
    id_partida INT AUTO_INCREMENT PRIMARY KEY,
    fecha_inicio DATETIME DEFAULT CURRENT_TIMESTAMP,
    ganador VARCHAR(50) NULL,
    estado VARCHAR(20) DEFAULT 'En curso'
);


CREATE TABLE log_juego (
    id_log INT AUTO_INCREMENT PRIMARY KEY,
    id_partida INT NOT NULL,
    id_jugador INT NOT NULL,
    movimiento TEXT NOT NULL,
    fecha_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_partida) REFERENCES partida(id_partida) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (id_jugador) REFERENCES jugador(id_jugador) ON DELETE CASCADE ON UPDATE CASCADE
);