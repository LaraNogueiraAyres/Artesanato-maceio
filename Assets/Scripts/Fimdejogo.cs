using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Fim de jogo: quando TODOS os objetivos forem concluídos, espera o diálogo da última
/// compra fechar e mostra uma tela no mesmo estilo do menu inicial (cartão creme, bandeirinhas
/// de festa junina), com resumo, confetes e fanfarra. A música e os sons ambiente são parados.
///
/// Onde colocar o arquivo: junto dos outros scripts do jogo (ex.: Assets/Scripts).
///   NÃO coloque em uma pasta "Editor".
/// Onde adicionar o componente: no canvas UI_Feira (o mesmo objeto do DialogoBarraca).
///   O painel é criado sozinho, em código, na primeira vez que for preciso.
/// </summary>
public class FimDeJogo : MonoBehaviour
{
    [Header("Textos")]
    public string titulo = "Parabéns!";
    public string subtitulo = "Feira completa!";
    [TextArea]
    public string mensagem = "Você completou todos os objetivos e levou o melhor de Alagoas para casa.";

    [Header("Som")]
    [Tooltip("Opcional: seu efeito sonoro de vitória. Vazio = usa uma fanfarra gerada por código.")]
    public AudioClip somVitoria;
    [Range(0f, 1f)] public float volumeVitoria = 0.8f;
    [Tooltip("Para a música e os sons ambiente da cena quando a tela de fim aparece.")]
    public bool pararMusicaEAmbiente = true;

    [Header("Confetes")]
    public bool confetes = true;
    public int quantidadeConfetes = 140;
    [Tooltip("Por quantos segundos os confetes continuam caindo. 0 = para sempre.")]
    public float duracaoConfetes = 10f;

    [Header("Opções")]
    [Tooltip("Segundos de espera depois que o último diálogo fecha.")]
    public float atrasoAposDialogo = 0.6f;
    public bool mostrarBotaoSair = true;
    [Tooltip("Opcional: sprite de cantos arredondados. Vazio = copia do cartão do Menu automaticamente.")]
    public Sprite spriteCantos;

    [Header("Eventos (ligue animações etc.)")]
    public UnityEvent aoTerminar;

    // Mesma paleta do FeiraUIBuilder
    static readonly Color Creme = new Color32(0xFF, 0xF4, 0xDC, 255);
    static readonly Color Marrom = new Color32(0x3B, 0x2A, 0x20, 255);
    static readonly Color Terracota = new Color32(0xC8, 0x55, 0x3D, 255);
    static readonly Color Turquesa = new Color32(0x28, 0xA3, 0x9A, 255);
    static readonly Color Mostarda = new Color32(0xF2, 0xB1, 0x34, 255);
    static readonly Color Lilas = new Color32(0xA9, 0x8B, 0xC9, 255);
    static readonly Color Rosa = new Color32(0xE8, 0x90, 0x7F, 255);
    static readonly Color[] CoresFesta = { Terracota, Mostarda, Turquesa, Lilas, Rosa };

    class Confete
    {
        public RectTransform rt;
        public float x, y, vy, balanco, fase, vrot, rot, flip, vflip;
        public bool ativo;
    }

    ObjetivosUI objetivos;
    GameObject painel;
    RectTransform cartaoRt;
    RectTransform areaConfetes;
    TMP_Text textoResumo;
    AudioSource fonteSom;
    readonly List<Confete> pecas = new List<Confete>();
    bool terminou;
    bool chovendo;
    float tempoTotal;
    float tempoAberto;

    // ---------------------------------------------------------------- ciclo de vida

    void Start()
    {
        objetivos = ObjetivosUI.Instancia;
        if (objetivos != null) objetivos.AoConcluir += AoConcluirObjetivo;
        else Debug.LogWarning("FimDeJogo: ObjetivosUI não encontrado na cena.");
    }

    void OnDestroy()
    {
        if (objetivos != null) objetivos.AoConcluir -= AoConcluirObjetivo;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (painel == null || !painel.activeSelf) return;

        // Mantém o jogo travado e o mouse livre enquanto a tela de fim está aberta.
        GameEstado.Pausado = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float dt = Time.unscaledDeltaTime;
        tempoAberto += dt;

        AnimarCartao();
        AnimarConfetes(dt);
    }

    // ---------------------------------------------------------------- lógica

    void AoConcluirObjetivo(Objetivo _)
    {
        if (terminou || objetivos == null || objetivos.objetivos.Count == 0) return;

        foreach (var o in objetivos.objetivos)
            if (!o.concluido) return;

        terminou = true;
        StartCoroutine(MostrarDepoisDoDialogo());
    }

