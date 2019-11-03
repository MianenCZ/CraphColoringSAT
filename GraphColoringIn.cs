using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GraphColoring
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			if (args.Length != 2 || args.Length == 0 || args[0] == "-h" || args[0] == "-help" || args[0] == "--help")
			{
				Console.WriteLine($"Usage: GraphColoringIn.exe file_name [positive integer]color_count");
				return;
			}
			if (!int.TryParse(args[1], out int k) || k <= 0)
			{
				Console.WriteLine($"String \"{args[1]}\" is not an INTEGER");
				Console.WriteLine($"Usage: GraphColoringIn.exe file_name [positive integer]color_count");
				return;
			}

			bool load = Graph.FromDimacs(args[0], out Graph g);
			//Console.WriteLine($"File {args[0]} loaded: {load}");
			if (!load)
			{
				Console.WriteLine($"File {args[0]} cannot be loaded as Graph in DIMACS standard");
				return;
			}
			//Console.WriteLine(g.ToString());
			string res = Graph.GetCollorSat(g, k);

			//Console.WriteLine(res);
			File.WriteAllText($"{args[0]}.cnf", res);

		}
	}

	public class Graph
	{
		public int VerticesCount { get; private set; }
		public int EdgesCount { get; private set; }

		private HashSet<Tuple<int, int>> Edges;

		public Graph(int VerticesCount, int EdgesCount)
		{
			this.VerticesCount = VerticesCount;
			this.EdgesCount = EdgesCount;
			this.Edges = new HashSet<Tuple<int, int>>();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="VertexA"></param>
		/// <param name="VertexB"></param>
		/// <exception cref="ArgumentException"><paramref name="VertexA"/> or <paramref name="VertexB"/> is not in range [1 to VerticesCount]</exception>
		public void AddEdge(int VertexA, int VertexB)
		{
			if (VertexA >= this.VerticesCount || VertexA < 0)
			{
				throw new ArgumentException($"Vertex index {VertexA} is not in range [1 to {this.VerticesCount}]");
			}
			if (VertexB >= this.VerticesCount || VertexB < 0)
			{
				throw new ArgumentException($"Vertex index {VertexB} is not in range [1 to {this.VerticesCount}]");
			}
			this.Edges.Add(new Tuple<int, int>(VertexA, VertexB));
		}

		public Tuple<int, int>[] GetEdges()
		{
			return this.Edges.ToArray();
		}

		public override string ToString()
		{
			StringBuilder bld = new StringBuilder();
			bld.AppendLine($"Graph: {this.VerticesCount} Vertices, {this.EdgesCount} Edges");
			Tuple<int, int>[] e = GetEdges();
			for (int i = 0; i < e.Length; i++)
			{
				bld.AppendLine($"\tEdge: {e[i].Item1} - {e[i].Item2}");
			}
			return bld.ToString();
		}

		public static bool FromDimacs(string Path, out Graph graph)
		{
			graph = null;
			Graph g = null;
			string[] Lines;
			try
			{
				Lines = File.ReadAllLines(Path);
			}
			catch
			{
				return false;
			}

			for (int i = 0; i < Lines.Length; i++)
			{
				string[] data = Lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				switch (data[0])
				{
					case "c":
					{
						continue;
					}
					case "p":
					{
						if (data.Length != 4 || data[1] != "edge")
						{
							return false;
						}

						if (!int.TryParse(data[2], out int VC) || !int.TryParse(data[3], out int EC))
						{
							return false;
						}

						g = new Graph(VC, EC);
					}
					break;

					case "e":
					{
						if ((g is null) || data.Length != 3 || !int.TryParse(data[1], out int A) || !int.TryParse(data[2], out int B))
						{
							return false;
						}

						try
						{
							g.AddEdge(A-1, B-1);
						}
						catch { return false; }
					}
					break;

					default:
					{
						return false;
					}
				}
			}
			graph = g;
			return true;
		}

		public static string GetCollorSat(Graph g, int K)
		{
			int varC = g.VerticesCount * K;
			int clasueC = 0;

			Variables.K = K;

			StringBuilder sat = new StringBuilder();

			for (int i = 0; i < g.VerticesCount; i++)
			{
				for (int j = 0; j < K; j++)
				{
					sat.Append(Variables.ToCNF(i, j));
					sat.Append(" ");
				}
				clasueC++;
				sat.Append("0\n");
			}

			sat.AppendLine("c");

			Tuple<int, int>[] e = g.GetEdges();
			for (int i = 0; i < e.Length; i++)
			{
				for (int j = 0; j < K; j++)
				{
					sat.AppendLine($"-{Variables.ToCNF(e[i].Item1, j)} -{Variables.ToCNF(e[i].Item2, j)} 0");
					//Console.WriteLine(($"-{Variables.ToCNF(e[i].Item1, j)} -{Variables.ToCNF(e[i].Item2, j)} 0"));
					clasueC++;
				}
			}

			sat.AppendLine("c");

			Tuple<int, int>[] colorDuos = Variables.GetDuos(Enumerable.Range(0, K).ToArray());
			for (int i = 0; i < g.VerticesCount; i++)
			{
				for (int j = 0; j < colorDuos.Length; j++)
				{
					sat.AppendLine($"-{Variables.ToCNF(i, colorDuos[j].Item1)} -{Variables.ToCNF(i, colorDuos[j].Item2)} 0");
					clasueC++;
				}
			}

			StringBuilder res = new StringBuilder();

			res.AppendLine("c graph.cnf");
			res.AppendLine("c");
			res.AppendLine($"p cnf {varC} {clasueC}");
			res.AppendLine(sat.ToString());
			return res.ToString();
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
						// Console.WriteLine($"Combo: [{i} {j}]");
						duos.Add(new Tuple<int, int>(i, j));
					}
				}
				return duos.ToArray();
			}
		}
	}
}