using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Objetivo
{
    public string id;
    public string texto;
    public bool concluido;
}

/// <summary>
/// Lista de objetivos no canto da tela.
/// Uso: ObjetivosUI.Instancia.Adicionar("mel", "Comprar um mel");
///      ObjetivosUI.Instancia.Concluir("mel");
/// </summary>
public class ObjetivosUI : MonoBehaviour
{
    public static ObjetivosUI Instancia { get; private set; }

    [Header("Referências (preenchido pelo gerador)")]
    public GameObject modelo;
    public Transform pai;

    [Header("Objetivos iniciais (edite à vontade)")]
    public List<Objetivo> objetivos = new List<Objetivo>
    {
        new Objetivo { id = "mel", texto = "Comprar um mel" },
        new Objetivo { id = "vestido", texto = "Comprar um vestido" }
    };

    [Header("Cores")]
    public Color corNormal = new Color(0.23f, 0.16f, 0.125f);
    public Color corConcluido = new Color(0.50f, 0.45f, 0.40f);

    public event System.Action<Objetivo> AoConcluir;

    readonly List<GameObject> linhas = new List<GameObject>();

    void Awake() { Instancia = this; }
    void Start() { Reconstruir(); }

    public void Adicionar(string id, string texto)
    {
        if (objetivos.Exists(o => o.id == id)) return;
        objetivos.Add(new Objetivo { id = id, texto = texto });
        Reconstruir();
    }

    public void Concluir(string id)
    {
        var o = objetivos.Find(x => x.id == id);
        if (o == null || o.concluido) return;
        o.concluido = true;
        Reconstruir();
        AoConcluir?.Invoke(o);
    }

    public bool EstaConcluido(string id)
    {
        var o = objetivos.Find(x => x.id == id);
        return o != null && o.concluido;
    }

    public void Limpar()
    {
        objetivos.Clear();
        Reconstruir();
    }

    void Reconstruir()
    {
        foreach (var l in linhas) if (l) Destroy(l);
        linhas.Clear();
        if (modelo == null || pai == null) return;

        foreach (var o in objetivos)
        {
            var go = Instantiate(modelo, pai);
            go.name = "Obj_" + o.id;
            go.SetActive(true);

            var txt = go.transform.Find("Texto").GetComponent<TMP_Text>();
            var marca = go.transform.Find("Caixa/Fundo/Marca").GetComponent<Image>();
            txt.text = o.concluido ? "<s>" + o.texto + "</s>" : o.texto;
            txt.color = o.concluido ? corConcluido : corNormal;
            marca.enabled = o.concluido;

            linhas.Add(go);
        }
    }
}
