using UnityEngine;
using System.Collections.Generic;

public class AoEZone : MonoBehaviour
{
    [Header("대미지 설정")]
    public int baseDamage = 30;
    public float effectRadius = 3f;

    [Header("테스트용 시각화 (LineRenderer)")]
    public Color debugColor = Color.red;
    private LineRenderer lineRenderer;

    // 내부 컴포넌트 제어용 변수
    private Transform visualThrow;     // 1, 2단계용 투사체/경고 마크
    private GameObject visualEffect;   // 1, 2, 3단계용 최종 이펙트/블랙홀

    private int stage = 1;
    private Vector3 targetPos;
    private float timer;
    private float lifeTime = 5f;

    private Vector3 startPos;
    private float travelDuration;
    private bool isExploded = false;

    private float dotTimer;
    private float dotInterval = 0.5f;

    public void Init(int stage, Vector3 targetPosition)
    {
        this.stage = stage;
        this.targetPos = targetPosition;
        this.startPos = transform.position;

        SetupDebugCircle();

        // --- 단계별 자식 오브젝트 탐색 로직 (사용자 하이어라키 구조 기준) ---
        if (stage == 1 || stage == 2)
        {
            // 1, 2단계용 자식 찾기
            visualThrow = transform.Find("Visual_AoEAttack1_Throw");
            Transform expTransform = transform.Find("Visual_AoEAttack1_Explosion");
            if (expTransform != null) visualEffect = expTransform.gameObject;

            if (stage == 1)
            {
                float distance = Vector3.Distance(startPos, targetPos);
                travelDuration = Mathf.Clamp(distance * 0.15f, 1.0f, 3.0f);
                if (lineRenderer != null) lineRenderer.enabled = false;
            }
            else // stage == 2
            {
                Explode(baseDamage); // 소환 즉시 대미지
                if (visualThrow != null) visualThrow.gameObject.SetActive(false);
                if (visualEffect != null) visualEffect.SetActive(true);
                if (lineRenderer != null) lineRenderer.enabled = true;
                timer = 0f;
                Destroy(gameObject, 2.0f);
            }
        }
        else if (stage == 3)
        {
            // 💡 3단계 블랙홀: 스크린샷에 올려주신 자식 이름을 정확히 조준합니다.
            Transform rotateTransform = transform.Find("Visual_AoEAttack3_Rotate");
            if (rotateTransform != null)
            {
                visualEffect = rotateTransform.gameObject;
                // 블랙홀은 소환 즉시 보여야 하므로 강제로 활성화
                visualEffect.SetActive(true);
            }

            Explode(baseDamage); // 최초 소환 시 100% 대미지
            if (lineRenderer != null) lineRenderer.enabled = true;
            timer = 0f;
        }
    }

    void Update()
    {
        if (stage == 1)
        {
            UpdateAoEAttack1Movement();
        }
        else if (stage == 2)
        {
            UpdateAfterImage();
        }
        else if (stage == 3)
        {
            UpdateBlackHole();
        }
    }

    void SetupDebugCircle()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = debugColor;
        lineRenderer.endColor = debugColor;

        int vertexCount = 25;
        lineRenderer.positionCount = vertexCount;

        for (int i = 0; i < vertexCount; i++)
        {
            float circumferenceProgress = (float)i / (vertexCount - 1);
            float currentAngle = circumferenceProgress * 2f * Mathf.PI;

            float x = Mathf.Cos(currentAngle) * effectRadius;
            float y = Mathf.Sin(currentAngle) * effectRadius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    void UpdateAoEAttack1Movement()
    {
        if (isExploded) return;

        timer += Time.deltaTime;
        float progress = timer / travelDuration;

        if (progress >= 1f)
        {
            isExploded = true;
            transform.position = targetPos;
            if (lineRenderer != null) lineRenderer.enabled = true;

            if (visualThrow != null) visualThrow.gameObject.SetActive(false);
            if (visualEffect != null) visualEffect.SetActive(true);

            Explode(baseDamage);
            Destroy(gameObject, 0.6f);
            return;
        }

        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
        float height = Mathf.Sin(progress * Mathf.PI) * 2f;
        currentPos.y += height;
        transform.position = currentPos;
    }

    void UpdateAfterImage()
    {
        timer += Time.deltaTime;
        if (lineRenderer != null)
        {
            float alphaProgress = Mathf.Clamp01(1f - (timer / 2.0f));
            lineRenderer.startWidth = 0.1f * alphaProgress;
            lineRenderer.endWidth = 0.1f * alphaProgress;
        }
    }

    void UpdateBlackHole()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // 스크립트 기반 추가 회전
        transform.Rotate(Vector3.forward * 90f * Time.deltaTime);

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, effectRadius);
        float pullSpeed = 2.5f;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Vector3 pullDirection = (transform.position - enemy.transform.position).normalized;
                enemy.transform.position += pullDirection * pullSpeed * Time.deltaTime;
            }
        }

        dotTimer += Time.deltaTime;
        if (dotTimer >= dotInterval)
        {
            dotTimer = 0f;
            int dotDamage = Mathf.RoundToInt(baseDamage * 0.3f);
            Explode(dotDamage);
        }
    }

    void Explode(int damageToApply)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                string attackType = (stage == 3) ? "3단계 블랙홀" : (stage == 2 ? "2단계 즉발장판" : "1단계 수류탄폭발");
                Debug.Log($"[{attackType}] {hit.name}에게 {damageToApply}의 범위 대미지 부여!");
            }
        }
    }
}