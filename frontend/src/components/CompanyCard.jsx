import React from 'react';
import { Link } from 'react-router-dom';

export default function CompanyCard({ company }) {
  return (
    <div className="bg-white shadow-lg rounded-lg p-4 flex flex-col items-center">
      <img
        src={company.logo}
        alt={company.name}
        className="w-16 h-16 object-contain mb-4"
      />
      <h2 className="text-lg font-bold mb-2">{company.name}</h2>
      <p className="text-gray-600 text-sm text-center line-clamp-2">
        {company.summary}
      </p>
      <Link
        to={`/detail/${company.id}`}
        className="mt-4 bg-blue-500 text-white py-2 px-6 rounded hover:bg-blue-600 transition-all"
      >
        View Details
      </Link>
    </div>
  );
}
