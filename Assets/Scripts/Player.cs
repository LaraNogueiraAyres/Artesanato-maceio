using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody corpo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        corpo.MovePosition(corpo.position + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime);
        corpo.MoveRotation(corpo.rotation * Quaternion.Euler(new Vector3(0, Input.GetAxis("Mouse X"), 0)));
        corpo.MoveRotation(corpo.rotation * Quaternion.Euler(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0)));
        corpo.AddForce(new Vector3(0, Input.GetAxis("Jump"), 0) * 5f, ForceMode.Impulse);
    }
}
