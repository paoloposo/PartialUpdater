using System.Linq.Expressions;
using System.Reflection;

namespace PartialUpdater.Core;

public class PartialUpdater<TSource, TDestination>
{
    /// <summary>
    /// Stores registered map callbacks by property name
    /// </summary>
    private Dictionary<string, MapCallback> _callbacks = new();

    /// <summary>
    /// Callback that is called when a property is contained in a partial update.
    /// </summary>
    public delegate void MapCallback(TSource source, TDestination destination);

    /// <summary>
    /// Register a callback for when a property is contained in a partial update.
    /// </summary>
    /// <param name="propertyExpression">Must point to a property of the source.</param>
    /// <param name="callback"></param>
    /// <typeparam name="TMember"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException">if the propertyExpression does not point to a property of the source.</exception>
    public PartialUpdater<TSource, TDestination> Register<TMember>(Expression<Func<TSource, TMember>> propertyExpression, MapCallback callback)
    {
        PropertyInfo propertyInfo;
        switch (propertyExpression.Body)
        {
            case MemberExpression { Member: var member, Expression.NodeType: ExpressionType.Parameter or ExpressionType.Convert}:
                propertyInfo = (PropertyInfo) member;
                break;
            default:
                throw new ArgumentException($"Invalid expression: {propertyExpression}. Expression must resolve to top-level member.");
        }

        return Register(propertyInfo.Name, callback);
    }

    /// <summary>
    /// Register a callback for when a property is contained in a partial update.
    /// </summary>
    /// <param name="memberName">Name of the property.</param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public PartialUpdater<TSource, TDestination> Register(string memberName, MapCallback callback)
    {
        _callbacks[memberName] = callback;

        return this;
    }

    /// <summary>
    /// Apply a partial update to the destination.
    /// </summary>
    /// <param name="source">The object containing the update.</param>
    /// <param name="destination">The object the update is applied to.</param>
    /// <param name="properties">A list of properties of the source that shall be respected in the update.</param>
    public void Apply(TSource source, TDestination destination, IEnumerable<PropertyInfo> properties)
    {
        Apply(source, destination, properties.Select(propertyInfo => propertyInfo.Name));
    }

    /// <summary>
    /// Apply a partial update to the destination.
    /// </summary>
    /// <param name="source">The object containing the update.</param>
    /// <param name="destination">The object the update is applied to.</param>
    /// <param name="properties">A list of properties of the source that shall be respected in the update.</param>
    public void Apply(TSource source, TDestination destination, IEnumerable<string> properties)
    {
        foreach (var property in properties)
        {
            if (!_callbacks.ContainsKey(property)) continue;
            var callback = _callbacks[property];
            callback(source, destination);
        }
    }

}