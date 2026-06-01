using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 정보 및 프리팹")]
    public GameObject bulletPrefab;
    [Range(1, 3)] public int weaponStage = 1; // 1, 2, 3단계 조절 변수
    public float scanRange = 10f;

    [Header("1단계 설정 (재장전)")]
    private int maxAmmo = 10;
    private int currentAmmo = 10;
    private float reloadDuration = 2f;
    private bool isReloading = false;

    // 타이머 변수들
    private float fireTimer;
    private float burstTimer; // 3단계 전방위 발사용
    private Transform target;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // 1단계 재장전 중이면 공격 메커니즘 일시 정지
        if (weaponStage == 1 && isReloading) return;

        fireTimer += Time.deltaTime;
        if (weaponStage == 3) burstTimer += Time.deltaTime;

        // --- 기본 자동 공격 타이머 체크 ---
        float currentFireRate = (weaponStage == 3) ? 0.25f : 0.5f; // 3단계는 0.25초, 나머지는 0.5초

        if (fireTimer >= currentFireRate)
        {
            fireTimer = 0f;
            FindClosestEnemy();

            if (target != null)
            {
                FirePattern();
            }
        }

        // --- 3단계 전방위 추가 공격 타이머 체크 ---
        if (weaponStage == 3 && burstTimer >= 1f)
        {
            burstTimer = 0f;
            FindClosestEnemy();
            FireOmniDirectional();
        }
    }

    // 각 단계별 발사 분기 처리
    void FirePattern()
    {
        Vector3 baseDir = (target.position - transform.position).normalized;

        switch (weaponStage)
        {
            case 1:
                // 1단계: 1발씩 발사 후 잔탄수 감소
                CreateBullet(baseDir, false);
                currentAmmo--;

                if (currentAmmo <= 0)
                {
                    StartCoroutine(ReloadRoutine());
                }
                break;

            case 2:
                // 2단계: 메인(적 방향) 1발 + 120도 간격 사이드 2발 (삼각형 배치)
                CreateBullet(baseDir, false); // 메인 탄 (유도 없음 요청하셨으나 원하시면 아래처럼 설정 가능)

                // 좌우 120도 회전된 방향 계산
                Vector3 leftDir = Quaternion.Euler(0, 0, 120) * baseDir;
                Vector3 rightDir = Quaternion.Euler(0, 0, -120) * baseDir;

                CreateBullet(leftDir, false);  // 뒤쪽 대각선 탄 1 (유도 없음)
                CreateBullet(rightDir, false); // 뒤쪽 대각선 탄 2 (유도 없음)
                break;

            case 3:
                // 3단계: 기본 0.25초 주기 탄 (고성능 유도)
                CreateBullet(baseDir, true);
                break;
        }
    }

    // 3단계 전용: 1초마다 터지는 8방향 전방위 유도탄
    void FireOmniDirectional()
    {
        for (int i = 0; i < 8; i++)
        {
            // 360도를 8로 나누어 45도 간격으로 발사 방향 수립
            float angle = i * 45f;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

            // 어느 방향으로 생성되든 강한 유도력을 지니고 타겟을 쫓아감
            CreateBullet(dir, true);
        }
    }

    // 총알 생성 공용 헬퍼 함수
    void CreateBullet(Vector3 direction, bool useStrongHoming)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (useStrongHoming && target != null)
        {
            // 먼저 생성 당시의 방향으로 론칭한 뒤, 타겟과 강한 유도력(예: 15f)을 넘겨줌
            bullet.Launch(direction);
            bullet.LaunchHoming(target, 15f);
        }
        else
        {
            bullet.Launch(direction);
        }
    }

    // 1단계용 재장전 코루틴
    System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadDuration);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    // 가장 가까운 적 탐색 로직 (기존 유지)
    void FindClosestEnemy()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, scanRange);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D t in targets)
        {
            if (t.CompareTag("Enemy"))
            {
                float distanceToEnemy = Vector3.Distance(transform.position, t.transform.position);
                if (distanceToEnemy < closestDistance)
                {
                    closestDistance = distanceToEnemy;
                    closestEnemy = t.transform;
                }
            }
        }
        target = closestEnemy;
    }
}