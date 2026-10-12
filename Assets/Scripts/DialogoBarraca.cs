using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Cérebro das interações da feira (fica no canvas UI_Feira):
/// - mostra a dica "E  Falar com o vendedor" quando o jogador está perto de uma barraca
/// - abre o diálogo com a tecla E
/// - fluxo: saudação -> pergunta -> acerto (paga, ganha item, conclui objetivo) -> fechar
/// - monta a lista de objetivos automaticamente a partir das barracas da cena
/// Criado pelo menu: Feira > 6 - Criar Painel de Dialogo
/// </summary>
public class DialogoBarraca : MonoBehaviour
{
    public static DialogoBarraca Instancia { get; private set; }

    [Header("Referências (preenchido pelo gerador)")]
    public GameObject painel;
    public GameObject dica;
    public TMP_Text textoNome;
    public TMP_Text textoCorpo;
    public Button botaoContinuar;
    public TMP_Text textoBotaoContinuar;
    public Button[] botoesResposta = new Button[4];

    [Header("Opções")]
    [Tooltip("Troca a lista de objetivos pela das barracas da cena (use Ordem/TextoObjetivo em cada barraca).")]
    public bool objetivosAutomaticos = true;

    enum Etapa { Saudacao, Pergunta, Fim }

    InteracaoBarraca atual;     // barraca com o diálogo aberto
    InteracaoBarraca proxima;   // barraca em que o jogador está perto
    Etapa etapa;
    InterfaceFeira ui;

    void Awake()
    {
        Instancia = this;
        ui = GetComponent<InterfaceFeira>();

        if (painel == null || textoCorpo == null || botaoContinuar == null)
        {
            Debug.LogError("DialogoBarraca: referências faltando. Apague 'Dialogo' e 'DicaInteragir' do UI_Feira e rode Feira > 6 - Criar Painel de Dialogo.");
            enabled = false;
            return;
        }

        botaoContinuar.onClick.AddListener(AoContinuar);
        for (int i = 0; i < botoesResposta.Length; i++)
        {
            int indice = i;
            if (botoesResposta[i] != null)
                botoesResposta[i].onClick.AddListener(() => Responder(indice));
        }

        painel.SetActive(false);
        if (dica != null) dica.SetActive(false);
    }

    void Start()
    {
        if (!objetivosAutomaticos || ObjetivosUI.Instancia == null) return;

        var barracas = FindObjectsByType<InteracaoBarraca>(FindObjectsSortMode.None);
        System.Array.Sort(barracas, (a, b) => a.ordem.CompareTo(b.ordem));

        ObjetivosUI.Instancia.Limpar();
        foreach (var b in barracas)
            if (!string.IsNullOrEmpty(b.idObjetivo))
                ObjetivosUI.Instancia.Adicionar(b.idObjetivo, b.textoObjetivo);
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
        GameEstado.DialogoAberto = false;
    }

    void Update()
    {
        bool mostrarDica = proxima != null && !GameEstado.DialogoAberto && !GameEstado.Pausado;
        if (dica != null && dica.activeSelf != mostrarDica) dica.SetActive(mostrarDica);

        if (GameEstado.Pausado || proxima == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!GameEstado.DialogoAberto) Abrir(proxima);
            else Fechar();
        }
    }

    // ---------------- chamado pelas barracas (trigger) ----------------

    public void EntrouEm(InteracaoBarraca barraca) { proxima = barraca; }

    public void SaiuDe(InteracaoBarraca barraca)
    {
        if (proxima == barraca) proxima = null;
        if (atual == barraca) Fechar();
    }

    // ---------------- abrir / fechar ----------------

    void Abrir(InteracaoBarraca barraca)
    {
        atual = barraca;
        painel.SetActive(true);
        GameEstado.DialogoAberto = true;
        if (ui != null) ui.AtualizarCursor();

        if (textoNome != null) textoNome.text = barraca.nomeBarraca;
        MostrarAlternativas(false);

        if (barraca.JaComprou)
        {
            textoCorpo.text = "Você já levou " + barraca.nomeItem + " daqui. Continue explorando a feira!";
            etapa = Etapa.Fim;
            MostrarContinuar("Fechar");
        }
        else
        {
            textoCorpo.text = barraca.mensagem;
            etapa = Etapa.Saudacao;
            MostrarContinuar("Começar desafio");
        }
    }

    public void Fechar()
    {
        if (painel != null) painel.SetActive(false);
        atual = null;
        GameEstado.DialogoAberto = false;
        if (ui != null) ui.AtualizarCursor();
    }

    // ---------------- fluxo ----------------

    void AoContinuar()
    {
        if (atual == null) return;

        if (etapa == Etapa.Saudacao) IniciarPergunta();
        else if (etapa == Etapa.Fim) Fechar();
    }

    void IniciarPergunta()
    {
        etapa = Etapa.Pergunta;
        textoCorpo.text = atual.pergunta;
        botaoContinuar.gameObject.SetActive(false);

        for (int i = 0; i < botoesResposta.Length; i++)
        {
            var b = botoesResposta[i];
            if (b == null) continue;

            bool existe = i < atual.alternativas.Length;
            b.gameObject.SetActive(existe);
            if (existe)
            {
                var t = b.GetComponentInChildren<TMP_Text>();
                if (t != null) t.text = atual.alternativas[i];
            }
        }
    }

    void Responder(int indice)
    {
        if (atual == null || etapa != Etapa.Pergunta) return;

        if (indice == atual.respostaCorreta) Acertou();
        else textoCorpo.text = "Ainda não! Tente de novo.\n" + atual.pergunta;
    }

    void Acertou()
    {
        MostrarAlternativas(false);
        etapa = Etapa.Fim;

        var inv = InventarioJogador.Instancia;
        if (inv != null)
        {
            if (!inv.Gastar(atual.preco))
            {
                textoCorpo.text = "Você acertou, mas o saldo não dá para levar " + atual.nomeItem + ".";
                MostrarContinuar("Fechar");
                return;
            }
            inv.AdicionarItem(atual.nomeItem, atual.quantidadeItem);
        }

        atual.MarcarComprado();

        if (ObjetivosUI.Instancia != null)
            ObjetivosUI.Instancia.Concluir(atual.idObjetivo);

        textoCorpo.text = "<b>Acertou!</b> Você pagou " + InventarioJogador.Formatar(atual.preco) +
                          " por " + atual.nomeItem + ".\n" + atual.curiosidade;
        MostrarContinuar("Concluir");
    }

    // ---------------- ajudantes ----------------

    void MostrarAlternativas(bool mostrar)
    {
        foreach (var b in botoesResposta)
            if (b != null) b.gameObject.SetActive(mostrar);
    }

    void MostrarContinuar(string rotulo)
    {
        if (textoBotaoContinuar != null) textoBotaoContinuar.text = rotulo;
        botaoContinuar.gameObject.SetActive(true);
    }
}