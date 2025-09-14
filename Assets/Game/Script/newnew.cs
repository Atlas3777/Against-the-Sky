using UnityEngine;

public class newnew : MonoBehaviour
{
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    [Header("References")]
    public Transform cam;             // камеру сюда
    public Transform aimTarget;      // точка в мире, куда смотреть
    public LayerMask aimMask;        // по чему можно целиться (например, ground + enemies)

    [Header("Movement")]
    public CharacterController controller;
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;

    [Header("Aiming")]
    public Animator animator;
    public Transform spine; // точка вращения прицела (например, Chest/Spine_2)

    Vector2 input;
    Vector3 velocity;
    
    void Update()
    {
        HandleInput();
        Move();
            //Aim();
        UpdateAnimator();
    }

    void HandleInput()
    {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void Move()
    {
        // Движение относительно камеры
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 move = camForward.normalized * input.y + camRight.normalized * input.x;
        controller.SimpleMove(move * moveSpeed);
    }


    void Aim()
    {
        // Кидаем луч от камеры по направлению мыши
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimMask))
        {
            aimTarget.position = hit.point;
        }

        // Поворот верхней части тела (ручная IK-имитация)
        Vector3 aimDir = aimTarget.position - spine.position;
        Quaternion lookRot = Quaternion.LookRotation(aimDir);
        spine.rotation = Quaternion.Slerp(spine.rotation, lookRot, 10f * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        animator.SetFloat(Vertical, input.y);
        animator.SetFloat(Horizontal, input.x);
        //animator.SetBool(Shoot, true); // включаем аим-слой
    }
}
