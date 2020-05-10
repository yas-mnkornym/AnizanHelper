// Trip.cs

namespace Twin.Tools
{
	using System;
	using FreeBSD.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// トリップを生成するクラス
	/// http://www.egroups.co.jp/files/miscprj-dev/
	/// http://www.egroups.co.jp/files/miscprj-dev/CSharp/Etc/TripMaker.zipを使わせていただきました
	/// ↓変わったみたい
	/// http://www.gir-lab.com/pukiwiki/pukiwiki.php
	/// ↓
	/// 消えちゃったみたい
	/// ↓
	/// NTwin23.105 2009/06/19 2ちゃんねる１２桁トリップ仕様により大幅変更
	/// </summary>
	public class Trip
	{
		/// <summary>
		/// 指定したキーを使用してトリップを生成
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string Create( string key )
		{
			if (key == null) {
				throw new ArgumentNullException("key");
			}
			if (key.Length == 0) {
				throw new ArgumentException("key");
			}

			string trip="???";
			if ( key.Length >= 12 )
			{
				if ( (key[0] == '#') || (key[0] == '$') )
				{
					Match mm = Regex.Match( key , "^#([0-9A-Fa-f]{16})([./0-9A-Za-z]{0,2})$" );
					if ( mm.Success )
					{
						byte[] pb = HexString2ByteArray( mm.Groups[1].Value );
						key = Encoding.ASCII.GetString( pb );
						trip = CreateCore( key , mm.Groups[2].Value );
					}
				}
				else
				{
					byte[] instr = System.Text.Encoding.ASCII.GetBytes( key );
					System.Security.Cryptography.SHA1 sha1
					 = System.Security.Cryptography.SHA1.Create();
					byte[] hash = sha1.ComputeHash( instr );
					trip = Convert.ToBase64String( hash ).Substring( 0 , 12 );
				}
			}
			else
			{
				string saltKey = key + "H.";
				trip = CreateCore( key , new string( new char[] { saltKey[1] , saltKey[2] } ) );
			}

			return trip;
		}

		public static string CreateCore( string key , string salt )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( "key" );
			}
			if ( key.Length == 0 )
			{
				throw new ArgumentException( "key" );
			}

			char s1 = salt[0] , s2 = salt[1];
			if ( s1 < '.' || 'z' < s1 ) s1 = '.';
			if ( s2 < '.' || 'z' < s2 ) s2 = '.';
			if ( ':' <= s1 && s1 <= '@' ) s1 += (char)7;
			if ( ':' <= s2 && s2 <= '@' ) s2 += (char)7;
			if ( '[' <= s1 && s1 <= '`' ) s1 += (char)6;
			if ( '[' <= s2 && s2 <= '`' ) s2 += (char)6;
			salt = s1.ToString() + s2.ToString();

			TraditionalDES des = new TraditionalDES();
			string hash = des.Crypt( key , salt );
			return hash.Substring( hash.Length - 10 );
		}

		public static byte[] HexString2ByteArray( string str )
		{
			int digits = str.Length / 2;
			byte[] bytes = new byte[digits];
			for ( int i=0; i<digits; i++ )
			{
				bytes[i] = Convert.ToByte( str.Substring( i*2 , 2 ) , 16 );
			}
			return bytes;
		}
	}
}
