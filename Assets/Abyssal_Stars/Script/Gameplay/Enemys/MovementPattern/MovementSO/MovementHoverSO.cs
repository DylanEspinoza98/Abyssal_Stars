using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Hover", menuName = "Boss Patterns/Movement/Hover")]
public class MovementHoverSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Amplitud de la vibración (muy pequeña para que sea sutil).")]
    [SerializeField] private float vibrateAmplitude = 0.08f;

    [Tooltip("Velocidad de la vibración.")]
    [SerializeField] private float vibrateSpeed = 8f;

    [Tooltip("Velocidad para llegar al centro al iniciar la fase.")]
    [SerializeField] private float returnSpeed = 3f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        bool useLocal = bossTransform.parent != null
                     && bossTransform.parent.GetComponent<Camera>() != null;

        Vector3 GetCurrentPos() => useLocal
            ? bossTransform.localPosition
            : bossTransform.position;

        void SetPos(Vector3 pos)
        {
            if (useLocal) bossTransform.localPosition = pos;
            else bossTransform.position = pos;
        }

        float z = GetCurrentPos().z;
        Vector3 center = new Vector3(zoneCenter.x, zoneCenter.y, z);

        while (Vector3.Distance(GetCurrentPos(), center) > 0.05f)
        {
            SetPos(Vector3.MoveTowards(GetCurrentPos(), center, returnSpeed * Time.deltaTime));
            yield return null;
        }

        float timeAccum = 0f;
        while (true)
        {
            timeAccum += Time.deltaTime;
            float offsetX = Mathf.Sin(timeAccum * vibrateSpeed) * vibrateAmplitude;
            float offsetY = Mathf.Sin(timeAccum * vibrateSpeed * 1.3f) * vibrateAmplitude;

            SetPos(new Vector3(center.x + offsetX, center.y + offsetY, center.z));
            yield return null;
        }
    }
}