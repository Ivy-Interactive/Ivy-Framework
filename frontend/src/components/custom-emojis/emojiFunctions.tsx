import { emojiMap } from './emojiMap';
import DOMPurify from 'dompurify';

export function parseEmojis(text: string, size: number = 24): string {
  const pattern = new RegExp(
    '(' +
      Object.keys(emojiMap)
        .map(k => k.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'))
        .join('|') +
      ')',
    'g'
  );

  const rawHtml = text.replace(pattern, match => {
    const emoji = emojiMap[match];
    if (!emoji) return match;

    return `<img src="${emoji.src}" width="${size}" height="${size}" style="display:inline-block;vertical-align:middle;" />`;
  });
  // Sanitize before returning
  return DOMPurify.sanitize(rawHtml, {
    ALLOWED_TAGS: ['svg', 'polygon', 'path', 'img'],
    ALLOWED_ATTR: [
      'xmlns',
      'viewBox',
      'width',
      'height',
      'fill',
      'stroke',
      'stroke-width',
      'points',
      'src',
      'alt',
      'style',
    ],
  });
}
