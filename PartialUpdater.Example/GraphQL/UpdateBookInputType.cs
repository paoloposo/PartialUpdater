using PartialUpdater.Example.Model.Input;

namespace PartialUpdater.Example.GraphQL;

public class UpdateBookInputType : InputObjectType<UpdateBookInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateBookInput> descriptor)
    {
        // Suppose you don't like how the model property is named.
        // Changing the field name to something completely arbitrary is supported by the extension methods.
        descriptor
            .Field(input => input.Increment)
            .Name("incrementEdition");
    }
}