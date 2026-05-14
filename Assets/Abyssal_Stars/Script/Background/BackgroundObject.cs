using UnityEngine;

public class BackgroundObject : MonoBehaviour
{
    private float _speed;
    private float _killY;

    public void Setup(float speed, float killY)
    {
        _speed = speed;
        _killY = killY;
    }

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime, Space.World);

        if (transform.position.y < _killY)
        {
            Destroy(gameObject);
        }
    }
}