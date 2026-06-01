using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("추적할 대상")]
    public Transform target; // 카메라가 따라갈 플레이어의 Transform

    [Header("카메라 감도")]
    public float smoothing = 5f; // 카메라가 따라오는 부드러움 정도

    // 탑다운 2D 환경에서 카메라가 유지해야 할 Z축 깊이 (보통 -10)
    private Vector3 offset = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        // 플레이어가 움직인 직후(LateUpdate)에 카메라를 이동시킵니다.
        if (target != null)
        {
            // 목표 위치 = 플레이어 위치 + Z축 오프셋
            Vector3 targetPosition = target.position + offset;

            // Vector3.Lerp를 사용하여 현재 카메라 위치에서 목표 위치까지 부드럽게 보간 이동
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}