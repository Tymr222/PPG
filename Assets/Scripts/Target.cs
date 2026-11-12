using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static string LastMessage = "";
    public static float MessageUntil = 0f;
    public static int HitCount = 0;

    public Color hitColor = Color.red;
    public float flashTime = 0.3f;

    Renderer rend;
    Color originalColor;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;
    }

    public void Hit(string projectileName)
    {
        HitCount++;
        LastMessage = "TRAFIENIE w tarczę! (pocisk: " + projectileName + ")  Trafień: " + HitCount;
        MessageUntil = Time.time + 2f;

        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        rend.material.color = hitColor;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = originalColor;
    }
}