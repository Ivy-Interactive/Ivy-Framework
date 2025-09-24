export type EmojiEntry = {
  type: 'svg' | 'img';
  src: string;
};

export const emojiMap: Record<string, EmojiEntry> = {
  ':ivy-branded-star:': {
    src: './ivy-branded-star.svg',
    type: 'svg',
  },
};
