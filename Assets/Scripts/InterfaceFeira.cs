using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla menu inicial/pausa, "Como jogar", HUD e inventário.
/// Teclas: ESC = menu/pausa, TAB ou I = inventário.
/// </summary>
public class InterfaceFeira : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject menu;
    public GameObject painelMenuPrincipal;
    public GameObject painelComoJogar;
    public GameObject inventario;
    public CanvasGroup hud;

    [Header("Textos")]
    public TMP_Text textoBotaoJogar;
    public TMP_Text textoDinheiroHUD;
    public TMP_Text textoDinheiroInventario;

    [Header("Lista do inventário")]
    public Transform listaItens;
    public GameObject modeloItem;
    public GameObject textoVazio;

    [Header("Opções")]
    public bool comecarComMenu = true;
    public bool travarCursorNoJogo = true;

    bool jogoIniciado;
    readonly List<GameObject> linhas = new List<GameObject>();

    void Awake()
    {
        Time.timeScale = 1f;
        GameEstado.Pausado = false;
    }

    void Start()
    {
        if (InventarioJogador.Instancia != null)
            InventarioJogador.Instancia.AoMudar += AtualizarDinheiroEItens;
        AtualizarDinheiroEItens();

        if (comecarComMenu) AbrirMenu();
        else Jogar();
    }

    void OnDestroy()
    {
        if (InventarioJogador.Instancia != null)
            InventarioJogador.Instancia.AoMudar -= AtualizarDinheiroEItens;
        Time.timeScale = 1f;
        GameEstado.Pausado = false;
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        if (k.escapeKey.wasPressedThisFrame)
        {
            if (painelComoJogar.activeSelf) MostrarPrincipal();
            else if (inventario.activeSelf) FecharInventario();
            else if (menu.activeSelf) { if (jogoIniciado) Jogar(); }
            else AbrirMenu();
        }
        else if ((k.tabKey.wasPressedThisFrame || k.iKey.wasPressedThisFrame) && !menu.activeSelf)
        {
            if (inventario.activeSelf) FecharInventario();
            else AbrirInventario();
        }
    }

    // ---------------- Menu (ligados aos botões) ----------------

    public void Jogar()
    {
        jogoIniciado = true;
        menu.SetActive(false);
        inventario.SetActive(false);
        hud.alpha = 1f;
        Pausar(false);
    }

    public void AbrirMenu()
    {
        menu.SetActive(true);
        inventario.SetActive(false);
        MostrarPrincipal();
        if (textoBotaoJogar) textoBotaoJogar.text = jogoIniciado ? "Continuar" : "Jogar";
        hud.alpha = 0f;
        Pausar(true);
    }

    public void MostrarComoJogar()
    {
        painelMenuPrincipal.SetActive(false);
        painelComoJogar.SetActive(true);
    }

    public void MostrarPrincipal()
    {
        painelComoJogar.SetActive(false);
        painelMenuPrincipal.SetActive(true);
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------------- Inventário ----------------

    public void AbrirInventario()
    {
        AtualizarDinheiroEItens();
        inventario.SetActive(true);
        Pausar(true);
    }

    public void FecharInventario()
    {
        inventario.SetActive(false);
        Pausar(false);
    }

    void AtualizarDinheiroEItens()
    {
        var inv = InventarioJogador.Instancia;
        string din = InventarioJogador.Formatar(inv != null ? inv.dinheiro : 0f);
        if (textoDinheiroHUD) textoDinheiroHUD.text = din;
        if (textoDinheiroInventario) textoDinheiroInventario.text = din;

        if (listaItens == null || modeloItem == null) return;

        foreach (var l in linhas) if (l) Destroy(l);
        linhas.Clear();

        int total = 0;
        if (inv != null)
        {
            foreach (var item in inv.itens)
            {
                if (item.quantidade <= 0) continue;
                var go = Instantiate(modeloItem, listaItens);
                go.SetActive(true);
                go.transform.Find("Nome").GetComponent<TMP_Text>().text = item.nome;
                go.transform.Find("Qtd").GetComponent<TMP_Text>().text = "x" + item.quantidade;
                linhas.Add(go);
                total++;
            }
        }
        if (textoVazio) textoVazio.SetActive(total == 0);
    }

    // ---------------- Pausa e cursor ----------------

    void Pausar(bool pausar)
    {
        GameEstado.Pausado = pausar;
        Time.timeScale = pausar ? 0f : 1f;
        Cursor.visible = pausar || !travarCursorNoJogo;
        Cursor.lockState = (pausar || !travarCursorNoJogo) ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
