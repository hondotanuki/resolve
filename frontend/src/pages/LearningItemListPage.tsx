import { Link } from "react-router-dom";
import { useListLearningItems } from "../api/generated";

export function LearningItemListPage() {
  const { data: response, isPending, isError } = useListLearningItems();
  if (isPending) {
    return <p className="text-sm text-zinc-500">Loading...</p>;
  }
  if (isError || response.status !== 200) {
    return (
      <p className="text-xl font-semibold">
        LearningItemの取得に失敗しました。
      </p>
    );
  }

  const items = response.data;

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold">Learning Items</h1>
      </div>

      {items.length === 0 ? (
        <p className="text-sm text-zinc-500">LearningItemはまだありません。</p>
      ) : (
        <div className="space-y-3">
          {items.map((item) => (
            <Link
              key={String(item.id)}
              to={`/learning-items/${item.id}`}
              className="block rounded-xl border border-zinc-200 bg-white p-5 transition hover:border-zinc-400"
            >
              <p className="mb-1 text-xs text-zinc-500">{item.itemType}</p>

              <h2 className="font-medium">{item.title}</h2>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
