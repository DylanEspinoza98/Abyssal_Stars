using UnityEngine;

public class PowerUpFamiliar : PowerUp
{
    protected override void OnCollected(PlayerHealth player, PlayerShooter shooter)
    {
        if (shooter != null)
            shooter.ActivateFamiliar();
    }
}
