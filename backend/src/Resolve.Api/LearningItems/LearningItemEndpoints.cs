namespace Resolve.Api;

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
