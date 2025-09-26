import { emojiMap } from './emojiMap';
import { Node, Parent, Text } from 'unist';
import { visit } from 'unist-util-visit';

export function remarkCustomEmojiPlugin() {
  return (tree: Node) => {
    visit(tree, 'text', (node: Text, index: number, parent: Parent) => {
      if (!parent || !node.value) return;

      const parts = node.value.split(/(:[a-zA-Z0-9-_]+:)/g);
      if (parts.length === 1) return;

      const newNodes = parts.map((part: string) => {
        if (emojiMap[part]) {
          return {
            type: 'emoji',
            name: part,
            data: {
              hName: 'emoji',
              hProperties: { name: part },
            },
          };
        }
        return { type: 'text', value: part };
      });

      parent.children.splice(index, 1, ...newNodes);
    });
  };
}

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
