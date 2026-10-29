using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public CameraController cameraController;   // przeciągnij Main Camera
    public Animator animator;                    // przeciągnij Animator z Player

    public float walkSpeed = 2f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    public float mouseSensitivity = 2f;
    public float turnSpeed = 540f;               // obrót w stronę celu w top-down

    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;

    CharacterController cc;
    float verticalVelocity;
    bool isCrouching;
    bool hasTarget;
    Vector3 target;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        bool topDown = cameraController.topDown;
        bool grounded = cc.isGrounded;
        if (grounded && verticalVelocity < 0f) verticalVelocity = -2f;

        // ---------- Kucanie (C) ----------
        isCrouching = Input.GetKey(KeyCode.C) && !topDown;
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * 12f);
        cc.center = new Vector3(0f, cc.height / 2f, 0f);

        float speed = isCrouching ? crouchSpeed : walkSpeed;
        Vector3 move = Vector3.zero;

        if (!topDown)
        {
            // ---------- Obrót myszą (pkt 5) ----------
            transform.Rotate(0f, Input.GetAxis("Mouse X") * mouseSensitivity, 0f);

            // ---------- Ruch WASD (pkt 3) ----------
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            move = (transform.right * h + transform.forward * v);
            if (move.magnitude > 1f) move.Normalize();
            move *= speed;
            hasTarget = false;   // anuluj ruch po kliknięciu po powrocie do TPP

            // ---------- Skok (Left Shift) ----------
            if (Input.GetKeyDown(KeyCode.LeftShift) && grounded && !isCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (Input.GetKeyDown(KeyCode.LeftShift) && grounded && !isCrouching)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
        }
        else
        {
            // ---------- Ruch do punktu klikniętego PPM (pkt 8) ----------
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = cameraController.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    target = hit.point;
                    hasTarget = true;
                }
            }

            if (hasTarget)
            {
                Vector3 toTarget = target - transform.position;
                toTarget.y = 0f;

                if (toTarget.magnitude < 0.15f)
                {
                    hasTarget = false;
                }
                else
                {
                    Vector3 dir = toTarget.normalized;
                    Quaternion look = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
                    move = dir * walkSpeed;
                }
            }
        }

        // ---------- Grawitacja i ruch z kolizjami (pkt 4) ----------
        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = move + Vector3.up * verticalVelocity;
        cc.Move(velocity * Time.deltaTime);

        // ---------- Animator ----------
        if (animator != null)
        {
            bool inAir = !cc.isGrounded;
            animator.SetFloat("Speed", new Vector3(move.x, 0f, move.z).magnitude);
            animator.SetFloat("MotionSpeed", 1f);
            animator.SetBool("Grounded", cc.isGrounded);
            animator.SetBool("Jump", inAir && verticalVelocity > 0f);
            animator.SetBool("FreeFall", inAir && verticalVelocity <= 0f);
            animator.SetBool("Crouching", isCrouching);
        }
    }
}