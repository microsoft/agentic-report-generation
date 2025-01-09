import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import axios from 'axios';
import companiesData from '../data/companiesData';
import ChatInterface from '../components/ChatInterface';

export default function CompanyDetail() {
  const { companyId } = useParams();
  const [company, setCompany] = useState(null);
  const [news, setNews] = useState([]);
  const [chatOpen, setChatOpen] = useState(true);
  const [chatMessages, setChatMessages] = useState([]);
  const [userInput, setUserInput] = useState('');

  useEffect(() => {
    // Fetch the company data from local or API
    const currentCompany = companiesData.find((c) => c.id === companyId);
    setCompany(currentCompany);

    // Fetch news articles for the given company
    // Replace this with your own API endpoint or a real news API
    async function fetchNews() {
      try {
        // Example placeholder API call
        // In a real app, you might call something like:
        // `https://newsapi.org/v2/everything?q=${company.name}&apiKey=XYZ`
        const response = await axios.get('/api/company-news', {
          params: { company: currentCompany.name },
        });
        // Example response data
        // setNews(response.data.articles);
        setNews([
          {
            title: 'Sample Headline 1',
            thumbnail: 'https://via.placeholder.com/150',
            date: '2025-01-01',
            source: 'Tech Times',
            snippet: 'This is a sample snippet about the company news.',
            url: '#'
          },
          {
            title: 'Sample Headline 2',
            thumbnail: 'https://via.placeholder.com/150',
            date: '2025-01-02',
            source: 'Biz Journal',
            snippet: 'Another news snippet about the company...',
            url: '#'
          }
        ]);
      } catch (error) {
        console.error(error);
      }
    }

    if (currentCompany) {
      fetchNews();
    }
  }, [companyId]);

  // Chatbot logic (Mock example)
  async function handleSendMessage() {
    if (!userInput.trim()) return;
    const userMessage = { role: 'user', content: userInput.trim() };

    // Add user message to chat
    setChatMessages((prev) => [...prev, userMessage]);
    setUserInput('');

    try {
      // Example AI API call
      const response = await axios.post('/api/chat', {
        messages: [...chatMessages, userMessage],
        company: company?.name,
      });
      // Suppose the AI response returns in `response.data.answer`
      const botMessage = { role: 'assistant', content: response.data.answer };
      setChatMessages((prev) => [...prev, botMessage]);
    } catch (error) {
      console.error(error);
    }
  }

  if (!company) {
    return (
      <div className="p-8 text-center">
        <h2 className="text-xl font-semibold">Company not found</h2>
        <Link to="/" className="text-blue-500 underline">
          Go to Home
        </Link>
      </div>
    );
  }

  return (
    <div className="min-h-screen p-4 flex flex-col md:flex-row">
      {/* Main Content */}
      <div className="flex-1 md:mr-4 mb-4">
        {/* Breadcrumb */}
        <div className="mb-4">
          <Link to="/" className="text-blue-500 underline">
            Home
          </Link>
          <span className="mx-2">{'>'}</span>
          <span className="text-gray-600">{company.name}</span>
        </div>

        {/* Company Header */}
        <div className="flex items-center mb-6">
          <img
            src={company.logo}
            alt={company.name}
            className="w-12 h-12 object-contain mr-4"
          />
          <h1 className="text-2xl font-bold">{company.name} Overview</h1>
        </div>

        {/* Latest News Section */}
        <div>
          <h2 className="text-xl font-semibold mb-4">Latest News</h2>
          <div className="space-y-4">
            {news.length === 0 ? (
              <p>No news available.</p>
            ) : (
              news.map((article, index) => (
                <div
                  key={index}
                  className="bg-white p-4 rounded shadow flex flex-col md:flex-row"
                >
                  <img
                    src={article.thumbnail}
                    alt={article.title}
                    className="w-32 h-32 object-cover mr-4 mb-4 md:mb-0"
                  />
                  <div>
                    <h3 className="text-lg font-bold">{article.title}</h3>
                    <p className="text-sm text-gray-500">
                      {article.date} | {article.source}
                    </p>
                    <p className="mt-2 text-gray-700">{article.snippet}</p>
                    <a
                      href={article.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="mt-2 inline-block text-blue-500 underline"
                    >
                      Read More
                    </a>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {/* Sidebar Chatbot */}
      <ChatInterface />
    </div>
  );
}
