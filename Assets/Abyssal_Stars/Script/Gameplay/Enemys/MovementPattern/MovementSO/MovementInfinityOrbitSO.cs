using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Infinity Orbit", menuName = "Boss Patterns/Movement/Infinity Orbit")]
public class MovementInfinityOrbitSO : MovementPatternSO
{
    [Header("Fase 2: Órbita del Caos (Figura de 8)")]
    [Tooltip("Qué tan rápido traza el símbolo de infinito.")]
    [SerializeField] private float orbitSpeed = 2f;

    [Tooltip("Qué tan ancho es el movimiento de lado a lado.")]
    [SerializeField] private float width = 5f;

    [Tooltip("Qué tan alto/bajo llega en los picos del 8.")]
    [SerializeField] private float height = 3f;

    [Header("Efecto de Temblor Permanente")]
    [SerializeField] private bool applyJitter = true;
    [SerializeField] private float jitterIntensity = 0.05f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float timer = 0f;

        while (true)
        {
            timer += Time.deltaTime * orbitSpeed;

            float xOffset = Mathf.Sin(timer) * width;
            float yOffset = Mathf.Sin(timer * 2f) * (height / 2f);

            Vector3 basePosition = new Vector3(zoneCenter.x + xOffset, zoneCenter.y + yOffset, bossTransform.localPosition.z);

            if (applyJitter)
            {
                Vector2 randomShake = Random.insideUnitCircle * jitterIntensity;
                bossTransform.localPosition = basePosition + (Vector3)randomShake;
            }
            else
            {
                bossTransform.localPosition = basePosition;
            }

            yield return null;
        }
    }

    public override void OnStopped(Transform bossTransform)
    {
    }
}