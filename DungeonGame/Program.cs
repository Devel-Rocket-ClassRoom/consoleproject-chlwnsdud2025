using DungeonGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DungeonGame.SaveLoadJson;




namespace DungeonGame
{

    public enum Move_Result
    {
        Door,
        Back,
        Monster,
        Wall,
        Move,
        Clear,
        None
    }
    abstract public class Character
    {
        public int row_Location { get; set; }
        public int col_Location { get; set; }

        public int lifeCount { get; set; }
        public int attackDamage { get; set; }
        public string name { get; set; }


        public (int, int) Current_Location(char[,] map, char c)
        {

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == c)
                    {
                        row_Location = i;
                        col_Location = j;
                    }
                }
            }
            return (row_Location, col_Location);
        }
        abstract public void Spawn(int row, int col, char[,] map);
        abstract public bool Damaged(int damage);
        abstract public void Died();
    }
    public class Player : Character
    {
        public bool P_isDead = false;
        public Player()
        {
            lifeCount = 150;
            attackDamage = 50;
        }
        override public void Spawn(int row, int col, char[,] map)
        {
            map[row, col] = 'P';
            row_Location = row;
            col_Location = col;

        }
        override public void Died()
        {
            Console.WriteLine("Player 사망!!! - GMAE_OVER");
            P_isDead = true;
        }
        public Move_Result Player_Move(char[,] map, int xamount, int yamount, int x, int y, List<Monster> monsterGroup)
        {
            if (map[x, y] == '#')
            {
                return Move_Result.Wall; // 벽에 막힘
            }
            else if (map[x, y] == 'M')
            {
                Console.WriteLine("Player : 몬스터 공격!!");
                for (int i = 0; i < monsterGroup.Count; i++)
                {
                    if (monsterGroup[i].row_Location == x && monsterGroup[i].col_Location == y)
                    {
                        bool M_isDead = Player_Attack(map, monsterGroup[i]);
                        if (M_isDead)
                        {
                            monsterGroup.RemoveAt(i);
                            map[x, y] = ' ';
                        }

                    }
                }
                //map[x + xamount, y + yamount] = ' ';
                //map[x, y] = 'P';
                //몬스터 인덱스 제거


                return Move_Result.Monster;
            }
            else if (map[x, y] == 'D')
            {
                return Move_Result.Door; // 다음 스테이지 문
            }
            else if (map[x, y] == 'O')
            {
                return Move_Result.Back; // 이전 스테이지 문
            }
            else
            {
                map[x + xamount, y + yamount] = ' ';
                map[x, y] = 'P';
                return Move_Result.Move; // 일반 이동 성공
            }
        }
        override public bool Damaged(int damage)
        {
            lifeCount -= damage;
            if (lifeCount > 0)
            {
                Console.WriteLine("Player : 공격받음, 남은 체력 : " + lifeCount);
                return false;
            }
            else
            {
                Died();
                return true;
            }
        }
        public bool Player_Attack(char[,] map, Monster monster)
        {
            return monster.Damaged(attackDamage);
        }
    }

    
    public class Monster : Character
    {
        static Random rand = new Random();

        private int chasingRange = 10;
        private int attackRange = 1;
        private int percent = 50;
        public Monster() 
        {
            lifeCount = 100;
            attackDamage = 3;
        }

        override public bool Damaged(int damage)
        {
            lifeCount -= damage;
            if (lifeCount > 0)
            {
                Console.WriteLine("Monster : 공격받음, 남은 체력 : " + lifeCount);
                return false;
            }
            else
            {
                Died();
                return true;
            }
        }
        override public void Spawn(int row, int col, char[,] map)
        {
            map[row, col] = 'M';

            row_Location = row;
            col_Location = col;
        }//생성 및 세팅(체력, 공격력 등)
        override public void Died()
        {
            Console.WriteLine("Monster 사망");
        }
        public int Count_Monster(List<Monster> monsters)
        {
            int MonsterLocation = 0;

            for (int i = 0; i < monsters.Count; i++)
            {
                MonsterLocation++;
            }
            return MonsterLocation;
        }
        public void Attack_OR_Move_Monster(char[,] map, int turncount, Player player)
        {
            (int, int) PlayerLoc = player.Current_Location(map, 'P');
            if (turncount % 3 == 0)
            {
                //공격턴

                Monster_Attack(map, PlayerLoc, player);
            }
            else if (turncount % 3 != 0)
            {

                //움직이는 턴

                if (Check_Player(map, PlayerLoc, chasingRange))
                {
                    Monster_Move_To_Player(map, PlayerLoc);
                }
                else
                {
                    Monster_Random_Move(map);
                }
            }

        }//턴에 따라 Attack OR Move
        private void Monster_Attack(char[,] map, (int, int) PlayerLoc, Player player)
        {
            if (Check_Player(map, PlayerLoc, attackRange))
            {
                Console.WriteLine("Monster : 공격! - 확률: " + percent);
                Random ranl = new Random();
                int kk = ranl.Next(0, 2);

                if (kk == 1) { player.Damaged(attackDamage); }
                else { Console.WriteLine("Player : 공격 회피!!"); }

            }
        }
        private void Monster_Random_Move(char[,] map)//상하좌우 랜덤위치 이동
        {

            int a = rand.Next(1, 5);
            if (a == 1)
            {
                if (map[row_Location + 1, col_Location] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    row_Location++;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else if (a == 2)
            {
                if (map[row_Location - 1, col_Location] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    row_Location--;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else if (a == 3)
            {
                if (map[row_Location, col_Location + 1] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    col_Location++;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else if (a == 4)
            {
                if (map[row_Location, col_Location - 1] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    col_Location--;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else { }

        }

        private void Monster_Move_To_Player(char[,] map, (int, int) playerLoc)
        {

            if (playerLoc.Item1 > row_Location)
            {
                if (map[row_Location + 1, col_Location] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    row_Location++;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else if (playerLoc.Item1 < row_Location)
            {
                if (map[row_Location - 1, col_Location] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    row_Location--;
                    map[row_Location, col_Location] = 'M';
                }
            }

            if (playerLoc.Item2 > col_Location)
            {
                if (map[row_Location, col_Location + 1] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    col_Location++;
                    map[row_Location, col_Location] = 'M';
                }
            }
            else if (playerLoc.Item2 < col_Location)
            {
                if (map[row_Location, col_Location - 1] != ' ') { }
                else
                {
                    map[row_Location, col_Location] = ' ';
                    col_Location--;
                    map[row_Location, col_Location] = 'M';
                }
            }

        }//플레이어와몬스터의 거리 계산해서 플레이어기준 좌우위아래 위치파악 플레이어 방향으로 이동
        public bool Check_Player(char[,] map, (int, int) playerLoc, int chasingRange)//플레이어 위치 받아서 특정거리 안쪽이면 true 아니면 false
        {
            double dx = row_Location - playerLoc.Item1;
            double dy = col_Location - playerLoc.Item2;

            if (Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)) <= chasingRange)
            {
                return true;
            }
            return false;
        }
   
    
    }
    public class MapController
    {
        public List<char[,]> totalMap = new List<char[,]>();
        public int mapcount = 0;

        public void NewMap(char[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (i == 0 || i == map.GetLength(0) - 1 || j == 0 || j == map.GetLength(1) - 1)
                    {
                        map[i, j] = '#';
                    }
                    else
                    {
                        map[i, j] = ' ';
                    }
                }

            }
        }

        
        public char[,] CreateMap(string Level, List<Monster> monsterG, Player player)
        {
            //랜덤 크기의 맵 생성
            Random randmap = new Random();
            int mapsizer = 10;
            int mapsizec = 10;
            int enemyCount = 4;
            int wallCount = 2;

            switch (Level)
            {
                case "Easy":
                    mapsizer = randmap.Next(10, 13);
                    mapsizec = randmap.Next(15, 25);
                    enemyCount = randmap.Next(2, 4);
                    wallCount = randmap.Next(3, 5);
                    break;
                case "Normal":
                    mapsizer = randmap.Next(10, 25);
                    mapsizec = randmap.Next(25, 35);
                    enemyCount = randmap.Next(5, 8);
                    wallCount = randmap.Next(3, 7);
                    break;
                case "Hard":
                    mapsizer = randmap.Next(10, 25);
                    mapsizec = randmap.Next(35, 45);
                    enemyCount = randmap.Next(10, 15);
                    wallCount = randmap.Next(6, 10);
                    break;

            }


            char[,] map = new char[mapsizer, mapsizec];
            NewMap(map);



            //맵 내부 벽 랜덤 스폰
            Wall_Generator(map, wallCount);


            //플레이어 고정 위치 스폰

            player.Spawn(1, 1, map);

            //몬스터 랜덤 위치 스폰
            Random rand = new Random();
            monsterG.Clear();

            for (int i = 0; i < enemyCount; i++)
            {
                int ranRT;
                int ranCT;

                do
                {
                    ranRT = rand.Next(1, mapsizer);
                    ranCT = rand.Next(1, mapsizec);

                } while (map[ranRT, ranCT] != ' '); // 랜덤위치가 ' ' 가 아니라면 랜덤위치 다시 생성

                int ranR = ranRT;
                int ranC = ranCT;
                Monster newM = new Monster();
                newM.Spawn(ranR, ranC, map);
                monsterG.Add(newM);

            }

            return map;
        }
        public void Wall_Generator(char[,] map, int wallCount)
        {
            for (int count = 0; count < wallCount; count++)
            {
                
                Random rand = new Random();

                int a = rand.Next(2, map.GetLength(0) - 3);
                int b = rand.Next(2, map.GetLength(1) - 3);

                int sid = rand.Next(1, 5);

                if (map[a, b] == ' ' && map[a, b + 1] == ' ' && map[a, b + 2] == ' ' && map[a + 1, b] == ' ' && map[a + 2, b] == ' ')
                {
                    switch (sid)
                    {
                        case 1:
                            map[a, b] = '#';
                            map[a, b + 1] = '#';
                            map[a, b + 2] = '#';
                            break;
                        case 2:
                            map[a, b] = '#';
                            map[a + 1, b] = '#';
                            map[a + 2, b] = '#';
                            break;
                        case 3:
                            map[a, b] = '#';
                            map[a, b + 1] = '#';
                            map[a + 1, b] = '#';
                            break;
                        default:
                            map[a, b] = '#';
                            map[a, b + 1] = '#';
                            map[a + 1, b] = '#';
                            break;

                    }
                   
                }
                else
                {
                    count--;
                }
        }

        }
        public void Door_Generate(char[,] map)
        {
            int r = map.GetLength(0);
            int c = map.GetLength(1);

            map[0, 1] = 'O';
            map[r - 2, c - 1] = 'D';

        }
        public void PrintMap(char[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j]);
                }
                Console.WriteLine();
            }
        }
    }
    public class DungeonGame
    {
        string Level = "Easy";
        int stage_count = 3;
        int turnCount = 0;

        MapController mapManager = new MapController();
        Player player = new Player();
        List<Monster> monsterGroup = new List<Monster>();
        Monster moster = new Monster();
        SaveLoadJson JsonMap = new SaveLoadJson();

        public List<List<char>> list { get; set; }
        public DungeonGame()
        {

        }
        public void Init_Game()
        {
            Console.Write("난이도 입력(Easy, Normal, Hard): ");

            Level = Console.ReadLine();
            while (true)
            {
                if (!(Level == "Easy" || Level == "Normal" || Level == "Hard"))
                {
                    Console.Write("잘못된 입력입니다.다시 입력(Easy, Normal, Hard): ");

                    Level = Console.ReadLine();
                }
                else
                {
                    break;
                }
            }


            Console.Write("플레이 할 스테이지 수 입력: ");
            stage_count = int.Parse(Console.ReadLine());
        }
        public void PlayGame()
        {

            Init_Game();

            char[,] Map2 = mapManager.CreateMap(Level, monsterGroup, player);
            mapManager.totalMap.Add(Map2);
            JsonMap.test_map = JsonMap.ConvertMap1(Map2);
            JsonMap.SaveGameData(JsonMap.test_map);

            mapManager.PrintMap(Map2);

            while (true)
            {

                Move_Result a = Input_And_UpdateMap(ref Map2);

                if(a == Move_Result.Clear)
                {
                    break;
                }

                if (player.P_isDead)
                {
                    break;
                }

            }
        }
        public Move_Result Input_And_UpdateMap(ref char[,] map)
        {
            for (int i = 0; i < 2; i++) { Console.WriteLine(); }

            turnCount++;
            Console.WriteLine("현재 턴 :" + turnCount);

            Console.Write("input W,A,S,D: ");
            string input = Console.ReadLine();

            Console.Clear();
            Console.SetCursorPosition(0, 0);

            var PlayerPos = player.Current_Location(map, 'P');
            int x = PlayerPos.Item1;
            int y = PlayerPos.Item2;

            Move_Result moveResult = Move_Result.Move;

            //플레이어 이동
            switch (input.ToUpper())
            {
                case "W": moveResult = player.Player_Move(map, 1, 0, x - 1, y, monsterGroup); break;
                case "S": moveResult = player.Player_Move(map, -1, 0, x + 1, y, monsterGroup); break;
                case "A": moveResult = player.Player_Move(map, 0, 1, x, y - 1, monsterGroup); break;
                case "D": moveResult = player.Player_Move(map, 0, -1, x, y + 1, monsterGroup); break;
                default:
                    Console.WriteLine("잘못된 입력입니다.");
                    return Move_Result.None
                        ;
            }

            //몬스터 행동
            for (int i = 0; i < monsterGroup.Count; i++)
            {
                monsterGroup[i].Attack_OR_Move_Monster(map, turnCount, player);
            }

            //결과 업데이트
            if (moveResult == Move_Result.Wall) // 벽
            {
                mapManager.PrintMap(map);
                Console.WriteLine("이동할 수 없습니다.");
            }
            else if (moveResult == Move_Result.Monster) // 몬스터
            {

                if (moster.Count_Monster(monsterGroup) == 0) // 몬스터 다 잡음
                {
                    mapManager.Door_Generate(map);
                    mapManager.PrintMap(map);
                    Console.WriteLine("Stage_Clear!!");
                }
                else // 몬스터 남음
                {
                    mapManager.PrintMap(map);
                    Console.WriteLine("남은 적 수 : " + moster.Count_Monster(monsterGroup));
                }
            }
            else if (moveResult == Move_Result.Move) // 이동
            {
                mapManager.PrintMap(map);
            }

            if (moveResult == Move_Result.Door) // 출구
            {
                mapManager.mapcount++;
                if (mapManager.mapcount >= stage_count) // 마지막 스테이지
                {
                    Console.WriteLine("All_Stage_CLear!!");
                    return Move_Result.Clear;
                }
                if (mapManager.mapcount >= mapManager.totalMap.Count()) //List에 저장안된맵은 저장
                {

                    map = mapManager.CreateMap(Level, monsterGroup, player);
                    mapManager.totalMap.Add(map);

                    JsonMap.test_map = JsonMap.ConvertMap1(map);
                    JsonMap.SaveGameData(JsonMap.test_map);

                    mapManager.PrintMap(map);
                }
                else// List에 있는 맵은 그냥 불러옴
                {
                    map = mapManager.totalMap[mapManager.mapcount];
                    mapManager.PrintMap(map);
                }
            }
            else if (moveResult == Move_Result.Back) // 입구
            {
                mapManager.mapcount -= 1;
                map = mapManager.totalMap[mapManager.mapcount];
                mapManager.PrintMap(map);
            }

            //다음 턴 예고
            if (turnCount % 3 == 2) { Console.WriteLine("다음턴은 몬스터 공격 턴!"); }
            else { Console.WriteLine("다음턴은 몬스터 이동 턴!"); }

            return Move_Result.None;
        }
    }



    public class SaveLoadJson
    {

        public List<List<char>> test_map;
        GameData gdata = new GameData();
        // 2차원 맵을 List로 바꾸는 함수
        public void MapGenerate()
        {
            //for (int i = 0; i < map.GetLength(0); i++)
            //{
            //    for (int j = 0; j < map.GetLength(1); j++)
            //    {
            //        if (i == 0 || i == map.GetLength(0) - 1 || j == 0 || j == map.GetLength(1) - 1)
            //        {
            //            map[i, j] = '#';
            //        }
            //        else
            //        {
            //            map[i, j] = ' ';
            //        }
            //    }

            //}
            //test_map = ConvertMap1(map);
        }
        public List<List<char>> ConvertMap1(char[,] map)
        {
            var list = new List<List<char>>();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                var row = new List<char>();
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    row.Add(map[i, j]);
                }
                list.Add(row);
            }

            return list;
        }

        // 2차원 맵을 char[][]로 바꾸는 함수
        public char[][] ConvertMap2(char[,] map)
        {
            int rows = map.GetLength(0);
            int cols = map.GetLength(1);
            char[][] result = new char[rows][];

            for (int i = 0; i < rows; i++)
            {
                result[i] = new char[cols];
                for (int j = 0; j < cols; j++)
                {
                    result[i][j] = map[i, j];
                }
            }

            return result;
        }

        // GameData를 Json으로 저장하는 테스트 함수
        public void SaveGameData(List<List<char>> testMap)
        {
           
            gdata.AA(testMap);
            // 저장할 파일 경로
            string folderPath = "./GameData";
            string filePath = Path.Combine(folderPath, "data.json");  // 폴더와 파일 이름 합치기

            // 폴더가 존재하지 않을 경우 생성
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 직렬화
            string result = JsonSerializer.Serialize(gdata, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(filePath, result);

            // 테스트 출력
            //Console.WriteLine(result);
        }

        // Json으로 저장된 GameData를 읽는 테스트 함수
        public void LoadGameData()
        {
            string folderPath = "./GameData";
            string filePath = Path.Combine(folderPath, "data.json");  // 폴더와 파일 이름 합치기

            // 역직렬화
            string s = File.ReadAllText(filePath);
            GameData mm = JsonSerializer.Deserialize<GameData>(s);

            if (mm != null)
            {
                Console.WriteLine("읽기 성공!: " + mm);
            }
            else
            {
                Console.WriteLine("정상적인 데이터가 아닙니다.");
            }
        }

        public class GameData
        {
            // Json에 포함
            private List<List<char>> testMap;
            [JsonInclude] private List<List<List<char>>> mapList = new List<List<List<char>>>();

            public void AA(List<List<char>> testMap)
            {
                this.testMap = testMap;
                this.mapList.Add(this.testMap);
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            DungeonGame a = new DungeonGame();
            a.PlayGame();

        }
    }
}
