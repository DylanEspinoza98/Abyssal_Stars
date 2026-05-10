using UnityEngine;

public class PowerUpLife : PowerUp
{
    protected override void OnCollected(PlayerHealth player, PlayerShooter shooter)
    {
        player.AddLife();
    }
}