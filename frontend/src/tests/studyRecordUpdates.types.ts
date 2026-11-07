import type {
  StudyRecordUpdate,
  useStudyRecordUpdate,
} from "../api/studyRecordUpdates";

// tscで契約を検証する。実行時テストとしては読み込まない。
export function checkStudyRecordUpdateTypes(
  update: ReturnType<typeof useStudyRecordUpdate>["update"],
) {
  const valid: StudyRecordUpdate[] = [
    { result: "success" },
    { studiedAt: "2026-11-07T09:00:00Z" },
    { durationSeconds: null },
    { memo: null },
    { result: "failure", memo: "復習" },
  ];
  valid.forEach((data) => void update(1, data));

  // @ts-expect-error 更新項目が必要
  void update(1, {});
  // @ts-expect-error undefinedは項目を指定したことにならない
  void update(1, { result: undefined });
  // @ts-expect-error resultはnull不可
  void update(1, { result: null });
  // @ts-expect-error resultは定義済みの値のみ
  void update(1, { result: "invalid" });
  // @ts-expect-error 有効な項目があっても他の項目にundefinedは指定できない
  void update(1, { result: "success", memo: undefined });
}
