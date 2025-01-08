// src/pages/CompanyDetail.jsx
import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import axios from 'axios';
import companiesData from '../data/companiesData';
import ChatInterface from '../components/ChatInterface';

export default function CompanyDetail() {
  const { companyId } = useParams();
  const [company, setCompany] = useState(null);
  const [news, setNews] = useState([]);

  // For resizing
  const [chatWidth, setChatWidth] = useState(400);
  const [isResizing, setIsResizing] = useState(false);

  useEffect(() => {
    const currentCompany = companiesData.find((c) => c.id === companyId);
    setCompany(currentCompany);

    async function fetchNews() {
      try {
        const response = await axios.get('/api/company-news', {
          params: { company: currentCompany.name },
        });
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

  /* ----------------- 
     Handle Resizing 
   ------------------ */
  const handleMouseDown = (e) => {
    e.preventDefault();
    setIsResizing(true);
  };
    setIsResizing(true);
  };

  useEffect(() => {
    const handleMouseMove = (e) => {
      if (!isResizing) return;
      const newWidth = window.innerWidth - e.clientX;
      if (newWidth < 200) {
        setChatWidth(200);
      } else if (newWidth > window.innerWidth * 0.9) {
        setChatWidth(window.innerWidth * 0.9);
      } else {
        setChatWidth(newWidth);
      }
    };

    const handleMouseUp = () => {
      setIsResizing(false);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isResizing]);

  if (!company) {
    return (
      <div className="p-8 text-center">
        <h2 className="text-xl font-semibold">Company not found</h2>
        <Link to="/" className="text-primary underline">
          Go to Home
        </Link>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col md:flex-row bg-surface">
      {/* Main Content */}
      <div className="flex-1 md:mr-4 mb-4 px-4 py-6">
        {/* Breadcrumb */}
        <div className="mb-2 text-sm">
          <Link to="/" className="text-primary underline">
            Home
          </Link>
          <span className="mx-2">{'>'}</span>
          <span className="text-textLight">{company.name}</span>
        </div>

        {/* Company Header */}
        <div className="flex items-center mb-6">
          <img
            src={company.logo}
            alt={company.name}
            className="w-12 h-12 object-contain mr-4"
          />
          <h1 className="text-2xl font-bold text-textDefault">{company.name} Overview</h1>
        </div>

        {/* Latest News Section */}
        <div>
          <h2 className="text-xl font-semibold mb-4 text-textDefault">Latest News</h2>
          <div className="space-y-4">
            {news.length === 0 ? (
              <p className="text-textLight">No news available.</p>
            ) : (
              news.map((article, index) => (
                <div
                  key={index}
                  className="bg-backgroundSurface p-4 rounded shadow flex flex-col md:flex-row border border-borderDefault"
                >
                  <img
                    src={article.thumbnail}
                    alt={article.title}
                    className="w-32 h-32 object-cover mr-4 mb-4 md:mb-0 rounded"
                  />
                  <div>
                    <h3 className="text-lg font-bold text-textDefault">{article.title}</h3>
                    <p className="text-sm text-textMuted">
                      {article.date} | {article.source}
                    </p>
                    <p className="mt-2 text-textDefault">{article.snippet}</p>
                    <a
                      href={article.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="mt-2 inline-block text-primary underline text-sm"
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

      {/* Sidebar Chat: Resizable */}
      <div
        className="relative bg-backgroundSurface shadow-md"
        style={{
          width: `${chatWidth}px`,
          minWidth: '200px'
        }}
      >
        {/* The ChatInterface itself */}
        <ChatInterface />

        {/* Resizing handle (drag this to resize) */}
        <div
          className="absolute top-0 left-0 w-2 h-full cursor-col-resize z-10"
          onMouseDown={handleMouseDown}
        ></div>
      </div>
    </div>
  );
}
