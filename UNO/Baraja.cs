using System;
using System.Collections.Generic;
using System.Linq;

public class Baraja
{
    private List<Carta> cartas;

    public Baraja()
    {
        cartas = new List<Carta>();
        Inicializar_Cartas();
    }

    private void Inicializar_Cartas()
    {
        // ROJAS
        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.NUMERO, 0));

        for (int numero = 1; numero <= 9; numero++)
        {
            cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.NUMERO, numero));
            cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.NUMERO, numero));
        }

        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.BLOQUEA));
        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.BLOQUEA));

        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.REVERSA));
        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.REVERSA));

        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.MASDOS));
        cartas.Add(new Carta(ColorCarta.ROJO, TipoCarta.MASDOS));

        // AMARILLAS
        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.NUMERO, 0));

        for (int numero = 1; numero <= 9; numero++)
        {
            cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.NUMERO, numero));
            cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.NUMERO, numero));
        }

        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.BLOQUEA));
        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.BLOQUEA));

        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.REVERSA));
        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.REVERSA));

        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.MASDOS));
        cartas.Add(new Carta(ColorCarta.AMARILLO, TipoCarta.MASDOS));

        // VERDES
        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.NUMERO, 0));

        for (int numero = 1; numero <= 9; numero++)
        {
            cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.NUMERO, numero));
            cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.NUMERO, numero));
        }

        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.BLOQUEA));
        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.BLOQUEA));

        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.REVERSA));
        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.REVERSA));

        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.MASDOS));
        cartas.Add(new Carta(ColorCarta.VERDE, TipoCarta.MASDOS));

        // AZULES
        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.NUMERO, 0));

        for (int numero = 1; numero <= 9; numero++)
        {
            cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.NUMERO, numero));
            cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.NUMERO, numero));
        }

        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.BLOQUEA));
        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.BLOQUEA));

        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.REVERSA));
        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.REVERSA));

        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.MASDOS));
        cartas.Add(new Carta(ColorCarta.AZUL, TipoCarta.MASDOS));

        // ESPECIALES
        for (int i = 0; i < 4; i++)
        {
            cartas.Add(new Carta(ColorCarta.ESPECIAL, TipoCarta.COMODIN));
            cartas.Add(new Carta(ColorCarta.ESPECIAL, TipoCarta.MASCUATRO));
        }
    }
    
    public void Barajar()
    {
        Random random = new Random();

        for (int i = cartas.Count - 1; i > 0; i--)
        {
            int posicion = random.Next(i + 1);

            Carta temporal = cartas[i];
            cartas[i] = cartas[posicion];
            cartas[posicion] = temporal;
        }
    }
    
    public Carta Robar_Carta()
    {
        if (cartas.Count == 0)
            return null;

        Carta carta = cartas[0];
        cartas.RemoveAt(0);

        return carta;
    }

    // USO EXCLUSIVO DE LA PILA DE CARTAS
    public void Regresar_Carta(Carta carta)
    {
        this.cartas.Add(carta);
        this.Barajar();
    }

    public void RecargarDesdePila(List<Carta> cartasEnPila)
    {
        if (cartasEnPila.Count <= 1) return;

        Carta ultimaCarta = cartasEnPila[cartasEnPila.Count - 1];

        List<Carta> cartasParaReciclar = cartasEnPila.Take(cartasEnPila.Count - 1).ToList();

        cartasEnPila.Clear();
        cartasEnPila.Add(ultimaCarta);

        foreach (Carta carta in cartasParaReciclar)
        {
            this.cartas.Add(carta);
        }
        this.Barajar();
    }
}