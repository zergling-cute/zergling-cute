using UnityEngine;
using UnityEngine.UI;

public class StorySpriteChanger : MonoBehaviour
{
    // === 유니티 에디터에서 연결 ===
    [Header("스프라이트를 바꿀 UI Image")]
    [SerializeField] private Image targetImage;

    [Header("스토리별 스프라이트 (순서대로)")]
    [Tooltip("스토리 0: Animal, 1: Fantasy, 2: Alien")]
    [SerializeField] private Sprite[] storySprites;

    // === 실행 로직 ===
    private void Start()
    {
        UpdateSpriteByStory();
    }

    /// <summary>
    /// GameManager의 스토리에 따라 스프라이트를 변경합니다.
    /// </summary>
    public void UpdateSpriteByStory()
    {
        if (targetImage == null)
        {
            Debug.LogError("targetImage가 할당되지 않았습니다. 인스펙터 창에서 Image 컴포넌트를 연결해주세요.");
            return;
        }

        // GameManager의 Story 값을 가져옵니다.
        int currentStory = GameManager.instance.Story;

        // storySprites 배열의 인덱스 범위를 벗어나지 않도록 값을 보정합니다.
        int spriteIndex = Mathf.Clamp(currentStory, 0, storySprites.Length - 1);

        // 해당 인덱스에 스프라이트가 존재하는지 확인합니다.
        if (storySprites[spriteIndex] != null)
        {
            // 타겟 이미지의 스프라이트를 변경합니다.
            targetImage.sprite = storySprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning($"스토리 {currentStory}에 해당하는 스프라이트가 storySprites 배열에 없습니다.");
        }
    }
}
