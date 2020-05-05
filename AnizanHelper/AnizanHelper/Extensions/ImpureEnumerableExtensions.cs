using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnizanHelper
{
	public static class ImpureEnumerableExtensions
	{
		public static IEnumerable<TSource> Do<TSource>(
			this IEnumerable<TSource> enumerable,
			Action<TSource> action)
		{
			if (action == null) { throw new ArgumentNullException(nameof(action)); }

			foreach (var item in enumerable)
			{
				action.Invoke(item);
				yield return item;
			}
		}

		public static void ForEach<TSource>(
			this IEnumerable<TSource> enumerable,
			Action<TSource> action)
		{
			if (action == null) { throw new ArgumentNullException(nameof(action)); }

			foreach (var item in enumerable)
			{
				action.Invoke(item);
			}
		}

		public static void ForEach<TSource>(
			this IEnumerable<TSource> enumerable,
			Action<TSource, int> action)
		{
			if (action == null) { throw new ArgumentNullException(nameof(action)); }

			var i = 0;
			foreach (var item in enumerable)
			{
				action.Invoke(item, i);
				++i;
			}
		}

		public static async Task ForEachAsync<TSource>(
			this IEnumerable<TSource> enumerable,
			Func<TSource, Task> action,
			bool continueOnCapturedContext = false)
		{
			if (action == null) { throw new ArgumentNullException(nameof(action)); }

			foreach (var item in enumerable)
			{
				await action.Invoke(item).ConfigureAwait(continueOnCapturedContext);
			}
		}

		public static async Task ForEachAsync<TSource>(
			this IEnumerable<TSource> enumerable,
			Func<TSource, int, Task> action,
			bool continueOnCapturedContext = false)
		{
			if (action == null) { throw new ArgumentNullException(nameof(action)); }

			var i = 0;
			foreach (var item in enumerable)
			{
				await action.Invoke(item, i).ConfigureAwait(continueOnCapturedContext);
				++i;
			}
		}
	}
}
