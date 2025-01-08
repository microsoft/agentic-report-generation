import React from 'react';

export default function SearchBar({ searchTerm, setSearchTerm }) {
  return (
    <div className="flex items-center w-full md:w-1/2 bg-white p-2 rounded shadow-md">
      <input
        type="text"
        className="flex-grow outline-none px-2 text-gray-700"
        placeholder="Search companies..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
      />
    </div>
  );
}
