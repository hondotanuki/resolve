using Resolve.Api.Contracts;
using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class ItemTypeMapping
{
    public static bool TryParse(string value, out ItemType itemType)
    {
        switch (value)
        {
            case ApiValues.ItemTypes.AlgorithmProblem:
                itemType = ItemType.AlgorithmProblem;
                return true;

            case ApiValues.ItemTypes.EnglishWord:
                itemType = ItemType.EnglishWord;
                return true;

            case ApiValues.ItemTypes.EnglishArticle:
                itemType = ItemType.EnglishArticle;
                return true;

            default:
                itemType = default;
                return false;
        }
    }

    public static string ToApiValue(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.AlgorithmProblem => ApiValues.ItemTypes.AlgorithmProblem,

            ItemType.EnglishWord => ApiValues.ItemTypes.EnglishWord,

            ItemType.EnglishArticle => ApiValues.ItemTypes.EnglishArticle,

            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null),
        };
    }
}
