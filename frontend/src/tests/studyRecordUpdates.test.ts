import { describe, expect, it } from "vitest";
import { hasStudyRecordUpdate } from "../api/studyRecordUpdates";
import type { UpdateStudyRecordRequest } from "../api/generated/models";

describe("hasStudyRecordUpdate", () => {
  it("既知の更新項目がなければfalseを返す", () => {
    expect(hasStudyRecordUpdate({})).toBe(false);
    const unknownOnly = { unknown: "ignored" } as UpdateStudyRecordRequest;
    expect(hasStudyRecordUpdate(unknownOnly)).toBe(false);
    const inherited = Object.create({
      result: "success",
    }) as UpdateStudyRecordRequest;
    expect(hasStudyRecordUpdate(inherited)).toBe(false);
  });

  it("undefinedは更新と扱わず、null・0・空文字列は更新と扱う", () => {
    const undefinedResult: unknown = { result: undefined };
    expect(
      hasStudyRecordUpdate(undefinedResult as UpdateStudyRecordRequest),
    ).toBe(false);
    expect(hasStudyRecordUpdate({ memo: null })).toBe(true);
    expect(hasStudyRecordUpdate({ durationSeconds: 0 })).toBe(true);
    expect(hasStudyRecordUpdate({ memo: "" })).toBe(true);
  });
});
