using System.Reflection;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using PartialUpdater.Core;

namespace PartialUpdater.HotChocolate;

public static class ResolverContextExtensions
{
    /// <summary>
    /// Get a list of properties for a particular argument that are provided in the partial update.
    /// </summary>
    /// <param name="context">The resolver context.</param>
    /// <param name="argumentName">The name of the argument.</param>
    /// <typeparam name="TSourceType">The backing type of the argument.</typeparam>
    /// <returns></returns>
    public static IEnumerable<PropertyInfo> GetIncludedProperties<TSourceType>(this IResolverContext context, string argumentName)
    {
        IInputField argumentInput = context.GetRawArgumentByName(argumentName);
        IReadOnlyList<ObjectFieldNode> fields = context.ArgumentLiteral<ObjectValueNode>(argumentName).Fields;
        return fields.Select(field => argumentInput.GetFieldRuntimePropertyInfo<TSourceType>(field.Name.Value));
    }

    /// <summary>
    /// Get information on an argument as it was provided by the client.
    /// </summary>
    /// <param name="context">The resolver context.</param>
    /// <param name="argumentName">The name of the argument.</param>
    /// <returns></returns>
    public static IInputField GetRawArgumentByName(this IResolverContext context, string argumentName)
    {
        return context.Selection.Field.Arguments.First(inputField => inputField.Name.Value == argumentName);
    }

    /// <summary>
    /// Shortcut for getting the PartialUpdater from the service provider and applying an update.
    /// </summary>
    /// <param name="context">The resolver context.</param>
    /// <param name="updater">[Obsolete]</param>
    /// <param name="sourceArgumentName">The name of the mutation's argument that will be applied to the destination.</param>
    /// <param name="destination">The object that is being updated.</param>
    /// <typeparam name="TSource">The backing type of the argument.</typeparam>
    /// <typeparam name="TDestination">The type of the object being updated.</typeparam>
    public static void PartiallyUpdate<TSource, TDestination>(this IResolverContext context, string sourceArgumentName, TDestination destination)
    {
        var updater = context.Service<PartialUpdater<TSource, TDestination>>();
        IEnumerable<PropertyInfo> includedProperties = context.GetIncludedProperties<TSource>(sourceArgumentName);
        var argument = context.ArgumentValue<TSource>(sourceArgumentName);
        updater.Apply(argument, destination, includedProperties);
    }
}