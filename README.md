
### 현재 진행 중인 작업
- Lobby
	- 화면 구성 기획 필요
	- 프리팹으로 스테이지 버튼 만들어서 세팅하기.
	
	- 기획
		- 화면 하단에 스크롤로 스테이지 선택 가능
    - 화면 가운데엔 유저의 현재 캐릭터(햄스터)가 보이도록

### 남아있는 작업
- Player Drag
	- Drag Line Z포지션 변경 (몬스터, 씨앗보다 뒤에 그려져야 한다.)
	- Line에 Texture 넣기
	- 드래그 떼면 Line Delete 처리

[Pending]
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
	- Mask 이미지 알파값 0으로 바꿔주는 효과

- InApp 결제
	- 스테이지 별 3개로 클리어 해주도록 함

- Firebase
	- 앱 푸시
	- Ouath
  

<p align="center">
<a href="https://hits.seeyoufarm.com"><img src="https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FSukereamTeam%2Fhamsterisfree&count_bg=%2379C83D&title_bg=%235C5C5C&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false"/></a>                                       
</p>
