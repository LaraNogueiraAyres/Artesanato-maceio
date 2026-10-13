using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cria a tela de DERROTA no mesmo estilo da UI da feira.
/// Menu: Feira > 9 - Criar Tela de Derrota   (a de vitória é a da colega: FimDeJogo)
/// </summary>
public static class FeiraDerrotaBuilder
{
    static readonly Color Creme = Hex("FFF4DC");
    static readonly Color Marrom = Hex("3B2A20");
    static readonly Color Terracota = Hex("C8553D");
    static readonly Color Turquesa = Hex("28A39A");

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c; }
    static Sprite Arredondado { get { return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); } }

    [MenuItem("Feira/9 - Criar Tela de Derrota")]
    public static void Criar()
    {
        var canvasGO = GameObject.Find("UI_Feira");
        if (canvasGO == null || canvasGO.GetComponent<InterfaceFeira>() == null)
        {
            EditorUtility.DisplayDialog("Feira", "Nao achei o canvas UI_Feira. Rode antes: Feira > 5 - Criar Interface.", "OK");
            return;
        }
        Transform raiz = canvasGO.transform;
        if (raiz.Find("TelaDerrota") != null)
        {
            EditorUtility.DisplayDialog("Feira", "A TelaDerrota ja existe. Apague-a (dentro de UI_Feira) antes de criar de novo.", "OK");
            return;
        }

        var fundo = Novo("TelaDerrota", raiz);
        Esticar(fundo);
        Img(fundo, new Color(Marrom.r, Marrom.g, Marrom.b, 0.78f), false);
        Undo.RegisterCreatedObjectUndo(fundo.gameObject, "TelaDerrota");

        RectTransform ext, miolo;
        Cartao("Cartao", fundo, Marrom, Creme, 8, out ext, out miolo);
        Ancorar(ext, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 700));

        var titulo = Texto("Titulo", miolo, "Fim de feira", 64, Terracota, TextAlignmentOptions.Center, FontStyles.Bold);
        var rtT = titulo.rectTransform;
        rtT.anchorMin = new Vector2(0, 1); rtT.anchorMax = new Vector2(1, 1);
        rtT.pivot = new Vector2(0.5f, 1);
        rtT.sizeDelta = new Vector2(-80, 100);
        rtT.anchoredPosition = new Vector2(0, -40);

        var resumo = Texto("Resumo", miolo, "Resumo", 34, Marrom, TextAlignmentOptions.Top);
        resumo.enableAutoSizing = true;
        resumo.fontSizeMin = 24; resumo.fontSizeMax = 34;
        resumo.rectTransform.anchorMin = Vector2.zero; resumo.rectTransform.anchorMax = Vector2.one;
        resumo.rectTransform.offsetMin = new Vector2(60, 170);
        resumo.rectTransform.offsetMax = new Vector2(-60, -160);

        var jogar = Botao("BotaoJogarDeNovo", miolo, "Jogar de novo", Turquesa, new Vector2(420, 84));
        Ancorar(jogar.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(-240, 45), new Vector2(420, 84));
        var sair = Botao("BotaoSair", miolo, "Sair", Marrom, new Vector2(420, 84));
        Ancorar(sair.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(240, 45), new Vector2(420, 84));

        var p = canvasGO.GetComponent<ProgressoJogo>();
        if (p == null) p = canvasGO.AddComponent<ProgressoJogo>();
        p.painelFim = fundo.gameObject;
        p.textoTitulo = titulo;
        p.textoResumo = resumo;
        p.botaoJogarDeNovo = jogar;
        p.botaoSair = sair;
        EditorUtility.SetDirty(p);

        fundo.gameObject.SetActive(false);

        Selection.activeGameObject = canvasGO;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Tela de derrota criada! Salve a cena (Ctrl+S).");
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