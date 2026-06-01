using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;

    private Vector3 targetDirection;
    private bool isHoming = false;
    private float homingForce = 0f;
    private Transform targetEnemy;

    // 기본 직선 발사 설정
    public void Launch(Vector3 direction)
    {
        targetDirection = direction.normalized;
        isHoming = false;

        RotateTowardsDirection(targetDirection);
    }

    // 3단계용 고성능 유도 발사 설정
    public void LaunchHoming(Transform target, float force)
    {
        targetEnemy = target;
        isHoming = true;
        homingForce = force;
    }

    void Start()
    {
        // 생성된 지 5초가 지나면 이 총알을 자동으로 삭제합니다.
        // 5초면 화면 밖으로 충분히 벗어날 시간입니다.
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (isHoming && targetEnemy != null && targetEnemy.gameObject.activeSelf)
        {
            // 가장 가까운 적의 방향을 실시간으로 계산하여 부드럽게 꺾임 (유도력 적용)
            Vector3 desiredDir = (targetEnemy.position - transform.position).normalized;
            targetDirection = Vector3.Lerp(targetDirection, desiredDir, homingForce * Time.deltaTime).normalized;

            RotateTowardsDirection(targetDirection);
            transform.position += targetDirection * speed * Time.deltaTime;
        }
        else
        {
            // 유도가 아니거나 타겟이 사라지면 마지막 방향으로 직선 이동
            transform.position += targetDirection * speed * Time.deltaTime;
        }
    }

    void RotateTowardsDirection(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 충돌 시 적에게 데미지를 주는 로직 연동부
            Destroy(gameObject);
        }
    }
}