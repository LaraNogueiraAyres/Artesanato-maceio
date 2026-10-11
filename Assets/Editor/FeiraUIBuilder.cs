using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Monta toda a interface da feira (menu, objetivos, dinheiro, inventário) na cena.
/// Menu: Feira > 5 - Criar Interface
/// </summary>
public static class FeiraUIBuilder
{
    // Paleta inspirada nas barracas e no mar de Alagoas
    static readonly Color Creme = Hex("FFF4DC");
    static readonly Color Marrom = Hex("3B2A20");
    static readonly Color Terracota = Hex("C8553D");
    static readonly Color Mostarda = Hex("F2B134");
    static readonly Color Turquesa = Hex("28A39A");
    static readonly Color Lilas = Hex("A98BC9");
    static readonly Color Rosa = Hex("E8907F");
    static readonly Color Areia = Hex("F3DFB2");

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c; }
    static Sprite Arredondado { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); } }
    static Sprite Circulo { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"); } }

    [MenuItem("Feira/5 - Criar Interface (Menu, Objetivos e Inventario)")]
    public static void CriarInterface()
    {
        if (GameObject.Find("UI_Feira") != null)
        {
            EditorUtility.DisplayDialog("Feira", "A interface (UI_Feira) ja existe na cena. Apague-a antes de criar de novo.", "OK");
            return;
        }

        // ---------- Sistemas (dados) ----------
        var sistemas = new GameObject("Sistemas");
        Undo.RegisterCreatedObjectUndo(sistemas, "Sistemas");
        sistemas.AddComponent<InventarioJogador>();

        // ---------- EventSystem (necessario para clicar nos botoes) ----------
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            var g = new GameObject("EventSystem");
            es = g.AddComponent<EventSystem>();
            Undo.RegisterCreatedObjectUndo(g, "EventSystem");
        }
        var antigo = es.GetComponent<StandaloneInputModule>();
        if (antigo != null) Object.DestroyImmediate(antigo);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
        {
            var m = es.gameObject.AddComponent<InputSystemUIInputModule>();
            m.AssignDefaultActions();
        }

        // ---------- Canvas ----------
        var canvasGO = new GameObject("UI_Feira", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        Undo.RegisterCreatedObjectUndo(canvasGO, "UI Feira");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Transform raiz = canvasGO.transform;

        // =====================================================  HUD
        var hud = Novo("HUD", raiz);
        Esticar(hud);
        var cg = hud.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; cg.interactable = false;

        // ---- Objetivos (canto superior esquerdo)
        RectTransform objExt, objIn;
        Cartao("PainelObjetivos", hud, Marrom, Creme, 6, out objExt, out objIn);
        Ancorar(objExt, new Vector2(0, 1), new Vector2(30, -30), new Vector2(480, 0));
        var vExt = objExt.gameObject.AddComponent<VerticalLayoutGroup>();
        vExt.padding = new RectOffset(6, 6, 6, 6);
        vExt.childControlWidth = true; vExt.childControlHeight = true;
        vExt.childForceExpandWidth = true; vExt.childForceExpandHeight = false;
        var csf = objExt.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var vIn = objIn.gameObject.AddComponent<VerticalLayoutGroup>();
        vIn.padding = new RectOffset(26, 26, 16, 22);
        vIn.spacing = 12;
        vIn.childControlWidth = true; vIn.childControlHeight = true;
        vIn.childForceExpandWidth = true; vIn.childForceExpandHeight = false;

        var tObj = Texto("Titulo", objIn, "Objetivos", 36, Terracota, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        tObj.gameObject.AddComponent<LayoutElement>().preferredHeight = 46;
        var divObj = Novo("Divisor", objIn);
        Img(divObj, Mostarda, false);
        divObj.gameObject.AddComponent<LayoutElement>().preferredHeight = 4;

        // modelo de linha de objetivo
        var modObj = Novo("ModeloObjetivo", objIn);
        var h = modObj.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 14; h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = false;
        var caixa = Novo("Caixa", modObj);
        Img(caixa, Marrom, true, 0.5f);
        var leC = caixa.gameObject.AddComponent<LayoutElement>();
        leC.preferredWidth = 32; leC.preferredHeight = 32; leC.minWidth = 32; leC.minHeight = 32;
        var fundo = Novo("Fundo", caixa); Esticar(fundo, 4); Img(fundo, Creme, true, 0.5f);
        var marca = Novo("Marca", fundo); Esticar(marca, 5); Img(marca, Turquesa, true, 0.5f);
        var tLinha = Texto("Texto", modObj, "Objetivo", 28, Marrom, TextAlignmentOptions.MidlineLeft);
        var leT = tLinha.gameObject.AddComponent<LayoutElement>();
        leT.flexibleWidth = 1; leT.minHeight = 36;
        modObj.gameObject.SetActive(false);

        var objUI = objExt.gameObject.AddComponent<ObjetivosUI>();
        objUI.modelo = modObj.gameObject;
        objUI.pai = objIn;

        // ---- Dinheiro (canto superior direito)
        RectTransform dExt, dIn;
        Cartao("PainelDinheiro", hud, Marrom, Mostarda, 6, out dExt, out dIn);
        Ancorar(dExt, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(340, 84));
        var moeda = Novo("Moeda", dIn);
        Ancorar(moeda, new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(54, 54));
        var imMoeda = moeda.gameObject.AddComponent<Image>();
        imMoeda.sprite = Circulo; imMoeda.color = Terracota;
        var cifrao = Texto("Cifrao", moeda, "$", 36, Creme, TextAlignmentOptions.Center, FontStyles.Bold);
        Esticar(cifrao.rectTransform);
        var txtDinHUD = Texto("Valor", dIn, "R$ 0,00", 42, Marrom, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        Esticar(txtDinHUD.rectTransform);
        txtDinHUD.rectTransform.offsetMin = new Vector2(84, 0);
        txtDinHUD.rectTransform.offsetMax = new Vector2(-22, 0);

        // ---- Dica de teclas (embaixo)
        var dica = Novo("DicaTeclas", hud);
        Ancorar(dica, new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(560, 56));
        Img(dica, new Color(Marrom.r, Marrom.g, Marrom.b, 0.8f), true, 0.5f);
        var tDica = Texto("Texto", dica, "TAB  Inventário        ESC  Menu", 26, Creme);
        Esticar(tDica.rectTransform);

        // =====================================================  INVENTARIO
        RectTransform iExt, iIn;
        Cartao("Inventario", raiz, Marrom, Creme, 8, out iExt, out iIn);
        Ancorar(iExt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 720));

        var cab = Novo("Cabecalho", iIn);
        Topo(cab, 96, 16, -16);
        Img(cab, Turquesa, true, 0.5f);
        var tCab = Texto("Titulo", cab, "Inventário", 56, Creme, TextAlignmentOptions.Center, FontStyles.Bold);
        Esticar(tCab.rectTransform);

        var linhaDin = Novo("LinhaDinheiro", iIn);
        Topo(linhaDin, 64, 32, -128);
        var tDinL = Texto("Rotulo", linhaDin, "Dinheiro", 38, Marrom, TextAlignmentOptions.MidlineLeft);
        Esticar(tDinL.rectTransform);
        var tDinV = Texto("Valor", linhaDin, "R$ 0,00", 44, Terracota, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        Esticar(tDinV.rectTransform);

        var divInv = Novo("Divisor", iIn);
        Topo(divInv, 4, 32, -200);
        Img(divInv, Mostarda, false);

        var lista = Novo("Lista", iIn);
        lista.anchorMin = Vector2.zero; lista.anchorMax = Vector2.one;
        lista.offsetMin = new Vector2(32, 70); lista.offsetMax = new Vector2(-32, -216);
        lista.gameObject.AddComponent<RectMask2D>();
        var vl = lista.gameObject.AddComponent<VerticalLayoutGroup>();
        vl.spacing = 10; vl.padding = new RectOffset(0, 0, 8, 0);
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childControlWidth = true; vl.childControlHeight = true;
        vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;

        var modItem = Novo("ModeloItem", lista);
        Img(modItem, Areia, true, 0.5f);
        var hi = modItem.gameObject.AddComponent<HorizontalLayoutGroup>();
        hi.padding = new RectOffset(24, 24, 8, 8);
        hi.childAlignment = TextAnchor.MiddleLeft;
        hi.childControlWidth = true; hi.childControlHeight = true;
        hi.childForceExpandWidth = false; hi.childForceExpandHeight = true;
        var leI = modItem.gameObject.AddComponent<LayoutElement>();
        leI.preferredHeight = 64; leI.minHeight = 64;
        var tNome = Texto("Nome", modItem, "Item", 32, Marrom, TextAlignmentOptions.MidlineLeft);
        tNome.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        var tQtd = Texto("Qtd", modItem, "x1", 32, Terracota, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        var leQ = tQtd.gameObject.AddComponent<LayoutElement>();
        leQ.preferredWidth = 120; leQ.minWidth = 120;
        modItem.gameObject.SetActive(false);

        var vazio = Texto("TextoVazio", iIn, "Sua sacola está vazia.\nCompre algo nas barracas!", 36, new Color(Marrom.r, Marrom.g, Marrom.b, 0.6f), TextAlignmentOptions.Center, FontStyles.Italic);
        vazio.rectTransform.anchorMin = Vector2.zero; vazio.rectTransform.anchorMax = Vector2.one;
        vazio.rectTransform.offsetMin = new Vector2(32, 70); vazio.rectTransform.offsetMax = new Vector2(-32, -216);

        var tFechar = Texto("DicaFechar", iIn, "TAB ou ESC para fechar", 26, new Color(Marrom.r, Marrom.g, Marrom.b, 0.7f));
        Ancorar(tFechar.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(600, 40));
        iExt.gameObject.SetActive(false);

        // =====================================================  MENU
        var menuRt = Novo("Menu", raiz);
        Esticar(menuRt);
        Img(menuRt, new Color(0.12f, 0.07f, 0.04f, 0.85f), false);

        RectTransform mExt, mIn;
        Cartao("Cartao", menuRt, Marrom, Creme, 10, out mExt, out mIn);
        Ancorar(mExt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 900));
        Bandeirinhas(mIn, 980, 14);

        // ---- Painel principal
        var pPrin = Novo("PainelPrincipal", mIn);
        Esticar(pPrin);
        var tTitulo = Texto("Titulo", pPrin, "Feira de Artesanato", 84, Terracota, TextAlignmentOptions.Center, FontStyles.Bold);
        Ancorar(tTitulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(940, 110));
        var tAl = Texto("Subtitulo", pPrin, "de Alagoas", 60, Turquesa, TextAlignmentOptions.Center, FontStyles.Bold);
        Ancorar(tAl.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -235), new Vector2(940, 80));
        var tSub = Texto("Descricao", pPrin, "Compre os produtos da feira resolvendo desafios de matemática!", 30, Marrom, TextAlignmentOptions.Center, FontStyles.Italic);
        Ancorar(tSub.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(860, 50));

        var botoes = Novo("Botoes", pPrin);
        Ancorar(botoes, new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(560, 360));
        var vb = botoes.gameObject.AddComponent<VerticalLayoutGroup>();
        vb.spacing = 26; vb.childAlignment = TextAnchor.MiddleCenter;
        vb.childControlWidth = true; vb.childControlHeight = true;
        vb.childForceExpandWidth = true; vb.childForceExpandHeight = false;
        Button btJogar = Botao("BotaoJogar", botoes, "Jogar", Terracota, new Vector2(560, 96));
        Button btComo = Botao("BotaoComoJogar", botoes, "Como jogar", Turquesa, new Vector2(560, 96));
        Button btSair = Botao("BotaoSair", botoes, "Sair", Marrom, new Vector2(560, 96));

        // ---- Painel Como Jogar
        var pComo = Novo("PainelComoJogar", mIn);
        Esticar(pComo);
        var tComoT = Texto("Titulo", pComo, "Como jogar", 70, Terracota, TextAlignmentOptions.Center, FontStyles.Bold);
        Ancorar(tComoT.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -125), new Vector2(900, 90));

        string cor = "<b><color=#C8553D>";
        string fim = "</color></b>";
        string corpo =
            cor + "Objetivo" + fim + "\n" +
            "Passeie pela feira e compre os itens da sua lista de objetivos.\n\n" +
            cor + "Como comprar" + fim + "\n" +
            "Chegue perto de uma barraca e aperte E para falar com o vendedor. Resolva corretamente a conta de matemática para pagar pelo produto.\n\n" +
            cor + "Controles" + fim + "\n" +
            "<b>W A S D</b>  -  andar\n" +
            "<b>Mouse</b>  -  olhar ao redor\n" +
            "<b>E</b>  -  falar com o vendedor\n" +
            "<b>TAB</b>  -  abrir o inventário\n" +
            "<b>ESC</b>  -  pausar / menu";
        var tCorpo = Texto("Corpo", pComo, corpo, 30, Marrom, TextAlignmentOptions.TopLeft);
        tCorpo.enableAutoSizing = true;
        tCorpo.fontSizeMin = 20; tCorpo.fontSizeMax = 30;
        tCorpo.rectTransform.anchorMin = Vector2.zero; tCorpo.rectTransform.anchorMax = Vector2.one;
        tCorpo.rectTransform.offsetMin = new Vector2(70, 170);
        tCorpo.rectTransform.offsetMax = new Vector2(-70, -190);

        Button btVoltar = Botao("BotaoVoltar", pComo, "Voltar", Turquesa, new Vector2(360, 84));
        Ancorar(btVoltar.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(360, 84));
        pComo.gameObject.SetActive(false);

        // =====================================================  Script principal
        var ui = canvasGO.AddComponent<InterfaceFeira>();
        ui.menu = menuRt.gameObject;
        ui.painelMenuPrincipal = pPrin.gameObject;
        ui.painelComoJogar = pComo.gameObject;
        ui.inventario = iExt.gameObject;
        ui.hud = cg;
        ui.textoBotaoJogar = btJogar.GetComponentInChildren<TMP_Text>();
        ui.textoDinheiroHUD = txtDinHUD;
        ui.textoDinheiroInventario = tDinV;
        ui.listaItens = lista;
        ui.modeloItem = modItem.gameObject;
        ui.textoVazio = vazio.gameObject;

        UnityEventTools.AddPersistentListener(btJogar.onClick, ui.Jogar);
        UnityEventTools.AddPersistentListener(btComo.onClick, ui.MostrarComoJogar);
        UnityEventTools.AddPersistentListener(btSair.onClick, ui.Sair);
        UnityEventTools.AddPersistentListener(btVoltar.onClick, ui.MostrarPrincipal);

        Selection.activeGameObject = canvasGO;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Interface criada! Salve a cena (Ctrl+S) e de Play.");
    }

    // ------------------------------------------------------------ AJUDANTES

    static RectTransform Novo(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        return rt;
    }

    static void Esticar(RectTransform rt, float margem = 0f)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(margem, margem);
        rt.offsetMax = new Vector2(-margem, -margem);
    }

    static void Ancorar(RectTransform rt, Vector2 ancora, Vector2 pos, Vector2 tam)
    {
        rt.anchorMin = ancora; rt.anchorMax = ancora; rt.pivot = ancora;
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
    }

    // Faixa esticada na largura, presa ao topo
    static void Topo(RectTransform rt, float altura, float margemLateral, float y)
    {
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(-2f * margemLateral, altura);
        rt.anchoredPosition = new Vector2(0, y);
    }

    static Image Img(RectTransform rt, Color cor, bool arredondado = true, float ppu = 0.4f)
    {
        var im = rt.gameObject.AddComponent<Image>();
        im.color = cor;
        if (arredondado)
        {
            im.sprite = Arredondado;
            im.type = Image.Type.Sliced;
            im.pixelsPerUnitMultiplier = ppu;
        }
        return im;
    }

    static TextMeshProUGUI Texto(string nome, Transform pai, string txt, float tam, Color cor,
        TextAlignmentOptions al = TextAlignmentOptions.Center, FontStyles estilo = FontStyles.Normal)
    {
        var rt = Novo(nome, pai);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = txt; t.fontSize = tam; t.color = cor;
        t.alignment = al; t.fontStyle = estilo;
        t.raycastTarget = false;
        return t;
    }

    // Cartao com borda: devolve o externo (posicione este) e o miolo (coloque o conteudo aqui)
    static void Cartao(string nome, Transform pai, Color borda, Color miolo, float espessura,
        out RectTransform externo, out RectTransform interno)
    {
        externo = Novo(nome, pai);
        Img(externo, borda, true, 0.4f);
        var sombra = externo.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.35f);
        sombra.effectDistance = new Vector2(0, -8);
        interno = Novo("Miolo", externo);
        Esticar(interno, espessura);
        Img(interno, miolo, true, 0.4f);
    }

    static Button Botao(string nome, Transform pai, string rotulo, Color cor, Vector2 tam)
    {
        var rt = Novo(nome, pai);
        rt.sizeDelta = tam;
        var im = Img(rt, Color.white, true, 0.5f);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var cb = b.colors;
        cb.normalColor = cor;
        cb.highlightedColor = Color.Lerp(cor, Color.white, 0.25f);
        cb.pressedColor = Color.Lerp(cor, Color.black, 0.25f);
        cb.selectedColor = cor;
        cb.disabledColor = Color.gray;
        cb.fadeDuration = 0.1f;
        b.colors = cb;
        var sombra = rt.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0, 0, 0, 0.3f);
        sombra.effectDistance = new Vector2(0, -5);
        var t = Texto("Rotulo", rt, rotulo, 44, Creme, TextAlignmentOptions.Center, FontStyles.Bold);
        Esticar(t.rectTransform);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = tam.x; le.preferredHeight = tam.y;
        return b;
    }

    // Fio de bandeirinhas coloridas (estilo festa junina) no topo do cartao
    static void Bandeirinhas(Transform pai, float largura, int quantidade)
    {
        var cont = Novo("Bandeirinhas", pai);
        cont.anchorMin = new Vector2(0, 1); cont.anchorMax = new Vector2(1, 1);
        cont.pivot = new Vector2(0.5f, 1);
        cont.anchoredPosition = Vector2.zero; cont.sizeDelta = new Vector2(0, 80);

        var fio = Novo("Fio", cont);
        fio.anchorMin = new Vector2(0, 1); fio.anchorMax = new Vector2(1, 1);
        fio.pivot = new Vector2(0.5f, 1);
        fio.anchoredPosition = new Vector2(0, -16); fio.sizeDelta = new Vector2(0, 4);
        Img(fio, Marrom, false);

        Color[] cores = { Terracota, Mostarda, Turquesa, Lilas, Rosa };
        float passo = largura / quantidade;
        for (int i = 0; i < quantidade; i++)
        {
            var f = Novo("Bandeira_" + i, cont);
            f.anchorMin = new Vector2(0, 1); f.anchorMax = new Vector2(0, 1);
            f.pivot = new Vector2(0.5f, 0.5f);
            f.sizeDelta = new Vector2(36, 36);
            f.anchoredPosition = new Vector2(passo * (i + 0.5f), -40);
            f.localRotation = Quaternion.Euler(0, 0, 45);
            Img(f, cores[i % cores.Length], false);
        }
    }
}
