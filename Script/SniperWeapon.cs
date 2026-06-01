using UnityEngine;

public class SniperWeapon : MonoBehaviour
{
    [Header("단계별 프리팹 설정")]
    public GameObject sniperBeam1Prefab; // 1단계 (단순 PNG 투사체)
    public GameObject sniperBeam2Prefab; // 2단계 (정면 레이저 애니메이션)
    public GameObject sniperBeam3Prefab; // 3단계 (양방향 레이저 애니메이션)

    [Header("저격총 성능 설정")]
    [Range(1, 3)] public int weaponStage = 1;
    public float fireRate = 1.5f;       // 발사 속도 (쿨타임)
    public float scanRange = 15f;       // 적을 감지할 사정거리
    public LayerMask enemyLayer;        // 감지할 적의 레이어 (인스펙터에서 Enemy로 지정)

    [Header("런타임 정보 (확인용)")]
    public Transform target;            // 조준 타겟

    private float fireTimer;

    void Update()
    {
        // 1. 매 프레임 가장 가까운 적을 탐색하여 target에 실시간 갱신
        FindClosestEnemy();

        // 2. 발사 타이머 계산
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            if (target != null)
            {
                fireTimer = 0f;
                FireSniper(); // 타겟이 있을 때만 발사!
            }
        }
    }

    // 범위 내에서 가장 가까운 적을 찾아 target에 넣어주는 함수
    void FindClosestEnemy()
    {
        // 플레이어 주변 scanRange 반경 내의 enemyLayer를 가진 모든 콜라이더 스캔
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRange, enemyLayer);

        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D hit in hits)
        {
            // 부딪힌 오브젝트와의 거리 계산
            float distanceToEnemy = Vector3.Distance(transform.position, hit.transform.position);

            // 더 가까운 적을 발견하면 갱신
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = hit.transform;
            }
        }

        // 최종 선택된 가장 가까운 적을 타겟으로 설정 (없으면 자동으로 null이 됨)
        target = closestEnemy;
    }

    public void FireSniper()
    {
        if (target == null) return;

        // 타겟 방향 계산
        Vector3 fireDirection = (target.position - transform.position).normalized;

        // 현재 단계에 맞는 프리팹 선택
        GameObject selectedPrefab = null;
        switch (weaponStage)
        {
            case 1: selectedPrefab = sniperBeam1Prefab; break;
            case 2: selectedPrefab = sniperBeam2Prefab; break;
            case 3: selectedPrefab = sniperBeam3Prefab; break;
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning($"SniperWeapon: {weaponStage}단계 프리팹이 지정되지 않았습니다.");
            return;
        }

        // 플레이어 위치에 프리팹 소환
        GameObject beamObj = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

        if (beamObj != null)
        {
            SniperBeam sniperBeam = beamObj.GetComponent<SniperBeam>();
            if (sniperBeam != null)
            {
                // 생성된 빔에 단계와 방향을 전달하며 초기화
                sniperBeam.Init(weaponStage, fireDirection);
            }
            else
            {
                Debug.LogError($"{selectedPrefab.name} 프리팹에 SniperBeam 스크립트가 없습니다!");
                Destroy(beamObj);
            }
        }
    }

    // 에디터 씬(Scene) 뷰에서 사정거리를 직관적인 선으로 보여주는 가이드 기능
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}