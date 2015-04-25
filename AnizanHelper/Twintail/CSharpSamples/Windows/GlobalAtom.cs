// GlobalAtom.cs

namespace CSharpSamples.Winapi
{
	using System;
	using System.Text;
	using System.Collections;
	using CSharpSamples.Winapi;

	/// <summary>
	/// GlobalAtom ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	public class GlobalAtom
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="atomString"></param>
		/// <returns></returns>
		public static int Add(string atomString)
		{
			int atom = WinApi.GlobalAddAtom(atomString);
			return atom;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atom"></param>
		public static void Delete(int atom)
		{
			WinApi.GlobalDeleteAtom(atom);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atom"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static string Get(int atom, int bufferSize)
		{
			StringBuilder buffer = new StringBuilder(bufferSize);
			WinApi.GlobalGetAtomName(atom, buffer, bufferSize);

			return buffer.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atomString"></param>
		/// <returns></returns>
		public static int Find(string atomString)
		{
			return WinApi.GlobalFindAtom(atomString);
		}
	}
}
