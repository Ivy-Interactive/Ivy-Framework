import { emojiMap } from './emojiMap';

export function CustomEmoji({ name }: { name: string }) {
  const { src } = emojiMap[name];
  return (
    <img
      src={src}
      alt={name}
      style={{
        width: '18px',
        height: '18px',
        verticalAlign: 'text-top',
        display: 'inline-block',
      }}
    />
  );
}
