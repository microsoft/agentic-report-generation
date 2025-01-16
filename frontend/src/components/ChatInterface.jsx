import React, { useState, useEffect } from 'react';
import { MessageSquare, User, FileText, ChevronDown } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import remarkBreaks from 'remark-breaks';
import remarkGfm from 'remark-gfm';

// For converting Markdown to HTML:
import { unified } from 'unified';
import parse from 'remark-parse';
import remark2rehype from 'remark-rehype';
import rehypeStringify from 'rehype-stringify';

// For PDF generation:
import { jsPDF } from 'jspdf';

// Your own helper that talks to the AI model
import { reportGeneration } from '../helpers';

const ChatInterface = ({ company }) => {
  const { company_name, companyId } = company;
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // If AI is loading, scroll the 3-dot animation into view
  useEffect(() => {
    if (isLoading) {
      const element = document.querySelector('.dot-flashing');
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'end' });
      }
    }
  }, [isLoading]);

  // Keep messages scrolled to bottom when new messages arrive
  useEffect(() => {
    if (messages.length > 0 && !isLoading) {
      const messagesContainer = document.querySelector('.overflow-y-auto');
      if (messagesContainer) {
        const lastMessage = messagesContainer.lastElementChild;
        if (lastMessage) {
          lastMessage.scrollIntoView({ behavior: 'smooth', block: 'end' });
        }
      }
    }
  }, [messages, isLoading]);

  // Sample "Quick Queries"
  const quickQueries = [
    'Generate an executive summary',
    'Summarize executive & board changes',
    `Summarize ${company_name} activity (3 years)`,
    'Confirm ASN status',
    'Summarize financials',
    'Summarize corporate timeline',
  ];

  /**
   * Send user input to AI
   */
  const handleSend = async () => {
    if (!inputValue.trim()) return;
    setIsLoading(true);

    try {
      const prompt = `company id: ${companyId}\n${inputValue}`;
      const response = await reportGeneration(prompt);

      // 1) User message
      const userMessage = {
        id: messages.length + 1,
        type: 'user',
        content: inputValue,
      };

      // 2) AI message (Markdown)
      const aiResponse = {
        id: messages.length + 2,
        type: 'ai',
        content: response, 
      };

      setMessages((prev) => [...prev, userMessage, aiResponse]);
      setInputValue('');
    } catch (error) {
      console.error('Error fetching response:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickQuery = (query) => {
    setInputValue(query);
  };

  // Send on Enter (unless Shift+Enter)
  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  /**
   * Generate a text-based PDF from the AI’s Markdown
   * with custom inline styles for better bullet/heading alignment.
   */
  const generatePDF = async (markdown) => {
    try {
      // 1) Convert the Markdown to plain HTML
      const processed = await unified()
        .use(parse)
        .use(remarkGfm)
        .use(remark2rehype)
        .use(rehypeStringify)
        .process(markdown);
      
        console.log('processed', processed);

      const rawHtml = processed.value; // The final HTML string

      // 2) Wrap the HTML with our own style for spacing, bullet points, etc.
      // Adjust these CSS rules as needed.
      const styledHtml = `
        <!DOCTYPE html>
        <html>
          <head>
            <meta charset="utf-8"/>
            <style>
              body {
                font-family: "Helvetica", sans-serif;
                font-size: 12px;
                margin: 20px;
                color: #111111;
              }
              h1, h2, h3, h4 {
                font-weight: bold;
                margin-bottom: 0.4em;
                margin-top: 0.8em;
              }
              p {
                margin: 0.5em 0;
                line-height: 1.4;
              }
              ul, ol {
                margin: 0.5em 0 0.5em 1.4em;
                padding: 0;
              }
              li {
                margin: 0.25em 0;
                line-height: 1.3;
              }
              strong {
                font-weight: bold;
              }
              em {
                font-style: italic;
              }
            </style>
          </head>
          <body>
            <div class="prose max-w-none prose-ul:my-1 prose-li:my-1 prose-li:leading-tight prose-p:my-2 prose-headings:my-2">
              ${rawHtml}
            </div>
          </body>
        </html>
      `;

      // 3) Create jsPDF instance
      const doc = new jsPDF({
        putOnlyUsedFonts: true,
        orientation: 'p',
        unit: 'pt',
        format: 'a4',
      });

      console.log('styledHtml', String(styledHtml));
      // 4) Convert the HTML to PDF
      await doc.html(String(styledHtml), {
        x: 30,
        y: 30,
        width: 550,   // A4 is 595pt wide, minus margins
        windowWidth: 1000, 
      });

      // 5) Download
      doc.save('report.pdf');
    } catch (err) {
      console.error('Error generating PDF:', err);
    }
  };

  return (
    <div className="h-screen flex flex-col overflow-hidden text-textDefault">
      {/* Main chat area */}
      <div className="flex-1 overflow-hidden flex flex-col max-w-5xl w-full mx-auto px-4 py-4">
        {messages.length === 0 ? (
          <div className="flex-1 flex flex-col items-center justify-center">
            <MessageSquare className="w-6 h-6 stroke-[1.5] mb-3 text-primary" />
            <h1 className="text-2xl font-medium mb-2">What can I help with?</h1>
            <p className="text-textLight">
              Ask about company insights or generate reports
            </p>
            {isLoading && (
              <div className="py-4 flex justify-start">
                <div className="max-w-[75%] flex items-start gap-3">
                  <div className="bg-backgroundSurface px-4 py-3 rounded-2xl border border-borderDefault">
                    <div className="dot-flashing"></div>
                  </div>
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="flex-1 overflow-y-auto rounded-2xl bg-backgroundMessage p-4">
            <div className="max-w-3xl mx-auto">
              {messages.map((message) => (
                <div key={message.id} className="py-4">
                  <div
                    className={`flex ${
                      message.type === 'user' ? 'justify-end' : 'justify-start'
                    }`}
                  >
                    {/* Message wrapper */}
                    <div
                      className={`flex items-start gap-3 ${
                        message.type === 'user' ? 'max-w-[75%] flex-row-reverse' : 'w-full flex-row'
                      }`}
                    >
                      {/* Avatar */}
                      <div className="flex-shrink-0 w-7 h-7 rounded flex items-center justify-center bg-backgroundSurface border border-borderDefault">
                        {message.type === 'user' ? (
                          <User className="w-4 h-4 text-primary" />
                        ) : (
                          <MessageSquare className="w-4 h-4 text-primary" />
                        )}
                      </div>

                      {/* Actual message content */}
                      <div className={`min-w-0 ${message.type === 'user' ? 'items-end' : 'items-start'}`}>
                        <div
                          className={
                            message.type === 'user'
                              ? 'bg-primary text-white px-4 py-3 rounded-2xl'
                              : 'bg-backgroundSurface px-4 py-3 rounded-2xl border border-borderDefault'
                          }
                        >
                          {message.type === 'ai' ? (
                            <ReactMarkdown
                              remarkPlugins={[remarkBreaks, remarkGfm]}
                              className="
                                prose
                                max-w-none
                                prose-ul:my-1
                                prose-li:my-1
                                prose-li:leading-tight
                                prose-p:my-2
                                prose-headings:my-2
                                prose-table:my-2
                                prose-table:w-full
                                prose-table:overflow-x-auto
                                prose-table:block
                                prose-td:p-2
                                prose-td:border
                                prose-td:border-borderDefault
                                prose-th:p-2
                                prose-th:border
                                prose-th:border-borderDefault
                                prose-th:bg-backgroundSurface
                              "
                            >
                              {message.content}
                            </ReactMarkdown>
                          ) : (
                            <span>{message.content}</span>
                          )}
                        </div>

                        {/* Citations (if any) */}
                        {message.citations && (
                          <div className="mt-4 w-full text-sm bg-backgroundSurface rounded-lg overflow-hidden border border-borderDefault">
                            <div className="px-4 py-2 border-b border-borderDefault">
                              <p className="font-medium text-sm text-textDefault">Sources</p>
                            </div>
                            <div className="p-4 space-y-2">
                              {message.citations.map((citation, index) => (
                                <div
                                  key={index}
                                  className="flex justify-between items-center text-textLight"
                                >
                                  <span>{citation.source}</span>
                                  <span className="text-xs">{citation.date}</span>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}

                        {/* "Generate PDF" button for AI messages */}
                        {message.type === 'ai' && (
                          <button
                            className="mt-4 inline-flex items-center space-x-2 px-3 py-2 bg-backgroundSurface hover:bg-buttonHover text-textDefault rounded-lg border border-borderDefault transition-colors text-sm"
                            onClick={() => generatePDF(message.content)}
                          >
                            <FileText className="w-4 h-4" />
                            <span>Generate PDF</span>
                            <ChevronDown className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}

              {/* AI is typing animation */}
              {isLoading && (
                <div className="py-4 flex justify-start">
                  <div className="max-w-[75%] flex items-start gap-3">
                    <div className="flex-shrink-0 w-7 h-7 rounded flex items-center justify-center bg-backgroundSurface border border-borderDefault">
                      <MessageSquare className="w-4 h-4 text-primary" />
                    </div>
                    <div className="bg-backgroundSurface px-4 py-3 rounded-2xl border border-borderDefault">
                      <div className="dot-flashing"></div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Bottom section: quick queries + input */}
        <div className="mt-4 space-y-4">
          <div className="flex flex-wrap gap-2 justify-center">
            {quickQueries.map((query, index) => (
              <button
                key={index}
                onClick={() => handleQuickQuery(query)}
                className="px-4 py-2 text-sm bg-backgroundSurface hover:bg-buttonHover text-textLight rounded-lg border border-borderDefault transition-colors"
              >
                {query}
              </button>
            ))}
          </div>

          <div className="flex items-center space-x-3 bg-backgroundSurface rounded-xl border border-borderDefault shadow-lg">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Ask about company executives, board changes, or requesting a full summary..."
              className="flex-1 px-4 py-3.5 bg-transparent focus:outline-none text-textDefault placeholder-textLight"
              disabled={isLoading}
            />
            <button
              onClick={handleSend}
              className="px-4 py-2 mx-2 bg-primary hover:bg-secondary text-white text-sm rounded-lg transition-colors"
              disabled={isLoading}
            >
              {isLoading ? 'Loading...' : 'Send'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatInterface;
