using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 처리를 위해 필수
using TMPro; // TextMeshPro 사용을 위해 필수

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // [핵심] 인스펙터에서 버튼마다 다르게 입력할 툴팁 내용
    [TextArea(3, 10)]
    public string tooltipContent;

    // 씬에 있는 모든 TooltipTrigger가 공유할 단 하나의 툴팁 표시창 (static으로 선언)
    private static GameObject tooltipDisplayObject;
    private static TextMeshProUGUI tooltipText;

    void Awake()
    {
        // static 변수는 처음에 딱 한 번만 찾아주면 됩니다.
        if (tooltipDisplayObject == null)
        {
            // Hierarchy 창에 있는 "TooltipDisplayText" 오브젝트를 이름으로 찾습니다.
            tooltipDisplayObject = GameObject.Find("TooltipDisplayText");

            if (tooltipDisplayObject != null)
            {
                tooltipText = tooltipDisplayObject.GetComponent<TextMeshProUGUI>();
                tooltipDisplayObject.SetActive(false); // 시작 시 확실하게 숨김 처리
            }
            else
            {
                Debug.LogError("오류: 씬에 'TooltipDisplayText' 라는 이름의 오브젝트가 없습니다!");
            }
        }
    }

    // 마우스 커서가 이 오브젝트(버튼) 영역에 들어왔을 때 자동으로 호출됩니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipDisplayObject == null || string.IsNullOrEmpty(tooltipContent))
        {
            return;
        }

        // 1. 툴팁 표시창의 텍스트를 이 오브젝트의 "tooltipContent"로 변경
        tooltipText.text = tooltipContent;

        // 2. 툴팁 표시창을 활성화해서 화면에 보여줌
        tooltipDisplayObject.SetActive(true);
    }

    // 마우스 커서가 이 오브젝트 영역에서 빠져나갔을 때 자동으로 호출됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 빠져나가면 툴팁을 숨기는 정적 함수를 호출합니다.
        HideTooltip();
    }

    /// <summary>
    /// 외부 스크립트(예: PauseUI)에서도 툴팁을 강제로 끌 수 있도록 제공하는 정적 메서드
    /// </summary>
    public static void HideTooltip()
    {
        if (tooltipDisplayObject != null)
        {
            // 툴팁 표시창을 다시 비활성화해서 숨김
            tooltipDisplayObject.SetActive(false);
        }
    }
}
