
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private CharacterController controlador;

    public Transform camera;
    public Rigidbody corpo;
    public float velocidade = 9f;
    public Vector2 movimentacao;
    public Vector2 olhar;

    float rotacaoVertical = 0.0f;

    void Start()
    {
        controlador = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector3 arrumandoMov = new Vector3(movimentacao.x, 0.0f, movimentacao.y);

        Vector3 direcaoMundo = transform.TransformDirection(arrumandoMov);

        float alturaAntes = transform.position.y;

        controlador.Move(direcaoMundo * velocidade * Time.deltaTime);

        float diferencaY = transform.position.y - alturaAntes;

        if (Mathf.Abs(diferencaY) > 0.01f)
        {
            Debug.LogWarning("Move alterou Y: " + diferencaY);
        }

        float rotacaoHorizontal = olhar.x * velocidade * Time.deltaTime;
        transform.Rotate(0, rotacaoHorizontal, 0);

        rotacaoVertical -= olhar.y * velocidade * Time.deltaTime;
        rotacaoVertical = Mathf.Clamp(rotacaoVertical, -80f, 80f);
        camera.localRotation = Quaternion.Euler(rotacaoVertical, 0, 0);
    }

    void OnMove(InputValue value)
    {
        //Debug.Log("Movimentando" + movimentacao);
        movimentacao = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        //Debug.Log("Olhando" + olhar);
        olhar = value.Get<Vector2>();   
    }
}
