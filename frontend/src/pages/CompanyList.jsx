// src/pages/CompanyList.jsx
import React, { useState, useEffect } from 'react';
import Header from '../components/Header';
import SearchBar from '../components/SearchBar';
import CompanyCard from '../components/CompanyCard';
import {getCompanies} from '../helpers';

export default function CompanyList() {
  const [searchTerm, setSearchTerm] = useState('');

  const [companiesData, setCompaniesData] = useState([]);

  useEffect(() => {
    const fetchCompanies = async () => {
      const data = await getCompanies();
      setCompaniesData(data);
      console.log(data);
    };

    fetchCompanies();
  }, []); // Empty dependency array ensures this runs once on mount

  const filteredCompanies = companiesData.filter((company) =>
    company.company_name && company.company_name.toLowerCase().includes(searchTerm.toLowerCase())
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
