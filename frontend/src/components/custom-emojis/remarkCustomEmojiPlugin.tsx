import { emojiMap } from './emojiMap';
import { RootContent, Node, Parent, Text, Image } from 'mdast';
import { visit } from 'unist-util-visit';

/**
 * Remark plugin to replace :emoji-name: in text with a custom emoji node.
 * This creates Image nodes with `data.hName = 'emoji'` so ReactMarkdown can render them.
 */
export function remarkCustomEmojiPlugin() {
  return (tree: Node) => {
    // For each 'text' node in the markdown
    visit(tree, 'text', (node: Text, index: number, parent: Parent) => {
      if (!parent || !node.value) return;

      // find the :emoji-name:
      const parts = node.value.split(/(:[a-zA-Z0-9-_]+:)/g);
      if (parts.length === 1) return;

      // For each known emojis, create an image node with hName emoji and a property name that contains the emoji-name
      const newNodes: RootContent[] = parts.map<RootContent>(part => {
        if (emojiMap[part]) {
          const imgNode: Image = {
            type: 'image',
            url: emojiMap[part].src, // This is actually not used
            alt: part, // This is actually not used
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