    IEnumerator MostrarDepoisDoDialogo()
    {
        // O objetivo é concluído com o diálogo ainda aberto: espere o jogador fechar.
        while (GameEstado.DialogoAberto) yield return null;
        yield return new WaitForSeconds(atrasoAposDialogo);
        Mostrar();
    }

    void Mostrar()
    {
        tempoTotal = Time.timeSinceLevelLoad;

        if (painel == null && !Construir()) return;

        textoResumo.text = MontarResumo();
        painel.transform.SetAsLastSibling();   // por cima de tudo
        painel.SetActive(true);
        GameEstado.Pausado = true;
        tempoAberto = 0f;

        if (pararMusicaEAmbiente) PararSons();
        TocarVitoria();
        if (confetes) IniciarConfetes();

        aoTerminar.Invoke();
    }

    string MontarResumo()
    {
        var sb = new StringBuilder();

        int feitos = 0, total = 0;
        if (objetivos != null)
        {
            total = objetivos.objetivos.Count;
            foreach (var o in objetivos.objetivos) if (o.concluido) feitos++;
            sb.AppendLine("<b><color=#C8553D>Objetivos:</color></b> " + feitos + "/" + total);
        }

        var inv = InventarioJogador.Instancia;
        if (inv != null)
        {
            sb.AppendLine("<b><color=#C8553D>Você levou:</color></b>");
            foreach (var i in inv.itens)
                if (i.quantidade > 0) sb.AppendLine("•  " + i.quantidade + "x " + i.nome);
            sb.AppendLine("<b><color=#C8553D>Saldo final:</color></b> " + InventarioJogador.Formatar(inv.dinheiro));
        }

        int t = Mathf.FloorToInt(tempoTotal);
        sb.AppendLine("<b><color=#C8553D>Tempo:</color></b> " + (t / 60).ToString("00") + ":" + (t % 60).ToString("00"));
        return sb.ToString();
    }

    // ---------------------------------------------------------------- botões

