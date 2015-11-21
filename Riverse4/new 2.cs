using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverse03
{
	class Reverse
	{
		int[] board = new int[100];
		int wall = -1, WHITE = 2, BLACK = 1, player, wstones = 0, bstones = 0, nowturn = 1;
		int[] dir = { -10, -9, -11, -1, 1, 11, 9, 10 };
		bool endflag = false;

		//外部操作系
		public Reverse(int mode)
		{
			int wside = 0, bside = 0;
			while (mode==1)
			{
				Console.WriteLine("プレイモードを選択\n人 v s人：１、人 vs com：２、com vs com：３");
				int playmode = int.Parse(Console.ReadLine());
				if (playmode == 2)
				{
					Console.WriteLine("どちらが先手ですか？player:1, com:2");
					if (int.TryParse(Console.ReadLine(), out player) == false)
						continue;
					else
					{
						Console.WriteLine("comのモードを番号で選択してください。");
						if (player == WHITE && int.TryParse(Console.ReadLine(), out bside) == false)
							continue;
						else if (int.TryParse(Console.ReadLine(), out wside) == false)
							continue;
					}
				}
				else if (playmode == 3)
				{
					string mes = "のcomのモードを番号で選択してください。";
					Console.WriteLine("先手" + mes);
					if (int.TryParse(Console.ReadLine(), out bside) == false)
						continue;
					else
					{
						Console.WriteLine("後手" + mes);
						if (int.TryParse(Console.ReadLine(), out wside) == false)
							continue;
					}
				}
				else
					continue;
				ResetBoard();
				Game(wside, bside);
			}
			if (mode == 2)
				ResetBoard();
		}

		public int[] getmap { get { return board; } }
		public void PublicPrintBoard() => PrintBoard(board);
		public void ChangeTurn() => nowturn = 3 - nowturn;
		public void GetLogMove(string cmd)
		{
			wstones = 0; bstones = 0;
			if (ExistLegalMove(nowturn))
			{
				int index = CmdToIndex(cmd);
				board[index] = nowturn;
				FlipStone(index);
			}
			else
			{
				nowturn = Otherside(nowturn);
				GetLogMove(cmd);
			}
		}
		protected void GetComMove(string cmd)
		{
			wstones = 0; bstones = 0;
			if (ExistLegalMove(nowturn))
			{
				int index = CmdToIndex(cmd);
				board[index] = nowturn;
				FlipStone(index);
			}
			else
			{
				nowturn = Otherside(nowturn);
			}
		}
		public void GetMove()
		{
			if (ExistLegalMove(nowturn))
			{
				endflag = false;
				string cmd = "";
				for (int temp = 0; temp < 2; temp++)
				{
					cmd += Console.ReadKey().KeyChar.ToString();
				}
				int index = CmdToIndex(cmd);
				if (IsLegalMove(index, nowturn))
				{
					board[index] = nowturn;
					FlipStone(index);
				}
				else
				{
					PrintBoard(board);
					Console.WriteLine("ここにはおけません。");
					WriteCanMove(nowturn);
					GetMove();
				}
			}
			else
			{
				endflag = true;
				Console.WriteLine("おけるところはありません。");
			}
		}

		//変換系
		protected int xyToIndex(int x, int y) => y * 10 + x;
		protected int CmdToIndex(string cmd)
		{
			int[] xy = new int[2];
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
						xy[1] = int.Parse(cmd[i].ToString());
						break;
					case 'a':
						xy[0] = 1;
						break;
					case 'b':
						xy[0] = 2;
						break;
					case 'c':
						xy[0] = 3;
						break;
					case 'd':
						xy[0] = 4;
						break;
					case 'e':
						xy[0] = 5;
						break;
					case 'f':
						xy[0] = 6;
						break;
					case 'g':
						xy[0] = 7;
						break;
					case 'h':
						xy[0] = 8;
						break;
				}
			}
			return xy[0] + xy[1] * 10;
		}
		protected string IndexToCmd(int index)
		{
			int x = index / 10;
			int y = index % 10;
			string result = "";
			switch (y)
			{
				case 1: result += "a"; break;
				case 2: result += "b"; break;
				case 3: result += "c"; break;
				case 4: result += "d"; break;
				case 5: result += "e"; break;
				case 6: result += "f"; break;
				case 7: result += "g"; break;
				case 8: result += "h"; break;
			}
			return result + Convert.ToString(x);
		}
		protected int Otherside(int nowside) => 3 - nowside;

		//探索系
		bool ExistLegalMove(int nowside)
		{
			for (int y = 1; y <= 8; y++)
			{
				for (int x = 1; x <= 8; x++)
				{
					if (IsLegalMove(xyToIndex(x, y), nowside))
					{
						return true;
					}
				}
			}
			return false;
		}
		int CountTurnOver(int target, int direction, int nowside)
		{
			int i;
			for (i = 1; board[target + i * direction] == Otherside(nowturn); i++)
			{ }
			if (board[target + i * direction] == nowturn)
			{
				return i - 1;
			}
			else
			{
				return 0;
			}
		}
		protected bool IsLegalMove(int target, int nowside)
		{
			if (target < 0 || target > 100) return false;
			else if (board[target] != 0) return false;
			bool result = false;
			for (int i = 0; i < 8; i++)
			{
				if (CountTurnOver(target, dir[i], nowside) > 0)
				{
					result = true;
					break;
				}
			}
			return result;
		}
		void CountStone()
		{
			wstones = 0;
			bstones = 0;
			for (int y = 0; y <= 8; y++)
			{
				for (int x = 1; x <= 8; x++)
				{
					int worb = board[xyToIndex(x, y)];
					if (worb == 2)
					{
						wstones++;
						//Console.Write("{0}w{1}, ",wstones, xyToIndex(x, y));
					}
					else if (worb == 1)
					{
						bstones++;
						//Console.Write("{0}b{1}, ",bstones,xyToIndex(x, y));
					}
				}
			}
		}

		//盤面変更処理系
		void ResetBoard()
		{
			for (int index = 0; index < 100; index++)
			{
				if (index < 11 || index > 89 || index % 10 == 0 || index % 10 == 9)
					board[index] = wall;
			}
			board[44] = WHITE;
			board[45] = BLACK;
			board[54] = BLACK;
			board[55] = WHITE;
		}
		void FlipStone(int targetindex)
		{
			for (int i = 0; i < 8; i++)
			{
				int count = CountTurnOver(targetindex, dir[i], nowturn);
				for (int j = 0; j <= count; j++)
					board[targetindex + j * dir[i]] = nowturn;
			}
		}

		//表示系
		void PrintBoard(int[] map)
		{
			//Console.Clear();
			CountStone();
			Console.WriteLine("\n	a b c d e f g h");
			Console.WriteLine("  # # # # # # # # # #");
			for (int y = 1; y <= 8; y++)
			{
				Console.Write("{0} # ", y);
				for (int x = 1; x <= 8; x++)
				{
					Console.Write("{0} ", map[xyToIndex(x, y)]);
				}
				Console.Write("#\n");
			}
			Console.WriteLine("  # # # # # # # # # #");
			//Console.WriteLine("white:{0},black: {1}", wstones, bstones);
		}
		void WriteCanMove(int nowside)
		{
			for (int y = 1; y <= 8; y++)
			{
				for (int x = 1; x <= 8; x++)
				{
					if (IsLegalMove(xyToIndex(x, y), nowside))
					{
						Console.Write(IndexToCmd(xyToIndex(x, y)) + " ");
					}
				}
			}
			Console.Write(" >");
		}
		public void WriteResult()
		{
			string result;
			if (wstones > bstones)
				result = "White win!";
			else if (wstones < bstones)
				result = "Black win!";
			else
				result = "draw";
			Console.Write("white: {0}\nblack: {1}\n{2}", wstones, bstones, result);
		}

		//ゲーム進行処理
		public void Game(int w_is, int b_is)
		{
			//var cmd = Console.ReadKey();
			//引数に0=プレイヤー…それ以外=com。ただし該当するcomがない場合はプレイヤー操作
			PrintBoard(board);
			int nowplayer;
			if (ExistLegalMove(nowturn))
			{
				if (nowturn == BLACK)
					nowplayer = b_is;
				else
					nowplayer = w_is;
				if (nowplayer == 0)
					GetMove();
				switch (nowplayer)
				{
					default:
						GetMove();
						break;
					case 1:
						GetComMove(ReturnCOM1cmd(board, nowturn));
						nowturn = Otherside(nowturn);
						break;
				}
			}
			else if(Array.IndexOf(board,0)==-1)
			{
				endflag = true;
				WriteResult();
			}
			else
			{
				nowturn = Otherside(nowturn);
			}
			if (endflag == false)
				Game(w_is, b_is);
			
		}

		//NPC関係
		public string ReturnCOM1cmd(int[] nowboard,int com1turn)
		{
			string cmd = "";
			for (int y = 1; y <= 8; y++)
			{
				for (int x = 1; x <= 8; x++)
				{
					if (IsLegalMove(xyToIndex(x, y), com1turn))
					{
						cmd = IndexToCmd(xyToIndex(x, y));
						Console.WriteLine(cmd);
						y = 9;
						break;
					}
				}
			}
			return cmd;
		}
	}
	class Command
	{
		public string ReturnCmd(int[] board,int nowturn)
		{
			string cmd = "";
			return cmd;
		}
	}
	class COM1 : Command
	{

	}
	static class Program
	{
		static void Main()
		{
			Console.Write("実践モード: 1  ログモード: 2 >");
			int setmode1 = 0;
			if (int.TryParse(Console.ReadKey().KeyChar.ToString(), out setmode1) == false)
				Main();
			Console.WriteLine();
			if (setmode1 == 1)
			{
				var vattle = new Reverse(setmode1);
			}
			else if (setmode1 == 2)
			{
				int temp = 0;
				if (int.TryParse(Console.ReadLine(), out temp) == false)
					Main();
				string[] log = SetLog(temp);

				Reverse newgame = new Reverse(setmode1);

				int turn = 0;
				while (true)
				{
					newgame.PublicPrintBoard();
					//Console.WriteLine(turn);
					//newgame.GetMove();
					try
					{
						newgame.GetLogMove(log[turn]);
					}
					catch (IndexOutOfRangeException)
					{
						break;
					}
					newgame.ChangeTurn();
					turn++;
				}
				newgame.WriteResult();
			}
			else
				Main();
			var cmd = Console.ReadKey().KeyChar.ToString();
			if (cmd == "1")
				Main();
		}
		static string[] SetLog(int i)
		{
			string[] shortB = { "f5", "d6", "c5", "f4", "e7", "f6", "g5", "e6", "e3", };
			string[] shortW = { "f5", "f6", "e6", "f4", "e3", "d2", "d3", "d6", "c4", "b4" };
			string[] test1 = { "c4", "c3", "d3", "c5", "d6", "f4", "f5", "e6", "c6", "f6", "g5", "h6", "e7", "f8", "b4", "a3", "b5", "a5", "d7", "c8", "b6", "a6", "b3", "f3", "g7", "e3", "c2", "h8", "g4", "c1", "b7", "a8", "b2", "a1", "a4", "h3", "g2", "h1", "a7", "g3", "d2", "d1", "e1", "e2", "a2", "f1", "f7", "h4", "h5", "b1", "h7", "g1", "h2", "f2", "g6", "e8", "d8", "b8", "c7", "g8" };
			//以下参考サイト→http://murakami-takeshi.blog.so-net.ne.jp/2013-09-01-1
			//1回　w35/b29
			string txttest2 = "c4c3d3e3d2b4f4f3d6e6f2c6c5b5b6e2d1c1a5e1f5f6a4b3f7a3f1g1e7e8a2c2b2a6a7g5g4h4g6g2f8d7h6g3h3h5c8a1b1d8b7a8c7b8h1h2g8h8g7h7";
			string[] test2 = SubstringAtCount(txttest2, 2);
			//2回 w25/b39
			string txttesst3 = "f5f6e6f4g5g6e7h6f3e3g4c5d6h3h5d3h7f7f8c6d7d8c8e8c4g3b5b6c3a5c7b7c2b4f2f1e2g2a4e1d1h4g7d2c1b3a3a2a8b8a6a7a1b1b2h8g8h2h1g1";
			string[] test3 = SubstringAtCount(txttesst3, 2);
			//3回 w37/b27
			string txttest4 = "c4c3d3e3d2b4f4f3d6e6f2c6c5b5a4a5e2d1a6b6c1b1c2b3a7e1a3g5f5g6f1g1h6g3c7g4d7b2f7e8f6h5h4h3h2d8f8e7a1c8h1g2h7a2b8g8b7a8g7h8";
			string[] test4 = SubstringAtCount(txttest4, 2);
			//4回 w45/b19
			string[] test5 = SubstringAtCount("f5d6c3d3c4f4f6g5e6f7g6c5f3e7e3g4h4h6d7e2d2h5h3g3c6c8h7c7e1b6d8b5h2b4e8f8g7h8b3h1f2c2a6a3g8a4c1a5a2f1a7d1b7g2g1b1b2a1b8a8", 2);
			//5回 w34/b30
			string[] test6 = SubstringAtCount("c4c3d3e3f4d6e6b4c6g5g4f6f5f3g3h4h3h5h6g6e7c5b5a6f2e2e1c7f7d7b6a5d8c8e8f8d2c2b1c1d1g1b3a2a4a3b8a8a7b7a1b2f1h2h1g2g7h8g8h7", 2);
			//6回 w32/b32
			string[] test7 = SubstringAtCount("f5d6c3d3c4f4f6f3e6e7d7g6g5c5b6c6b5c7f8e8f7h6d2g4c8b3b4a5a6d8a4a3h3h5e3e2f1c1e1c2g3g7f2a7h4h2b7a8b8d1h7h8g8g1a2a1b1b2h1g2", 2);
			int testnum = 0;
			switch (i)
			{
				case 2:
					return shortW;
				case 3:
					return shortB;
				case 4:
					return test1;
				case 5:
					return test2;
				case 6:
					return test3;
				case 7:
					return test4;
				case 8:
					return test5;
				case 9:
					return test6;
				case 10:
					return test7;
				default:
					Console.Write("\n>");
					testnum = int.Parse(Console.ReadLine());
					break;
			}
			switch (testnum)
			{
				default:
					return shortB;
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
		static string GetPutStoneCommand()
		{
			Console.WriteLine("2つのキーを入力,例)2→02");
			var getkey1 = Console.ReadKey();
			var getkey2 = Console.ReadKey();
			return getkey1.KeyChar.ToString() + getkey2.KeyChar.ToString();

		}
	}
}