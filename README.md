중요! <초기설정>
안하면 길안내 안됨

1.처음에 파일 불러오면 상단에 월드 생성을 하는 Simulator 창이 없을 것임 코드를 옮기는 과정에서 Readme 파일이 싹 다 같이 옮겨짐.
오류가 떠서 안되는건데. 아래에 Console창에 오류내용이 나와있음. 오류내용 더블클릭하면 해당 오류 파일을 알려줌. 그거 걍 다 delete로 삭제하셈. 그러면 빨간 오류들은 다 사라질 것임, 노란 오류들은 상관 없으니 걍 냅두셈. 해당 오류 파일 다 삭제 하면 위에  Simulator 창이 뜰 것임. 이제 2번으로 ㄱㄱ


2.
-유니티 상단 메뉴에서 Window > AI > Navigation 창을 염.
-우측 창에서 [Agents] 탭을 클릭.
-Agent Radius (반경): 0.15 , Step Height (계단 오르기): 0.8 (단차 극복)Max Slope (최대 경사-각): 60으로 설정

3.
-유니티 화면 맨 꼭대기 메뉴 줄에서 Edit을 클릭
-밑에서 두 번째쯤에 있는 Project Settings...를 클릭해 창을 열음.
-열린 창의 왼쪽 메뉴 목록에서 Tags and Layers를 찾아서 클릭
-오른쪽 화면에 Tags, Sorting Layers, Layers 3가지 항목이 보일 텐데, 세 번째인 Layers 왼쪽의 화살표(▶)를 눌러서 펼침
-비어있는 User Layer 8 빈칸에 SafePath라고 입력
-상단 메뉴에서 Simulator > 전체 월드 생성


승민짱 코드 초기설정(비콘)

필수 환경 세팅 (가장 중요 🚨)
1.이 프로젝트는 벽과 바닥의 재질을 인식하여 신호를 깎아냅니다. 따라서 반드시 맵의 장애물들에 태그(Tag)를 설정해야 시스템이 정상 작동합니다.
<태그(Tag) 추가하기>
-유니티 상단 메뉴에서 Edit -> Project Settings -> Tags and Layers 클릭
-Tags 리스트에 아래 3개의 태그를 스펠링을 맞춰서 똑같이 추가합니다 : ConcreteFloor (콘크리트 바닥, 천장용 / 투과 시 -15dBm 감쇄) Wall (일반 벽용 / 투과 시 -5dBm 감쇄) Glass (유리창용 / 투과 시 -2dBm 감쇄)

2.파일을 만들었으면 유니티 씬(Scene)에 있는 물체들에게 기능을 달아주어야 합니다.
-맵에 배치할 모든 '비콘' 오브젝트들을 선택한 뒤, 우측 Inspector 창으로 BeaconNode.cs를 드래그해서 넣어줍니다. (이후 Tx Power는 -50, Path Loss N은 3 정도로 설정합니다.)
-TestManager.cs (시스템 관리자): 좌측 Hierarchy 창 빈 공간을 우클릭하고 Create Empty를 눌러 빈 오브젝트를 만듭니다. 이름을 @TestManager로 바꾸고(@ 붙여야함), 이 TestManager.cs 를 드래그해서 넣어줍니다.
