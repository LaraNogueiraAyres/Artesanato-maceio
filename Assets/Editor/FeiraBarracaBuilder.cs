using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Gera, no mesmo estilo low-poly das outras barracas:
///  - Barraca de Tapioca (com AreaInteracao + InteracaoBarraca ja configurados)
///  - Tripe com caixa de som
/// Coloque este arquivo numa pasta "Editor" (ex.: Assets/Editor).
/// Menu: Feira > 7 - Criar Barraca de Tapioca  /  Feira > 8 - Criar Tripe com Caixa de Som
///
/// Convencao: a FRENTE (lado do cliente) e o eixo Z+ (seta azul). O pivo fica no chao, no centro.
/// O vendedor (menu 3) nasce no pivo, que ja e atras do balcao.
/// </summary>
public static class FeiraBarracaBuilder
{
    const string PastaPrefabs = "Assets/Prefabs/Barracas";
    const string PastaMats = "Assets/Materiais/Barracas";

    // ============================================================ BARRACA DE TAPIOCA

    [MenuItem("Feira/7 - Criar Barraca de Tapioca")]
    public static void CriarBarracaTapioca()
    {
        GarantirPasta(PastaPrefabs);
        GarantirPasta(PastaMats);

        Material mPoste = Mat("Poste", "5B6C84");
        Material mMadeira = Mat("Madeira", "6B4A33");
        Material mTopo = Mat("BalcaoTopo", "A47148");
        Material mTerracota = Mat("Tapioca_Terracota", "C8553D");
        Material mTeto = Mat("Teto", "D9E1EA");
        Material mParede = Mat("Parede", "F2EDE0");
        Material mPrateleira = Mat("Prateleira", "8A6A4A");
        Material mChapa = Mat("Chapa", "2E2E33");
        Material mMassa = Mat("Tapioca_Massa", "F3E3B5");
        Material mBandeja = Mat("Bandeja", "E8E8E8");
        Material mTampa = Mat("Tampa", "3B2A20");
        Material mFolha = Mat("Folha", "3E8E41");
        Material[] recheios =
        {
            Mat("Coco", "FFFFFF"),
            Mat("Queijo", "F2C14E"),
            Mat("CarneSol", "9C4A2F"),
            Mat("Chocolate", "4A2C1D"),
            Mat("Banana", "F6D743"),
            Mat("Leite", "F7EBD2"),
        };

        var raiz = new GameObject("Barraca_Tapioca");
        Transform T = raiz.transform;

        // ---------- Postes (4 cantos) ----------
        float[] px = { -1.55f, 1.55f };
        float[] pz = { -0.85f, 0.95f };
        foreach (float x in px)
            foreach (float z in pz)
                P("Poste", PrimitiveType.Cylinder, T, new Vector3(x, 1.35f, z), new Vector3(0.08f, 1.35f, 0.08f), mPoste);

        // ---------- Teto e toldo ----------
        P("Teto", PrimitiveType.Cube, T, new Vector3(0, 2.74f, 0.05f), new Vector3(3.7f, 0.08f, 1.95f), mTeto);
        P("ToldoFrente", PrimitiveType.Cube, T, new Vector3(0, 2.52f, 1.0f), new Vector3(3.7f, 0.38f, 0.05f), mTerracota);
        P("ToldoEsq", PrimitiveType.Cube, T, new Vector3(-1.83f, 2.57f, 0.05f), new Vector3(0.05f, 0.28f, 1.95f), mTerracota);
        P("ToldoDir", PrimitiveType.Cube, T, new Vector3(1.83f, 2.57f, 0.05f), new Vector3(0.05f, 0.28f, 1.95f), mTerracota);

        // ---------- Paredes (fundo + laterais curtas) ----------
        P("ParedeFundo", PrimitiveType.Cube, T, new Vector3(0, 1.25f, -0.85f), new Vector3(3.2f, 2.5f, 0.06f), mParede, true);
        P("ParedeEsq", PrimitiveType.Cube, T, new Vector3(-1.58f, 1.25f, -0.4f), new Vector3(0.05f, 2.5f, 0.9f), mParede, true);
        P("ParedeDir", PrimitiveType.Cube, T, new Vector3(1.58f, 1.25f, -0.4f), new Vector3(0.05f, 2.5f, 0.9f), mParede, true);

        // ---------- Balcao ----------
        P("Balcao", PrimitiveType.Cube, T, new Vector3(0, 0.5f, 0.5f), new Vector3(3.2f, 1.0f, 0.7f), mMadeira, true);
        P("BalcaoFrente", PrimitiveType.Cube, T, new Vector3(0, 0.5f, 0.86f), new Vector3(2.8f, 0.62f, 0.02f), mTerracota);
        P("BalcaoTopo", PrimitiveType.Cube, T, new Vector3(0, 1.03f, 0.5f), new Vector3(3.3f, 0.06f, 0.8f), mTopo);
        const float topo = 1.06f; // altura do tampo

        // ---------- Chapa de tapioca (esquerda) ----------
        P("Chapa", PrimitiveType.Cylinder, T, new Vector3(-0.8f, topo + 0.03f, 0.45f), new Vector3(0.7f, 0.03f, 0.7f), mChapa);
        P("Tapioca_Chapa1", PrimitiveType.Cylinder, T, new Vector3(-0.8f, topo + 0.068f, 0.45f), new Vector3(0.5f, 0.008f, 0.5f), mMassa);
        P("Tapioca_Chapa2", PrimitiveType.Cylinder, T, new Vector3(-0.5f, topo + 0.012f, 0.62f), new Vector3(0.28f, 0.006f, 0.28f), mMassa);

        // ---------- Bandeja de tapiocas prontas (direita) ----------
        P("Bandeja", PrimitiveType.Cube, T, new Vector3(0.75f, topo + 0.01f, 0.5f), new Vector3(0.55f, 0.02f, 0.38f), mBandeja);
        for (int i = 0; i < 3; i++)
            P("TapiocaPronta", PrimitiveType.Cube, T, new Vector3(0.58f + i * 0.17f, topo + 0.065f, 0.5f), new Vector3(0.15f, 0.07f, 0.2f), mMassa);

        // ---------- Potes de recheio no balcao ----------
        for (int i = 0; i < 3; i++)
        {
            float x = -0.05f + i * 0.22f;
            P("PoteRecheio", PrimitiveType.Cylinder, T, new Vector3(x, topo + 0.09f, 0.68f), new Vector3(0.16f, 0.09f, 0.16f), recheios[i]);
            P("Tampa", PrimitiveType.Cylinder, T, new Vector3(x, topo + 0.185f, 0.68f), new Vector3(0.17f, 0.012f, 0.17f), mTampa);
        }

        // ---------- Prateleiras com potes ----------
        float[] alturasPrateleira = { 1.4f, 1.85f };
        for (int s = 0; s < alturasPrateleira.Length; s++)
        {
            float y = alturasPrateleira[s];
            P("Prateleira", PrimitiveType.Cube, T, new Vector3(0, y, -0.68f), new Vector3(3.0f, 0.04f, 0.3f), mPrateleira);
            for (int i = 0; i < 10; i++)
            {
                float h = 0.16f + ((i + s) % 3) * 0.04f;
                float x = -1.35f + i * 0.3f;
                P("Pote", PrimitiveType.Cylinder, T, new Vector3(x, y + 0.02f + h * 0.5f, -0.68f),
                    new Vector3(0.17f, h * 0.5f, 0.17f), recheios[(i + s * 2) % recheios.Length]);
            }
        }

        // ---------- Letreiros (texto 3D, lido de frente, por isso gira 180 em Y) ----------
        Letreiro("LetreiroToldo", T, "TAPIOCA", new Vector3(0, 2.52f, 1.03f), new Vector2(3.4f, 0.32f), true);
        Letreiro("LetreiroPreco", T, "Tapioca  R$ 3,00", new Vector3(0, 0.5f, 0.875f), new Vector2(2.6f, 0.4f), false);

        // ---------- Plantas nas laterais ----------
        Planta("Planta_E", T, new Vector3(-1.95f, 0, 0.8f), mTerracota, mFolha);
        Planta("Planta_D", T, new Vector3(1.95f, 0, 0.8f), mTerracota, mFolha);

        // ---------- Area de interacao + dados da compra ----------
        var area = new GameObject("AreaInteracao");
        area.transform.SetParent(T, false);
        var col = area.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1f, 1.45f);
        col.size = new Vector3(3.4f, 2f, 1.2f);