    public void JogarDeNovo()
    {
        // GameEstado é estático e sobrevive ao recarregar a cena: zere à mão.
        GameEstado.Pausado = false;
        GameEstado.DialogoAberto = false;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Teste rápido: Play, clique com o botão direito no componente e escolha esta opção.</summary>
    [ContextMenu("Testar tela de fim")]
    void Testar()
    {
        if (!Application.isPlaying) return;
        terminou = true;
        Mostrar();
    }

    // ---------------------------------------------------------------- som

    void PararSons()
    {
        // Pausa tudo que toca (música, ambiente, passos...). A fanfarra ignora essa pausa.
        AudioListener.pause = true;

        // Para de vez as fontes existentes (caso alguma ignore a pausa do listener).
        foreach (var s in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            if (s != fonteSom) s.Stop();
    }

    void TocarVitoria()
    {
        if (fonteSom == null)
        {
            var go = new GameObject("SomFimDeJogo");
            go.transform.SetParent(transform, false);
            fonteSom = go.AddComponent<AudioSource>();
            fonteSom.playOnAwake = false;
            fonteSom.loop = false;
            fonteSom.spatialBlend = 0f;               // som 2D
            fonteSom.ignoreListenerPause = true;      // toca mesmo com AudioListener.pause
        }

        if (somVitoria == null) somVitoria = GerarFanfarra();
        fonteSom.volume = volumeVitoria;
        fonteSom.clip = somVitoria;
        fonteSom.Play();
    }

    /// <summary>Fanfarra curtinha (arpejo de dó maior + acorde) gerada por código.</summary>
    static AudioClip GerarFanfarra()
    {
        const int taxa = 44100;
        const float duracao = 2.4f;
        int n = (int)(taxa * duracao);
        var amostras = new float[n];

        // freq, início (s), duração (s)
        float[,] notas =
        {
            { 523.25f, 0.00f, 0.50f },   // C5
            { 659.25f, 0.13f, 0.50f },   // E5
            { 783.99f, 0.26f, 0.50f },   // G5
            { 1046.5f, 0.39f, 1.90f },   // C6 (acorde final)
            { 1318.5f, 0.39f, 1.90f },   // E6
            { 1567.98f, 0.39f, 1.90f },  // G6
        };

        for (int k = 0; k < notas.GetLength(0); k++)
        {
            float f = notas[k, 0];
            int ini = (int)(notas[k, 1] * taxa);
            int len = (int)(notas[k, 2] * taxa);
            for (int i = 0; i < len && ini + i < n; i++)
            {
                float t = i / (float)taxa;
                float env = Mathf.Min(1f, t / 0.008f) * Mathf.Exp(-3.2f * t / notas[k, 2]);
                float w = 2f * Mathf.PI * f * t;
                float v = Mathf.Sin(w) + 0.4f * Mathf.Sin(2f * w) + 0.15f * Mathf.Sin(3f * w);
                amostras[ini + i] += v * env * 0.16f;
            }
        }

        for (int i = 0; i < n; i++) amostras[i] = Mathf.Clamp(amostras[i], -1f, 1f);

        var clip = AudioClip.Create("FanfarraVitoria", n, 1, taxa, false);
        clip.SetData(amostras, 0);
        return clip;
    }

    // ---------------------------------------------------------------- animações

    void AnimarCartao()
    {
        if (cartaoRt == null) return;
        // "pop" de entrada com leve estouro (ease out back)
        float p = Mathf.Clamp01(tempoAberto / 0.45f);
        float c1 = 1.70158f, c3 = c1 + 1f;
        float e = 1f + c3 * Mathf.Pow(p - 1f, 3) + c1 * Mathf.Pow(p - 1f, 2);
        float s = Mathf.LerpUnclamped(0.7f, 1f, e);
        cartaoRt.localScale = new Vector3(s, s, 1f);
    }

    void IniciarConfetes()
    {
        if (areaConfetes == null) return;

        areaConfetes.gameObject.SetActive(true);
        areaConfetes.SetAsLastSibling();   // por cima do cartão
        chovendo = true;
        if (duracaoConfetes > 0f) StartCoroutine(PararChuvaDepois(duracaoConfetes));

        float h = AlturaArea();
        foreach (var c in pecas)
        {
            Reiniciar(c, true, h);
            c.rt.gameObject.SetActive(true);
        }
    }

    IEnumerator PararChuvaDepois(float s)
    {
        yield return new WaitForSecondsRealtime(s);
        chovendo = false;
    }

    float LarguraArea()
    {
        float w = areaConfetes != null ? areaConfetes.rect.width : 0f;
        return w > 10f ? w : 1920f;
    }

    float AlturaArea()
    {
        float h = areaConfetes != null ? areaConfetes.rect.height : 0f;
        return h > 10f ? h : 1080f;
    }

    void Reiniciar(Confete c, bool inicial, float h)
    {
        float w = LarguraArea();
        c.x = Random.Range(-w * 0.5f, w * 0.5f);
        // no início, espalha acima da tela para a chuva "entrar" em ondas
        c.y = h * 0.5f + 40f + Random.Range(0f, inicial ? h * 0.9f : 120f);
        c.vy = Random.Range(260f, 620f);
        c.balanco = Random.Range(1.5f, 4f);
        c.fase = Random.Range(0f, 6.28f);
        c.vrot = Random.Range(-240f, 240f);
        c.rot = Random.Range(0f, 360f);
        c.flip = Random.Range(0f, 6.28f);
        c.vflip = Random.Range(4f, 10f);
        c.ativo = true;
    }

    void AnimarConfetes(float dt)
    {
        if (!confetes || pecas.Count == 0) return;

        float h = AlturaArea();
        bool algumAtivo = false;

        foreach (var c in pecas)
        {
            if (!c.ativo) continue;

            c.y -= c.vy * dt;
            c.fase += c.balanco * dt;
            c.rot += c.vrot * dt;
            c.flip += c.vflip * dt;

            if (c.y < -h * 0.5f - 40f)
            {
                if (chovendo) Reiniciar(c, false, h);
                else { c.ativo = false; c.rt.gameObject.SetActive(false); continue; }
            }

            algumAtivo = true;
            c.rt.anchoredPosition = new Vector2(c.x + Mathf.Sin(c.fase) * 45f, c.y);
            c.rt.localRotation = Quaternion.Euler(0, 0, c.rot);
            // girar no ar: achata no eixo Y como papel real
            float esc = Mathf.Max(0.15f, Mathf.Abs(Mathf.Cos(c.flip)));
            c.rt.localScale = new Vector3(1f, esc, 1f);
        }

        if (!algumAtivo && !chovendo) areaConfetes.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------- construção da UI

    bool Construir()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("FimDeJogo: nenhum Canvas encontrado para criar a tela de fim.");
            return false;
        }

        ResolverSprite(canvas);

        // Fundo escuro igual ao do menu inicial; bloqueia cliques no que está atrás
        var fundo = Novo("FimDeJogo_Painel", canvas.transform);
        Esticar(fundo);
        Img(fundo, new Color(0.12f, 0.07f, 0.04f, 0.85f), false);
        painel = fundo.gameObject;

        // Cartão: borda marrom + miolo creme + sombra (igual ao Menu)
        RectTransform ext, miolo;
        Cartao("Cartao", fundo, Marrom, Creme, 10, out ext, out miolo);
        Ancorar(ext, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 900));
        cartaoRt = ext;

        Bandeirinhas(miolo, 980, 14);

        // Título + subtítulo (mesmas fontes/posições do menu)
        var tTitulo = Texto("Titulo", miolo, titulo, 84, Terracota, TextAlignmentOptions.Center, FontStyles.Bold);
        Ancorar(tTitulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(940, 110));
        var tSub = Texto("Subtitulo", miolo, subtitulo, 60, Turquesa, TextAlignmentOptions.Center, FontStyles.Bold);
        Ancorar(tSub.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -235), new Vector2(940, 80));

