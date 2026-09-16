#  다중 테마(Multi-Theme) 기반 통합 UI 및 연출 시스템

## 1. 시스템 개요 (Overview)
게임 내 3가지 세계관(Animal, Fantasy, Alien)에 따라 로비, 컷신, 인게임 UI, 심지어 몬스터의 외형까지 실시간으로 변화하는 **통합 동적 스키닝(Dynamic Skinning) 시스템**입니다. 
파편화되기 쉬운 UI 스크립트들을 중앙 상태(State)에 의존하도록 설계하여 결합도를 낮추고, 코루틴 기반의 경량 애니메이션과 정적(Static) 객체 관리를 통해 렌더링 성능을 최적화했습니다.

* **핵심 키워드:** `Dynamic Skinning`, `State-Driven UI`, `Coroutine Animation`, `Memory Optimization`

---

## 2. 핵심 아키텍처 및 데이터 흐름 (Architecture)

중앙의 `GameManager.instance.Story` 값이 변경되면, 씬 내의 모든 UI 컨트롤러와 비주얼 체인저가 자신의 상태를 구독하여 테마에 맞는 리소스를 즉각 로드하고 화면에 반영합니다.

```mermaid
flowchart TD
    A[(GameManager.Story)] -->|State: 0 (Animal)| B[로비 UI 팝업 및 배경 갱신]
    A -->|State: 1 (Fantasy)| C[테마 전용 컷신 코루틴 재생]
    A -->|State: 2 (Alien)| D[몬스터 스프라이트 및 애니메이터 교체]
    
    B --> E[StorySpriteChanger / StoryTMPChanger]
    C --> F[StoryTitleUI]
    D --> G[MonsterVisualSwitcher]
```

---

## 3. 핵심 기능 구현 및 코드 발췌 (Key Features & Code)

### 3.1 상태 주도형 동적 리소스 교체 (State-Driven Skinning)
UI 이미지, 텍스트, 몬스터 등 수많은 객체가 개별적으로 작동하지 않고, 중앙의 `Story` 값에 따라 스스로 적절한 에셋을 찾아 렌더링하도록 모듈화했습니다.

```csharp
// StorySpriteChanger.cs 발췌: 스토리 번호에 따른 안전한 이미지 교체
public void UpdateSpriteByStory()
{
    int currentStory = GameManager.instance.Story;
    // 인덱스 OutOfRange 예외를 방지하기 위한 안전장치
    int spriteIndex = Mathf.Clamp(currentStory, 0, storySprites.Length - 1);

    if (storySprites[spriteIndex] != null)
    {
        targetImage.sprite = storySprites[spriteIndex];
    }
}
```
> **🔗 관련 전체 코드 보기**
>  [`StorySpriteChanger.cs` (UI 이미지 교체)](./Scripts/StorySpriteChanger.cs)
>  [`StoryTMPChanger.cs` (UI 텍스트 교체)](./Scripts/StoryTMPChanger.cs)
>  [`MonsterVisualSwitcher.cs` (몬스터 외형 교체)](./Scripts/MonsterVisualSwitcher.cs)
>  [`StoryChoice.cs` / `StoryTitleUI.cs` / `TitleUI.cs` (로비 및 컷신 제어)](./Scripts/StoryChoice.cs)


### 3.2 코루틴을 활용한 경량 UI 렌더링 최적화
레벨업 텍스트가 떠오르는 연출 시, 무거운 유니티 내장 `Animator` 대신 수학적 보간(`Vector3.Lerp`)과 코루틴으로 직접 제어하여 성능을 대폭 최적화했습니다.

```csharp
// LevelUpText.cs 발췌: 수학적 보간을 이용한 팝업 애니메이션 로직
private IEnumerator AnimateAndDeactivate(float totalDuration, float peakScale, float moveUpDistance)
{
    float timer = 0f;
    float animDuration = totalDuration * 0.4f; 

    while (timer < animDuration)
    {
        float ratio = timer / animDuration;
        // 크기 팽창 및 상승 위치 이동을 동시에 연산
        rectTransform.localScale = Vector3.Lerp(originalScale, Vector3.one * peakScale, ratio);
        
        Vector3 targetPos = originalPosition + Vector3.up * moveUpDistance;
        rectTransform.localPosition = Vector3.Lerp(originalPosition, targetPos, ratio);

        timer += Time.deltaTime;
        yield return null;
    }
    // 애니메이션 종료 후 비활성화 처리
    gameObject.SetActive(false); 
}
```
> **🔗 관련 전체 코드 보기**
>  [`LevelUpText.cs` (레벨업 텍스트 코루틴 연출)](./Scripts/LevelUpText.cs)
>  [`LevelUpEffect.cs` (레벨업 파티클 이펙트)](./Scripts/LevelUpEffect.cs)
>  [`LevelDisplay.cs` (레벨 데이터 UI 바인딩)](./Scripts/LevelDisplay.cs)


### 3.3 정적(Static) 객체 기반 전역 툴팁 시스템
씬 내 수십 개의 버튼이 각각 툴팁 UI를 가지지 않고, 단 하나의 `Static GameObject`를 공유하도록 설계하여 메모리 낭비를 막고 글로벌 제어(일시정지 시 강제 종료 등)가 가능하도록 구축했습니다.

```csharp
// TooltipTrigger.cs 발췌: 메모리 공유형 툴팁 시스템
private static GameObject tooltipDisplayObject; // 모든 버튼이 공유

public void OnPointerEnter(PointerEventData eventData)
{
    tooltipText.text = tooltipContent;
    tooltipDisplayObject.SetActive(true);
}

public static void HideTooltip()
{
    // 정적 메서드를 통해 PauseUI 등 외부 시스템에서 즉각 툴팁을 제어 가능
    if (tooltipDisplayObject != null) tooltipDisplayObject.SetActive(false);
}
```
> **🔗 관련 전체 코드 보기**
> 👉 [`TooltipTrigger.cs` (전역 툴팁 트리거 및 데이터 설정)](./Scripts/TooltipTrigger.cs)
