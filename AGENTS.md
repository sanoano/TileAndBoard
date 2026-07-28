# Repository Guidance

## Unity Pipeline

- When the Unity Editor is running, use the installed Unity Pipeline CLI for tasks that benefit from live Editor access.
- Check connectivity with `unity status`.
- Target this project explicitly with:

  ```bash
  unity command --project-path /Users/neener/Projects/TileAndBoard
  ```

- Prefer Pipeline commands for scene inspection, GameObjects, components, prefabs, Play Mode, Unity tests, console output, captures, and other Editor-dependent operations.
- Use ordinary repository tools for source-only work when live Editor access would not add useful validation or context.
- Treat Pipeline mutations like other project edits: inspect first, preserve unrelated user changes, and verify the result.
- After completing a task that adds functions or changes function signatures, update `Documentation.md` to match before finishing, update function signatures but do not write any comments or descriptions.
