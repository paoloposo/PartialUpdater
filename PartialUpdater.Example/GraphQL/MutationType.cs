using PartialUpdater.Example.Model;
using PartialUpdater.Example.Model.Input;
using PartialUpdater.HotChocolate;

namespace PartialUpdater.Example.GraphQL;

public class MutationType : ObjectType
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field("updateBook")
            .Argument("id", argumentDescriptor => argumentDescriptor
                .Type<NonNullType<IntType>>()
                .Description("Set this to 17."))
            .Argument("input", argumentDescriptor => argumentDescriptor.
                Type<NonNullType<UpdateBookInputType>>())
            .Resolve(context =>
            {
                var id = context.ArgumentValue<int>("id");
                if (id != 17) throw new Exception("book not found");
                
                // this would be loaded from query model
                var book = new Book
                {
                    Id = 17,
                    Title = "Animal Farm",
                    Author = "George Orwell",
                    Edition = 4,
                };
                
                // apply the partial update to the destination (book)
                // only the fields of the 'input' argument which are actually set (including explicitly set to null)
                // by the client are actually mapped to the destination
                context.PartiallyUpdate<UpdateBookInput, Book>("input", book);

                // submit changes...
                
                return book;
            });
    }
}