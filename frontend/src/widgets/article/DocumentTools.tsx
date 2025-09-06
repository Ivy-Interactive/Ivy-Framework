import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { TooltipProvider } from '@/components/ui/tooltip';
import { useToast } from '@/hooks/use-toast';
import { Copy, Download, ChevronDown } from 'lucide-react';
import React from 'react';

interface DocumentToolsProps {
  articleRef: React.RefObject<HTMLElement | null>;
  documentSource?: string;
  title?: string;
}

export const DocumentTools: React.FC<DocumentToolsProps> = ({
  articleRef,
  documentSource,
  title = 'document',
}) => {
  const { toast } = useToast();

  // Function to load all tabs by clicking through them
  const loadAllTabs = async (): Promise<void> => {
    if (!articleRef.current) return;

    // Find all tab elements with aria-role="tab"
    const tabs = articleRef.current.querySelectorAll('[role="tab"]');

    if (tabs.length === 0) return;

    // Click through each tab and wait for content to load
    for (const tab of Array.from(tabs)) {
      try {
        // Check if tab is not already selected
        const isSelected = tab.getAttribute('aria-selected') === 'true';

        if (!isSelected && tab instanceof HTMLElement) {
          // Click the tab
          tab.click();

          // Wait a bit for content to load
          await new Promise(resolve => setTimeout(resolve, 100));
        }
      } catch (error) {
        console.warn('Failed to click tab:', error);
      }
    }

    // Wait a bit more for all content to be fully rendered
    await new Promise(resolve => setTimeout(resolve, 200));
  };

  // Function to wait for actual expansion completion
  const waitForExpansion = async (
    element: Element,
    timeout = 3000
  ): Promise<boolean> => {
    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
      // Check multiple expansion indicators
      const isExpanded =
        element.getAttribute('data-state') === 'open' ||
        element.getAttribute('aria-expanded') === 'true' ||
        // Also check if CollapsibleContent is visible
        element.querySelector('[data-state="open"]') !== null;

      if (isExpanded) {
        // Additional verification: ensure content is actually rendered
        const content = element.querySelector(
          '[class*="CollapsibleContent"], [data-state="open"]'
        );
        if (content && (content as HTMLElement).offsetHeight > 0) {
          // Wait a tiny bit more for animations to complete
          await new Promise(resolve => setTimeout(resolve, 50));
          return true;
        }
      }

      // Short polling interval - check every 50ms
      await new Promise(resolve => setTimeout(resolve, 50));
    }

    return false;
  };

  // Function to expand all details elements
  const expandDetailsElements = async (): Promise<void> => {
    if (!articleRef.current) return;

    const detailsElements =
      articleRef.current.querySelectorAll('[role="details"]');
    if (detailsElements.length === 0) return;

    for (const element of Array.from(detailsElements)) {
      try {
        // Check if element is closed
        const isClosed =
          element.getAttribute('data-state') === 'closed' ||
          element.getAttribute('aria-expanded') === 'false';

        if (isClosed) {
          // Dispatch expand event
          const expandEvent = new CustomEvent('expand', {
            bubbles: true,
            cancelable: true,
            detail: { element },
          });
          element.dispatchEvent(expandEvent);

          // If not cancelled, try to click trigger
          if (!expandEvent.defaultPrevented) {
            const trigger =
              element.querySelector(
                '[role="button"], button, .trigger, summary'
              ) || element;
            if (trigger instanceof HTMLElement) {
              trigger.click();

              const expanded = await waitForExpansion(element, 3000);
              if (!expanded) {
                console.warn(
                  'Failed to expand details element after 3s:',
                  element
                );
              }
            }
          }
        }
      } catch (error) {
        console.warn('Failed to expand details element:', error);
      }
    }
  };

  const copyTextContent = async () => {
    try {
      // Show loading state
      toast({
        title: 'Loading Content...',
        description: 'Expanding sections and loading all tabs...',
      });

      // First, expand all data-state="closed" elements
      await expandDetailsElements();

      // Then, click through all unloaded tabs to ensure content is loaded
      await loadAllTabs();

      // Extract API sections from the rendered page
      const apiContent = extractApiSections();

      if (!apiContent.trim()) {
        toast({
          title: 'Copy Failed',
          description: 'No API content found to copy',
          variant: 'destructive',
        });
        return;
      }

      await navigator.clipboard.writeText(apiContent);
      toast({
        title: 'Copied!',
        description: 'API sections copied to clipboard',
      });
    } catch (error) {
      try {
        // Fallback: try to copy text content from the page
        if (articleRef.current) {
          const textContent = articleRef.current.textContent || '';
          if (textContent.trim()) {
            await navigator.clipboard.writeText(textContent);
            toast({
              title: 'Copied!',
              description: 'Document text copied to clipboard (fallback)',
            });
            return;
          }
        }

        toast({
          title: 'Copy Failed',
          description:
            error instanceof Error ? error.message : 'Failed to copy content',
          variant: 'destructive',
        });
      } catch {
        toast({
          title: 'Copy Failed',
          description: 'Failed to copy text to clipboard',
          variant: 'destructive',
        });
      }
    }
  };

  // Extract API sections from the rendered page
  const extractApiSections = (): string => {
    if (!articleRef.current) return '';

    const articleElement = articleRef.current;
    let apiContent = '';

    // Look for API-related sections
    const apiSelectors = [
      'h1, h2, h3, h4, h5, h6', // Headings
      'p', // Paragraphs
      'ul, ol', // Lists
      'table', // Tables
      'pre, code', // Code blocks
      'div[class*="api"]', // Divs with "api" in class
      'section[class*="api"]', // Sections with "api" in class
      '[data-testid*="api"]', // Elements with "api" in test ID
      '[role="terminal"]', // Terminal elements
      '[role="details"]', // Details elements
    ];

    // Find all elements that might contain API content
    const apiElements = articleElement.querySelectorAll(
      apiSelectors.join(', ')
    );

    // Process elements in document order
    const processedElements = new Set<Element>();

    apiElements.forEach(element => {
      if (processedElements.has(element)) return;

      const tagName = element.tagName.toLowerCase();
      const text = element.textContent?.trim() || '';

      // Skip empty elements
      if (!text && !['table', 'ul', 'ol'].includes(tagName)) return;

      // Skip elements that are inside already processed containers
      if (isInsideProcessedElement(element, processedElements)) return;

      // Check if this element is part of an API section
      if (isApiSection(element)) {
        // Handle expandable details - extract header first
        if (element.getAttribute('role') === 'details') {
          const summaryElement = element.querySelector('[role="summary"]');
          if (summaryElement && !processedElements.has(summaryElement)) {
            const summaryText = summaryElement.textContent?.trim();
            if (summaryText) {
              apiContent += `### ${summaryText}\n\n`;
              processedElements.add(summaryElement);
            }
          }
          // The content will be processed by other selectors
          return;
        }

        switch (tagName) {
          case 'h1':
          case 'h2':
          case 'h3':
          case 'h4':
          case 'h5':
          case 'h6': {
            const level = parseInt(tagName[1]);
            apiContent += `${'#'.repeat(level)} ${text}\n\n`;
            processedElements.add(element);
            break;
          }

          case 'p':
            if (text && !element.closest('li, td, th')) {
              apiContent += `${text}\n\n`;
              processedElements.add(element);
            }
            break;

          case 'ul':
          case 'ol':
            if (!processedElements.has(element)) {
              apiContent += extractListToMarkdown(element);
              processedElements.add(element);
            }
            break;

          case 'table':
            if (!processedElements.has(element)) {
              const { tableMarkdown, usedCodeBlocks } =
                extractTableWithAssociatedContent(
                  element as HTMLTableElement,
                  articleElement
                );
              apiContent += tableMarkdown;
              processedElements.add(element);
              // Mark all child elements as processed to avoid duplication
              markChildrenAsProcessed(element, processedElements);
              // Mark used code blocks as processed
              usedCodeBlocks.forEach(codeBlock =>
                processedElements.add(codeBlock)
              );
            }
            break;

          case 'pre':
            if (!processedElements.has(element)) {
              const codeElement = element.querySelector('code');
              const codeText = codeElement
                ? codeElement.textContent
                : element.textContent;
              apiContent += `\`\`\`\n${codeText || ''}\n\`\`\`\n\n`;
              processedElements.add(element);
              // Mark child code elements as processed
              if (codeElement) processedElements.add(codeElement);
            }
            break;

          case 'div':
            // Handle terminal elements
            if (
              element.getAttribute('role') === 'terminal' &&
              !processedElements.has(element)
            ) {
              const terminalContent = extractTerminalContent(element);
              if (terminalContent) {
                apiContent += `\`\`\`terminal\n${terminalContent}\n\`\`\`\n\n`;
                processedElements.add(element);
                // Mark all child elements as processed to avoid duplication
                markChildrenAsProcessed(element, processedElements);
              }
            }
            break;

          case 'code':
            // Only process inline code (not inside pre, table, or other containers)
            if (
              !element.closest('pre, table, ul, ol') &&
              !processedElements.has(element)
            ) {
              // Skip standalone code elements to avoid duplication
            }
            break;
        }
      }
    });

    return apiContent.trim();
  };

  // Helper function to check if an element is inside an already processed container
  const isInsideProcessedElement = (
    element: Element,
    processedElements: Set<Element>
  ): boolean => {
    let parent = element.parentElement;
    while (parent) {
      if (processedElements.has(parent)) {
        return true;
      }
      parent = parent.parentElement;
    }
    return false;
  };

  // Helper function to mark all child elements as processed
  const markChildrenAsProcessed = (
    container: Element,
    processedElements: Set<Element>
  ): void => {
    const allChildren = container.querySelectorAll('*');
    allChildren.forEach(child => processedElements.add(child));
  };

  // Check if an element is part of an API section
  const isApiSection = (element: Element): boolean => {
    // Check if the element or its ancestors contain API-related content
    const apiKeywords = [
      'constructor',
      'supported types',
      'properties',
      'events',
      'api',
      'interface',
      'class',
      'method',
      'parameter',
      'return',
      'example',
      'usage',
    ];

    // Check the element itself
    const elementText = element.textContent?.toLowerCase() || '';
    if (apiKeywords.some(keyword => elementText.includes(keyword))) {
      return true;
    }

    // Check parent elements
    let parent = element.parentElement;
    while (parent) {
      const parentText = parent.textContent?.toLowerCase() || '';
      if (apiKeywords.some(keyword => parentText.includes(keyword))) {
        return true;
      }
      parent = parent.parentElement;
    }

    // Check for specific API section indicators
    const apiIndicators = [
      '[class*="api"]',
      '[class*="constructor"]',
      '[class*="properties"]',
      '[class*="events"]',
      '[data-testid*="api"]',
      'h1, h2, h3, h4, h5, h6', // Headings often indicate section boundaries
    ];

    return apiIndicators.some(selector => {
      try {
        return element.matches(selector) || element.closest(selector);
      } catch {
        return false;
      }
    });
  };

  // Enhanced function to extract table with associated content (like code blocks that follow)
  const extractTableWithAssociatedContent = (
    table: HTMLTableElement,
    articleElement: HTMLElement
  ): { tableMarkdown: string; usedCodeBlocks: Element[] } => {
    let tableMarkdown = '\n';
    const rows = Array.from(table.querySelectorAll('tr'));
    const usedCodeBlocks: Element[] = [];

    if (rows.length === 0) return { tableMarkdown: '', usedCodeBlocks: [] };

    // First, extract the basic table structure
    const tableData: string[][] = [];

    rows.forEach(row => {
      const cells = Array.from(row.querySelectorAll('td, th'));
      const cellTexts = cells.map(cell => {
        let cellText = cell.textContent || '';
        // Basic cleaning
        cellText = cellText.replace(/\{[^}]*\}/g, '');
        cellText = cellText.replace(/\[data-[^\]]*\]/g, '');
        cellText = cellText.replace(/[a-zA-Z-]+\s*\{[^}]*\}/g, '');
        cellText = cellText.replace(/::-webkit-scrollbar[^;]*;?/g, '');
        cellText = cellText.replace(/[a-zA-Z-]+\s*:\s*[^;]*;?/g, '');
        cellText = cellText.trim().replace(/\s+/g, ' ');
        return cellText.replace(/\|/g, '\\|');
      });
      tableData.push(cellTexts);
    });

    // Look for code blocks that might contain the missing table data
    const codeBlocks = Array.from(
      articleElement.querySelectorAll('pre code, code')
    );
    const codeValues: { text: string; element: Element }[] = [];

    // Find code blocks that appear after this table
    const tablePosition = Array.from(
      articleElement.querySelectorAll('*')
    ).indexOf(table);

    codeBlocks.forEach(codeBlock => {
      const codePosition = Array.from(
        articleElement.querySelectorAll('*')
      ).indexOf(codeBlock);
      if (codePosition > tablePosition) {
        const codeText = codeBlock.textContent?.trim();
        if (codeText && codeText.length > 0 && codeText.length < 200) {
          // Reasonable length for table values
          codeValues.push({ text: codeText, element: codeBlock });
        }
      }
    });

    // Try to populate empty cells with code values
    if (codeValues.length > 0 && tableData.length > 1) {
      let codeIndex = 0;

      // Skip header row, start from data rows
      for (
        let rowIndex = 1;
        rowIndex < tableData.length && codeIndex < codeValues.length;
        rowIndex++
      ) {
        const row = tableData[rowIndex];

        // For each empty cell in "Default Value" and "Setters" columns (typically columns 2 and 3)
        for (
          let colIndex = 2;
          colIndex < row.length && codeIndex < codeValues.length;
          colIndex++
        ) {
          if (!row[colIndex] || row[colIndex].trim() === '') {
            row[colIndex] = codeValues[codeIndex].text;
            usedCodeBlocks.push(codeValues[codeIndex].element);
            codeIndex++;
          }
        }
      }
    }

    // Generate the markdown table
    tableData.forEach((row, rowIndex) => {
      if (row.some(text => text.length > 0)) {
        tableMarkdown += `| ${row.join(' | ')} |\n`;

        // Add separator row after header
        if (rowIndex === 0 && rows[0]?.querySelector('th')) {
          tableMarkdown += `| ${row.map(() => '---').join(' | ')} |\n`;
        }
      }
    });

    return { tableMarkdown: tableMarkdown + '\n', usedCodeBlocks };
  };

  // Helper function to extract lists as markdown
  const extractListToMarkdown = (list: Element): string => {
    const items = Array.from(list.querySelectorAll(':scope > li')); // Direct children only
    const isOrdered = list.tagName.toLowerCase() === 'ol';
    let listMarkdown = '\n';

    items.forEach((item, index) => {
      const text = item.textContent?.trim() || '';
      if (text) {
        const prefix = isOrdered ? `${index + 1}. ` : '- ';
        listMarkdown += `${prefix}${text}\n`;
      }
    });

    return listMarkdown + '\n';
  };

  // Helper function to extract terminal content using proper ARIA roles
  const extractTerminalContent = (terminalElement: Element): string => {
    let terminalContent = '';

    // Look for terminal lines using the role="log" attribute
    const terminalLines = Array.from(
      terminalElement.querySelectorAll('[role="log"]')
    );

    if (terminalLines.length > 0) {
      // Process each terminal line
      terminalLines.forEach((line, index) => {
        if (index > 0) terminalContent += '\n';

        // Check if this is a command line using the aria-label
        const ariaLabel = line.getAttribute('aria-label');
        const isCommand = ariaLabel === 'Command';

        // Look for the content element using role
        const contentElement = line.querySelector('[role="terminal-text"]');

        if (contentElement) {
          const contentText = contentElement.textContent?.trim() || '';

          if (isCommand) {
            // This is a command line - add the prompt
            terminalContent += `> ${contentText}`;
          } else {
            // This is output
            terminalContent += contentText;
          }
        } else {
          // Fallback: get text from the entire line
          const fallbackText = line.textContent?.trim() || '';
          if (fallbackText) {
            if (isCommand) {
              terminalContent += `> ${fallbackText}`;
            } else {
              terminalContent += fallbackText;
            }
          }
        }
      });
    } else {
      // Fallback: get all text content from the terminal
      terminalContent = terminalElement.textContent?.trim() || '';
    }

    return terminalContent;
  };

  const generateFileName = (): string => {
    if (title && title !== 'document') {
      return title
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }

    if (articleRef.current) {
      const firstHeading = articleRef.current.querySelector('h1');
      if (firstHeading?.textContent) {
        return firstHeading.textContent
          .toLowerCase()
          .replace(/\s+/g, '-')
          .replace(/[^a-z0-9-]/g, '')
          .substring(0, 50);
      }
    }

    if (documentSource) {
      try {
        const url = new URL(documentSource);
        const pathParts = url.pathname.split('/');
        const fileName = pathParts[pathParts.length - 1];

        if (fileName && fileName.includes('.')) {
          return fileName
            .split('.')[0]
            .toLowerCase()
            .replace(/[^a-z0-9-]/g, '-');
        }
      } catch {
        // Ignore URL parsing errors
      }
    }

    const timestamp = new Date().toISOString().split('T')[0];
    return `ivy-document-${timestamp}`;
  };

  const saveAsMarkdown = async () => {
    try {
      // Show loading state
      toast({
        title: 'Preparing Download...',
        description: 'Expanding sections and loading all tabs...',
      });

      // First, expand all data-state="closed" elements
      await expandDetailsElements();

      // Then, click through all unloaded tabs to ensure content is loaded
      await loadAllTabs();

      // Extract the same content that gets copied
      const apiContent = extractApiSections();

      if (!apiContent.trim()) {
        toast({
          title: 'Download Failed',
          description: 'No API content found to download',
          variant: 'destructive',
        });
        return;
      }

      // Create and download the file
      const blob = new Blob([apiContent], { type: 'text/markdown' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${generateFileName()}.md`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      toast({
        title: 'Downloaded!',
        description: 'API sections saved as markdown file',
      });
    } catch (error) {
      console.error('Error downloading markdown:', error);
      toast({
        title: 'Download Failed',
        description:
          error instanceof Error
            ? error.message
            : 'Failed to download markdown',
        variant: 'destructive',
      });
    }
  };

  return (
    <TooltipProvider>
      <div className="flex">
        <Button
          variant="ghost"
          size="sm"
          onClick={copyTextContent}
          className="h-8 px-2 flex items-center gap-1 rounded-r-none border-r border-border/50"
        >
          <Copy className="w-4 h-4" />
          <span className="text-xs">Copy Page</span>
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              size="sm"
              className="h-8 px-1 flex items-center gap-1 rounded-l-none"
            >
              <ChevronDown className="w-4 h-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-48">
            <DropdownMenuItem onClick={saveAsMarkdown}>
              <Download className="w-4 h-4 mr-2" />
              Download as Markdown
            </DropdownMenuItem>
            {/* <DropdownMenuSeparator />
            <DropdownMenuItem disabled>
              <ExternalLink className="w-4 h-4 mr-2" />
              Open in ChatGPT
            </DropdownMenuItem>
            <DropdownMenuItem disabled>
              <ExternalLink className="w-4 h-4 mr-2" />
              Open in Claude
            </DropdownMenuItem> */}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </TooltipProvider>
  );
};
