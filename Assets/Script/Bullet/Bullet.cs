using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const float MAX_TIME_LIFE = 3f;
    private float _lifeTime = 0f;
    public Vector2 Velocity;

    private void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        _lifeTime += Time.deltaTime;

        if (_lifeTime > MAX_TIME_LIFE)
        Disable();
    }

    private void Disable()
    {
        Destroy(gameObject);
    }
}
