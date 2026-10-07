import { defineConfig } from "orval";

export default defineConfig({
  resolve: {
    input: {
      target: "http://localhost:5041/openapi/v1.json",
    },

    output: {
      target: "./src/api/generated/api.ts",
      schemas: "./src/api/generated/models",
      client: "fetch",
      clean: true,
    },
  },
});
