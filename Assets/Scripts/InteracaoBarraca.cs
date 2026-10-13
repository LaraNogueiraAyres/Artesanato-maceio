using UnityEngine;

/// <summary>
/// Fica na AreaInteracao (trigger) de cada barraca interativa.
/// Guarda só os DADOS da barraca (texto, pergunta, preço) e avisa o DialogoBarraca
/// quando o jogador entra/sai da área. Quem mostra "E", diálogo e compra é o DialogoBarraca.
/// </summary>
public class InteracaoBarraca : MonoBehaviour
{
    [Header("Barraca")]
    public string nomeBarraca = "Barraca de Bebidas";
    [TextArea]
    public string mensagem = "Bem-vindo à barraca de bebidas!";

    [Header("Tipo")]
    [Tooltip("Marcado = ponto cultural (ex.: monumento). Só mostra o texto da Curiosidade, sem pergunta nem compra. Desmarque 'Eh Missao'.")]
    public bool somenteInformativo = false;
    [Tooltip("Marcado = item da lista de missões. Desmarcado = item extra (gasta o dinheiro da lista).")]
    public bool ehMissao = true;

    [Header("Produto / compra")]
    public string nomeItem = "Suco";
    public int quantidadeItem = 3;
    [Tooltip("Quanto sai do saldo quando o jogador acerta (em reais).")]
    public float preco = 24f;

    [Header("Objetivo (aparece sozinho na lista da tela, só para missões)")]
    [Tooltip("Id único por produto (ex.: bebidas, mel, barro). Barracas com o mesmo id contam como a mesma compra.")]
    public string idObjetivo = "bebidas";
    public string textoObjetivo = "Comprar 3 sucos";
    [Tooltip("Ordem na lista de objetivos (0 aparece primeiro).")]
    public int ordem = 0;

    [Header("Desafio")]
    [TextArea]
    public string pergunta = "Pergunta de matemática aqui.";
    public string[] alternativas = { "A", "B", "C", "D" };
    [Tooltip("0 = primeira alternativa, 1 = segunda, etc.")]
    public int respostaCorreta = 1;
    [TextArea]
    public string curiosidade = "Curiosidade cultural sobre este produto de Alagoas.";

    public bool JaComprou { get; private set; }
    public void MarcarComprado() { JaComprou = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialogoBarraca.Instancia != null) DialogoBarraca.Instancia.EntrouEm(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialogoBarraca.Instancia != null) DialogoBarraca.Instancia.SaiuDe(this);
    }
}