using UnityEngine;

public class PowerUpShotgun : PowerUp
{
    protected override void OnCollected(PlayerHealth player, PlayerShooter shooter)
    {
        if (shooter != null)
            shooter.ActivateShotgun();
    }
}
