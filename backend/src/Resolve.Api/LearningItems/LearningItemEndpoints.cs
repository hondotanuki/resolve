using Resolve.Api.LearningItems.Create;
using Resolve.Api.LearningItems.GetById;
using Resolve.Api.LearningItems.List;

namespace Resolve.Api.LearningItems;

public static class LearningItemEndpoints
{
    public static void MapLearningItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/learning-items");

        group.MapCreateLearningItem();
        group.MapListLearningItems();
        group.MapGetLearningItemById();
    }
}
