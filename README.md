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