        // Mensagem em itálico (como a descrição do menu)
        var tMsg = Texto("Mensagem", miolo, mensagem, 30, Marrom, TextAlignmentOptions.Center, FontStyles.Italic);
        tMsg.enableAutoSizing = true; tMsg.fontSizeMin = 20; tMsg.fontSizeMax = 30;
        Ancorar(tMsg.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(860, 80));

        // Resumo
        var res = Texto("Resumo", miolo, "", 32, Marrom, TextAlignmentOptions.TopLeft);
        res.enableAutoSizing = true;
        res.fontSizeMin = 20; res.fontSizeMax = 32;
        res.rectTransform.anchorMin = Vector2.zero;
        res.rectTransform.anchorMax = Vector2.one;
        res.rectTransform.offsetMin = new Vector2(150, 170);
        res.rectTransform.offsetMax = new Vector2(-150, -410);
        textoResumo = res;

        // Botões
        float x = mostrarBotaoSair ? -215f : 0f;
        var bJogar = Botao("BotaoJogarDeNovo", miolo, "Jogar de novo", Terracota, new Vector2(420, 92));
        Ancorar(bJogar.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(x, 48), new Vector2(420, 92));
        bJogar.onClick.AddListener(JogarDeNovo);

        if (mostrarBotaoSair)
        {
            var bSair = Botao("BotaoSair", miolo, "Sair", Marrom, new Vector2(420, 92));
            Ancorar(bSair.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(215, 48), new Vector2(420, 92));
            bSair.onClick.AddListener(Sair);
        }

        // Confetes (por cima do cartão, sem bloquear cliques)
        areaConfetes = Novo("Confetes", fundo);
        Esticar(areaConfetes);
        for (int i = 0; i < quantidadeConfetes; i++)
        {
            var p = Novo("Confete_" + i, areaConfetes);
            p.anchorMin = p.anchorMax = p.pivot = new Vector2(0.5f, 0.5f);
            p.sizeDelta = new Vector2(Random.Range(14f, 26f), Random.Range(8f, 14f));
            var im = p.gameObject.AddComponent<Image>();
            im.color = CoresFesta[Random.Range(0, CoresFesta.Length)];
            im.raycastTarget = false;
            p.gameObject.SetActive(false);
            pecas.Add(new Confete { rt = p });
        }
        areaConfetes.gameObject.SetActive(false);

        painel.SetActive(false);
        return true;
    }

    // Pega o sprite arredondado do cartão do Menu (assim o visual é idêntico).
    void ResolverSprite(Canvas canvas)
    {
        if (spriteCantos != null) return;

        var cartaoMenu = canvas.transform.Find("Menu/Cartao");
        if (cartaoMenu != null)
        {
            var im = cartaoMenu.GetComponent<Image>();
            if (im != null && im.sprite != null) { spriteCantos = im.sprite; return; }
        }
#if UNITY_EDITOR
        spriteCantos = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#endif
    }

    // ---------------------------------------------------------------- ajudantes de UI (iguais aos do FeiraUIBuilder)

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

    Image Img(RectTransform rt, Color cor, bool arredondar = true, float ppu = 0.4f)
    {
        var im = rt.gameObject.AddComponent<Image>();
        im.color = cor;
        if (arredondar && spriteCantos != null)
        {
            im.sprite = spriteCantos;
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

    void Cartao(string nome, Transform pai, Color borda, Color miolo, float espessura,
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

    Button Botao(string nome, Transform pai, string rotulo, Color cor, Vector2 tam)
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
        return b;
    }

    // Fio de bandeirinhas coloridas (estilo festa junina) no topo do cartão
    void Bandeirinhas(Transform pai, float largura, int quantidade)
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

        float passo = largura / quantidade;
        for (int i = 0; i < quantidade; i++)
        {
            var f = Novo("Bandeira_" + i, cont);
            f.anchorMin = new Vector2(0, 1); f.anchorMax = new Vector2(0, 1);
            f.pivot = new Vector2(0.5f, 0.5f);
            f.sizeDelta = new Vector2(36, 36);
            f.anchoredPosition = new Vector2(passo * (i + 0.5f), -40);
            f.localRotation = Quaternion.Euler(0, 0, 45);
            Img(f, CoresFesta[i % CoresFesta.Length], false);
        }
    }
}