using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public bool topDown = false;

    [Header("Kamera TPP")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 3f;
    public float pivotHeight = 1.6f;
    public float mouseSensitivity = 2f;
    public float pitchMin = -20f;
    public float pitchMax = 60f;

    [Header("Kamera top-down")]
    public float topMoveSpeed = 15f;
    public float topHeight = 15f;
    public float topMinHeight = 5f;
    public float topMaxHeight = 40f;
    public float topAngle = 60f;
    public float topZoomSpeed = 5f;

    [Header("Przejście")]
    public float blendTime = 0.5f;

    float pitch = 15f;
    float blend = 0f;            // 0 = TPP, 1 = top-down
    Vector3 focus;               // punkt na ziemi, na który patrzy kamera top-down

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // ---------- Przełączanie SPACJĄ (pkt 7) ----------
        if (Input.GetKeyDown(KeyCode.Space))
        {
            topDown = !topDown;
            if (topDown)
            {
                focus = player.position;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // płynne przejście między kamerami
        float step = Time.deltaTime / blendTime;
        blend = Mathf.MoveTowards(blend, topDown ? 1f : 0f, step);
        float t = Mathf.SmoothStep(0f, 1f, blend);

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // ---------- Obliczenia kamery TPP (pkt 5, 6) ----------
        if (!topDown)
        {
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
        }

        Quaternion tppRot = Quaternion.Euler(pitch, player.eulerAngles.y, 0f);
        Vector3 pivot = player.position + Vector3.up * pivotHeight;
        Vector3 tppPos = pivot - tppRot * Vector3.forward * distance;

        // kamera nie wchodzi w ściany
        if (Physics.Linecast(pivot, tppPos, out RaycastHit hit))
        {
            if (hit.transform != player)
                tppPos = hit.point + hit.normal * 0.2f;
        }

        // ---------- Obliczenia kamery top-down (pkt 7) ----------
        if (topDown)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dir = new Vector3(h, 0f, v);
            if (dir.magnitude > 1f) dir.Normalize();
            focus += dir * topMoveSpeed * Time.deltaTime;

            topHeight = Mathf.Clamp(topHeight - scroll * topZoomSpeed * 10f, topMinHeight, topMaxHeight);
        }

        Quaternion topRot = Quaternion.Euler(topAngle, 0f, 0f);
        Vector3 topPos = focus + new Vector3(0f, topHeight, -topHeight / Mathf.Tan(topAngle * Mathf.Deg2Rad));

        // ---------- Ustawienie kamery ----------
        transform.position = Vector3.Lerp(tppPos, topPos, t);
        transform.rotation = Quaternion.Slerp(tppRot, topRot, t);
    }
}