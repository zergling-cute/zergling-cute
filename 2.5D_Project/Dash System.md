# 플레이어 대시 시스템 (Dash System)

## 1. 기능 개요

플레이어가 바라보는 방향으로 빠르게 이동하는 대시 시스템이다. 대시 사용 시 일정량의 스태미나를 소모하며, 짧은 시간 동안 중력의 영향을 받지 않고 이동한다.

대시 사용 횟수를 제한하기 위해 쿨타임을 적용하였으며, 스태미나가 부족하거나 쿨타임이 남아 있는 경우 대시를 사용할 수 없도록 구현하였다.

## 2. AI 활용 및 입력 프롬프트

**활용 도구:** ChatGPT

**활용 목적:** 기존 플레이어 컨트롤러에 대시 시스템 추가 및 코드 구현 보조

**입력 프롬프트:**

> 기존 Unity 2.5D 플레이어 컨트롤러에 스태미나 소모, 쿨타임, 중력 비활성화 기능을 갖춘 방향 기반 대시 시스템을 추가해 줘.

**AI 활용 결과:**

입력한 프롬프트를 바탕으로 대시 시스템의 기본 구조와 주요 코드를 생성하고, 기존 플레이어 이동 및 스태미나 시스템과 연동하였다.

## 3. 주요 기능 및 설정

| 항목 | 설정값 | 설명 |
|---|---|---|
| 입력 키 | Left Shift | 대시 실행 |
| 대시 속도 | 28 | 대시 중 이동 속도 |
| 대시 지속 시간 | 0.25초 | 대시 유지 시간 |
| 스태미나 소모량 | 20 | 대시 1회 사용 시 소모량 |
| 쿨타임 | 3초 | 대시 재사용 대기 시간 |
| 이동 방향 | 좌 / 우 | 캐릭터가 바라보는 방향 |

※ 모든 수치는 Inspector에서 조정할 수 있도록 SerializeField로 선언하였다.

## 4. 동작 과정

1. 플레이어가 Left Shift 키를 입력한다.
2. 대시 쿨타임 및 현재 대시 상태를 검사한다.
3. 스태미나가 충분한지 확인한다.
4. 조건을 만족하면 스태미나를 소모하고 대시를 시작한다.
5. 캐릭터가 바라보는 방향으로 대시 속도를 적용한다.
6. 대시 지속 시간이 종료되면 대시 상태를 해제하고 중력을 복구한다.

## 5. 주요 구현 내용

### 5-1. 대시 상태 및 변수 관리

대시의 상태와 지속 시간, 쿨타임을 관리하기 위한 변수를 선언하였다.

```csharp
[SerializeField] private float dashSpeed = 28f;
[SerializeField] private float dashDuration = 0.25f;

private bool isDashing;
private float dashTimer;

[SerializeField] private float dashCoolTime = 3f;
private float dashCoolTimeTimer;
```

- `dashSpeed`: 대시 이동 속도
- `dashDuration`: 대시 지속 시간
- `isDashing`: 현재 대시 상태
- `dashTimer`: 대시 지속 시간 관리
- `dashCoolTime`: 대시 쿨타임
- `dashCoolTimeTimer`: 남은 쿨타임 관리

### 5-2. 대시 시작 및 사용 조건 검사

`StartDash()` 함수를 통해 대시 실행 조건을 검사하고 대시를 시작하도록 구현하였다.

```csharp
private void StartDash()
{
    // 쿨타임 중이거나 이미 대시 중이면 실행하지 않음
    if (dashCoolTimeTimer > 0f || isDashing)
        return;

    // 스태미나가 부족하면 대시하지 않음
    if (!playerStats.UseStamina(DashStaminaCost))
        return;

    isDashing = true;
    dashTimer = dashDuration;

    // 쿨타임 설정
    dashCoolTimeTimer = dashCoolTime;

    // 대시 중 중력 비활성화
    rb.useGravity = false;

    // 캐릭터가 바라보는 방향으로 대시
    float dashDirection = isFacingRight ? 1f : -1f;

    rb.linearVelocity = new Vector3(
        dashDirection * dashSpeed,
        0f,
        0f
    );
}
```

**핵심 구현 사항**

- 쿨타임 및 중복 사용 방지
- 스태미나 시스템과 연동
- 캐릭터의 방향을 기준으로 대시 방향 결정
- 대시 시작 시 중력 비활성화
- Rigidbody의 속도를 이용한 즉각적인 이동

### 5-3. 대시 이동 처리

`FixedUpdate()`에서 대시 상태를 확인하여 일반 이동과 대시 이동을 분리하였다.

```csharp
if (isDashing)
{
    float dashDirection = isFacingRight ? 1f : -1f;

    rb.linearVelocity = new Vector3(
        dashDirection * dashSpeed,
        currentVelocity.y,
        0f
    );
}
else
{
    // 일반 이동 처리
}
```

대시 중에는 일반 이동 입력보다 대시 속도를 우선 적용하도록 구현하였다.

이를 통해 대시 도중 일반 이동 입력이 대시 속도에 영향을 주지 않도록 처리하였다.

### 5-4. 대시 종료 및 중력 복구

`UpdateDash()`에서 대시 지속 시간을 관리한다.

```csharp
private void UpdateDash()
{
    if (!isDashing)
        return;

    dashTimer -= Time.deltaTime;

    if (dashTimer <= 0f)
    {
        isDashing = false;

        // 대시 종료 후 중력 복구
        rb.useGravity = true;
    }
}
```

대시 타이머가 종료되면 대시 상태를 해제하고 중력을 다시 활성화하여 일반적인 물리 움직임으로 전환한다.

## 6. 구현 시 고려사항

### 물리 이동과 입력 처리 분리

- `Update()`에서 키 입력과 대시 상태를 관리한다.
- `FixedUpdate()`에서 Rigidbody의 속도를 제어한다.
- 입력 처리와 물리 처리를 분리하여 이동 로직을 관리하였다.

### 스태미나 및 쿨타임 연동

- 스태미나 시스템의 `UseStamina()`를 활용하여 대시 사용 가능 여부를 검사한다.
- 대시 시작 시 쿨타임을 설정하여 연속 사용을 방지한다.

### 확장 가능성

현재 대시 시스템에는 추후 다음 기능을 추가할 수 있도록 고려하였다.

- 대시 중 피격 무시 및 무적 판정
- 대시 중 충돌 및 적 감지 처리
- 대시 종료 후 이동 속도 처리
- 대시 애니메이션 및 이펙트 연동

## 7. 구현 결과

캐릭터의 바라보는 방향을 기준으로 빠르게 이동하는 대시 시스템을 구현하였다.

스태미나와 쿨타임을 연동하여 대시 사용을 제한하고, 대시 중 중력을 비활성화하여 일반 이동 및 점프와 구분되는 이동 동작을 구현하였다.

**관련 스크립트:** `PlayerController3D.cs`
