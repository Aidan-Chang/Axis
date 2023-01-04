export interface MiniProfiler {
  container: {
    classList: DOMTokenList
  },
  options: {
    colorScheme: string
  },
  fetchResults(ids: string[]): void;
}