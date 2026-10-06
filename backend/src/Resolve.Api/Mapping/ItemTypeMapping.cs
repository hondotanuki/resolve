using Resolve.Domain;

namespace Resolve.Api.Mapping;

public static class ItemTypeMapping
{
    public static bool TryParse(string value, out ItemType itemType)
    {
        switch (value)
        {
            case "algorithm_problem":
                itemType = ItemType.AlgorithmProblem;
                return true;

            case "english_word":
                itemType = ItemType.EnglishWord;
                return true;

            case "english_article":
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
            ItemType.AlgorithmProblem => "algorithm_problem",

            ItemType.EnglishWord => "english_word",

            ItemType.EnglishArticle => "english_article",

            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null),
        };
    }
}
