using PartialUpdater.Core;
using PartialUpdater.Example.GraphQL;
using PartialUpdater.Example.Model;
using PartialUpdater.Example.Model.Input;

var builder = WebApplication.CreateBuilder(args);

// build a very simple GraphQL server
builder.Services
    .AddGraphQLServer()
    .AddQueryType<QueryType>()
    .AddMutationType<MutationType>()
    .AddType<BookType>()
    .AddType<UpdateBookInputType>();

// this can be done more cleverly in the future, but it works for now
builder.Services
    .AddSingleton(_ =>
    {
        var updater = new PartialUpdater<UpdateBookInput, Book>();
        // this specifies how source fields are mapped to destination fields
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
        return updater;
    });

var app = builder.Build();

app.MapGet("/", () => "Hello world!");

app.MapGraphQL();

app.Run();