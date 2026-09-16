using TMPro;
using UnityEngine;

public class StoryTMPChanger : MonoBehaviour
{
    [Header("바꿀 대상 TMP")]
    [SerializeField] private TextMeshProUGUI targetTMP;

    [Header("스토리별 텍스트")]
    [SerializeField]
    private string[] storyTexts = {
        "Animal Story",
        "Fantasy Story",
        "Alien Story"
    };

    private void Start()
    {
        UpdateTMPByStory();
    }

    public void UpdateTMPByStory()
    {
        if (targetTMP == null)
        {
            Debug.LogError("targetTMP가 할당되지 않았습니다.");
            return;
        }

        int currentStory = GameManager.instance.Story;
        int index = Mathf.Clamp(currentStory, 0, storyTexts.Length - 1);

        // 바로 문자열 대입
        targetTMP.text = storyTexts[index];
    }
}
