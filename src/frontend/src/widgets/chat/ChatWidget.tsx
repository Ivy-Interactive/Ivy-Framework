import { ChatBubble, ChatBubbleMessage } from '@/components/ChatBubble';
import { ChatInput } from '@/components/ChatInput';
import { ChatMessageList } from '@/components/ChatMessageList';
import { useEventHandler } from '@/components/event-handler';
import { MessageLoading } from '@/components/MessageLoading';
import { Button } from '@/components/ui/button';
import { CornerDownLeft, Square } from 'lucide-react';
import React, { FormEvent, useState, KeyboardEvent, ReactNode } from 'react';
import { User, LucideStars } from 'lucide-react';
import { TextShimmer } from '@/components/TextShimmer';
import { getHeight, getWidth } from '@/lib/styles';

interface ChatMessageWidgetProps {
  id: number;
  children?: ReactNode[];
  sender: 'User' | 'Assistant';
}

export const ChatMessageWidget: React.FC<ChatMessageWidgetProps> = ({
  id,
  sender = 'User',
  children,
}) => {
  return (
    <ChatBubble key={id} variant={sender === 'User' ? 'sent' : 'received'}>
      {sender == 'Assistant' && (
        <div className="bg-muted p-2 rounded-full h-9 w-9 flex items-center justify-center">
          <LucideStars />
        </div>
      )}

      {sender == 'User' && (
        <div className="bg-muted p-2 rounded-full h-9 w-9 flex items-center justify-center">
          <User />
        </div>
      )}

      <ChatBubbleMessage variant={sender === 'User' ? 'sent' : 'received'}>
        <div>{children}</div>
      </ChatBubbleMessage>
    </ChatBubble>
  );
};

ChatMessageWidget.displayName = 'ChatMessageWidget';

interface ChatWidgetProps {
  id: string;
  placeholder?: string;
  streaming?: boolean;
  children: React.ReactElement<ChatMessageWidgetProps>[];
  width?: string;
  height?: string;
}

export const ChatWidget: React.FC<ChatWidgetProps> = ({
  id,
  children,
  placeholder = 'Type a message...',
  streaming = false,
  width = 'Full',
  height = 'Full',
}) => {
  const eventHandler = useEventHandler();

  const messageWidgets = React.Children.toArray(children).filter(child => {
    if (!React.isValidElement(child)) return false;

    // Direct component check
    if (
      (child.type as React.ComponentType<unknown>)?.displayName ===
      'ChatMessageWidget'
    ) {
      return true;
    }

    // MemoizedWidget check - look at node.type prop
    const props = child.props as { node?: { type?: string } };
    if (props.node?.type === 'Ivy.ChatMessage') {
      return true;
    }

    return false;
  });

  // Check if any ChatMessage contains ChatLoading as its child
  const hasLoadingWidget = React.Children.toArray(children).some(child => {
    if (
      React.isValidElement(child) &&
      (child.type as React.ComponentType<unknown>)?.displayName ===
        'ChatMessageWidget' &&
      child.props &&
      typeof child.props === 'object' &&
      'children' in child.props
    ) {
      // Check children of ChatMessage for ChatLoadingWidget
      const messageChildren = React.Children.toArray(
        (child.props as { children?: ReactNode }).children
      );
      return messageChildren.some(
        msgChild =>
          React.isValidElement(msgChild) &&
          (msgChild.type as React.ComponentType<unknown>)?.displayName ===
            'ChatLoadingWidget'
      );
    }
    return false;
  });

  const isLoading = hasLoadingWidget || streaming;

  const [input, setInput] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!input.trim()) return;
    setInput('');
    eventHandler('OnSend', id, [input.trim()]);
  };

  const handleCancel = (e: React.MouseEvent) => {
    e.preventDefault();
    eventHandler('OnCancel', id, []);
  };

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e as unknown as FormEvent);
    }
  };

  const style = {
    ...getWidth(width),
    ...getHeight(height),
  };

  return (
    <div className="flex flex-col" style={style}>
      <div className="flex-1 overflow-hidden">
        <ChatMessageList>{messageWidgets}</ChatMessageList>
      </div>

      <div className="m-4">
        <form
          onSubmit={handleSubmit}
          className="relative rounded-field border bg-background focus-within:ring-1 focus-within:ring-ring p-1"
        >
          <ChatInput
            value={input}
            onChange={e => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="min-h-12 resize-none rounded-box bg-background border-0 p-3 shadow-none focus-visible:ring-0"
          />
          <div className="flex items-center p-3 pt-0 justify-between">
            {isLoading ? (
              <Button
                type="button"
                onClick={handleCancel}
                variant="destructive"
                className="ml-auto gap-1.5"
              >
                Cancel Request
                <Square className="size-3.5 fill-black" />
              </Button>
            ) : (
              <Button type="submit" className="ml-auto gap-1.5">
                Send Message
                <CornerDownLeft className="size-3.5" />
              </Button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
};

type ChatLoadingWidgetProps = Record<never, never>;

export const ChatLoadingWidget: React.FC<ChatLoadingWidgetProps> = () => {
  return <MessageLoading />;
};

ChatLoadingWidget.displayName = 'ChatLoadingWidget';

interface ChatStatusWidgetProps {
  text: string;
}

export const ChatStatusWidget: React.FC<ChatStatusWidgetProps> = ({ text }) => {
  return (
    <TextShimmer
      duration={1.2}
      className="font-medium [--base-color:#0bae59] [--base-gradient-color:#15d06e]"
    >
      {text}
    </TextShimmer>
  );
};
