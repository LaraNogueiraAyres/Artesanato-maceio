using UnityEngine;

/// <summary>
/// Cliente que passeia pela feira: anda até um destino, para um pouco, escolhe outro.
/// Sem pontos definidos, ele vaga aleatoriamente ao redor de onde nasceu.
/// Desvia de obstáculos parando e escolhendo outro caminho (precisa de Colliders nas barracas).
/// </summary>
public class ClienteNPC : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 1.6f;
    public float velocidadeGiro = 6f;
    public Vector2 tempoParada = new Vector2(1.5f, 5f);

    [Header("Destinos")]
    [Tooltip("Opcional: pontos da feira por onde ele pode passar.")]
    public Transform[] pontos;
    [Tooltip("Usado quando não há pontos: raio de passeio ao redor do ponto inicial.")]
    public float raioVagar = 10f;

    [Header("Obstáculos")]
    public float distanciaObstaculo = 1.0f;
    public LayerMask camadasObstaculo = ~0;

    [Header("Comportamento")]
    public bool olharParaJogador = true;
    public float distanciaOlhar = 3f;

    Vector3 origem, destino;
    float espera;
    bool andando;
    Transform jogador;
    readonly RaycastHit[] hits = new RaycastHit[8];

    void Start()
    {
        origem = transform.position;
        espera = Random.Range(0f, tempoParada.y);
        var p = GameObject.FindWithTag("Player");
        if (p) jogador = p.transform;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (!andando)
        {
            if (olharParaJogador && jogador) OlharPara(jogador.position, dt);
            espera -= dt;
            if (espera <= 0f) { EscolherDestino(); andando = true; }
            return;
        }

        Vector3 dir = destino - transform.position;
        dir.y = 0f;
        if (dir.magnitude < 0.3f) { Parar(); return; }

        OlharPara(destino, dt);

        if (Bloqueado()) { Parar(); return; }
        transform.position += transform.forward * velocidade * dt;
    }

    void OlharPara(Vector3 alvo, float dt)
    {
        Vector3 d = alvo - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.01f) return;
        if (!andando && d.magnitude > distanciaOlhar) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), dt * velocidadeGiro);
    }

    bool Bloqueado()
    {
        Vector3 o = transform.position + Vector3.up * 0.9f;
        int n = Physics.SphereCastNonAlloc(o, 0.25f, transform.forward, hits,
                                           distanciaObstaculo, camadasObstaculo,
                                           QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (!hits[i].collider.transform.IsChildOf(transform)) return true;
        return false;
    }

    void Parar()
    {
        andando = false;
        espera = Random.Range(tempoParada.x, tempoParada.y);
    }

    void EscolherDestino()
    {
        if (pontos != null && pontos.Length > 0)
        {
            Vector3 p = pontos[Random.Range(0, pontos.Length)].position;
            destino = new Vector3(p.x, transform.position.y, p.z);
        }
        else
        {
            Vector2 c = Random.insideUnitCircle * raioVagar;
            destino = new Vector3(origem.x + c.x, transform.position.y, origem.z + c.y);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? origem : transform.position, raioVagar);
    }
}
