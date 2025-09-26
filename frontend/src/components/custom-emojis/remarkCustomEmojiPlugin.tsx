import { emojiMap } from './emojiMap';
import { RootContent, Node, Parent, Text, Image } from 'mdast';
import { visit } from 'unist-util-visit';

export function remarkCustomEmojiPlugin() {
  return (tree: Node) => {
    visit(tree, 'text', (node: Text, index: number, parent: Parent) => {
      if (!parent || !node.value) return;

      const parts = node.value.split(/(:[a-zA-Z0-9-_]+:)/g);
      if (parts.length === 1) return;

      const newNodes: RootContent[] = parts.map<RootContent>(part => {
        if (emojiMap[part]) {
          const imgNode: Image = {
            type: 'image',
            url: emojiMap[part].src,
            alt: part,
            data: {
              hName: 'emoji',
              hProperties: { name: part },
            },
          };
          return imgNode;
        }
        const textNode: Text = { type: 'text', value: part };
        return textNode;
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
