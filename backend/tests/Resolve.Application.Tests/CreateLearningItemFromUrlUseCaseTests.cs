using Resolve.Application.Interfaces;
using Resolve.Application.LearningItems.Create;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class CreateLearningItemFromUrlUseCaseTests
{
    [Fact]
    public async Task 既存問題なら外部取得せず返す()
    {
        var repository = new FakeLearningItemRepository
        {
            FindByUrlResult = new LearningItem(
                "existing",
                ProblemSource.AtCoder,
                ContestCategory.Beginner,
                'A'
            ),
        };
        var provider = new FakeProvider();
        var useCase = new CreateLearningItemFromUrlUseCase(repository, provider);
        var result = await useCase.ExecuteAsync(
            new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=en")
        );
        Assert.False(result.Created);
        Assert.Equal(0, provider.GetCount);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task 新規問題なら取得結果を保存する()
    {
        var repository = new FakeLearningItemRepository();
        var provider = new FakeProvider();
        var useCase = new CreateLearningItemFromUrlUseCase(repository, provider);
        var result = await useCase.ExecuteAsync(
            new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=en")
        );
        Assert.True(result.Created);
        Assert.Equal(1, provider.GetCount);
        Assert.Equal("abc482", repository.ReceivedItem!.ContestId);
        Assert.Equal(
            "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            repository.ReceivedItem.ProblemUrl
        );
    }

    private sealed class FakeProvider : IProblemMetadataProvider
    {
        public int GetCount { get; private set; }

        public Uri Normalize(Uri uri) => new("https://atcoder.jp/contests/abc482/tasks/abc482_a");

        public Task<ProblemMetadata> GetAsync(
            Uri uri,
            CancellationToken cancellationToken = default
        )
        {
            GetCount++;
            return Task.FromResult(
                new ProblemMetadata(
                    uri,
                    "Nigiri",
                    ProblemSource.AtCoder,
                    ContestCategory.Beginner,
                    'A',
                    "abc482",
                    "ABC 482",
                    new DateOnly(2026, 10, 31)
                )
            );
        }
    }
}
