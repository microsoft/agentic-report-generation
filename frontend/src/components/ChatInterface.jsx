import React, { useState } from 'react';
import { MessageSquare, User, FileText, ChevronDown } from 'lucide-react';

const ChatInterface = () => {
  const [messages, setMessages] = useState([]);
  const [inputValue, setInputValue] = useState('');

  const quickQueries = [
    'Generate an executive summary',
    'Summarize executive/board changes',
    'Summarize RRA activity (3 years)',
    'Confirm ASN status',
    'Summarize financials',
    'Summarize corporate timeline'
  ];

  const handleSend = () => {
    if (!inputValue.trim()) return;

    // Add user message
    const userMessage = {
      id: messages.length + 1,
      type: 'user',
      content: inputValue,
    };

    // Add AI response (sample data)
    const aiResponse = {
      id: messages.length + 2,
      type: 'ai',
      content: (
        <div className="space-y-4">
          <div>
            <h3 className="font-semibold mb-2">Overview</h3>
            <p className="mb-2">
              Tesla, Inc. is a leading electric vehicle and clean energy company known for its electric cars, battery energy storage, and solar products. The company aims to accelerate the world's transition to sustainable energy.
            </p>
            <div className="space-y-1">
              <p>• Company Type: Public (NASDAQ: TSLA)</p>
              <p>• Employees: 127,855 (2023)</p>
              <p>• Industry: Automotive, Energy</p>
              <p>• Index: S&P 500, NASDAQ-100</p>
              <p>• Headquarters: Austin, Texas, USA</p>
            </div>
          </div>
          <div>
            <h3 className="font-semibold mb-2">Recent Executive Changes</h3>
            <div className="space-y-1">
              <p>• CFO transition: Zachary Kirkhorn stepped down (Aug 2023)</p>
              <p>• Vaibhav Taneja appointed as CFO (Aug 2023)</p>
            </div>
          </div>
        </div>
      ),
      citations: [
        {
          source: 'Annual Report (10-K)',
          date: 'FY 2023',
          type: 'SEC Filing'
        },
        {
          source: 'Q3 2023 Earnings Call Transcript',
          date: 'October 18, 2023',
          type: 'Earnings Call'
        }
      ]
    };

    setMessages([...messages, userMessage, aiResponse]);
    setInputValue('');
  };

  const handleQuickQuery = (query) => {
    setInputValue(query);
  };

  React.useEffect(() => {
    if (inputValue) {
      handleSend();
    }
  }, [inputValue]);

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
            <p className="text-textLight">Ask about company insights or generate reports</p>
          </div>
        ) : (
          <div className="flex-1 overflow-y-auto rounded-2xl bg-backgroundMessage p-4">
            <div className="max-w-3xl mx-auto">
              {messages.map(message => (
                <div key={message.id} className="py-4">
                  <div className={`flex ${message.type === 'user' ? 'justify-end' : 'justify-start'}`}>
                    {/* Message content wrapper */}
                    <div className={`max-w-[75%] flex items-start gap-3 ${message.type === 'user' ? 'flex-row-reverse' : 'flex-row'}`}>
                      {/* Avatar - Show on left for AI, right for user */}
                      <div className="flex-shrink-0 w-7 h-7 rounded flex items-center justify-center bg-backgroundSurface border border-borderDefault">
                        {message.type === 'user' ? (
                          <User className="w-4 h-4 text-primary" />
                        ) : (
                          <MessageSquare className="w-4 h-4 text-primary" />
                        )}
                      </div>

                      {/* Message content */}
                      <div className={`min-w-0 ${message.type === 'user' ? 'items-end' : 'items-start'}`}>
                        <div className={`prose prose-invert max-w-none prose-p:leading-relaxed 
                          ${message.type === 'user' ? 'bg-primary text-white px-4 py-3 rounded-2xl' : 'bg-backgroundSurface px-4 py-3 rounded-2xl border border-borderDefault'}`}>
                          {message.content}
                        </div>

                        {/* Citations */}
                        {message.citations && (
                          <div className="mt-4 w-full text-sm bg-backgroundSurface rounded-lg overflow-hidden border border-borderDefault">
                            <div className="px-4 py-2 border-b border-borderDefault">
                              <p className="font-medium text-sm text-textDefault">Sources</p>
                            </div>
                            <div className="p-4 space-y-2">
                              {message.citations.map((citation, index) => (
                                <div key={index} className="flex justify-between items-center text-textLight">
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
            />
            <button 
              onClick={handleSend}
              className="px-4 py-2 mx-2 bg-primary hover:bg-secondary text-white text-sm rounded-lg transition-colors"
            >
              Send
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatInterface;
