using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Triangle", menuName = "Boss Patterns/Attack/Triangle")]
public class PatternTriangleSO : AttackPatternSO
{
    [Header("Forma del Triángulo")]
    [Tooltip("Cuántas balas forman cada lado del triángulo (sin contar el vértice).")]
    [Min(1)]
    public int bulletsPerSide = 4;

    [Tooltip("Ángulo total de apertura de la base del triángulo.")]
    [Range(20f, 160f)]
    public float baseSpreadAngle = 80f;

    [Tooltip("Velocidad de la bala del vértice (la más rápida — define la punta).")]
    public float tipSpeed = 8f;

    [Tooltip("Velocidad de las balas de la base (más lentas — definen la base).")]
    public float baseSpeed = 3.5f;

    [Header("Cadencia")]
    [Tooltip("Pausa entre olas de triángulo.")]
    public float wavePause = 1.2f;

    [Tooltip("Ángulo central. 270 = triángulo apuntando hacia abajo.")]
    public float centerAngle = 270f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            FireTriangle(turret);
            yield return new WaitForSeconds(wavePause);
        }
    }

    private void FireTriangle(BossTurret turret)
    {
        turret.FireSingleBullet(ApplyMirror(centerAngle), tipSpeed);


        float angleStep = (baseSpreadAngle * 0.5f) / bulletsPerSide;

        for (int i = 1; i <= bulletsPerSide; i++)
        {
            float t = (float)i / bulletsPerSide;
            float speed = Mathf.Lerp(tipSpeed, baseSpeed, t);
            float offset = angleStep * i;

            turret.FireSingleBullet(ApplyMirror(centerAngle + offset), speed);
            turret.FireSingleBullet(ApplyMirror(centerAngle - offset), speed);
        }
    }
}