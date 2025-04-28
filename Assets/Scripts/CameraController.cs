using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float shiftMultiplier = 2f;
    public float rotationSpeed = 200f;

    private Vector3 lastMousePosition;
    private float totalZoom = 0;

    void Update()
    {
        HandleKeyboardMovement();
        HandleMouseRotation();
    }

    void HandleKeyboardMovement()
    {
        float speed = moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= shiftMultiplier;

        Vector3 move = Vector3.zero;
        
        // WASD移动
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        
        // QE升降
        if (Input.GetKey(KeyCode.Q)) move -= Vector3.up;
        if (Input.GetKey(KeyCode.E)) move += Vector3.up;

        if (move != Vector3.zero)
            transform.position += move.normalized * speed;
    }

    void HandleMouseRotation()
    {
        if (Input.GetMouseButtonDown(1)) // 右键按下
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)) // 右键按住
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // 绕Y轴旋转（水平移动）
            transform.RotateAround(transform.position, Vector3.up, 
                delta.x * rotationSpeed * Time.deltaTime);
                
            // 绕X轴旋转（垂直移动）
            transform.RotateAround(transform.position, transform.right, 
                -delta.y * rotationSpeed * Time.deltaTime);
        }
    }

}