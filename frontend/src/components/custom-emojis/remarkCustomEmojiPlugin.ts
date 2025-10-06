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

      // For each emoji pattern, create an image node with hName emoji and a property name that contains the emoji-name
      const newNodes: RootContent[] = parts.map<RootContent>(part => {
        // Check if this part looks like an emoji pattern (:name:)
        if (part.startsWith(':') && part.endsWith(':')) {
          const imgNode: Image = {
            type: 'image',
            url: emojiMap[part]?.src || '', // Use empty string if not found
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
