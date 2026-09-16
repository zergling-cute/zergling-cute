using UnityEngine;
using TMPro;

public class LevelDisplay : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    private Player player;

    void Start()
    {
        player = GameManager.instance.player;
        if (player == null)
        {
            Debug.LogWarning("GameManager에 플레이어가 등록되지 않았습니다.");
            return;
        }

        UpdateLevelText();
    }

    void Update()
    {
        if (player != null)
        {
            UpdateLevelText();
        }
    }

    void UpdateLevelText()
    {
        levelText.text = "LV. " + player.GetCurrentLevel().ToString();
    }
}
