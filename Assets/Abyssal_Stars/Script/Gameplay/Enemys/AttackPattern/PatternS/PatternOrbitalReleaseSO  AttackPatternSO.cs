using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "New Orbital Release", menuName = "Boss Patterns/Attack/Orbital Release")]
public class PatternOrbitalReleaseSO : AttackPatternSO
{
    [Header("Configuración del Escudo")]
    [Min(3)] public int bulletCount = 8;
    public float orbitRadius = 2.5f;
    [Tooltip("Grados por segundo a los que gira el escudo.")]
    public float orbitSpeed = 120f;

    [Header("Tiempos del Ciclo")]
    public float orbitTime = 3.0f;
    public float cooldownTime = 1.0f;

    [Header("Disparo Ofensivo")]
    public float releaseSpeed = 8f;
    [Tooltip("Velocidad inicial al salir. Más bajo = arranque más lento y dramático.")]
    public float releaseStartSpeed = 1f;
    [Tooltip("Segundos que tarda cada bala en alcanzar releaseSpeed.")]
    public float accelerationTime = 1.2f;
    [Tooltip("Delay entre el disparo de cada bala, crea el efecto de ola.")]
    public float waveDelay = 0.06f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret == null) yield break;

        EnemyBase boss = turret.GetComponentInParent<EnemyBase>();
        Transform bossTransform = boss != null ? boss.transform : turret.transform;

        float angleStep = 360f / bulletCount;
        List<EnemyBullet> shieldBullets = new List<EnemyBullet>();

        while (true)
        {
            foreach (var old in shieldBullets)
                if (old != null) old.gameObject.SetActive(false);
            shieldBullets.Clear();

            for (int i = 0; i < bulletCount; i++)
            {
                EnemyBullet realBullet = turret.SpawnBulletWithoutFiring();
                if (realBullet != null)
                {
                    realBullet.gameObject.SetActive(true);
                    realBullet.transform.SetParent(bossTransform);
                    realBullet.SetShieldMode(true);

                    Rigidbody2D rb = realBullet.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.bodyType = RigidbodyType2D.Kinematic;
                    }

                    shieldBullets.Add(realBullet);
                }
            }

            float timer = 0f;
            float currentRotation = 0f;

            while (timer < orbitTime)
            {
                currentRotation += orbitSpeed * Time.deltaTime;
                Vector3 center = bossTransform.position;

                for (int i = 0; i < shieldBullets.Count; i++)
                {
                    if (shieldBullets[i] == null || !shieldBullets[i].gameObject.activeInHierarchy)
                        continue;

                    float angle = currentRotation + (angleStep * i);
                    float rad = ApplyMirror(angle) * Mathf.Deg2Rad;

                    float x = Mathf.Cos(rad);
                    float y = Mathf.Sin(rad);

                    shieldBullets[i].transform.position = center + new Vector3(x, y, 0f) * orbitRadius;

                    float lookAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
                    shieldBullets[i].transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
                }

                timer += Time.deltaTime;
                yield return null;
            }

            Vector3 releaseCenter = bossTransform.position;

            for (int i = 0; i < shieldBullets.Count; i++)
            {
                if (shieldBullets[i] != null && shieldBullets[i].gameObject.activeInHierarchy)
                {
                    shieldBullets[i].transform.SetParent(null);
                    shieldBullets[i].SetShieldMode(false);

                    Vector2 outwardDirection = (shieldBullets[i].transform.position - releaseCenter).normalized;

                    shieldBullets[i].Fire(outwardDirection, releaseStartSpeed);

                    turret.StartCoroutine(
                        AccelerateBullet(shieldBullets[i], outwardDirection)
                    );

                    yield return new WaitForSeconds(waveDelay);
                }
            }

            yield return new WaitForSeconds(cooldownTime);
        }
    }

    private IEnumerator AccelerateBullet(EnemyBullet bullet, Vector2 direction)
    {
        float elapsed = 0f;

        while (elapsed < accelerationTime)
        {
            if (bullet == null || !bullet.gameObject.activeInHierarchy)
                yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / accelerationTime;

            float currentSpeed = Mathf.Lerp(releaseStartSpeed, releaseSpeed, t * t);
            bullet.Velocity = direction * currentSpeed;

            yield return null;
        }

        if (bullet != null && bullet.gameObject.activeInHierarchy)
            bullet.Velocity = direction * releaseSpeed;
    }
}