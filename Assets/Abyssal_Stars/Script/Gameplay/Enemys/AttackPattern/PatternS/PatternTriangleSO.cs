using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Triangle", menuName = "Boss Patterns/Attack/Triangle")]
public class PatternTriangleSO : AttackPatternSO
{
    [Header("Forma del Triángulo")]
    [Tooltip("Cuántas balas forman cada lado (sin contar el vértice).")]
    [Min(1)]
    [SerializeField] private int bulletsPerSide = 4;

    [Tooltip("Ángulo total de apertura de la base.")]
    [Range(20f, 160f)]
    [SerializeField] private float baseSpreadAngle = 80f;

    [Header("Velocidad")]
    [Tooltip("Velocidad de TODAS las balas. Escala el triángulo, pero no deforma su forma.")]
    [Min(0.01f)]
    [SerializeField] private float bulletSpeed = 5f;

    [Header("Espaciado visual")]
    [Tooltip(
        "Tiempo entre cada fila (punta → base). " +
        "Controla la 'altura' del triángulo independientemente de la velocidad. " +
        "Más alto = triángulo más alargado."
    )]
    [Min(0.005f)]
    [SerializeField] private float rowDelay = 0.08f;

    [Header("Cadencia")]
    [Tooltip("Pausa entre olas, contada desde el último disparo de la ola anterior.")]
    [Min(0f)]
    [SerializeField] private float wavePause = 1.0f;

    [Tooltip("Ángulo central del triángulo. 270 = apunta hacia abajo.")]
    [SerializeField] private float centerAngle = 270f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            yield return FireTriangle(turret);  
            yield return new WaitForSeconds(wavePause);
        }
    }

    private IEnumerator FireTriangle(BossTurret turret)
    {
        float angleStep = (baseSpreadAngle * 0.5f) / bulletsPerSide;

        turret.FireSingleBullet(ApplyMirror(centerAngle), bulletSpeed);

        for (int i = 1; i <= bulletsPerSide; i++)
        {
            yield return new WaitForSeconds(rowDelay);

            float offset = angleStep * i;
            turret.FireSingleBullet(ApplyMirror(centerAngle + offset), bulletSpeed);
            turret.FireSingleBullet(ApplyMirror(centerAngle - offset), bulletSpeed);
        }
    }
}