using UnityEngine;

public class LevelUpEffect : Effect
{
    private void FixedUpdate()
    {
        // 플레이어 위치
        Vector3 playerPos = GameManager.instance.player.transform.position;

        // GameManager의 Story 값 가져오기
        int story = GameManager.instance.Story;

        // 스토리별 높이 오프셋 (자유롭게 조정 가능)
        float yOffset = 0f;

        switch (story)
        {
            case 0:
                yOffset = 2.5f; // 스토리 0일 때
                break;
            case 1:
                yOffset = 2.5f; // 스토리 1일 때
                break;
            case 2:
                yOffset = 0f; // 스토리 2일 때
                break;
            default:
                yOffset = 2.0f; // 기본값
                break;
        }

        // 위치 적용
        transform.position = playerPos + new Vector3(0f, yOffset, 0f);
    }
}
