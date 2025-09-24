import { emojiMap } from './emojiMap';

export function parseEmojis(text: string, size: number = 24): string {
  const pattern = new RegExp(
    '(' +
      Object.keys(emojiMap).map(k => k.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')) +
      ')',
    'g'
  );

  return text.replace(pattern, match => {
    const emoji = emojiMap[match];
    if (!emoji) return match;

    return `<img src="${emoji.src}" width="${size}" height="${size}" style="display:inline-block;vertical-align:middle;" />`;
  });
}
