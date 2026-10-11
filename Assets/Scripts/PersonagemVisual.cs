using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Cuida da aparência (cores/acessórios aleatórios) e da animação procedural
/// (andar, respirar, acenar) de qualquer personagem da feira.
/// Funciona sem Animator: a velocidade é lida pelo deslocamento do objeto.
/// </summary>
public class PersonagemVisual : MonoBehaviour
{
    [Header("Partes (preenchido pelo gerador)")]
    public Transform bracoE, bracoD, pernaE, pernaD, cabeca;
    public Renderer[] rendPele, rendCabelo, rendRoupa, rendCalca, rendAvental;
    public GameObject cabeloLongo, chapeu, sacola;

    [Header("Aparência")]
    [Tooltip("Sorteia cores, chapéu, cabelo e altura ao iniciar.")]
    public bool sortearAparencia = true;
    [Range(0, 1)] public float chanceChapeu = 0.3f;
    [Range(0, 1)] public float chanceCabeloLongo = 0.5f;
    [Range(0, 1)] public float chanceSacola = 0.5f;
    public bool sortearAltura = true;

    [Tooltip("Para o Player em 1ª pessoa: o corpo não aparece, só a sombra.")]
    public bool somenteSombra = false;

    [Header("Animação")]
    public float radPorMetro = 3.5f;
    public float anguloBraco = 35f;
    public float anguloPerna = 30f;

    Vector3 ultimaPos;
    float fase, intensidade, tempo, tempoAceno;

    // ---------- Paletas ----------
    static Color C(int r, int g, int b) { return new Color(r / 255f, g / 255f, b / 255f); }
    static readonly Color[] Peles = {
        C(241,201,165), C(224,172,131), C(198,134,66), C(160,105,63), C(141,85,36), C(107,66,38), C(74,44,23) };
    static readonly Color[] Cabelos = {
        C(26,18,9), C(59,35,20), C(107,68,35), C(201,162,90), C(122,46,30), C(138,138,138), C(17,17,17) };
    static readonly Color[] Roupas = {
        C(228,87,46), C(41,161,156), C(243,167,18), C(168,198,134), C(102,155,188),
        C(192,108,132), C(246,114,128), C(53,92,125), C(245,245,235), C(150,120,190) };
    static readonly Color[] Calcas = {
        C(47,62,92), C(91,75,58), C(58,58,58), C(122,106,83), C(31,78,95) };
    static readonly Color[] Aventais = {
        C(246,231,193), C(209,73,91), C(0,121,140), C(237,174,73) };

    static Color Sorteia(Color[] p) { return p[Random.Range(0, p.Length)]; }

    void Awake()
    {
        ultimaPos = transform.position;
        if (sortearAparencia) Sortear();
        if (somenteSombra) AplicarSomenteSombra();
    }

    [ContextMenu("Sortear aparência")]
    public void Sortear()
    {
        Pintar(rendPele, Sorteia(Peles));
        Pintar(rendCabelo, Sorteia(Cabelos));
        Pintar(rendRoupa, Sorteia(Roupas));
        Pintar(rendCalca, Sorteia(Calcas));
        Pintar(rendAvental, Sorteia(Aventais));

        if (chapeu) chapeu.SetActive(Random.value < chanceChapeu);
        if (cabeloLongo) cabeloLongo.SetActive(Random.value < chanceCabeloLongo);
        if (sacola) sacola.SetActive(Random.value < chanceSacola);

        if (sortearAltura) transform.localScale *= Random.Range(0.93f, 1.07f);
    }

    static void Pintar(Renderer[] rs, Color cor)
    {
        if (rs == null || rs.Length == 0 || rs[0] == null) return;
        Material m = new Material(rs[0].sharedMaterial); // 1 material por categoria
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
        if (m.HasProperty("_Color")) m.SetColor("_Color", cor);
        foreach (var r in rs) if (r) r.sharedMaterial = m;
    }

    void AplicarSomenteSombra()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    public void Acenar(float duracao = 1.8f) { tempoAceno = duracao; }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        tempo += dt;

        Vector3 delta = transform.position - ultimaPos;
        delta.y = 0f;
        ultimaPos = transform.position;
        float vel = delta.magnitude / dt;
        if (vel > 20f) vel = 0f; // teleporte: ignora

        intensidade = Mathf.Lerp(intensidade, Mathf.Clamp01(vel / 1.2f), dt * 10f);
        fase += Mathf.Min(vel * radPorMetro, 12f) * dt;

        float s = Mathf.Sin(fase) * intensidade;
        float parado = 1f - intensidade;
        float balanco = Mathf.Sin(tempo * 1.3f) * 2f * parado;

        Girar(pernaE, Quaternion.Euler(s * anguloPerna, 0, 0), dt);
        Girar(pernaD, Quaternion.Euler(-s * anguloPerna, 0, 0), dt);
        Girar(bracoE, Quaternion.Euler(-s * anguloBraco, 0, balanco), dt);

        if (tempoAceno > 0f)
        {
            tempoAceno -= dt;
            Girar(bracoD, Quaternion.Euler(-150f, 0, Mathf.Sin(tempo * 14f) * 18f), dt);
        }
        else
            Girar(bracoD, Quaternion.Euler(s * anguloBraco, 0, -balanco), dt);

        Girar(cabeca, Quaternion.Euler(Mathf.Sin(tempo * 1.1f) * 1.5f * parado,
                                       Mathf.Sin(tempo * 0.6f) * 5f * parado, 0), dt);
    }

    static void Girar(Transform t, Quaternion alvo, float dt)
    {
        if (t) t.localRotation = Quaternion.Slerp(t.localRotation, alvo, dt * 14f);
    }
}
