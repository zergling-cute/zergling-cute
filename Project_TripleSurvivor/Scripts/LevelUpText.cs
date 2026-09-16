using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUpText : MonoBehaviour
{
    private RectTransform rectTransform; // UI 움직임을 위해 필요
    private Vector3 originalScale = Vector3.one; // 기본 크기
    private Vector3 originalPosition; // 기본 위치

    void Awake()
    {
        // UI 컴포넌트의 위치, 크기를 제어하는 RectTransform을 가져옵니다.
        rectTransform = GetComponent<RectTransform>();

        // 초기 위치를 저장합니다.
        if (rectTransform != null)
        {
            originalPosition = rectTransform.localPosition;
        }
        else
        {
            Debug.LogError("[LevelUpText] RectTransform을 찾을 수 없습니다. 이 스크립트는 Canvas 아래의 UI 요소에만 사용해야 합니다.");
        }
        if (GameManager.instance.Story == 0)//포레스트런
        {
            
        }
        if (GameManager.instance.Story == 1)//칠흑의세계
        {

        }
        if (GameManager.instance.Story == 2)//프로젝트에일리언
        {

        }
    }

    // 외부에서 호출할 함수: 오브젝트 활성화 및 코루틴 시작
    public void ActivateAndDeactivate()
    {
        gameObject.SetActive(true);

        // 기존의 코루틴 대신 애니메이션 코루틴을 시작합니다.
        // 매개변수: (총 지속 시간, 최대 크기 배율, 상승 이동 거리)
        StartCoroutine(AnimateAndDeactivate(0.4f, 1.4f, 50f));
    }

    // 애니메이션 및 비활성화를 처리하는 코루틴
    private IEnumerator AnimateAndDeactivate(float totalDuration, float peakScale, float moveUpDistance)
    {
        if (rectTransform == null) yield break;

        float timer = 0f;
        float animDuration = totalDuration * 0.4f; // 총 지속 시간의 40% 동안 팽창 및 상승
        float holdDuration = totalDuration * 0.6f; // 남은 60% 동안 유지

        // 위치 및 크기 초기화
        rectTransform.localPosition = originalPosition;
        rectTransform.localScale = originalScale;

        // 1. 팽창 및 상승 애니메이션
        while (timer < animDuration)
        {
            float ratio = timer / animDuration;

            // 크기 애니메이션: 기본 크기(1)에서 목표 크기(peakScale)로 커집니다.
            rectTransform.localScale = Vector3.Lerp(originalScale, Vector3.one * peakScale, ratio);

            // 위치 애니메이션: 기본 위치에서 위로 moveUpDistance만큼 이동합니다.
            Vector3 targetPos = originalPosition + Vector3.up * moveUpDistance;
            rectTransform.localPosition = Vector3.Lerp(originalPosition, targetPos, ratio);

            timer += Time.deltaTime;
            yield return null;
        }

        // 애니메이션 완료 후 최종 상태로 고정
        rectTransform.localScale = Vector3.one * peakScale;
        rectTransform.localPosition = originalPosition + Vector3.up * moveUpDistance;

        // 2. 표시 상태 유지
        yield return new WaitForSeconds(holdDuration);

        // 3. 최종 비활성화
        gameObject.SetActive(false);
    }
}
