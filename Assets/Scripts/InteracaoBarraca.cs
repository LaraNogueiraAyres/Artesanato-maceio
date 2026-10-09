
using UnityEngine;
using UnityEngine.InputSystem;

public class InteracaoBarraca : MonoBehaviour
{
    public string nomeBarraca = "Barraca de Bebidas";
    public string mensagem = "Bem-vindo à barraca de bebidas! Conheça os sabores da feira de Alagoas.";

    private bool jogadorPerto = false;

    void Update()
    {
        if (jogadorPerto && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log(nomeBarraca + ": " + mensagem);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = true;
            Debug.Log("Pressione E para interagir com " + nomeBarraca);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            Debug.Log("Você saiu da área da barraca.");
        }
    }
}