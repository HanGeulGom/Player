using UnityEngine;

public class AoEWeapon : MonoBehaviour
{
    [Header("단계별 프리팹 설정")]
    public GameObject aoeAttack1Prefab; // 1단계: 수류탄 투사체
    public GameObject aoeAttack2Prefab; // 2단계: 즉발 대미지 장판
    public GameObject aoeAttack3Prefab; // 3단계: 블랙홀 장판

    [Header("공격 성능 설정")]
    [Range(1, 3)] public int weaponStage = 1;
    public float scanRange = 15f;
    public LayerMask enemyLayer;

    [Header("런타임 정보 (확인용)")]
    public Transform target;

    private float fireTimer;

    void Update()
    {
        FindClosestEnemy();

        // 현재 단계에 맞는 쿨타임(fireRate) 설정
        float currentFireRate = (weaponStage == 3) ? 8f : 4f;

        fireTimer += Time.deltaTime;
        if (fireTimer >= currentFireRate)
        {
            if (target != null)
            {
                fireTimer = 0f;
                FireAoE();
            }
        }
    }

    void FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D hit in hits)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, hit.transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = hit.transform;
            }
        }
        target = closestEnemy;
    }

    void FireAoE()
    {
        if (target == null) return;

        GameObject selectedPrefab = null;
        switch (weaponStage)
        {
            case 1: selectedPrefab = aoeAttack1Prefab; break;
            case 2: selectedPrefab = aoeAttack2Prefab; break;
            case 3: selectedPrefab = aoeAttack3Prefab; break;
        }

        if (selectedPrefab == null) return;

        // 타겟의 현재 '위치 정보'를 캡쳐 (수류탄의 낙하지점 및 즉발 장판의 생성 위치가 됨)
        Vector3 targetPosition = target.position;

        // 1단계 수류탄은 플레이어 위치에서 생성되어 날아가고, 2·3단계 장판은 적 바닥에 바로 생성됨
        Vector3 spawnPosition = (weaponStage == 1) ? transform.position : targetPosition;

        GameObject aoeObj = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        if (aoeObj != null)
        {
            AoEZone aoeZone = aoeObj.GetComponent<AoEZone>();
            if (aoeZone != null)
            {
                // 생성된 장판/투사체 초기화 (단계, 목표 지점 전달)
                aoeZone.Init(weaponStage, targetPosition);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }
}