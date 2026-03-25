using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGame
{
    public interface IMonster
    {


    }
    abstract public class Character
    {
        public int row_Location;
        public int col_Location;

        public int lifeCount;
        public int attackDamage;
        public string name;
        

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
        public int Player_Move(char[,] map, int xamount, int yamount, int x, int y, List<Monster> monsterGroup)
        {
            if (map[x, y] == '#')
            {
                return 0; // 벽에 막힘
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


                return 3;
            }
            else if (map[x, y] == 'D')
            {
                return 1; // 다음 스테이지 문
            }
            else if (map[x, y] == 'O')
            {
                return 2; // 이전 스테이지 문
            }
            else
            {
                map[x + xamount, y + yamount] = ' ';
                map[x, y] = 'P';
                return 4; // 일반 이동 성공
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
    public class Monster : Character, IMonster
    {
        static Random rand = new Random();

        private int chasingRange = 10;
        private int attackRange = 1;
        private int percent = 50;
        public Monster() { }

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
            lifeCount = 100;
            attackDamage = 3;
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
                while (true)// 함수로 만들기!
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
                        break;
                    }



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

            mapManager.PrintMap(Map2);

            while (true)
            {

                int a = Input_And_UpdateMap(Map2);

                if (a == 1)
                {
                    mapManager.mapcount++;
                    if (mapManager.mapcount >= stage_count)
                    {
                        Console.WriteLine("All_Stage_CLear!!");
                        break;
                    }
                    if (mapManager.mapcount >= mapManager.totalMap.Count())
                    {

                        Map2 = mapManager.CreateMap(Level, monsterGroup, player);
                        mapManager.totalMap.Add(Map2);
                        mapManager.PrintMap(Map2);
                    }
                    else
                    {
                        Map2 = mapManager.totalMap[mapManager.mapcount];
                        mapManager.PrintMap(Map2);
                    }
                }
                else if (a == 2)
                {
                    mapManager.mapcount -= 1;
                    Map2 = mapManager.totalMap[mapManager.mapcount];
                    mapManager.PrintMap(Map2);
                }

                if (player.P_isDead)
                {
                    break;
                }

            }
        }
        public int Input_And_UpdateMap(char[,] map)
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

            int moveResult = 0;

            //플레이어 이동
            switch (input.ToUpper())
            {
                case "W": moveResult = player.Player_Move(map, 1, 0, x - 1, y, monsterGroup); break;
                case "S": moveResult = player.Player_Move(map, -1, 0, x + 1, y, monsterGroup); break;
                case "A": moveResult = player.Player_Move(map, 0, 1, x, y - 1, monsterGroup); break;
                case "D": moveResult = player.Player_Move(map, 0, -1, x, y + 1, monsterGroup); break;
                default:
                    Console.WriteLine("잘못된 입력입니다.");
                    return 0;
            }

            //몬스터 행동
            for (int i = 0; i < monsterGroup.Count; i++)
            {
                monsterGroup[i].Attack_OR_Move_Monster(map, turnCount, player);
            }

            //업데이트
            if (moveResult == 0)
            {
                mapManager.PrintMap(map);
                Console.WriteLine("이동할 수 없습니다.");
            }
            else if (moveResult == 3)
            {

                if (moster.Count_Monster(monsterGroup) == 0)
                {
                    mapManager.Door_Generate(map);
                    mapManager.PrintMap(map);
                    Console.WriteLine("Stage_Clear!!");
                }
                else
                {
                    mapManager.PrintMap(map);
                    Console.WriteLine("남은 적 수 : " + moster.Count_Monster(monsterGroup));
                }
            }
            else if (moveResult == 4)
            {
                mapManager.PrintMap(map);
            }

            if (moveResult == 1 || moveResult == 2)
            {
                return moveResult;
            }

            //다음 턴 예고
            if (turnCount % 3 == 2) { Console.WriteLine("다음턴은 몬스터 공격 턴!"); }
            else { Console.WriteLine("다음턴은 몬스터 이동 턴!"); }

            return 0;
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
