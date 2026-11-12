using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public CameraController cameraController;   
    public GameObject weapon;                    
    public Transform muzzle;                     
    public Projectile[] projectilePrefabs;       
    public float projectileSpeed = 30f;
    public float fireCooldown = 0.25f;
    public bool requireAim = true;               

    public bool IsAiming { get; private set; }

    int currentType = 0;
    float nextFireTime = 0f;
    Camera cam;
    Collider[] ownColliders;
    GUIStyle messageStyle;

    void Awake()
    {
        ownColliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        cam = cameraController.GetComponent<Camera>();
        if (weapon != null) weapon.SetActive(false);
    }

    void Update()
    {
        bool topDown = cameraController.topDown;

        // Celowanie na PPM 
        IsAiming = !topDown && Input.GetMouseButton(1);
        if (weapon != null) weapon.SetActive(IsAiming);

        if (topDown) return;

        // Zmiana typu pocisku klawiszem Q 
        if (Input.GetKeyDown(KeyCode.Q))
            currentType = (currentType + 1) % projectilePrefabs.Length;

        // Strzał na LPM 
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime && (IsAiming || !requireAim))
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        // punkt na środku ekranu
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 aimPoint = ray.GetPoint(200f);
        float best = Mathf.Infinity;

        foreach (RaycastHit h in Physics.RaycastAll(ray, 200f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.transform.IsChildOf(transform)) continue;                 
            if (h.collider.GetComponent<Projectile>() != null) continue;    
            if (h.distance < best) { best = h.distance; aimPoint = h.point; }
        }

        Vector3 dir = (aimPoint - muzzle.position).normalized;

        Projectile p = Instantiate(projectilePrefabs[currentType], muzzle.position, Quaternion.LookRotation(dir));

        
        Collider pc = p.GetComponent<Collider>();
        foreach (Collider c in ownColliders) Physics.IgnoreCollision(pc, c);

        p.Launch(dir, projectileSpeed);
    }

    // Prosty interfejs
    void OnGUI()
    {
        if (messageStyle == null)
        {
            messageStyle = new GUIStyle(GUI.skin.label);
            messageStyle.fontSize = 28;
            messageStyle.fontStyle = FontStyle.Bold;
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.normal.textColor = Color.yellow;
        }

        if (IsAiming)
        {
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(Screen.width / 2f - 3f, Screen.height / 2f - 3f, 6f, 6f), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        if (projectilePrefabs != null && projectilePrefabs.Length > 0)
            GUI.Label(new Rect(10, Screen.height - 40, 500, 30),
                "Pocisk: " + projectilePrefabs[currentType].typeName + "  (Q - zmiana)");

        if (Time.time < Target.MessageUntil)
            GUI.Label(new Rect(0, 60, Screen.width, 50), Target.LastMessage, messageStyle);
    }
}