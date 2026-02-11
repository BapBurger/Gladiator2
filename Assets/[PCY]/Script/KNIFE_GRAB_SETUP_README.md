# Knife Hand Tracking Grab Setup Guide

## 문제 상황
오른손 핸드 트래킹이 작동하지만, SM_Gladius 칼이 손을 따라오지 않는 문제

## 해결 방법: XR Interaction Toolkit 통합

이 가이드는 Meta Hand Tracking과 XR Interaction Toolkit을 통합하여 핸드 트래킹으로 칼을 잡을 수 있게 합니다.

---

## 📋 필수 조건

1. **Unity 패키지 확인**
   - ✅ Meta XR SDK (v77.0.0 이상)
   - ✅ XR Interaction Toolkit (v2.6.5 이상)
   - ✅ XR Plugin Management

2. **씬 구조**
   - ✅ OculusHand_R GameObject (핸드 트래킹)
   - ✅ SM_Gladius Prefab (`/Assets/Hivemind/GladitorArena/URP/Art/Prefabs/SM_Gladius.prefab`)

---

## 🛠️ 설정 단계

### 방법 1: 자동 설정 (권장)

#### 1단계: HandKnifeGrabSetup 스크립트 추가

1. Unity 에디터에서 **Hierarchy**에서 빈 GameObject 생성
   - 우클릭 → Create Empty
   - 이름: `KnifeGrabManager`

2. `KnifeGrabManager`에 **HandKnifeGrabSetup** 컴포넌트 추가
   - Add Component → Scripts → HandKnifeGrabSetup

3. Inspector에서 설정:
   ```
   [Knife Setup]
   ├─ Knife Prefab: SM_Gladius 프리팹 드래그 앤 드롭
   ├─ Knife Position Offset: (0, -0.05, 0.1)  // 손 기준 위치
   └─ Knife Rotation Offset: (-90, 0, 0)      // 손 기준 회전

   [Hand References]
   ├─ Right Hand Transform: OculusHand_R 드래그 앤 드롭
   └─ Attach Bone: b_r_wrist 또는 Hand/r_palm_center_marker

   [Grab Settings]
   ├─ Enable Grab Interaction: ✅ (체크)
   └─ Use Hand Gestures: ✅ (체크)
   ```

#### 2단계: OVRHandGrabInteractor 추가 (핀치 제스처 인식)

1. **Hierarchy**에서 `OculusHand_R` 선택

2. 자식 오브젝트로 빈 GameObject 생성
   - 우클릭 → Create Empty
   - 이름: `HandGrabInteractor`

3. `HandGrabInteractor`에 컴포넌트 추가:
   - **OVRHandGrabInteractor** (Add Component → Scripts)
   - **Sphere Collider** (자동 추가됨)

4. Inspector에서 설정:
   ```
   [OVR Hand Tracking]
   ├─ Is Right Hand: ✅ (체크)
   ├─ Pinch Threshold: 0.7
   ├─ Use Index Pinch: ✅
   └─ Use Grip: ✅

   [Hand Bones] - 비워두면 자동으로 찾음
   ├─ Index Tip: (비워둠 - 자동 검색)
   ├─ Thumb Tip: (비워둠 - 자동 검색)
   └─ Palm Center: (비워둠 - 자동 검색)

   [Sphere Collider]
   ├─ Is Trigger: ✅
   └─ Radius: 0.1
   ```

#### 3단계: XR Interaction Manager 추가

1. Hierarchy에서 빈 GameObject 생성
   - 이름: `XR Interaction Manager`

2. 컴포넌트 추가:
   - Add Component → XR → XR Interaction Manager

---

### 방법 2: 수동 설정 (프리팹 직접 수정)

만약 씬이 아닌 프리팹을 수정하려면:

1. **Project**에서 `SM_Gladius.prefab` 더블클릭으로 열기

2. Prefab에 컴포넌트 추가:
   - **Rigidbody**
     ```
     Mass: 0.5
     Drag: 1
     Angular Drag: 0.5
     Use Gravity: ✅
     Is Kinematic: ❌
     ```

   - **XR Grab Interactable**
     ```
     Movement Type: Instantaneous
     Track Position: ✅
     Track Rotation: ✅
     Smooth Position: ✅
     Smooth Rotation: ✅
     Smooth Position Amount: 5
     Smooth Rotation Amount: 5
     Throw On Detach: ❌
     ```

3. Prefab 저장 (Ctrl+S)

4. `OculusHand_R` 프리팹에도 위의 2단계처럼 **OVRHandGrabInteractor** 추가

---

## 🎮 사용 방법

### 런타임 동작

1. **Play 모드 진입**
   - 칼이 자동으로 오른손에 생성됨

2. **핸드 트래킹으로 칼 잡기**
   - **핀치 제스처**: 검지와 엄지를 붙여서 집기
   - **그립 제스처**: 주먹을 쥐듯이 손가락 오므리기
   - 칼 근처에서 제스처를 하면 자동으로 잡힘

3. **칼 놓기**
   - 손가락을 펴서 핀치/그립 해제

### 키보드 테스트 (디버깅용)

- **G 키**: 칼을 강제로 손에 부착
- **H 키**: 칼을 손에서 분리

---

## ⚙️ 상세 설정

### Knife Position/Rotation Offset 조정

칼의 위치나 각도가 이상하면 Inspector에서 조정:

