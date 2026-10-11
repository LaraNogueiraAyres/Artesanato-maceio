
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteracaoBarraca : MonoBehaviour
{
    public string nomeBarraca = "Barraca de Bebidas";
    [TextArea]
    public string mensagem = "Bem-vindo à barraca de bebidas! Conheça os sabores da feira de Alagoas.";

    public TMP_Text textoInteracao;
    public GameObject painelDialogo;
    public TMP_Text textoDialogo;

    private bool jogadorPerto = false;
    private bool dialogoAberto = false;

    void Start()
    {
        if (textoInteracao != null)
            textoInteracao.gameObject.SetActive(false);

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    void Update()
    {
        if (textoInteracao != null)
            textoInteracao.gameObject.SetActive(jogadorPerto && !dialogoAberto);

        if (jogadorPerto && Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!dialogoAberto)
                AbrirDialogo();
            else
                FecharDialogo();
        }
    }

    void AbrirDialogo()
    {
        dialogoAberto = true;

        if (painelDialogo != null)
            painelDialogo.SetActive(true);

        if (textoDialogo != null)
            textoDialogo.text = mensagem;

        if (textoInteracao != null)
            textoInteracao.gameObject.SetActive(false);
    }

    void FecharDialogo()
    {
        dialogoAberto = false;

        if (painelDialogo != null)
            painelDialogo.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            jogadorPerto = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jogadorPerto = false;
            FecharDialogo();
        }
    }
}