        var inter = area.AddComponent<InteracaoBarraca>();
        inter.nomeBarraca = "Barraca de Tapioca";
        inter.mensagem = "Bem-vindo à barraca de tapioca! Aqui a massa é feita na hora, quentinha, com coco, queijo ou carne de sol.";
        inter.nomeItem = "Tapioca";
        inter.quantidadeItem = 4;
        inter.preco = 12f;
        inter.idObjetivo = "tapioca";
        inter.textoObjetivo = "Comprar 4 tapiocas";
        inter.ordem = 2;
        inter.pergunta = "Cada tapioca custa R$ 3,00. Quanto custam 4 tapiocas?";
        inter.alternativas = new[] { "R$ 9,00", "R$ 10,00", "R$ 12,00", "R$ 15,00" };
        inter.respostaCorreta = 2;
        inter.curiosidade = "A tapioca é feita da goma da mandioca, um alimento herdado dos povos indígenas, e é um dos cafés da manhã mais queridos do Nordeste.";

        Finalizar(raiz, "Barraca_Tapioca");
    }

    // ============================================================ TRIPE + CAIXA DE SOM

    [MenuItem("Feira/8 - Criar Tripe com Caixa de Som")]
    public static void CriarTripe()
    {
        GarantirPasta(PastaPrefabs);
        GarantirPasta(PastaMats);

        Material mMetal = Mat("Tripe_Metal", "2B2B30");
        Material mPe = Mat("Tripe_Pe", "111114");
        Material mCorpo = Mat("Caixa_Corpo", "1E1E22");
        Material mCone = Mat("Caixa_Cone", "3A3A42");
        Material mAro = Mat("Caixa_Aro", "9AA0A8");
        Material mLed = Mat("Caixa_Led", "28A39A");

        var raiz = new GameObject("Tripe_CaixaSom");
        Transform T = raiz.transform;

        // Cubo central e haste
        Vector3 hub = new Vector3(0, 1.0f, 0);
        P("Cubo", PrimitiveType.Cylinder, T, hub, new Vector3(0.10f, 0.05f, 0.10f), mMetal);
        Haste("Haste", T, new Vector3(0, 1.0f, 0), new Vector3(0, 1.67f, 0), 0.05f, mMetal);
        P("Prato", PrimitiveType.Cylinder, T, new Vector3(0, 1.665f, 0), new Vector3(0.20f, 0.01f, 0.20f), mMetal);

        // Tres pernas abertas
        for (int i = 0; i < 3; i++)
        {
            float ang = (90f + i * 120f) * Mathf.Deg2Rad;
            Vector3 pe = new Vector3(Mathf.Cos(ang) * 0.6f, 0, Mathf.Sin(ang) * 0.6f);
            Haste("Perna" + (i + 1), T, new Vector3(0, 0.98f, 0), pe + Vector3.up * 0.02f, 0.045f, mMetal);
            P("Pe" + (i + 1), PrimitiveType.Sphere, T, pe + Vector3.up * 0.03f, new Vector3(0.07f, 0.06f, 0.07f), mPe);
        }

        // Caixa de som
        var caixa = new GameObject("Caixa").transform;
        caixa.SetParent(T, false);
        caixa.localPosition = new Vector3(0, 2.0f, 0);

        P("Corpo", PrimitiveType.Cube, caixa, Vector3.zero, new Vector3(0.45f, 0.65f, 0.38f), mCorpo);
        P("Alca", PrimitiveType.Cube, caixa, new Vector3(0, 0.34f, 0), new Vector3(0.18f, 0.03f, 0.04f), mMetal);

        // Falantes na frente (+Z): cilindros girados para olhar para frente
        Falante("AroGrave", caixa, new Vector3(0, -0.07f, 0.191f), 0.34f, mAro);
        Falante("Grave", caixa, new Vector3(0, -0.07f, 0.194f), 0.29f, mCone);
        Falante("AroAgudo", caixa, new Vector3(0, 0.21f, 0.191f), 0.15f, mAro);
        Falante("Agudo", caixa, new Vector3(0, 0.21f, 0.194f), 0.11f, mCone);
        P("Led", PrimitiveType.Sphere, caixa, new Vector3(0.17f, -0.29f, 0.19f), new Vector3(0.025f, 0.025f, 0.01f), mLed);

        // Para tocar musica: arraste um AudioClip em "Audio Clip" deste AudioSource e marque Play On Awake.
        var audio = caixa.gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = true;
        audio.spatialBlend = 1f;
        audio.minDistance = 2f;
        audio.maxDistance = 25f;

        // Colisor fino na haste, para o jogador nao atravessar
        var cap = raiz.AddComponent<CapsuleCollider>();
        cap.center = new Vector3(0, 1.2f, 0);
        cap.radius = 0.12f;
        cap.height = 2.4f;

        Finalizar(raiz, "Tripe_CaixaSom");
    }

    // ============================================================ AJUDANTES

    static void Finalizar(GameObject raiz, string nome)
    {
        string path = PastaPrefabs + "/" + nome + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, path);
        Object.DestroyImmediate(raiz);
        AssetDatabase.SaveAssets();

        // Coloca na cena, no ponto que a Scene View esta olhando, rente ao chao
        Vector3 pos = Vector3.zero;
        var sv = SceneView.lastActiveSceneView;
        if (sv != null) pos = sv.pivot;
        if (Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, out RaycastHit h, 50f, ~0, QueryTriggerInteraction.Ignore))
            pos = h.point;

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(inst, "Criar " + nome);
        inst.transform.position = pos;
        Selection.activeGameObject = inst;
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log(nome + " criado em " + path + " e colocado na cena. Salve a cena (Ctrl+S).");
    }

    static GameObject P(string nome, PrimitiveType tipo, Transform pai, Vector3 pos, Vector3 esc, Material mat, bool colisor = false)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nome;
        if (!colisor) Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localScale = esc;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    /// <summary>Cilindro esticado entre dois pontos (a -> b).</summary>
    static GameObject Haste(string nome, Transform pai, Vector3 a, Vector3 b, float diametro, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = nome;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(pai, false);
        Vector3 d = b - a;
        go.transform.localPosition = (a + b) * 0.5f;
        go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, d.normalized);
        go.transform.localScale = new Vector3(diametro, d.magnitude * 0.5f, diametro);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void Falante(string nome, Transform pai, Vector3 pos, float diametro, Material mat)
    {
        var go = P(nome, PrimitiveType.Cylinder, pai, pos, new Vector3(diametro, 0.012f, diametro), mat);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    static void Planta(string nome, Transform pai, Vector3 pos, Material mVaso, Material mFolha)
    {
        var planta = new GameObject(nome).transform;
        planta.SetParent(pai, false);
        planta.localPosition = pos;

        P("Vaso", PrimitiveType.Cylinder, planta, new Vector3(0, 0.17f, 0), new Vector3(0.35f, 0.17f, 0.35f), mVaso);

        for (int i = 0; i < 6; i++)
        {
            float altura = 0.8f + (i % 2) * 0.25f;
            var pivo = new GameObject("Folha" + (i + 1)).transform;
            pivo.SetParent(planta, false);
            pivo.localPosition = new Vector3(0, 0.34f, 0);
            pivo.localRotation = Quaternion.Euler(8f + (i % 3) * 6f, i * 60f, 0f);
            P("Lamina", PrimitiveType.Cube, pivo, new Vector3(0, altura * 0.5f, 0), new Vector3(0.08f, altura, 0.02f), mFolha);
        }
    }

    static void Letreiro(string nome, Transform pai, string texto, Vector3 pos, Vector2 tam, bool negrito)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0, 180f, 0); // legivel por quem esta na frente (Z+)

        var t = go.AddComponent<TextMeshPro>();
        t.text = texto;
        t.color = Hex("FFF4DC");
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = negrito ? FontStyles.Bold : FontStyles.Normal;
        t.enableAutoSizing = true;
        t.fontSizeMin = 1f;
        t.fontSizeMax = 6f;
        t.rectTransform.sizeDelta = tam;
    }

    static Color Hex(string h)
    {
        Color c;
        ColorUtility.TryParseHtmlString("#" + h, out c);
        return c;
    }

    static Material Mat(string nome, string hex)
    {
        string path = PastaMats + "/" + nome + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, path);
        }
        Color cor = Hex(hex);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
        if (m.HasProperty("_Color")) m.SetColor("_Color", cor);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static void GarantirPasta(string caminho)
    {
        var partes = caminho.Split('/');
        string atual = partes[0];
        for (int i = 1; i < partes.Length; i++)
        {
            string prox = atual + "/" + partes[i];
            if (!AssetDatabase.IsValidFolder(prox)) AssetDatabase.CreateFolder(atual, partes[i]);
            atual = prox;
        }
    }
}
