namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Act<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var element in source)
                action.Invoke(element);

            return source;
        }
    }
}