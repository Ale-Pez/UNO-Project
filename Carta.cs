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
		if (numero < 0 || numero > 9))
        {
			throw new ArgumentException("Una carta numérica debe tener un valor entre 0 y 9.\n");
		}

		this.color = color;
		this.tipo = tipo;

		if (tipo != TipoCarta.NUMERO)
		{
			Console.WriteLine($"ADVERTENCIA: Seleccionaste una carta de un tipo distinto a NUMERO. Se creara una del tipo {tipo} en su lugar.\n");
			this.numero = -1;
			return;
		}
		
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
        if (tipo != TipoCarta.NUMERO)
        {
            throw new ArgumentException($"Una carta del tipo {tipo} no puede tener valores numéricos.\n");
        } else if (numero < 0 || numero > 9)
        {
            throw new ArgumentException("Una carta numérica debe tener un valor entre 0 y 9.\n");
        }
        this.numero = numero;
	}
}
