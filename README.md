목차
============
- [프로젝트 개요](#프로젝트-개요)
- [작업](#작업)
  - [완료한 작업](#완료한-작업)
  - [현재 진행 중인 작업](#현재-진행-중인-작업)
  - [남아있는 작업](#남아있는-작업)

<br>

프로젝트 개요
============
- Client : [eggmong](https://github.com/eggmong)
  - 게임 개발의 전반적인 모든 것(프로그래밍, 기획, 아트)을 담당하고 있습니다.
  
- CI/CD : [BeautyfullCastle](https://github.com/BeautyfullCastle)
  - 빌드 CI/CD 를 담당하며 Pull Request 요청 시 [Code Review](https://github.com/SukereamTeam/hamsterisfree/pulls?q=is%3Apr+is%3Aclosed)를 맡고 있습니다.
  - 현재 repository에 push 시 Github Action을 통한 Android 빌드가 생성되도록 구성되어 있습니다.
 
- 개발 일지 (~ing)
  - [https://eggmong.github.io/categories/devlog](https://eggmong.github.io/categories/devlog)


작업
============
완료한 작업
--------------------
- System
  - Scene Controller
    - UniTask와 UniRx를 활용하여 Singleton으로 구현
    - 씬 전환 시 FadeIn/Out 처리
    - bool 변수를 사용하여 option으로 loading 씬을 삽입/비삽입 하도록 처리
  - Sheet Downloader
    - [Google Sheets(예시 : 프로젝트에서 쓰이는 StageTable)](https://docs.google.com/spreadsheets/d/1fvU3xywFyNZkfHUn1L4PFeJu_uEJMswXxMU7Dl_PcmM/edit?usp=sharing)로 작성된 데이터 테이블을 Unity에서 GUI 버튼을 클릭하여 `.csv` 형식으로 다운받도록 구현
    - 다운 받은 csv 파일을 `ScriptableObject`로 파싱하여 `Data Container` 프리팹에 할당
  - Data Container
    - 위에서 만들어진 ScriptableObject을 코드 내에서 사용할 수 있도록 만들어진 Singleton 클래스
    - 각 스테이지마다 사용되는 Sprite List도 들고 있도록 구현
  - Json Manager
    - `Newtonsoft.Json` 을 사용하여 UserData, StageData를 json 형식으로 파싱 및 저장/로드 구현
    - [Aes](https://learn.microsoft.com/ko-kr/dotnet/api/system.security.cryptography.aes?view=net-7.0) 라는
대칭형 암호화 클래스를 사용하여 데이터를 암호화하여 저장
      - Key 값으로 암호화/복호화를 수행하며, 저장할 때 마다 새로운 IV값을 생성하여 데이터를 저장

- Game
  - Tile
    - 게임에 등장하는 Tile들을 `TileBase` 클래스를 상속 받게 하여 구현
    - 각 Tile들의 기능들은 `ITileActor` 인터페이스를 사용하여 구현
  - Stage
    - StageTable 데이터에 따라 시간 제한, 도전기회 제한 Type 구현
  - Player
    - 화면 터치 시 Block Image가 FadeIn 으로 출력되도록 구현
    - 드래그 입력에 따른 Line Draw 구현
  - Lobby
	- 프리팹으로 스테이지 버튼 만들어서 세팅하기.
	- 화면 하단에 스크롤로 스테이지 선택 가능

현재 진행 중인 작업
--------------------

- Sound
  - Lobby, Game, UI Sound 재생/멈춤 기능 구현 (SoundManager)


남아있는 작업
--------------------
- Player Drag
	- Drag Line Z포지션 변경 (몬스터, 씨앗보다 뒤에 그려져야 한다.)
	- Line에 Texture 넣기
	- 드래그 떼면 Line Delete 처리

- Stage
	- 게임 종료 후 보상 받는 것 ~~처리~~ 코드 작업은 완료, 연출 UI 필요
		- GameEndFlow 에 게임 결과 팝업
			-> ~~별 갯수 0개면 Fail 로 처리 (Clear X)~~
			-> ~~얻은 씨앗 개수에 따라 별 지급~~
	
- UI 작업
   - Game Start UI, End UI (Popup)
      - Start UI -> 햄스터 한번 회전하는 연출 끝난 후 Start! (if Stage 1 : Tutorial)
      - End UI -> End Popup, 보상 연출
	  
- Resources.Load -> Addressable 로 변환

- 버전 체크 (웹 통신 필요)

- 광고 SDK
        - Unity LevelPlay(구 IronSource) 사용하여 구현
	- Mask 이미지 알파값 0으로 바꿔주는 효과를 얻을 수 있음

- InApp 결제
	- 스테이지 별 3개로 클리어 해주도록 함

- Firebase
	- 앱 푸시
	- Oauth 로그인
  

<p align="center">
<a href="https://hits.seeyoufarm.com"><img src="https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FSukereamTeam%2Fhamsterisfree&count_bg=%2379C83D&title_bg=%235C5C5C&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false"/></a>                                       
</p>
