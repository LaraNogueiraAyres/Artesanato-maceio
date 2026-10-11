using UnityEngine;

/// <summary>
/// Vendedor fica no lugar, acena e vira para o jogador quando ele chega perto
/// e volta a olhar para a frente quando ele vai embora.
/// </summary>
public class VendedorNPC : MonoBehaviour
{
    public Transform jogador;
    public float raioAtencao = 4f;
    public float velocidadeGiro = 5f;
    public bool acenarAoChegar = true;

    PersonagemVisual visual;
    Quaternion rotacaoInicial;
    bool jogadorDentro;

    void Start()
    {
        visual = GetComponent<PersonagemVisual>();
        rotacaoInicial = transform.rotation;
        if (jogador == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) jogador = p.transform;
        }
    }

    void Update()
    {
        if (jogador == null) return;

        Vector3 d = jogador.position - transform.position;
        d.y = 0f;
        bool perto = d.sqrMagnitude <= raioAtencao * raioAtencao;

        if (perto)
        {
            if (!jogadorDentro && acenarAoChegar && visual) visual.Acenar(1.8f);
            if (d.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(d), Time.deltaTime * velocidadeGiro);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoInicial,
                Time.deltaTime * velocidadeGiro);
        }
        jogadorDentro = perto;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioAtencao);
    }
}
