import { ChatBubble, ChatBubbleMessage } from '@/components/ChatBubble';
import { ChatInput } from '@/components/ChatInput';
import { ChatMessageList } from '@/components/ChatMessageList';
import { useEventHandler } from '@/components/event-handler';
import { MessageLoading } from '@/components/MessageLoading';
import { Button } from '@/components/ui/button';
import { CornerDownLeft } from 'lucide-react';
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
  sender,
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
  children: React.ReactElement<ChatMessageWidgetProps>[];
  width?: string;
  height?: string;
}

export const ChatWidget: React.FC<ChatWidgetProps> = ({
  id,
  children,
  placeholder,
  width,
  height,
}) => {
  const eventHandler = useEventHandler();

  const messageWidgets = React.Children.toArray(children).filter(
    child =>
      React.isValidElement(child) &&
      (child.type as React.ComponentType<unknown>)?.displayName ===
        'ChatMessageWidget'
  );

  const [input, setInput] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!input.trim()) return;
    setInput('');
    eventHandler('OnSendMessage', id, [input.trim()]);
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
          className="relative rounded-lg border bg-background focus-within:ring-1 focus-within:ring-ring p-1"
        >
          <ChatInput
            value={input}
            onChange={e => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="min-h-12 resize-none rounded-lg bg-background border-0 p-3 shadow-none focus-visible:ring-0"
          />
          <div className="flex items-center p-3 pt-0 justify-between">
            <Button type="submit" className="ml-auto gap-1.5">
              Send Message
              <CornerDownLeft className="size-3.5" />
            </Button>
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