```
Position Offset:
- X: 좌우 (양수 = 오른쪽)
- Y: 상하 (양수 = 위)
- Z: 앞뒤 (양수 = 앞)

Rotation Offset:
- X: Pitch (칼 위아래 기울기)
- Y: Yaw (칼 좌우 회전)
- Z: Roll (칼 롤링)

추천 설정:
Position: (0, -0.05, 0.1)
Rotation: (-90, 0, 0)
```

### Attach Bone 선택

칼을 붙일 손 뼈 선택:

- **b_r_wrist**: 손목 (안정적, 권장)
- **r_palm_center_marker**: 손바닥 중심
- **b_r_index1**: 검지 관절 (칼을 손가락으로 잡는 느낌)

### Pinch Threshold 조정

손가락 감도 조정:

```
0.5: 매우 민감 (살짝만 집어도 작동)
0.7: 보통 (권장)
0.9: 둔감 (확실하게 집어야 작동)
```

---

## 🐛 문제 해결

### 1. "칼이 생성되지 않아요"

**원인**: Knife Prefab이 할당되지 않음

**해결**:
- `KnifeGrabManager` → Inspector에서 `Knife Prefab` 필드 확인
- `SM_Gladius.prefab` 드래그 앤 드롭

---

### 2. "칼이 손을 따라오지 않아요"

**원인**: Hand Transform 참조가 없음

**해결**:
- Hierarchy에서 `OculusHand_R` 찾기
- `KnifeGrabManager` → Inspector → `Right Hand Transform`에 드래그

---

### 3. "핀치 제스처가 인식 안돼요"

**원인 1**: OVRHand 컴포넌트 누락

**해결**:
- `OculusHand_R` GameObject에 `OVRHand` 컴포넌트 확인
- 없으면 Add Component → OVR → OVR Hand

**원인 2**: Hand Bones 참조 오류

**해결**:
- Console 로그 확인 ("Found index tip", "Found thumb tip" 메시지)
- 자동 검색이 실패하면 수동으로 할당:
  - Index Tip: `b_r_index3`
  - Thumb Tip: `b_r_thumb3`

---

### 4. "칼이 이상한 위치에 있어요"

**해결**:
- Inspector에서 `Knife Position Offset` 조정
- Scene 뷰에서 실시간 확인하며 값 변경

---

### 5. "칼이 물리적으로 이상하게 움직여요"

**해결**:
- `SM_Gladius` Rigidbody 설정 확인
  ```
  Mass: 0.5 (가벼워야 함)
  Drag: 1-2 (공기 저항)
  Angular Drag: 0.5-1 (회전 저항)
  Interpolate: Interpolate (부드러운 움직임)
  ```

---

### 6. "손을 떼도 칼이 떨어지지 않아요"

**원인**: Enable Grab Interaction이 꺼짐

**해결**:
- `KnifeGrabManager` → Inspector
- `Enable Grab Interaction` ✅ 체크

---

## 📊 컴포넌트 구조

```
씬 구조:
├─ XR Interaction Manager (GameObject)
│  └─ XRInteractionManager (Component)
│
├─ KnifeGrabManager (GameObject)
│  └─ HandKnifeGrabSetup (Component)
│     ├─ Knife Prefab: SM_Gladius
│     └─ Right Hand Transform: OculusHand_R
│
└─ Hand (GameObject)
   └─ OculusHand_R (GameObject)
      ├─ OVRHand (Component) - Meta Hand Tracking
      ├─ Transform (Component)
      ├─ Animator (Component)
      │
      ├─ HandGrabInteractor (GameObject)
      │  ├─ OVRHandGrabInteractor (Component)
      │  └─ SphereCollider (Component, IsTrigger=true)
      │
      └─ [Hand Bones]
         ├─ b_r_wrist
         ├─ b_r_index3
         ├─ b_r_thumb3
         └─ ...
```

---

## 🎯 테스트 체크리스트

설정 완료 후 테스트:

- [ ] Play 모드에서 칼이 오른손에 생성됨
- [ ] 핀치 제스처로 칼을 잡을 수 있음
- [ ] 손을 움직이면 칼이 따라옴
- [ ] 손을 펴면 칼이 떨어짐
- [ ] G 키로 수동 부착 가능
- [ ] H 키로 수동 분리 가능
- [ ] Console에 에러 없음

---

## 📝 추가 정보

### 제작한 스크립트

1. **HandKnifeGrabSetup.cs**
   - 칼 생성 및 초기화
   - XRGrabInteractable 자동 추가
   - Rigidbody 설정

2. **OVRHandGrabInteractor.cs**
   - Meta Hand Tracking과 XR Interaction Toolkit 통합
   - 핀치 제스처 인식
   - 손 뼈 자동 검색

### 참고 문서

- [XR Interaction Toolkit Documentation](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.6/manual/index.html)
- [Meta XR SDK Documentation](https://developer.oculus.com/documentation/unity/)
- [OVRHand API Reference](https://developer.oculus.com/documentation/unity/unity-sf-hand-tracking/)

---

## 🆘 여전히 문제가 있다면

1. **Console 로그 확인**: 에러 메시지 읽기
2. **Scene 뷰에서 Gizmos 확인**: 노란색/녹색 구체(상호작용 범위) 보임
3. **Component가 모두 활성화되어 있는지 확인**: 체크박스가 켜져있는지
4. **OVRHand가 올바르게 추적 중인지 확인**: 손이 씬에 표시되는지

---

## ✅ 완료!

이제 오른손 핸드 트래킹으로 SM_Gladius 칼을 잡고 사용할 수 있습니다!

**핀치하여 잡고, 펴서 놓으세요!** 🗡️✋
