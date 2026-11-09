using Resolve.Application.Interfaces;
using Resolve.Application.LearningItems.Create;
using Resolve.Application.Tests.Fakes;
using Resolve.Domain;

namespace Resolve.Application.Tests;

public sealed class CreateLearningItemUseCaseTests
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
                'A',
                problemUrl: "https://atcoder.jp/contests/abc482/tasks/abc482_a",
                contestId: "abc482",
                contestName: "ABC 482",
                contestStartDate: new DateOnly(2026, 10, 31)
            ),
        };
        var provider = new FakeProvider();
        var useCase = new CreateLearningItemUseCase(repository, provider);
        var result = await useCase.ExecuteAsync(
            new CreateLearningItemInput(
                new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=en")
            )
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
        var useCase = new CreateLearningItemUseCase(repository, provider);
        var result = await useCase.ExecuteAsync(
            new CreateLearningItemInput(
                new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a?lang=en")
            )
        );
        Assert.True(result.Created);
        Assert.Equal(1, provider.GetCount);
        Assert.Equal("abc482", repository.ReceivedItem!.ContestId);
        Assert.Equal(
            "https://atcoder.jp/contests/abc482/tasks/abc482_a",
            repository.ReceivedItem.ProblemUrl
        );
    }

    [Fact]
    public async Task 情報取得失敗時は保存しない()
    {
        var repository = new FakeLearningItemRepository();
        var provider = new FakeProvider { Fail = true };
        var useCase = new CreateLearningItemUseCase(repository, provider);
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            useCase.ExecuteAsync(
                new CreateLearningItemInput(
                    new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a")
                )
            )
        );
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task 異なるURLの取得結果は保存しない()
    {
        var repository = new FakeLearningItemRepository();
        var provider = new FakeProvider { MismatchedUrl = true };
        var useCase = new CreateLearningItemUseCase(repository, provider);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            useCase.ExecuteAsync(
                new CreateLearningItemInput(
                    new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_a")
                )
            )
        );
        Assert.Equal(0, repository.CreateCallCount);
    }

    private sealed class FakeProvider : IProblemMetadataProvider
    {
        public int GetCount { get; private set; }
        public bool Fail { get; init; }
        public bool MismatchedUrl { get; init; }

        public Uri Normalize(Uri uri) => new("https://atcoder.jp/contests/abc482/tasks/abc482_a");

        public Task<ProblemMetadata> GetAsync(
            Uri uri,
            CancellationToken cancellationToken = default
        )
        {
            GetCount++;
            if (Fail)
                throw new HttpRequestException("Fetch failed.");
            return Task.FromResult(
                new ProblemMetadata(
                    MismatchedUrl
                        ? new Uri("https://atcoder.jp/contests/abc482/tasks/abc482_b")
                        : uri,
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
