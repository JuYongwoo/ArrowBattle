# ArrowBattle

## 프로젝트 설명
- **좌우 이동 + 스킬 기반 전투**. 기본 공격과 5개의 스킬이 존재합니다.
- **제한 시간** 내 승리 조건을 달성하면 승리, 시간이 만료되면 패배 처리됩니다.
- 타이틀 화면에서 **Play** 버튼으로 메인 씬으로 진입합니다.

### 조작
- 이동: `Horizontal` 축 (예: A/D, ←/→)  
- 스킬: `Q/W/E/R/Space` (Skill1~Skill5)  
- 기본 공격은 코드 상 `Attack` 스킬로 통합되어 있으며, 캐스팅/쿨타임 로직과 함께 동작합니다.

---

## 사용 기술 및 설계 원칙

### **이벤트 허브(ActionManager) 기반 입력–로직–UI 분리**
- `ActionManager`는 **C# Action/Func 집합**으로, 입력/버튼 등에서 **호출**하고(Player/시스템이) **구독**합니다.  
- 예: `leftRightMove(float)`, `useSkill(Skill)`, `CooldownUI(int, float)`, `setPlayerHPinUI(float,float)` 등.

### **ScriptableObject로 데이터 중심 설계**
- `CharacterStatDataSO`, `SkillDataSO`, `GameModeDataSO`로 **스탯/스킬/게임모드**를 데이터화.  
- 스킬 아이콘, 사운드, 데미지, 쿨타임, 캐스팅 시간, 투사체 속도/이동 타입 등을 SO로 관리.

### **UI 패널 단위 자동 바인딩**
- `Util.MapEnumChildObjects<TEnum,T>`로 **enum ↔ GameObject** 매핑 자동화.  
- `SkillPanel`, `PlayerPanel`, `EnemyPanel`, `TimePanel`, `ResultPanel` 등 **패널별 컴포넌트**가 스스로 `ActionManager`에 바인딩합니다.  
- 씬 전환 시 좀비 참조 방지용 해제 처리 포함(예: `OnDestroy`에서 자기 바인딩 해제).

### **간단한 FSM로 상태 전환**
- `CharacterBase`가 `Idle / Moving / UsingSkill` 상태를 관리, 애니메이션 파라미터와 연동.  
- 스킬 **쿨타임/캐스팅** 관리 및 투사체(`SkillProjectile`) 처리 포함.

### **Singleton 스타일의 매니저 허브**
- `ManagerObject`(DontDestroyOnLoad)가 `Audio/Resource/Input/Action/SkillDataBase`를 보유.  
- `MainScene`은 타이머 러너를 통해 **고정/실시간 반복 콜백**으로 시간 흐름과 UI 갱신을 수행.

---

## 스크립트 구조

```
├── Scripts
│   ├── Character                  # 캐릭터 기반 로직
│   │   ├── CharacterBase.cs       # 상태/이동/스킬 공통 로직
│   │   ├── CharacterStatManager.cs# SO→런타임 스탯 관리
│   │   ├── Enemy.cs               # 적 AI(좌우 왕복 이동 등)
│   │   └── Player.cs              # 플레이어 입력 구독/행동
│   │
│   ├── Managers                   # 전역 매니저
│   │   ├── ActionManager.cs       # 이벤트 허브(Action/Func 모음)
│   │   ├── AudioManager.cs        # BGM/효과음 재생
│   │   ├── InputManager.cs        # 키 입력→ActionManager 호출
│   │   ├── ManagerObject.cs       # 싱글톤 허브(DontDestroyOnLoad)
│   │   ├── ResourceManager.cs     # (Addressables 포함) 리소스 접근
│   │   └── SkillDataBaseManager.cs# 스킬 데이터/로직 집약
│   │
│   ├── Scene
│   │   └── MainScene.cs           # 타이머/승패 처리/반복 실행기
│   │
│   ├── Skill
│   │   └── SkillProjectile.cs     # 투사체 이동/피격/파괴
│   │
│   ├── SO                         # 데이터 ScriptableObject
│   │   ├── CharacterStatDataSO.cs
│   │   ├── GameModeDataSO.cs
│   │   └── SkillDataSO.cs
│   │
│   ├── UI                         # 패널 단위 UI
│   │   ├── PlayerPanel.cs         # 플레이어 HP UI
│   │   ├── EnemyPanel.cs          # 적 HP UI
│   │   ├── SkillPanel.cs          # 쿨다운/아이콘 표시
│   │   ├── TimePanel.cs           # 남은 시간 표시
│   │   ├── ResultPanel.cs         # 승/패 연출
│   │   └── TitlePanel.cs          # 타이틀→메인 씬 전환
│   │
│   └── Utils
│       ├── ABUtil.cs              # 좌/우 상대 판정(최근접 X 비교)
│       └── Util.cs                # enum↔GO 매핑 등 유틸
```

---
