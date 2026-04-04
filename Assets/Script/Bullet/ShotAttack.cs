using UnityEngine;

public static class ShotAttack 
{
    public static void SimpleShot(Vector2 origin, Vector2 velocity)
    {
        Bullet bullet = BulletPool.Instance.RequestBullet();
        bullet.transform.position = origin;
        bullet.Velocity = velocity;
    }
    public static void RadialShot (Vector2 origin, Vector2 aimDirection, RadialShotSettings settings)
    {
        float angleBetweenBullets = 360f / settings.NumberOfBullets;

        for (int i = 0; i < settings.NumberOfBullets; i++)
        {
            float bulletDirectionAngle = angleBetweenBullets * i;

            Vector2 bulletDirection = aimDirection.Rotate(bulletDirectionAngle);
            SimpleShot(origin, bulletDirection * settings.BulletSpeed);
        }
    }
}
