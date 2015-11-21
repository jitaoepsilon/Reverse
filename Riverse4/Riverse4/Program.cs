using System;
using System.Collections.Generic;

namespace Riverse4
{
    class Gameroot
    {
        protected int[] dir = { -11, -10, -9, -1, 1, 9, 10, 11 };

        /*座標変換系
        CmdToXy：コマンドは半角のみ。c3でも3cでも大丈夫。大文字も対応
            */
        protected int[] CmdToXy(string cmd)
        {
            int[] x_y = new int[2];
            if (cmd == "")
                return x_y;
            for (int i = 0; i < 2; i++)
            {
                switch (cmd[i])
                {
                    default:
                        continue;
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        x_y[1] = int.Parse(cmd[i].ToString());
                        break;
                    case 'a':
                    case 'A':
                        x_y[0] = 1;
                        break;
                    case 'b':
                    case 'B':
                        x_y[0] = 2;
                        break;
                    case 'c':
                    case 'C':
                        x_y[0] = 3;
                        break;
                    case 'd':
                    case 'D':
                        x_y[0] = 4;
                        break;
                    case 'e':
                    case 'E':
                        x_y[0] = 5;
                        break;
                    case 'f':
                    case 'F':
                        x_y[0] = 6;
                        break;
                    case 'g':
                    case 'G':
                        x_y[0] = 7;
                        break;
                    case 'h':
                    case 'H':
                        x_y[0] = 8;
                        break;
                }
            }
            return x_y;
        }
        protected int CmdToIndex(string cmd)
        {
            var xy1 = CmdToXy(cmd);
            return xy(xy1[0], xy1[1]);
        }
        protected string xyToCmd(int x, int y)
        {
            string result = Convert.ToString(y);
            switch (x)
            {
                case 1: result = result + "a"; break;
                case 2: result = result + "b"; break;
                case 3: result = result + "c"; break;
                case 4: result = result + "d"; break;
                case 5: result = result + "e"; break;
                case 6: result = result + "f"; break;
                case 7: result = result + "g"; break;
                case 8: result = result + "h"; break;
            }
            return result;
        }
        protected int xy(int x, int y) => y * 10 + x;
        protected int Otherside(int nowside) => 3 - nowside;

