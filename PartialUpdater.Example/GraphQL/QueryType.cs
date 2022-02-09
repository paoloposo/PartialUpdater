namespace PartialUpdater.Example.GraphQL;

public class QueryType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field("ping")
            .Resolve("Try the 'updateBook' mutation!");
    }
}