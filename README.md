# PartialUpdater

This is a very basic implementation of a partial updater.
It solves the problem of applying partial updates to model types through a mutation (aka patching), especially the problem of implicit null values when no value is provided.
It is in itself not specific to GraphQL nor [Hot Chocolate](https://github.com/ChilliCream/hotchocolate), but there are extension methods provided to make it easy to use with Hot Chocolate resolvers.

The implementation and feature set is still very crude, but it works as a proof of concept.

## How it works

The concept is incredibly simple. For a given model type, you can have one or more types that represent the partial updates you are allowed to make to an instance of the model type. At startup, you specify how each field of the partial update is mapped to the model type. Later, you can apply a partial update to an instance of the model by providing a list of fields that should be included. Since you can choose not to include fields that the client hasn't explicitly provided, you won't override unprovided properties with `null`. On the other hand, when `null` _is_ explicitly provided, you can handle that case.

In conjunction with Hot Chocolate mutation resolvers, the key part is getting a list of the explicitly set fields of an input object. The included extension methods provide a solution for this. Hot Chocolate already allows getting an argument in the form the client has provided it. However, since fields can be renamed (and aren't even equal by default, e.g. the `Author` property of a backing class would be mapped to an `author` GraphQL field), the correct mapping from the field name of the GraphQL input type to the property name of the backing C# class has to be found. Once this mapping is achieved, the list of properties can be passed to the partial updater.

## Usage

Create two types, one model (`Book`) and one that represents changes to it (`UpdateBookInput`):

```c#
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Edition { get; set; }
}

public class UpdateBookInput
{
    public string? DifferentlyNamedTitle { get; set; }
    public string? Author { get; set; }
    public bool? Increment { get; set; }
}
```

Create an instance of `PartialUpdater<UpdateBookInput, Book>` and tell it how to map properties from the source to the destination:

```c#
var updater = new PartialUpdater<UpdateBookInput, Book>();

updater
    .Register(input => input.DifferentlyNamedTitle, (source, destination) =>
    {
        destination.Title = source.DifferentlyNamedTitle ?? throw new Exception("title may not be null");
    })
    .Register(input => input.Author, (source, destination) =>
    {
        destination.Author = source.Author ?? throw new Exception("author may not be null");
    })
    .Register(input => input.Increment, (source, destination) =>
    {
        if (source.Increment ?? false) destination.Edition++;
    });
```

Once you have an existing `Book`, a populated `UpdateBookInput` and a list of properties of `UpdateBookInput` that should actually be included in the update, you can call the `PartialUpdater.Apply` method. Only the previously registered callbacks for properties in this list are actually called. The list can either be of type `IEnumerable<string>` (the names of the properties) or of type `IEnumerable<System.Reflection.PropertyInfo>`.

```c#
var book = new Book
{
    Id = 17,
    Title = "Animal Farm",
    Author = "George Orwell",
    Edition = 4,
};

var updateBookInput = new UpdateBookInput
{
    Increment = true,
    Title = "1984",
    // Author will default to null
};

var properties = new List<string> {"Increment"};

// apply the partial update
updater.Apply(updateBookInput, book, properties);
```

Both `Increment` and `Title` were set to non-null values. But since `properties` only included `Increment`, the book will now look like this:

```c#
Book
{
    Id = 17,
    Title = "Animal Farm",
    Author = "George Orwell",
    Edition = 5 // only this has changed
};
```

### Hot Chocolate integration

To use PartialUpdater with Hot Chocolate, several extension methods are provided to automatically retrieve the list of properties to include or to apply a partial update from a mutation resolver. In a resolver of a field with two arguments, `id` and `input`:

```c#
var id = context.ArgumentValue<int>("id");
var book = getBookById(id);

context.PartiallyUpdate<UpdateBookInput, Book>("input", book);

return book;
```

This requires that PartialUpdater has previously been registered in the service provider.

Check the source code to learn how the extension methods work.

## Try it out

This repo includes an example project with the above code and a minimal Hot Chocolate server. Start it up and run the following mutation:

```
mutation($id: Int!, $input: UpdateBookInput!) {
  updateBook(id: $id, input: $input) {
    id
    title
    author
    edition
  }
}
```

Variable:

```json
{
  "id": 17,
  "input": {
    < enter something here>
  }
}
```

Experiment with different fields on `input`. Mutations are not persisted across requests in this example, so you'll always start with the same book instance.