using System;

public enum ColorCarta
{
	ROJO,
	AMARILLO,
	VERDE,
	AZUL,
	ESPECIAL
}

public enum TipoCarta
{
	NUMERO,
	BLOQUEA,
	REVERSA,
	MASDOS,
	MASCUATRO,
	COMODIN
}

public class Carta
{
	private ColorCarta color;
	private TipoCarta tipo;
	private int numero;

	public Carta(ColorCarta color, TipoCarta tipo)
	{
        this.color = color;
        this.tipo = tipo;
        this.numero = -1;
    }

	public Carta(ColorCarta color, TipoCarta tipo, int numero)
	{
		this.color = color;
		this.tipo = tipo;
		this.numero = numero;
	}

	public ColorCarta getColor()
	{
		return color;
	}

	public TipoCarta getTipo()
	{
		return tipo;
	}

	public int getNumero()
	{
		return numero;
	}

	public void setColor(ColorCarta color)
	{
		this.color = color;
	}

	public void setTipo(TipoCarta tipo)
	{
		this.tipo = tipo;
	}

	public void setNumero(int numero)
	{
		this.numero = numero;
	}
}
