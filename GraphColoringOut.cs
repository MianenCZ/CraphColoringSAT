using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphColoringOut
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2 || args.Length == 0 || args[0] == "-h" || args[0] == "-help" || args[0] == "--help")
			{
				Console.WriteLine($"Usage: GraphColoringOut.exe file_name [positive integer]color_count");
				return;
			}
			if (!int.TryParse(args[1], out int k) || k <= 0)
			{
				Console.WriteLine($"String \"{args[1]}\" is not an INTEGER");
				Console.WriteLine($"Usage: GraphColoringOut.exe file_name [positive integer]color_count");
				return;
			}


			string result = "";
			try
			{
				result = File.ReadAllText(args[0]);
			}
			catch
			{
				Console.WriteLine($"File \"{args[0]}\" cannot be loaded");
				return;
			}
			Variables.K = k;
			FromOut(result);



		}

		public static void FromOut(string Data)
		{
			string[] d = Data.Split(new char[] { ' ', '\t', '\n', 'r' }, StringSplitOptions.RemoveEmptyEntries);

			int[] nums = d.Select(x => Convert.ToInt32(x)).Where(x => x > 0).ToArray();


			List<int>[] Res = new List<int>[Variables.K];
			for (int i = 0; i < Variables.K; i++)
			{
				Res[i] = new List<int>();
			}

			for (int i = 0; i < nums.Length; i++)
			{
				Variables.FromCNF(nums[i], out int N, out int C);
				//Console.WriteLine($"N: {N}\tC: {C}");
				Res[C].Add(N+1);
			}

			for (int i = 0; i < Variables.K; i++)
			{
				for (int j = 0; j < Res[i].Count; j++)
				{
					Console.Write($"{Res[i][j]} ");
				}
				Console.Write("\n");
			}

		}
	}

	public static class Variables
	{
		public static int K { get; set; }


		public static int ToCNF(int Node, int Color)
		{
			return ((Node * Variables.K) + Color) + 1;
		}

		public static void FromCNF(int Value, out int Node, out int Color)
		{
			Node = (Value - 1) / Variables.K;
			Color = (Value - 1) % Variables.K;
		}

		public static Tuple<int, int>[] GetDuos(int[] vals)
		{
			List<Tuple<int, int>> duos = new List<Tuple<int, int>>();
			for (int i = 0; i < vals.Length; i++)
			{
				for (int j = i + 1; j < vals.Length; j++)
				{
					Console.WriteLine($"Combo: [{i} {j}]");
					duos.Add(new Tuple<int, int>(i, j));
				}
			}
			return duos.ToArray();
		}
	}
}
