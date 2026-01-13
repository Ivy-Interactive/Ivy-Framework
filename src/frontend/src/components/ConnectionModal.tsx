import { TextShimmer } from './TextShimmer';

export function ConnectionModal() {
  return (
    <div className="fixed inset-0 bg-background/80 flex items-center justify-center z-[1000]">
      <div className="px-8 py-4 bg-card border border-border rounded-lg shadow-lg">
        <TextShimmer>Connection lost. Trying to reconnect...</TextShimmer>
      </div>
    </div>
  );
}
