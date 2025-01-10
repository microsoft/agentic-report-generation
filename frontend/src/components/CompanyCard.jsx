// src/components/CompanyCard.jsx
import React from 'react';
import { Link } from 'react-router-dom';

export default function CompanyCard({ company }) {
  return (
    <div className="bg-white border border-borderDefault rounded-lg p-4 flex flex-col items-center transition-transform hover:scale-[1.01] hover:shadow-md">
      <img
        src={company.thumbnail}
        alt={company.name}
        className="w-16 h-16 object-contain mb-4"
      />
      <h2 className="text-lg font-semibold mb-2 text-textDefault">{company.company_name}</h2>
      <p className="text-textLight text-sm text-center line-clamp-2">
        {company.company_description}
      </p>
      <Link
        to={`/detail/${company.id}`}
        className="mt-4 bg-primary text-white py-2 px-6 rounded hover:bg-secondary transition-all text-sm font-medium"
      >
        View Details
      </Link>
    </div>
  );
}
