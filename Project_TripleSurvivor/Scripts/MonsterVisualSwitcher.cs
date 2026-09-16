using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class MonsterVisualSwitcher : MonoBehaviour
{
    [Header("스토리별 스프라이트")]
    public Sprite story0Sprite;
    public Sprite story1Sprite;
    public Sprite story2Sprite;

    [Header("스토리별 애니메이터")]
    public RuntimeAnimatorController story0Animator;
    public RuntimeAnimatorController story1Animator;
    public RuntimeAnimatorController story2Animator;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        int story = GameManager.instance.Story;

        // 스프라이트 변경
        switch (story)
        {
            case 0:
                if (story0Sprite != null) spriteRenderer.sprite = story0Sprite;
                if (story0Animator != null) animator.runtimeAnimatorController = story0Animator;
                break;

            case 1:
                if (story1Sprite != null) spriteRenderer.sprite = story1Sprite;
                if (story1Animator != null) animator.runtimeAnimatorController = story1Animator;
                break;

            case 2:
                if (story2Sprite != null) spriteRenderer.sprite = story2Sprite;
                if (story2Animator != null) animator.runtimeAnimatorController = story2Animator;
                break;

            default:
                Debug.LogWarning("유효하지 않은 스토리 번호");
                break;
        }
    }
}
