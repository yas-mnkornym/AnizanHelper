// TypeCreator.cs

namespace Twin
{
	using System;
	using System.Collections;
	using Twin.IO;

	/// <summary>
	/// 登録されている型を代わりに初期化するクラス
	/// </summary>
	public sealed class TypeCreator
	{
		private static Hashtable typeTable = new Hashtable();

		private class BbsClassTypes
		{
			public Type ThreadHeader;
			public Type ThreadReader;
			public Type ThreadListReader;
			public Type PostBase;
		}

		/// <summary>
		/// bbsに対応する各クラスを登録
		/// </summary>
		/// <param name="bbs">登録する掲示板</param>
		/// <param name="headerType">bbsのに対応しているヘッダクラスを指定</param>
		/// <param name="readerType">bbsのスレッド読み込みに対応しているリーダーを指定</param>
		/// <param name="listReaderType">bbsのスレッド一覧読み込みに対応しているリーダーを指定</param>
		/// <param name="postType">bbsの投稿に対応しているクラスを指定</param>
		public static void Regist(BbsType bbs, 
			Type headerType, Type readerType, Type listReaderType, Type postType)
		{
			BbsClassTypes obj = new BbsClassTypes();
			obj.PostBase = postType;
			obj.ThreadHeader = headerType;
			obj.ThreadReader = readerType;
			obj.ThreadListReader = listReaderType;

			typeTable[bbs] = obj;
		}

		/// <summary></summary>
		/// <param name="bbs"></param>
		/// <returns></returns>
		private static BbsClassTypes CreateInternal(BbsType bbs)
		{
			if (typeTable.Contains(bbs))
			{
				return  (BbsClassTypes)typeTable[bbs];
			}
			throw new NotSupportedException(bbs.ToString());
		}

		/// <summary>
		/// bbsに対応したヘッダクラスを作成
		/// </summary>
		/// <param name="bbs"></param>
		/// <returns></returns>
		public static ThreadHeader CreateThreadHeader(BbsType bbs)
		{
			BbsClassTypes obj = CreateInternal(bbs);
			return (ThreadHeader)Activator.CreateInstance(obj.ThreadHeader);
		}

		/// <summary>
		/// bbsに対応したスレッドリーダーを作成
		/// </summary>
		/// <param name="bbs"></param>
		/// <returns></returns>
		public static ThreadReader CreateThreadReader(BbsType bbs)
		{
			BbsClassTypes obj = CreateInternal(bbs);
			return (ThreadReader)Activator.CreateInstance(obj.ThreadReader);
		}

		/// <summary>
		/// bbsに対応したスレッド一覧リーダーを作成
		/// </summary>
		/// <param name="bbs"></param>
		/// <returns></returns>
		public static ThreadListReader CreateThreadListReader(BbsType bbs)
		{
			BbsClassTypes obj = CreateInternal(bbs);
			return (ThreadListReader)Activator.CreateInstance(obj.ThreadListReader);
		}

		/// <summary>
		/// bbsに対応した投稿クラスを作成
		/// </summary>
		/// <param name="bbs"></param>
		/// <returns></returns>
		public static PostBase CreatePost(BbsType bbs)
		{
			BbsClassTypes obj = CreateInternal(bbs);
			return (PostBase)Activator.CreateInstance(obj.PostBase);
		}
	}
}
