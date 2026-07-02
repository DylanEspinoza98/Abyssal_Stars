using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shield Grant", menuName = "Boss Patterns/Attack/Shield Grant")]
public class PatternShieldGrantSO : AttackPatternSO
{
    [Header("Escudo")]
    [Tooltip("Prefab del escudo. Debe tener tag 'BulletShield', CircleCollider2D trigger " +
             "y el componente EnemyShield.")]
    [SerializeField] private GameObject shieldPrefab;

    [Tooltip("Si est� activo, el Monje tambi�n se protege a s� mismo.")]
    [SerializeField] private bool shieldSelf = false;

    [Header("Escaneo")]
    [Tooltip("Cada cu�ntos segundos busca enemigos nuevos sin escudo. " +
             "�til si spawnean enemigos despu�s del Monje.")]
    [SerializeField] private float scanInterval = 1.5f;

    [Header("Auto-Ajuste de Tama�o")]
    [Tooltip("Multiplicador para ajustar el escudo al tama�o del enemigo. " +
             "Aj�stalo si el escudo base queda muy grande o muy pegado al cuerpo.")]
    [SerializeField] private float sizeMultiplier = 2.5f;

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

            shieldObj.transform.localScale = inverseScale * enemy.PersonalShieldSize;

            EnemyShield shield = shieldObj.GetComponent<EnemyShield>();
            if (shield != null)
            {
                shield.Initialize(monk);
                shields.Add(shield);
            }
        }
    }
}