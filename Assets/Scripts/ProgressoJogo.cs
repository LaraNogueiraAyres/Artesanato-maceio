using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Regras de compra e DERROTA (fica no canvas UI_Feira, junto do DialogoBarraca).
/// - missões = barracas com "Eh Missao" marcado; as demais são extras
/// - compra por id: se comprou em uma barraca, a barraca repetida do mesmo produto fecha
/// - DERROTA: o saldo não cobre mais o custo das missões que faltam
/// A VITÓRIA é a tela da colega (FimDeJogo): ela abre quando todos os objetivos forem concluídos.
/// Criado pelo menu: Feira > 9 - Criar Tela de Derrota
/// </summary>
public class ProgressoJogo : MonoBehaviour
{
    public static ProgressoJogo Instancia { get; private set; }

    [Header("Regras")]
    [Tooltip("Até quantos erros no total ainda ganha a estrela de precisão (aparece no resumo da vitória).")]
    public int errosParaEstrela = 3;

    [Header("Tela de derrota (preenchido pelo gerador)")]
    public GameObject painelFim;
    public TMP_Text textoTitulo;
    public TMP_Text textoResumo;
    public Button botaoJogarDeNovo;
    public Button botaoSair;

    readonly Dictionary<string, float> missoes = new Dictionary<string, float>();
    readonly HashSet<string> comprados = new HashSet<string>();
    int erros;
    int extras;
    InterfaceFeira ui;

    public static string Chave(InteracaoBarraca b)
    {
        return string.IsNullOrEmpty(b.idObjetivo) ? b.nomeItem : b.idObjetivo;
    }

    void Awake()
    {
        Instancia = this;
        ui = GetComponent<InterfaceFeira>();
        GameEstado.FimDeJogo = false;

        if (painelFim == null || textoTitulo == null || textoResumo == null)
        {
            Debug.LogError("ProgressoJogo: referências faltando. Apague 'TelaDerrota' do UI_Feira e rode Feira > 9 - Criar Tela de Derrota.");
            return;
        }

        if (botaoJogarDeNovo != null) botaoJogarDeNovo.onClick.AddListener(JogarDeNovo);
        if (botaoSair != null) botaoSair.onClick.AddListener(Sair);
        painelFim.SetActive(false);
    }

    void Start()
    {
        foreach (var b in FindObjectsByType<InteracaoBarraca>(FindObjectsSortMode.None))
        {
            if (!b.ehMissao) continue;
            string chave = Chave(b);
            if (!missoes.ContainsKey(chave)) missoes.Add(chave, b.preco);
        }

        var inv = InventarioJogador.Instancia;
        float saldo = inv != null ? inv.dinheiro : 0f;
        float lista = CustoMissoesRestantes();
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
        GameEstado.FimDeJogo = false;
    }

    void Update()
    {
        // Enquanto a tela de derrota está aberta, mantém o mouse livre (como faz o FimDeJogo da colega).
        if (painelFim != null && painelFim.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ---------------- consultas e registros ----------------

    public bool JaComprou(string chave) { return comprados.Contains(chave); }

    public float CustoMissoesRestantes()
    {
        float total = 0f;
        foreach (var kv in missoes)
            if (!comprados.Contains(kv.Key)) total += kv.Value;
        return total;
    }

    public void RegistrarErro() { erros++; }

    public void RegistrarCompra(InteracaoBarraca b)
    {
        if (comprados.Add(Chave(b)) && !b.ehMissao) extras++;
    }

    /// <summary>Texto mostrado na saudação: preço, saldo e (para extras) o impacto na lista.</summary>
    public string ResumoOrcamento(InteracaoBarraca b)
    {
        var inv = InventarioJogador.Instancia;
        float saldo = inv != null ? inv.dinheiro : 0f;
        string s = "Preço: " + InventarioJogador.Formatar(b.preco) + "   |   Seu saldo: " + InventarioJogador.Formatar(saldo);

        if (b.ehMissao) return "<b>Item da lista.</b> " + s;

        float sobra = saldo - b.preco;
        float lista = CustoMissoesRestantes();
        s = "<b>Fora da lista.</b> " + s + "\nSe comprar, sobram " + InventarioJogador.Formatar(sobra) +
            " e ainda faltam " + InventarioJogador.Formatar(lista) + " da lista.";
        if (sobra + 0.001f < lista) s += " <b>Atenção: o dinheiro não vai dar!</b>";
        return s;
    }

    /// <summary>Linha de estrelas para a tela de vitória da colega (FimDeJogo.MontarResumo).</summary>
    public string ResumoEstrelas()
    {
        bool precisao = erros <= errosParaEstrela;
        bool colecionador = extras > 0;
        int estrelas = 1 + (precisao ? 1 : 0) + (colecionador ? 1 : 0);

        return "<b><color=#C8553D>Estrelas:</color></b> " + estrelas + " de 3\n" +
               "<b><color=#C8553D>Erros:</color></b> " + erros + "   |   <b><color=#C8553D>Extras:</color></b> " + extras;
    }

    // ---------------- derrota ----------------

    /// <summary>Chamado quando o diálogo fecha: vê se o jogador já não consegue mais vencer.</summary>
    public void AvaliarFim()
    {
        if (GameEstado.FimDeJogo || painelFim == null) return;

        var inv = InventarioJogador.Instancia;
        if (missoes.Count > 0 && CustoMissoesRestantes() > 0.001f &&
            inv != null && inv.dinheiro + 0.001f < CustoMissoesRestantes())
            Perdeu();
    }

    void Perdeu()
    {
        GameEstado.FimDeJogo = true;
        GameEstado.Pausado = true;
        Time.timeScale = 0f;

        var inv = InventarioJogador.Instancia;
        float saldo = inv != null ? inv.dinheiro : 0f;

        textoTitulo.text = "Fim de feira";
        textoResumo.text = "O dinheiro não dá mais para completar a lista.\n\n" +
                           "Saldo: " + InventarioJogador.Formatar(saldo) + "\n" +
                           "Ainda faltavam: " + InventarioJogador.Formatar(CustoMissoesRestantes()) + "\n\n" +
                           "Dica: itens fora da lista gastam o dinheiro das missões. Confira o preço antes de comprar!";
        painelFim.SetActive(true);

        if (ui != null) ui.AtualizarCursor();
    }

    void JogarDeNovo()
    {
        GameEstado.Pausado = false;
        GameEstado.DialogoAberto = false;
        GameEstado.FimDeJogo = false;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Sair()
    {
        if (ui != null) ui.Sair();
    }
}