// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Linq;

internal static class EnumerableExtensions
{
    public static IEnumerable<TResult> UnsafeCast<TResult>(this IEnumerable source)
        where TResult : class?
    {
        foreach (var item in source)
            yield return Unsafe.As<TResult>(item);
    }
}
