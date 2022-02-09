using System.Reflection;
using HotChocolate.Types;

namespace PartialUpdater.HotChocolate;

public static class InputFieldExtensions
{
    /// <summary>
    /// Get information about the runtime property that an argument's field is associated with.
    /// </summary>
    /// <param name="argument">The argument.</param>
    /// <param name="fieldName">The field.</param>
    /// <typeparam name="TSource">The backing type of the argument.</typeparam>
    /// <returns></returns>
    public static PropertyInfo GetFieldRuntimePropertyInfo<TSource>(this IInputField argument, string fieldName)
    {
        IInputType argumentType = argument.Type;
        // if the argument type is non-nullable, we only care about the encapsulated type
        if (argumentType is NonNullType) argumentType = (IInputType) argumentType.InnerType();
        InputObjectType<TSource> argumentGraphQlType = (InputObjectType<TSource>) argumentType;
        InputField field = argumentGraphQlType.Fields.First(field => field.Name == fieldName);
        // the property 'Property' of type 'PropertyInfo' cannot be read from the InputField instance because it is internal
        // therefore, we currently have to use reflection to get it
        return field.GetPropertyInfoOfInputFieldUsingReflection();
    }
    
    /// <summary>
    /// A hack to get the property named 'Property' of an InputField instance.
    /// </summary>
    /// <param name="instance">The inputField instance to the get the PropertyInfo of.</param>
    /// <returns></returns>
    public static PropertyInfo GetPropertyInfoOfInputFieldUsingReflection(this InputField instance)
    {
        Type type = typeof(InputField);
        PropertyInfo propertyInfo = type.GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance)!;
        dynamic value = propertyInfo.GetValue(instance)!;
        return (PropertyInfo) value;
    }
}