        /*盤面探索系
        ExistLegalMoveはIsLegalMoveを、IsLegalMoveはCountTurnOverを参照している。
        
            */
        protected int CountTurnOver(int target, int direction, int nowside, int[] nowboard)
        {
            int i;
            for (i = 1; nowboard[target + i * direction] == (3 - nowside); i++)
            { }
            if (nowboard[target + i * direction] == nowside)
            {
                //Console.WriteLine("nowside= {0},dir={1},target={2})", nowside,direction,xyToCmd(target%10,target/10));

                return i - 1;
            }
            else
            {
                return 0;
            }
        }
        protected bool IsLegalMove(int target, int nowside, int[] nowboard)
        {
            bool result = false;
            if (target < 0 || target > 100 || nowboard[target] != 0)
            {
                return result;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    if (CountTurnOver(target, dir[i], nowside, nowboard) > 0)
                    {
                        //Console.Write("islegal!");
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        protected bool ExistLegalMove(int nowside, int[] nowboard)
        {
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    if (IsLegalMove(xy(x, y), nowside, nowboard))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        //ぶっちゃけ引数はboardだけでいい気もする
        protected int[] CountStone(int nowwstone, int nowbstone, int[] board)
        {
            nowwstone = 0;
            nowbstone = 0;
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    int worb = board[xy(x, y)];
                    if (worb == 2)
                    {
                        nowwstone++;
                        //Console.Write("{0}w{1}, ",nowwstone, xy(x, y));
                    }
                    else if (worb == 1)
                    {
                        nowbstone++;
                        //Console.Write("{0}b{1}, ",nowbstone,xy(x, y));
                    }
                }
            }
            int[] result = { nowwstone, nowbstone };
            return result;
        }

        protected int[] FlipStone(int targetindex, int[] nowboard, int nowturn)
        {
            for (int i = 0; i < 8; i++)
            {
                int count = CountTurnOver(targetindex, dir[i], nowturn, nowboard);
                for (int j = 0; j <= count; j++)
                    nowboard[targetindex + j * dir[i]] = nowturn;
            }
            return nowboard;
        }
    }
    class Reverse : Gameroot
    {
        int[] board = new int[100];
        string[] boardlog = new string[60];//空きマス60個だからログの配列も60あれば足りる(?)
        int wstone = 0, bstone = 0, turn = 1, logturn = -1;
        int wall = -1, white = 2, black = 1;

        public Reverse()
        {
            ResetBoard();
            PrintBoard(board);
        }
        public int[] GetBoard { get { return board; } }
        /*cmdを受け取ってゲーム処理。ゲーム終了の有無を返す。
        今後は各ターンの変更情報をログに格納して、
        モードかかわらずターンコントロールできるようにしたい。
        ひとつ前、最初、ログモードなら一つあと、最後
            */
        public bool Game(string cmd, int mode)
        {
            Console.WriteLine("command: {0}",cmd);
            bool mypass = !ExistLegalMove(turn, board);
            if (mypass && ExistLegalMove(Otherside(turn), board) == false)//双方に合法手なし→終了
            {
                return true;
            }
            else if (mypass && mode == 0)//パス処理
            {
                ChangeSide();
                return false;
            }
            else if (!mypass)
            {
                //合法判定はMove内部。
                GetMove(cmd, mode);
            }
            else if (mypass && mode == 1)//棋譜再現の場合は石を打つ場所しか示されない。暗黙的にパスされる。
            {
                ChangeSide();
                Game(cmd, mode);
            }
            logturn++;
            ChangeSide();
            PrintBoard(board);
            return false;
        }

        void GetMove(string cmd,int mode)
        {
            int index = 0;
            while (true)
            {
                try
                {
                    index = CmdToIndex(cmd);
                    break;
                }
                catch
                {
                    //不適切なコマンド入力はプレイヤーのみの前提。
                    Console.WriteLine("正しい座標を入力してください。");
                    cmd = Console.ReadLine();
                    if (cmd != "pass")//一応パスもできるようにしておく
                        continue;
                }
            }
            //Console.WriteLine("GetMove.index= {0}", index);
            if (IsLegalMove(index, turn, board))
                DirectFlipStone(index);
            else if(mode==0)
            {
                WriteCanMove(turn);
                GetMove(Console.ReadLine(),mode);
            }
        }
        void ChangeSide() => turn = Otherside(turn);

        //変更処理系
        /*内部ログの格納は、
        DirectFlipStoneに入れるべきかなと思ってる。
        直接処理する場所だし。
            */
        void DirectFlipStone(int targetindex)
        {
            for (int i = 0; i < 8; i++)
            {
                int count = CountTurnOver(targetindex, dir[i], turn, board);
                for (int j = 0; j <= count; j++)
                    board[targetindex + j * dir[i]] = turn;
            }
        }
        void ResetBoard()
        {
            for (int index = 0; index < 100; index++)
            {
                if (index < 11 || index > 89 || index % 10 == 0 || index % 10 == 9)
                    board[index] = wall;
            }
            board[44] = white;
            board[45] = black;
            board[54] = black;
            board[55] = white;
        }

        //表示系
        void WriteCanMove(int nowside)
        {
            Console.Write("you can put");
            for (int y = 1; y <= 8; y++)
            {
                for (int x = 1; x <= 8; x++)
                {
                    if (IsLegalMove(xy(x, y), nowside, board))
                    {
                        Console.Write(xyToCmd(x, y) + " ");
                    }
                }
            }
            Console.Write(" >");
        }
        public void PrintBoard(int[] map)
        {
            //Console.Clear();
            CountStone(wstone, bstone, board);
            Console.WriteLine("\n 　 　A｜B｜C｜D｜E｜F｜G｜H");
            Console.WriteLine("  　 ― ― ― ― ― ― ― ― 　");
            for (int y = 1; y <= 8; y++)
            {
                Console.Write("{0} ｜ ", y);
                for (int x = 1; x <= 8; x++)
                {
                    if (map[xy(x, y)] == 0)
                        Console.Write("・ ");
                    else if (map[xy(x, y)] == 1)
                        Console.Write("○ ");
                    else if (map[xy(x, y)] == 2)
                        Console.Write("● ");
                    //Console.Write("{0} ", map[xy(x, y)]);
                }
                Console.Write("|\n");
            }
            Console.WriteLine("  　 ― ― ― ― ― ― ― ― 　");
            //Console.WriteLine("white:{0},black: {1}", wstones, bstones);
        }
        /*ゲーム終了後にログをメモ帳に出力したい。
        内部のログは返した石も格納する予定だから、
        出力する場合はその辺を省く必要あると思う。
        */
        public void WriteResult()
        {
            int[] countstone = CountStone(wstone, bstone, board);
            wstone = countstone[0];
            bstone = countstone[1];
            string result;
            if (wstone > bstone)
                result = "White win!";
            else if (wstone < bstone)
                result = "Black win!";
            else
                result = "draw";
            Console.Write("white: {0}\nblack: {1}\n{2}", wstone, bstone, result);
        }
    }
    class COM : Gameroot
    {
        int myturn;
        int mylevel;
        public COM(int getmyturn, int level)
        {
            myturn = getmyturn;
            mylevel = level;
        }
        public string ReturnCmd(int[] board)
        {
            Console.Write("now turn is: {0}...", myturn);
            switch (mylevel)
            {
                default:
                    return player();
                case 1:
                    return Com1(board);
                case 2:
                    return Com2(board);
            }
        }
        string Com1(int[] board)
        {
            string result = "";
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 8; x >= 0; x--)
                {
                    if (IsLegalMove(xy(x, y), myturn, board))
                    {
                        result = xyToCmd(x, y);
                        y = 10;
                        break;
                    }
                }
            }
            if (result == "")
                return "pass";
            return result;
        }
        int GetBoardPoint(int[] board, int[] boardpoint)
        {
            int result = 0;
            for (int n = 0; n < 100; n++)
            {
                result += board[n] * boardpoint[n];
            }
            return result;
        }
        string AllLegalMove(int[] board, int nowturn)
        {
            string result = "";
            for (int y = 0; y <= 8; y++)
            {
                for (int x = 0; x <= 8; x++)
                {
                    if (IsLegalMove(xy(x, y), nowturn, board))
                    {
                        result += xyToCmd(x, y) + " ";
                    }
                }
            }
            result = result.TrimEnd(' ');
            return result;
        }
        /*1つの盤面評価値のみを用いた指し手の選択
        合法手をすべて列挙
        各合法手について盤面評価を行う
        最大値を得た場合のコマンドを返す
        
            */
        string Com2(int[] board)
        {
            int[] boardpoint01 =
                {
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            0, 30,-12,  0, -1, -1,  0,-12, 30,  0,
            0,-12,-15, -3, -3, -3, -3,-15,-12,  0,
            0,  0, -3,  0, -1, -1,  0, -3,  0,  0,
            0, -1, -3, -1, -1, -1, -1, -3, -1,  0,
            0, -1, -3, -1, -1, -1, -1, -3, -1,  0,
            0,  0, -3,  0, -1, -1,  0, -3,  0,  0,
            0,-12,-15, -3, -3, -3, -3,-15,-12,  0,
            0, 30,-12,  0, -1, -1,  0,-12, 30,  0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };
            string result = "";
            int[] tempboard = new int[100];
            int maxpoint = int.MinValue;
            string[] legalmoves = AllLegalMove(board, myturn).Split(' ');
            int move_pattern_count = legalmoves.Length;
            if (legalmoves[0] == "")
                return "pass";
            for (int i = 0; i < move_pattern_count; i++)
            {
                //Console.WriteLine(">{0}, {1}",legalmoves[i],move_pattern_count);
                Array.Copy(board, tempboard, 100);
                tempboard = FlipStone(CmdToIndex(legalmoves[i]), tempboard, myturn);
                int temppoint = GetBoardPoint(board, boardpoint01);
                if (temppoint > maxpoint)
                {
                    result = legalmoves[i];
                    maxpoint = temppoint;
                }
            }
            return result;
        }
        string player()
        {
            return Console.ReadLine();
        }
    }
    static class Program
    {
        /*今後、盤面を一つ戻す処理。を実装したい
        
            
            最初一回初期化してひとつ前まで繰り返す。というのを考えたんですが、やめました…
        盤面のリセットも行わなければいけないことを忘れてました。
        そのうちやります。

        Gameのmode引数に違う数字渡せばいいかな。
        */
        static void Main()
        {
            Console.WriteLine("リバーシ　4.0　\n\nゲームの仕様上、置いた石を置きなおすことはできません。\n");
            int comMaxLevel = 2;//レベル追加したら書き換える
            int logMaxNumbers = 2;//増やしたら書き換える
            int comlev = 0, bcom = 0, wcom = 0;
            int mode = 1;

            //Console.WriteLine("棋譜再現は、実際の棋譜を自動的にシミュレーションする");
            //PlayerSelect(1, 2, "通常プレイ: 1  棋譜再現: 2");
            //modeに2を代入すれば棋譜再現モードが起動。普段のプレイにはいらないかなと。

            //棋譜再現の場合、再現する棋譜を指定

            if (mode == 2)
            {
                mode = PlayerSelect(1, logMaxNumbers, "再現する棋譜を選択してください。");
                var newgame = new Reverse();
                string[] logcommand = OutLog(mode);
                int logrange = logcommand.Length;
                for (int i = 0; i < logrange; i++)
                {
                    if (newgame.Game(logcommand[i], 1))
                    {
                        newgame.WriteResult();
                        break;
                    }
                    if (Console.ReadKey().Key == ConsoleKey.Q)
                    {
                        int endornot = PlayerSelect(0, 1, "棋譜再現モードを中断しますか？はい：０、いいえ：１");
                        if (endornot == 1)
                            break;
                    }
                }
            }
            
            

            /*以下通常プレイモード
            */
            else if (mode == 1)
            {
                mode = PlayerSelect(1, 3, "人vs人: 1 / 人vsCOM: 2 / COMvsCOM: 3");
                if (mode == 1)
                {
                    bcom = 0;
                    wcom = 0;
                }
                else if (mode == 2)
                {
                    mode = PlayerSelect(1, 2, "あなたは先手、後手のどちらにしますか。\n先手:1, 後手:2");
                    comlev = PlayerSelect(1, 2, "NPCの強さを数値で入力してください。");
                    bcom = mode == 1 ? 0 : comlev;
                    wcom = mode == 2 ? 0 : comlev;
                }
                else if (mode == 3)
                {
                    bcom = PlayerSelect(1, comMaxLevel, "先手NPCの強さを数値で入力してください。");
                    wcom = PlayerSelect(1, comMaxLevel, "後手NPCの強さを数値で入力してください。");
                }
                var blackside = new COM(1, bcom);
                var whiteside = new COM(2, wcom);
                var game = new Reverse();
                int i = 0;
                while (true)
                {
                    if (i % 2 + 1 == 1)
                    {
                        if (game.Game(blackside.ReturnCmd(game.GetBoard), 0))
                            break;
                    }
                    else
                    {
                        if (game.Game(whiteside.ReturnCmd(game.GetBoard), 0))
                            break;
                    }
                    i++;
                }
                game.WriteResult();
            }
            Console.ReadLine();
        }
        /*最終的には外部ファイルから棋譜を読み込めるようにしたい。
        その場合、ファイル名を指定してOutLogの引数にすればいいかなと。
            */
        static string[] OutLog(int mode)
        {
            //引数で呼び出されるのは1以上。
            string[] shortB = { "f5", "d6", "c5", "f4", "e7", "f6", "g5", "e6", "e3", };
            //1回　w35/b29
            string txttest2 = "c4c3d3e3d2b4f4f3d6e6f2c6c5b5b6e2d1c1a5e1f5f6a4b3f7a3f1g1e7e8a2c2b2a6a7g5g4h4g6g2f8d7h6g3h3h5c8a1b1d8b7a8c7b8h1h2g8h8g7h7";
            string[] test2 = SubstringAtCount(txttest2, 2);
            switch (mode)
            {
                default:
                    return shortB;
                case 1:
                    return shortB;
                case 2:
                    return test2;
            }

        }
        public static string[] SubstringAtCount(this string self, int count)
        {
            var result = new List<string>();
            var length = (int)Math.Ceiling((double)self.Length / count);

            for (int i = 0; i < length; i++)
            {
                int start = count * i;
                if (self.Length <= start)
                {
                    break;
                }
                if (self.Length < start + count)
                {
                    result.Add(self.Substring(start));
                }
                else
                {
                    result.Add(self.Substring(start, count));
                }
            }

            return result.ToArray();
        }
        static int PlayerSelect(int min, int max, string mes)
        {
            Console.WriteLine(mes);
            Console.Write("選択肢：");
            for (int i = min; i <= max; i++)
            {
                Console.Write(i + ", ");
            }
            Console.Write("\n入力してEnterを押してください>");
            int result = 0;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out result) == false)
                {
                    Console.WriteLine("数値を入力してください。");
                    continue;
                }
                if (result < min || result > max)
                {
                    Console.WriteLine("数値は範囲外です。{0}以上{1}以下を入力してください。", min, max);
                    continue;
                }
                break;
            }
            return result;
        }
    }
}