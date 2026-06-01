using UnityEngine;
using System.Collections.Generic;

public class SniperBeam : MonoBehaviour
{
    [Header("기본 세팅")]
    public float speed = 20f;     // 1단계 투사체가 날아가는 속도
    public int damage = 15;       // 관통 대미지
    public float lifetime = 0.4f; // 프리팹이 소멸할 시간 (2,3단계는 애니메이션 재생 시간에 맞춤)

    private int stage = 1;
    private Vector3 direction;
    private float timer;

    // 중복 타격 방지용 해시셋 (관통 시 한 번 맞은 적은 다시 데미지를 받지 않음)
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public void Init(int stage, Vector3 dir)
    {
        this.stage = stage;
        this.direction = dir.normalized;

        // 발사 방향을 바라보도록 빔/투사체 오브젝트 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // [2, 3단계 즉발 레이저] 생성되자마자 즉시 레이캐스트로 공격 범위 내의 모든 적 타격
        if (stage >= 2)
        {
            ExecuteLaserRaycast();
        }
    }

    void Update()
    {
        // 지정된 수명(lifetime)이 지나면 오브젝트 삭제
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // [1단계 관통 투사체] 매 프레임 지정된 방향과 속도로 날아감
        if (stage == 1)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    void ExecuteLaserRaycast()
    {
        // 단계별 레이저 두께와 최대 사정거리 설정
        float laserWidth = (stage == 3) ? 2.5f : 1.2f;
        float maxDistance = 30f;

        if (stage == 2)
        {
            // 2단계: 가장 가까운 타겟 방향 경로 상의 모든 적을 즉시 동시 타격
            CastLaserLine(transform.position, direction, maxDistance, laserWidth);
        }
        else if (stage == 3)
        {
            // 3단계: 타겟 방향(정면)과 그 반대 방향(후면) 경로 전체를 즉시 동시 타격
            CastLaserLine(transform.position, direction, maxDistance, laserWidth);
            CastLaserLine(transform.position, -direction, maxDistance, laserWidth);
        }
    }

    // 두께(width)를 가진 직선을 레이저처럼 그어 범위 내의 모든 적을 스캔하고 타격하는 함수
    void CastLaserLine(Vector3 start, Vector3 dir, float dist, float width)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, width * 0.5f, dir, dist);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                ApplyDamage(hit.collider);
            }
        }
    }

    // [1단계 관통 투사체용] 날아가다 적의 Collider2D와 겹치는 순간 호출되는 트리거 충돌
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1단계일 때만 작동하며, 적의 콜라이더를 만나면 데미지를 줌
        if (stage == 1 && collision != null && collision.CompareTag("Enemy"))
        {
            ApplyDamage(collision);
            // 💡 주의: 여기서 Destroy(gameObject)를 하지 않으므로 총알이 사라지지 않고 적을 '관통'합니다.
        }
    }

    void ApplyDamage(Collider2D enemyCollider)
    {
        // 이미 대미지를 입은 적이거나 콜라이더가 비어있다면 무시 (중복 타격 방지)
        if (enemyCollider == null || hitEnemies.Contains(enemyCollider)) return;

        hitEnemies.Add(enemyCollider);

        // 콘솔 확인용 및 실제 몬스터 피격 스크립트 연동부
        Debug.Log($"{enemyCollider.name}에게 {damage}의 관통 대미지 부여!");

        // 몬스터에게 실제 데미지를 주는 컴포넌트가 있다면 여기에 연동하시면 됩니다.
        // Enemy enemy = enemyCollider.GetComponent<Enemy>();
        // if (enemy != null) enemy.TakeDamage(damage);
    }
}