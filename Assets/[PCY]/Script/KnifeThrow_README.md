# 칼 던지기 시스템 (Knife Throw System)

## 개요
이 시스템은 VR 환경에서 칼을 던지고 다시 잡는 요요 같은 메커니즘을 구현합니다.

## 주요 기능
1. ✅ **상태 관리**: 칼이 손에 있는지(attached) 또는 공중에 있는지(flying) 인스펙터에서 실시간 확인
2. ✅ **물리 기반 던지기**: 손의 속도와 가속도를 기반으로 칼이 던져짐
3. ✅ **중력 시뮬레이션**: 위로 갈수록 감속, 내려올수록 가속
4. ✅ **Y축 회전**: 초록색 축(Y축)을 기준으로 빙글빙글 회전
5. ✅ **자동 귀환**: 최고점 도달 후 자동으로 손으로 돌아옴
6. ✅ **자동 잡기**: 일정 거리 이내로 들어오면 자동으로 손에 붙음

---

## 설치 방법

### 1단계: 스크립트 추가
프로젝트에 다음 두 스크립트가 추가되었습니다:
- `KnifeThrow.cs` - 칼의 물리 시뮬레이션 및 상태 관리
- `VRKnifeController.cs` - VR 컨트롤러 입력 처리 (선택사항)

### 2단계: Unity 설정

#### Option A: 키보드 입력 (테스트용)

1. **칼 오브젝트 설정**
   - Hierarchy에서 칼 GameObject를 선택
   - Inspector에서 `Add Component` → `KnifeThrow` 추가

2. **KnifeThrow 컴포넌트 설정**
   ```
   Is Attached: ✓ (체크)
   Hand Transform: [손 또는 컨트롤러 Transform 드래그]
   Hand Target Point: [손바닥 중심 Transform 드래그 - 없으면 Hand Transform과 동일하게]

   Throw Key: Space (또는 원하는 키)
   Velocity Multiplier: 2.0
   Min Throw Velocity: 1.0

   Gravity: 9.8
   Air Resistance: 0.05

   Rotation Speed: 360
   Rotation Axis: (0, 1, 0)  ← Y축 = 초록색

   Catch Distance: 0.15
   Return Force: 5.0

   Show Debug Info: ✓ (체크)
   ```

3. **테스트**
   - Play 버튼 클릭
   - `Space` 키를 눌러 칼 던지기
   - 칼이 빙글빙글 돌며 올라갔다가 손으로 돌아오는지 확인

#### Option B: VR 컨트롤러 입력 (권장)

1. **칼 오브젝트 설정** (Option A와 동일)

2. **VR Controller GameObject 생성**
   - Hierarchy에서 빈 GameObject 생성 (이름: "VR Knife Manager")
   - `Add Component` → `VRKnifeController` 추가

3. **VRKnifeController 컴포넌트 설정**
   ```
   Knife Throw: [KnifeThrow가 붙은 칼 GameObject 드래그]
   Controller Transform: [OVRControllerHelper 또는 Hand Transform 드래그]

   Throw Button: PrimaryIndexTrigger (트리거 버튼)
   Controller Hand: RTouch (오른손) 또는 LTouch (왼손)

   Min Throw Speed: 2.0
   Min Throw Acceleration: 5.0
   Velocity Sample Count: 5

   Show Debug Info: ✓ (체크)
   ```

4. **테스트**
   - VR 헤드셋 착용
   - 트리거 버튼을 누른 상태에서 손을 위로 휘두름
   - 트리거를 놓으면 칼이 던져짐
   - 칼이 자동으로 돌아오면 잡힘

---

## 인스펙터에서 확인할 수 있는 정보

### KnifeThrow 컴포넌트
- **Is Attached**: `true`면 칼이 손에 있음, `false`면 공중에 날아가는 중
  - ✅ **Contact 상태**: Is Attached = true
  - ❌ **Non-Contact 상태**: Is Attached = false

### Scene View (재생 중)
- **초록색 구**: 칼이 손에 있을 때
- **빨간색 구**: 칼이 날아가고 있을 때
- **노란색 구**: 손 타겟 포인트 (잡기 범위)
- **빨간색 선**: 칼의 속도 벡터

---

## 파라미터 튜닝 가이드

### 던지기 느낌 조정

