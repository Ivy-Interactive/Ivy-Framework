import { memo } from 'react';
import { emojiMap } from './emojiMap';

export const CustomEmoji = memo(({ name }: { name: string }) => {
  // Check if emoji exists in map
  const emoji = emojiMap[name];
  if (!emoji || !emoji.src) {
    return <span>{name}</span>;
  }

  // Render valid emoji
  return (
    <img
      src={emoji.src}
      alt={name}
      style={{
        width: '18px',
        height: '18px',
        verticalAlign: 'text-top',
        display: 'inline-block',
      }}
    />
  );
});
