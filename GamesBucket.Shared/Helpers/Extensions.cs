using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GamesBucket.Shared.Helpers
{
    public static class Extensions
    {
        public static bool Like(this string toSearch, string toFind)
        {
            return !string.IsNullOrEmpty(toSearch) && 
                   !string.IsNullOrEmpty(toFind) &&
                   new Regex(@"\A" + 
                             new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\")
                                 .Replace(toFind, ch => @"\" + ch)
                                 .Replace('_', '.')
                                 .Replace("%", ".*") + @"\z", RegexOptions.Singleline)
                       .IsMatch(toSearch);
        }
        
        public static string RemoveSubstring(this string source, string substring, bool removeTrailingSpaces = true)
        {
            if (string.IsNullOrEmpty(substring) || 
                !source.Contains(substring, StringComparison.OrdinalIgnoreCase)) return source;
            
            var strIndex = source.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
            var removeLength = substring.Length;
            if (removeTrailingSpaces)
            {
                removeLength += source
                    .Substring(strIndex + substring.Length, 
                        source.Length - (strIndex + substring.Length))
                    .TakeWhile(c => c == ' ').Count();   
            }
            
            return source.Remove(strIndex, removeLength).Trim();
        }

        public static string RemoveSubstringPattern(this string source, string pattern)
        {
            return Regex.Replace(source, pattern, string.Empty).Trim();
        }
        
        /// <summary>
        /// Split a collection into pages of an specified size
        /// </summary>
        /// <param name="source">Collection of objects</param>
        /// <param name="pageSize">Page size</param>
        /// <typeparam name="T">Collection item type</typeparam>
        /// <returns>Collection of pages</returns>
        public static IEnumerable<IEnumerable<T>> Page<T>(this IEnumerable<T> source, int pageSize)
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var currentPage = new List<T>(pageSize)
                {
                    enumerator.Current
                };

                while (currentPage.Count < pageSize && enumerator.MoveNext())
                {
                    currentPage.Add(enumerator.Current);
                }
                yield return new ReadOnlyCollection<T>(currentPage);
            }
        }
        
        /// <summary>
        /// Split a collection into chunks of an specified size
        /// </summary>
        /// <param name="values">Collection of objects</param>
        /// <param name="chunkSize">Chunks size</param>
        /// <typeparam name="TValue">Collection item type</typeparam>
        /// <returns>Collection of chunks</returns>
        public static IEnumerable<IEnumerable<TValue>> Chunk<TValue>(
            this IEnumerable<TValue> values, 
            int chunkSize)
        {
            using var enumerator = values.GetEnumerator();
            while(enumerator.MoveNext())
            {
                yield return GetChunk(enumerator, chunkSize).ToList();
            }
        }

        private static IEnumerable<T> GetChunk<T>(
            IEnumerator<T> enumerator,
            int chunkSize)
        {
            do
            {
                yield return enumerator.Current;
            } while(--chunkSize > 0 && enumerator.MoveNext());
        }
        
        public static PagedResult<T> GetPaged<T>(this IEnumerable<T> query, 
            int page, int pageSize) where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);
 
            var skip = (page - 1) * pageSize;     
            result.Results = query.Skip(skip).Take(pageSize).ToList();
 
            return result;
        }
        
        /// <summary>
        /// Tasks parallelism using a throttling mechanism to achieve a granular parallelism control
        /// </summary>
        /// <param name="source">Collection of items</param>
        /// <param name="asyncAction">Async function to be executed upon the collection</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of tasks allowed at the same time</param>
        /// <typeparam name="T">Collection item class</typeparam>
        /// <typeparam name="TResult">Return type of async function</typeparam>
        /// References:
        /// https://stackoverflow.com/questions/15136542/parallel-foreach-with-asynchronous-lambda
        /// https://www.nuget.org/packages/AsyncEnumerator
        /// https://markheath.net/post/constraining-concurrent-threads-csharp
        public static async Task<IEnumerable<TResult>> ParallelForEachAsync<T, TResult>(this IEnumerable<T> source, 
            Func<T, Task<TResult>> asyncAction, int maxDegreeOfParallelism)
        {
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var bag = new ConcurrentBag<TResult>();
            var tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    var response = await asyncAction(item).ConfigureAwait(false);
                    bag.Add(response);
                }
                finally
                {
                    throttler.Release();
                }
            });
            await Task.WhenAll(tasks);
            return bag.ToList();
        }
        
        /// <summary>
        /// Tasks parallelism using a throttling mechanism to achieve a granular parallelism control
        /// </summary>
        /// <param name="source">Collection of items</param>
        /// <param name="asyncAction">Async function to be executed upon the collection</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of tasks allowed at the same time</param>
        /// <typeparam name="T">Collection item class</typeparam>
        /// References:
        /// https://stackoverflow.com/questions/15136542/parallel-foreach-with-asynchronous-lambda
        /// https://www.nuget.org/packages/AsyncEnumerator
        /// https://markheath.net/post/constraining-concurrent-threads-csharp
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> source, 
            Func<T, Task> asyncAction, int maxDegreeOfParallelism)
        {
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var tasks = source.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    await asyncAction(item).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}