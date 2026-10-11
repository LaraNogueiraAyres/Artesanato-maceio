using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class ItemInventario
{
    public string nome;
    public int quantidade;
}

/// <summary>
/// Dinheiro e itens do jogador.
/// Uso (de qualquer script):
///   InventarioJogador.Instancia.Gastar(12.5f);
///   InventarioJogador.Instancia.AdicionarItem("Mel");
/// </summary>
public class InventarioJogador : MonoBehaviour
{
    public static InventarioJogador Instancia { get; private set; }

    public float dinheiro = 50f;
    public List<ItemInventario> itens = new List<ItemInventario>();

    public event Action AoMudar;

    void Awake() { Instancia = this; }

    public bool PodeGastar(float valor) { return dinheiro + 0.001f >= valor; }

    public bool Gastar(float valor)
    {
        if (!PodeGastar(valor)) return false;
        dinheiro = Arredondar(dinheiro - valor);
        AoMudar?.Invoke();
        return true;
    }

    public void Ganhar(float valor)
    {
        dinheiro = Arredondar(dinheiro + valor);
        AoMudar?.Invoke();
    }

    public void AdicionarItem(string nome, int quantidade = 1)
    {
        var existente = itens.Find(i => i.nome == nome);
        if (existente != null) existente.quantidade += quantidade;
        else itens.Add(new ItemInventario { nome = nome, quantidade = quantidade });
        AoMudar?.Invoke();
    }

    public bool TemItem(string nome) { return itens.Exists(i => i.nome == nome && i.quantidade > 0); }

    static float Arredondar(float v) { return Mathf.Round(v * 100f) / 100f; }

    /// <summary>Ex.: 12.5 -> "R$ 12,50"</summary>
    public static string Formatar(float valor)
    {
        return "R$ " + valor.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
    }
}
