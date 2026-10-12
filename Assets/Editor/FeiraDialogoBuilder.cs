using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cria o painel de diálogo das barracas no MESMO estilo do FeiraUIBuilder.
/// Menu: Feira > 6 - Criar Painel de Dialogo   (rode depois do passo 5)
/// </summary>
public static class FeiraDialogoBuilder
{
    static readonly Color Creme = Hex("FFF4DC");
    static readonly Color Marrom = Hex("3B2A20");
    static readonly Color Terracota = Hex("C8553D");
    static readonly Color Turquesa = Hex("28A39A");

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c; }
    static Sprite Arredondado { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); } }

    [MenuItem("Feira/6 - Criar Painel de Dialogo")]
    public static void Criar()
    {
        var canvasGO = GameObject.Find("UI_Feira");
        if (canvasGO == null || canvasGO.GetComponent<InterfaceFeira>() == null)
        {
            EditorUtility.DisplayDialog("Feira", "Nao achei o canvas UI_Feira. Rode antes: Feira > 5 - Criar Interface.", "OK");
            return;
        }
        Transform raiz = canvasGO.transform;
        if (raiz.Find("Dialogo") != null || raiz.Find("DicaInteragir") != null)
        {
            EditorUtility.DisplayDialog("Feira", "Dialogo/DicaInteragir ja existem. Apague os dois (dentro de UI_Feira) antes de criar de novo.", "OK");
            return;
        }

        // ---------- Cartao principal (embaixo da tela) ----------
        RectTransform ext, miolo;
        Cartao("Dialogo", raiz, Marrom, Creme, 8, out ext, out miolo);
        Ancorar(ext, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1400, 440));
        Undo.RegisterCreatedObjectUndo(ext.gameObject, "Dialogo");

        // ---------- Texto do dialogo ----------
        var corpo = Texto("Corpo", miolo, "Texto do dialogo", 36, Marrom, TextAlignmentOptions.TopLeft);
        corpo.enableAutoSizing = true;
        corpo.fontSizeMin = 24; corpo.fontSizeMax = 36;
        corpo.rectTransform.anchorMin = Vector2.zero; corpo.rectTransform.anchorMax = Vector2.one;
        corpo.rectTransform.offsetMin = new Vector2(50, 190);
        corpo.rectTransform.offsetMax = new Vector2(-50, -60);

        // ---------- Plaquinha com o nome da barraca (sobre a borda de cima) ----------
        var tag = Novo("PlaquinhaNome", ext);
        Ancorar(tag, new Vector2(0, 1), new Vector2(40, 30), new Vector2(480, 72));
        Img(tag, Terracota, true, 0.5f);
        var nome = Texto("Nome", tag, "Barraca", 38, Creme, TextAlignmentOptions.Center, FontStyles.Bold);
        Esticar(nome.rectTransform, 8);
        nome.enableAutoSizing = true; nome.fontSizeMin = 24; nome.fontSizeMax = 38;

        // ---------- 4 respostas em grade 2x2 ----------
        var resp = Novo("Respostas", miolo);
        resp.anchorMin = new Vector2(0, 0); resp.anchorMax = new Vector2(1, 0);
        resp.pivot = new Vector2(0.5f, 0);
        resp.sizeDelta = new Vector2(-100, 160);
        resp.anchoredPosition = new Vector2(0, 24);
        var grade = resp.gameObject.AddComponent<GridLayoutGroup>();
        grade.cellSize = new Vector2(600, 70);
        grade.spacing = new Vector2(24, 20);
        grade.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grade.constraintCount = 2;
        grade.childAlignment = TextAnchor.MiddleCenter;

        var botoes = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            botoes[i] = Botao("Resposta" + (i + 1), resp, "Alternativa", Turquesa, new Vector2(600, 70));
            var rot = botoes[i].GetComponentInChildren<TMP_Text>();
            rot.fontSize = 34; rot.enableAutoSizing = true;
            rot.fontSizeMin = 22; rot.fontSizeMax = 34;
        }

        // ---------- Botao continuar (mesmo lugar das respostas; nunca aparecem juntos) ----------
        var cont = Botao("BotaoContinuar", miolo, "Continuar", Terracota, new Vector2(460, 84));
        Ancorar(cont.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0, 45), new Vector2(460, 84));

        // ---------- Dica "E  Falar com o vendedor" (igual a DicaTeclas do HUD) ----------
        var dica = Novo("DicaInteragir", raiz);
        Ancorar(dica, new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(620, 64));
        Img(dica, new Color(Marrom.r, Marrom.g, Marrom.b, 0.85f), true, 0.5f);
        var tDica = Texto("Texto", dica, "E  Falar com o vendedor", 32, Creme);
        Esticar(tDica.rectTransform);
        Undo.RegisterCreatedObjectUndo(dica.gameObject, "DicaInteragir");

        // ---------- Script ----------
        var d = canvasGO.GetComponent<DialogoBarraca>();
        if (d == null) d = canvasGO.AddComponent<DialogoBarraca>();
        d.painel = ext.gameObject;
        d.dica = dica.gameObject;
        d.textoNome = nome;
        d.textoCorpo = corpo;
        d.botaoContinuar = cont;
        d.textoBotaoContinuar = cont.GetComponentInChildren<TMP_Text>();
        d.botoesResposta = botoes;
        EditorUtility.SetDirty(d);

        // fica por baixo do Menu (o menu/pausa cobre o dialogo)
        var menu = raiz.Find("Menu");
        if (menu != null)
        {
            dica.SetSiblingIndex(menu.GetSiblingIndex());
            ext.SetSiblingIndex(menu.GetSiblingIndex());
        }

        ext.gameObject.SetActive(false);
        dica.gameObject.SetActive(false);

        Selection.activeGameObject = canvasGO;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Painel de dialogo criado! Salve a cena (Ctrl+S).");
    }

    // ------------------------------------------------------------ AJUDANTES (iguais aos do FeiraUIBuilder)

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
}