| 원하는 효과 | 조정할 파라미터 | 추천값 |
|------------|----------------|--------|
| 더 높이 날아가게 | Velocity Multiplier ↑ | 3.0 ~ 5.0 |
| 더 빨리 떨어지게 | Gravity ↑ | 15.0 ~ 20.0 |
| 더 천천히 회전 | Rotation Speed ↓ | 180 |
| 더 빨리 회전 | Rotation Speed ↑ | 720 |
| 더 빨리 손으로 돌아오게 | Return Force ↑ | 10.0 ~ 15.0 |
| 잡기 쉽게 | Catch Distance ↑ | 0.25 |

### 물리 느낌 조정

```csharp
// 현실적인 중력
Gravity = 9.8f

// 달 중력 (1/6)
Gravity = 1.63f

// 과장된 만화 스타일
Gravity = 20.0f
Rotation Speed = 1080f
```

---

## 사용 예시

### 예시 1: 데스크톱 테스트
```csharp
// KnifeThrow 설정
Throw Key: Space
Hand Transform: Main Camera (또는 빈 GameObject)
Velocity Multiplier: 3.0

// 플레이 모드에서 Space 키로 테스트
```

### 예시 2: Oculus Quest VR
```csharp
// KnifeThrow 설정
Hand Transform: OVRHandPrefab/r_hand_world (또는 컨트롤러 Transform)
Hand Target Point: r_handPalm (손바닥 중심)

// VRKnifeController 설정
Controller Hand: RTouch
Throw Button: PrimaryIndexTrigger
Min Throw Speed: 2.5
```

---

## 디버그 모드

### Console 로그 (Show Debug Info = true)
```
칼 던지기! 초기 속도: 3.45 m/s, 방향: (0.2, 0.9, 0.1)
칼 잡기 완료!
```

### Scene View 시각화
- 노란색 선: 칼 → 손 타겟 포인트
- 빨간색 선: 칼의 속도 벡터
- 구체들: 칼 상태 표시 (초록/빨강)

---

## 고급 기능

### 코드에서 직접 제어

```csharp
// KnifeThrow 스크립트 참조 가져오기
KnifeThrow knife = GetComponent<KnifeThrow>();

// 특정 속도로 칼 던지기
Vector3 throwVelocity = new Vector3(0, 5, 2);
knife.ThrowKnifeWithVelocity(throwVelocity);

// 상태 확인
if (knife.IsKnifeAttached())
{
    Debug.Log("칼이 손에 있습니다");
}

// 현재 속도 확인
Vector3 vel = knife.GetKnifeVelocity();
float speed = vel.magnitude;

// 비행 시간 확인
float time = knife.GetFlightTime();
```

---

## 트러블슈팅

### 문제: 칼이 던져지지 않음
- Hand Transform이 제대로 설정되었는지 확인
- Throw Key가 눌리는지 확인 (Input.GetKeyDown)
- Is Attached가 true인지 확인

### 문제: 칼이 돌아오지 않음
- Hand Target Point가 설정되었는지 확인
- Return Force 값을 높여보기 (10~15)
- Catch Distance를 늘려보기 (0.2~0.3)

### 문제: 칼이 너무 빨리/느리게 날아감
- Velocity Multiplier 조정
- Min Throw Velocity 조정
- Gravity 값 확인

### 문제: 칼이 회전하지 않음
- Rotation Speed > 0 인지 확인
- Rotation Axis = (0, 1, 0) 인지 확인 (Y축)

---

## 구현 상세

### 물리 시뮬레이션
```
1. 던지기: 손 속도 * velocityMultiplier
2. 중력: velocity.y -= gravity * deltaTime
3. 공기저항: velocity -= velocity * airResistance * deltaTime
4. 귀환력: velocity += (handPos - knifePos).normalized * returnForce * deltaTime
5. 회전: Rotate(rotationAxis, rotationSpeed * deltaTime)
```

### 상태 머신
```
Attached → (Throw Input) → Flying → (Reach Hand) → Attached
    ↑                                                    ↓
    └────────────────────────────────────────────────────┘
```

---

## 라이선스
이 스크립트는 자유롭게 수정하고 사용할 수 있습니다.

---

## 추가 문의
스크립트 관련 질문이나 개선 사항이 있다면 알려주세요!
