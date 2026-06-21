using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shield Grant", menuName = "Boss Patterns/Attack/Shield Grant")]
public class PatternShieldGrantSO : AttackPatternSO
{
    [Header("Escudo")]
    [Tooltip("Prefab del escudo. Debe tener tag 'BulletShield', CircleCollider2D trigger " +
             "y el componente EnemyShield.")]
    public GameObject shieldPrefab;

    [Tooltip("Si está activo, el Monje también se protege a sí mismo.")]
    public bool shieldSelf = false;

    [Header("Escaneo")]
    [Tooltip("Cada cuántos segundos busca enemigos nuevos sin escudo. " +
             "Útil si spawnean enemigos después del Monje.")]
    public float scanInterval = 1.5f;

    [Header("Auto-Ajuste de Tamaño")]
    [Tooltip("Multiplicador para ajustar el escudo al tamaño del enemigo. " +
             "Ajústalo si el escudo base queda muy grande o muy pegado al cuerpo.")]
    public float sizeMultiplier = 2.5f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        EnemyBase monk = turret.GetComponentInParent<EnemyBase>();
        List<EnemyShield> shields = new List<EnemyShield>();

        yield return null;

        try
        {
            while (true)
            {
                GrantShieldsToUnprotectedEnemies(monk, shields);
                yield return new WaitForSeconds(scanInterval);
            }
        }
        finally
        {
            foreach (EnemyShield shield in shields)
            {
                if (shield != null)
                    shield.Dissolve();
            }
            shields.Clear();
        }
    }

    private void GrantShieldsToUnprotectedEnemies(EnemyBase monk, List<EnemyShield> shields)
    {
        if (shieldPrefab == null) return;

        shields.RemoveAll(s => s == null);

        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>();

        foreach (EnemyBase enemy in allEnemies)
        {
            if (!shieldSelf && enemy == monk) continue;

            if (enemy.GetComponentInChildren<EnemyShield>() != null) continue;

            GameObject shieldObj = Instantiate(
                shieldPrefab,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform
            );

            shieldObj.transform.localPosition = Vector3.zero;

            Vector3 inverseScale = new Vector3(
                enemy.transform.localScale.x != 0 ? 1f / enemy.transform.localScale.x : 1f,
                enemy.transform.localScale.y != 0 ? 1f / enemy.transform.localScale.y : 1f,
                enemy.transform.localScale.z != 0 ? 1f / enemy.transform.localScale.z : 1f
            );

            shieldObj.transform.localScale = inverseScale * enemy.personalShieldSize;

            EnemyShield shield = shieldObj.GetComponent<EnemyShield>();
            if (shield != null)
            {
                shield.Initialize(monk);
                shields.Add(shield);
            }
        }
    }
}