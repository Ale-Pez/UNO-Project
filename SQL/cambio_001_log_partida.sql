ALTER TABLE historial_partidas
CHANGE COLUMN id_historial id_partida INT NOT NULL AUTO_INCREMENT;

ALTER TABLE log_juego
MODIFY COLUMN id_log INT NOT NULL;

ALTER TABLE log_juego
DROP COLUMN id_log;

ALTER TABLE log_juego
ADD COLUMN id_partida INT NOT NULL,
MODIFY COLUMN id_jugador INT NOT NULL;

ALTER TABLE log_juego
ADD PRIMARY KEY (id_partida, id_jugador);

ALTER TABLE log_juegohistorial_partidas
ADD CONSTRAINT fk_log_historial_partidas
FOREIGN KEY (id_partida) REFERENCES historial_partidas(id_partida) 
ON DELETE CASCADE
ON UPDATE CASCADE;

