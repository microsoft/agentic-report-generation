// src/pages/CompanyDetail.jsx
import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import axios from 'axios';
import {getCompanies} from '../helpers';
import ChatInterface from '../components/ChatInterface';

export default function CompanyDetail() {
  const { companyId } = useParams();
  const [company, setCompany] = useState(null);
  const [news, setNews] = useState([]);
  const [companiesData, setCompaniesData] = useState([]);


  // For resizing
  const [chatWidth, setChatWidth] = useState(400);
  const [isResizing, setIsResizing] = useState(false);

  useEffect(() => {
    async function fetchCompanies() {
      const data = await getCompanies();
      setCompaniesData(data);
    }
    fetchCompanies();
  }, []); // Empty dependency array ensures this runs once on mount

  useEffect(() => {
    const currentCompany = companiesData.find((c) => c.id === companyId);
    setCompany(currentCompany);

    async function getNews() {
      try {
        const news = currentCompany.news_data;
        console.log('Fetching news for', currentCompany.news_data);
        setNews(news);
      } catch (error) {
        console.error(error);
      }
    }

    if (currentCompany) {
      getNews();
    }
  }, [companyId, companiesData]); // Run this effect when companyId or companiesData changes

  /* ----------------- 
     Handle Resizing 
   ------------------ */
  const handleMouseDown = (e) => {
    e.preventDefault();
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
          <span className="text-textLight">{company.company_name}</span>
        </div>

        {/* Company Header */}
        <div className="flex items-center mb-6">
          <img
            src={company.thumbnail}
            alt={company.company_name}
            className="w-12 h-12 object-contain mr-4"
          />
          <h1 className="text-2xl font-bold text-textDefault">{company.company_name} Overview</h1>
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
                    src={company.thumbnail || ""}
                    alt={article.headline}
                    className="w-32 h-32 object-cover mr-4 mb-4 md:mb-0 rounded"
                  />
                  <div>
                    <h3 className="text-lg font-bold text-textDefault">{article.headline}</h3>
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
        <ChatInterface company={company}/>

        {/* Resizing handle (drag this to resize) */}
        <div
          className="absolute top-0 left-0 w-2 h-full cursor-col-resize z-10"
          onMouseDown={handleMouseDown}
        ></div>
      </div>
    </div>
  );
}
