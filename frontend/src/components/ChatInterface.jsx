import React, { useState, useEffect } from 'react';
import { MessageSquare, User, FileText, ChevronDown } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import remarkBreaks from 'remark-breaks';
import remarkGfm from 'remark-gfm';
import { reportGeneration } from '../helpers';


const ChatInterface = ({ company }) => {
  const { company_name } = company;
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isLoading) {
      const element = document.querySelector('.dot-flashing');
      if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }, [isLoading]);

  const quickQueries = [
    'Generate an executive summary',
    'Summarize executive & board changes',
    `Summarize ${company_name} activity (3 years)`,
    'Confirm ASN status',
    'Summarize financials',
    'Summarize corporate timeline',
  ];

  const handleSend = async () => {
    if (!inputValue.trim()) return;
    setIsLoading(true);

    try {
      const prompt = `company name: ${company_name}\n${inputValue}`;
      const response = await reportGeneration(prompt);

      // User message
      const userMessage = {
        id: messages.length + 1,
        type: 'user',
        content: inputValue,
      };

      // AI response
      const aiResponse = {
        id: messages.length + 2,
        type: 'ai',
        content: response, // The Markdown content
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

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
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
                    {/* Message content wrapper */}
                    <div
                      className={`max-w-[75%] flex items-start gap-3 ${
                        message.type === 'user' ? 'flex-row-reverse' : 'flex-row'
                      }`}
                    >
                      {/* Avatar - Show on left for AI, right for user */}
                      <div className="flex-shrink-0 w-7 h-7 rounded flex items-center justify-center bg-backgroundSurface border border-borderDefault">
                        {message.type === 'user' ? (
                          <User className="w-4 h-4 text-primary" />
                        ) : (
                          <MessageSquare className="w-4 h-4 text-primary" />
                        )}
                      </div>

                      {/* Message content */}
                      <div
                        className={`min-w-0 ${
                          message.type === 'user' ? 'items-end' : 'items-start'
                        }`}
                      >
                        <div
                          className={`prose prose-invert max-w-none
                            prose-headings:font-bold
                            prose-p:leading-relaxed
                            ${
                              message.type === 'user'
                                ? 'bg-primary text-white px-4 py-3 rounded-2xl'
                                : 'bg-backgroundSurface px-4 py-3 rounded-2xl border border-borderDefault'
                            }`}
                        >
                          {message.type === 'ai' ? (
                            <ReactMarkdown
                              remarkPlugins={[remarkBreaks, remarkGfm]}
                              className="prose-sm sm:prose-base whitespace-pre-wrap"
                            >
                              {message.content}
                            </ReactMarkdown>
                          ) : (
                            message.content
                          )}
                        </div>

                        {/* Citations */}
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

                        {/* Generate Report button (for AI messages) */}
                        {message.type === 'ai' && (
                          <button className="mt-4 inline-flex items-center space-x-2 px-3 py-2 bg-backgroundSurface hover:bg-buttonHover text-textDefault rounded-lg border border-borderDefault transition-colors text-sm">
                            <FileText className="w-4 h-4" />
                            <span>Generate Full Report</span>
                            <ChevronDown className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}

              {/* 3-dot "AI is typing" animation */}
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

        {/* Bottom section with examples and input */}
        <div className="mt-4 space-y-4">
          {/* Example questions */}
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

          {/* Input box */}
          <div className="flex items-center space-x-3 bg-backgroundSurface rounded-xl border border-borderDefault shadow-lg">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder="Ask about company executives, board changes, or request a full summary..."
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
