// src/pages/CompanyList.jsx
import React, { useState } from 'react';
import Header from '../components/Header';
import SearchBar from '../components/SearchBar';
import CompanyCard from '../components/CompanyCard';
import companiesData from '../data/companiesData';

export default function CompanyList() {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredCompanies = companiesData.filter((company) =>
    company.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="min-h-screen flex flex-col">
      <Header appName="Explore Companies" />

      <div className="mt-6 px-4">
        <SearchBar searchTerm={searchTerm} setSearchTerm={setSearchTerm} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 p-6">
        {filteredCompanies.map((company) => (
          <CompanyCard key={company.id} company={company} />
        ))}
      </div>
    </div>
  );
}
