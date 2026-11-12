using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public int piecesPerAxis = 3;        
    public float explosionForce = 2f;
    public float pieceLifetime = 5f;
    bool broken = false;

    public void Break(Vector3 hitPoint)
    {
        if (broken) return;
        broken = true;

        Vector3 size = transform.lossyScale;
        Material mat = GetComponent<Renderer>().sharedMaterial;
        int n = piecesPerAxis;
        Vector3 pieceScale = new Vector3(size.x / n, size.y / n, size.z / n);

        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                {
                    Vector3 local = new Vector3(
                        -0.5f + (x + 0.5f) / n,
                        -0.5f + (y + 0.5f) / n,
                        -0.5f + (z + 0.5f) / n);

                    GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    piece.transform.position = transform.TransformPoint(local);
                    piece.transform.rotation = transform.rotation;
                    piece.transform.localScale = pieceScale;
                    piece.GetComponent<Renderer>().sharedMaterial = mat;

                    Rigidbody rb = piece.AddComponent<Rigidbody>();
                    rb.mass = 0.2f;
                    rb.AddExplosionForce(explosionForce, hitPoint, size.magnitude, 0.3f, ForceMode.Impulse);

                    Destroy(piece, pieceLifetime);
                }

        Destroy(gameObject);
    }
}