using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cria os prefabs dos personagens (Player, Vendedor, Cliente) e ajuda a colocá-los na cena.
/// Menu: Feira > ...
/// </summary>
public static class FeiraPersonagensBuilder
{
    const string PastaPrefabs = "Assets/Prefabs/Personagens";
    const string PastaMats = "Assets/Materiais/Personagens";

    enum Tipo { Player, Vendedor, Cliente }

    // ------------------------------------------------------------ MENUS

    [MenuItem("Feira/1 - Gerar Prefabs dos Personagens")]
    public static void GerarPrefabs()
    {
        GarantirPasta(PastaPrefabs);
        GarantirPasta(PastaMats);
        Criar(Tipo.Player);
        Criar(Tipo.Vendedor);
        Criar(Tipo.Cliente);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prefabs criados em " + PastaPrefabs);
    }

    [MenuItem("Feira/2 - Colocar Visual no Player")]
    public static void ColocarVisualNoPlayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PastaPrefabs + "/Player_Visual.prefab");
        var player = GameObject.FindWithTag("Player");
        if (prefab == null || player == null)
        {
            EditorUtility.DisplayDialog("Feira", "Gere os prefabs (menu 1) e garanta que existe um objeto com a tag Player.", "OK");
            return;
        }
        if (player.transform.Find("Visual_Personagem") != null)
        {
            EditorUtility.DisplayDialog("Feira", "O Player já tem o visual.", "OK");
            return;
        }

        var mr = player.GetComponent<MeshRenderer>();
        if (mr) { Undo.RecordObject(mr, "Esconder capsula"); mr.enabled = false; }

        float peY = -1f;
        var cc = player.GetComponent<CharacterController>();
        if (cc) peY = cc.center.y - cc.height * 0.5f;

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(inst, "Visual do Player");
        inst.name = "Visual_Personagem";
        inst.transform.SetParent(player.transform, false);
        inst.transform.localPosition = new Vector3(0, peY, 0);
        inst.transform.localRotation = Quaternion.identity;
        Selection.activeGameObject = inst;
        Debug.Log("Visual adicionado ao Player.");
    }

    [MenuItem("Feira/3 - Adicionar Vendedor na Barraca Selecionada")]
    public static void AdicionarVendedor()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PastaPrefabs + "/Vendedor.prefab");
        if (prefab == null) { EditorUtility.DisplayDialog("Feira", "Gere os prefabs primeiro (menu 1).", "OK"); return; }
        if (Selection.gameObjects.Length == 0) { EditorUtility.DisplayDialog("Feira", "Selecione uma ou mais barracas na Hierarchy.", "OK"); return; }

        GameObject ultimo = null;
        foreach (var barraca in Selection.gameObjects)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(inst, "Vendedor");
            inst.name = "Vendedor_" + barraca.name;
            inst.transform.position = barraca.transform.position;
            inst.transform.rotation = barraca.transform.rotation;
            ultimo = inst;
        }
        Selection.activeGameObject = ultimo;
        Debug.Log("Vendedor(es) criado(s) no pivô da barraca. Arraste para trás do balcão e gire (azul = frente) para o corredor.");
    }

    [MenuItem("Feira/4 - Espalhar 8 Clientes pela Feira")]
    public static void EspalharClientes()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PastaPrefabs + "/Cliente.prefab");
        if (prefab == null) { EditorUtility.DisplayDialog("Feira", "Gere os prefabs primeiro (menu 1).", "OK"); return; }

        Physics.SyncTransforms();

        var player = GameObject.FindWithTag("Player");
        Vector3 centro = player ? player.transform.position : Vector3.zero;

        // Descobre a altura real do chão olhando para baixo a partir do Player
        if (!Physics.Raycast(centro + Vector3.up * 0.5f, Vector3.down, out RaycastHit h0, 20f, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("Nenhum chao encontrado embaixo do Player. O Piso esta abaixo/acima demais ou sem Collider?");
            return;
        }
        float chaoY = h0.point.y;

        var pai = new GameObject("Clientes");
        Undo.RegisterCreatedObjectUndo(pai, "Clientes");

        var usados = new List<Vector3>();
        int criados = 0, tentativas = 0;
        int semChao = 0, alturaDiferente = 0, obstaculo = 0, pertoDemais = 0;

        while (criados < 8 && tentativas++ < 1000)
        {
            Vector2 c = Random.insideUnitCircle * 14f;
            Vector3 origem = new Vector3(centro.x + c.x, chaoY + 1.5f, centro.z + c.y);

            if (!Physics.Raycast(origem, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore)) { semChao++; continue; }
            if (Mathf.Abs(hit.point.y - chaoY) > 0.3f || hit.normal.y < 0.8f) { alturaDiferente++; continue; }
            if (Physics.CheckCapsule(hit.point + Vector3.up * 0.35f, hit.point + Vector3.up * 1.65f, 0.3f, ~0, QueryTriggerInteraction.Ignore)) { obstaculo++; continue; }

            bool perto = (hit.point - centro).sqrMagnitude < 4f;
            foreach (var u in usados) if ((u - hit.point).sqrMagnitude < 2.25f) { perto = true; break; }
            if (perto) { pertoDemais++; continue; }

            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(inst, "Cliente");
            inst.name = "Cliente_" + (criados + 1);
            inst.transform.SetParent(pai.transform, true);
            inst.transform.position = hit.point;
            inst.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            usados.Add(hit.point);
            criados++;
        }

        Debug.Log(criados + " clientes criados. Pontos rejeitados -> sem chao: " + semChao +
                  ", altura diferente: " + alturaDiferente + ", obstaculo: " + obstaculo +
                  ", perto demais: " + pertoDemais);

        if (criados == 0) Undo.DestroyObjectImmediate(pai);
    }

    // ------------------------------------------------------------ CONSTRUÇÃO

    static void Criar(Tipo tipo)
    {
        string pre = tipo.ToString();
        bool player = tipo == Tipo.Player;

        Material mPele = Mat(pre + "_Pele", new Color(0.88f, 0.67f, 0.51f));
        Material mCabelo = Mat(pre + "_Cabelo", new Color(0.23f, 0.14f, 0.08f));
        Material mRoupa = Mat(pre + "_Roupa", player ? new Color(0.80f, 0.12f, 0.15f) : new Color(0.16f, 0.63f, 0.61f));
        Material mCalca = Mat(pre + "_Calca", player ? new Color(0.18f, 0.27f, 0.45f) : new Color(0.18f, 0.24f, 0.36f));
        Material mAvental = Mat(pre + "_Avental", new Color(0.96f, 0.90f, 0.76f));
        Material mSapato = Mat("Comum_Sapato", new Color(0.25f, 0.16f, 0.10f));
        Material mOlho = Mat("Comum_Olho", new Color(0.05f, 0.04f, 0.04f));
        Material mPalha = Mat("Comum_Palha", new Color(0.86f, 0.72f, 0.38f));

        var pele = new List<Renderer>(); var cabelo = new List<Renderer>();
        var roupa = new List<Renderer>(); var calca = new List<Renderer>();
        var avental = new List<Renderer>();

        var raiz = new GameObject(player ? "Player_Visual" : pre);
        Transform T = raiz.transform;

        // Tronco e quadril
        P("Torso", PrimitiveType.Cube, T, new Vector3(0, 1.125f, 0), new Vector3(0.42f, 0.55f, 0.24f), mRoupa, roupa);
        P("Pelvis", PrimitiveType.Cube, T, new Vector3(0, 0.84f, 0), new Vector3(0.40f, 0.20f, 0.23f), mCalca, calca);
        P("Pescoco", PrimitiveType.Cylinder, T, new Vector3(0, 1.43f, 0), new Vector3(0.08f, 0.04f, 0.08f), mPele, pele);

        // Pernas
        Transform pernaE = Pivo("PernaE", T, new Vector3(-0.10f, 0.85f, 0));
        Transform pernaD = Pivo("PernaD", T, new Vector3(0.10f, 0.85f, 0));
        foreach (var perna in new[] { pernaE, pernaD })
        {
            P("Coxa", PrimitiveType.Cylinder, perna, new Vector3(0, -0.375f, 0), new Vector3(0.17f, 0.375f, 0.17f), mCalca, calca);
            P("Sapato", PrimitiveType.Cube, perna, new Vector3(0, -0.80f, 0.04f), new Vector3(0.18f, 0.10f, 0.28f), mSapato, null);
        }

        // Braços
        Transform bracoE = Pivo("BracoE", T, new Vector3(-0.275f, 1.34f, 0));
        Transform bracoD = Pivo("BracoD", T, new Vector3(0.275f, 1.34f, 0));
        foreach (var braco in new[] { bracoE, bracoD })
        {
            P("Manga", PrimitiveType.Cylinder, braco, new Vector3(0, -0.14f, 0), new Vector3(0.13f, 0.14f, 0.13f), mRoupa, roupa);
            P("Antebraco", PrimitiveType.Cylinder, braco, new Vector3(0, -0.44f, 0), new Vector3(0.10f, 0.16f, 0.10f), mPele, pele);
            P("Mao", PrimitiveType.Sphere, braco, new Vector3(0, -0.63f, 0), new Vector3(0.11f, 0.11f, 0.11f), mPele, pele);
        }

        // Sacola de compras (mão direita)
        GameObject sacola = P("Sacola", PrimitiveType.Cube, bracoD, new Vector3(0, -0.76f, 0.04f), new Vector3(0.20f, 0.24f, 0.10f), mPalha, null);
        sacola.SetActive(tipo == Tipo.Cliente);

        // Cabeça
        Transform cabeca = Pivo("Cabeca", T, new Vector3(0, 1.58f, 0));
        P("Crânio", PrimitiveType.Sphere, cabeca, Vector3.zero, new Vector3(0.24f, 0.27f, 0.25f), mPele, pele);
        P("OlhoE", PrimitiveType.Sphere, cabeca, new Vector3(-0.055f, 0.01f, 0.105f), new Vector3(0.035f, 0.04f, 0.03f), mOlho, null);
        P("OlhoD", PrimitiveType.Sphere, cabeca, new Vector3(0.055f, 0.01f, 0.105f), new Vector3(0.035f, 0.04f, 0.03f), mOlho, null);
        P("Nariz", PrimitiveType.Sphere, cabeca, new Vector3(0, -0.025f, 0.118f), new Vector3(0.035f, 0.045f, 0.04f), mPele, pele);
        P("Cabelo", PrimitiveType.Sphere, cabeca, new Vector3(0, 0.065f, -0.02f), new Vector3(0.27f, 0.20f, 0.27f), mCabelo, cabelo);

        GameObject longo = P("CabeloLongo", PrimitiveType.Cube, cabeca, new Vector3(0, -0.08f, -0.105f), new Vector3(0.25f, 0.30f, 0.07f), mCabelo, cabelo);
        longo.SetActive(false);

        // Chapéu de palha
        Transform chapeu = Pivo("Chapeu", cabeca, new Vector3(0, 0.14f, 0));
        P("Aba", PrimitiveType.Cylinder, chapeu, Vector3.zero, new Vector3(0.42f, 0.006f, 0.42f), mPalha, null);
        P("Copa", PrimitiveType.Cylinder, chapeu, new Vector3(0, 0.06f, 0), new Vector3(0.18f, 0.06f, 0.18f), mPalha, null);
        chapeu.gameObject.SetActive(player);

        // Avental (vendedor)
        GameObject av = P("Avental", PrimitiveType.Cube, T, new Vector3(0, 1.07f, 0.125f), new Vector3(0.36f, 0.50f, 0.02f), mAvental, avental);
        av.SetActive(tipo == Tipo.Vendedor);

        // Componente visual
        var pv = raiz.AddComponent<PersonagemVisual>();
        pv.bracoE = bracoE; pv.bracoD = bracoD; pv.pernaE = pernaE; pv.pernaD = pernaD; pv.cabeca = cabeca;
        pv.rendPele = pele.ToArray(); pv.rendCabelo = cabelo.ToArray(); pv.rendRoupa = roupa.ToArray();
        pv.rendCalca = calca.ToArray(); pv.rendAvental = avental.ToArray();
        pv.cabeloLongo = longo; pv.chapeu = chapeu.gameObject; pv.sacola = sacola;

        switch (tipo)
        {
            case Tipo.Player:
                pv.sortearAparencia = false;
                pv.somenteSombra = true;
                break;
            case Tipo.Vendedor:
                pv.chanceChapeu = 0.6f; pv.chanceCabeloLongo = 0.4f; pv.chanceSacola = 0f;
                break;
            case Tipo.Cliente:
                pv.chanceChapeu = 0.3f; pv.chanceCabeloLongo = 0.5f; pv.chanceSacola = 0.5f;
                break;
        }

        // Colisor e IA
        if (!player)
        {
            var col = raiz.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.9f, 0);
            col.radius = 0.28f;
            col.height = 1.8f;
        }
        if (tipo == Tipo.Vendedor) raiz.AddComponent<VendedorNPC>();
        if (tipo == Tipo.Cliente) raiz.AddComponent<ClienteNPC>();

        string path = PastaPrefabs + "/" + raiz.name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(raiz, path);
        Object.DestroyImmediate(raiz);
    }

    // ------------------------------------------------------------ AJUDANTES

    static GameObject P(string nome, PrimitiveType tipo, Transform pai, Vector3 pos, Vector3 esc, Material mat, List<Renderer> lista)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nome;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(pai, false);
        go.transform.localPosition = pos;
        go.transform.localScale = esc;
        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        if (lista != null) lista.Add(r);
        return go;
    }

    static Transform Pivo(string nome, Transform pai, Vector3 pos)
    {
        var t = new GameObject(nome).transform;
        t.SetParent(pai, false);
        t.localPosition = pos;
        return t;
    }

    static Material Mat(string nome, Color cor)
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