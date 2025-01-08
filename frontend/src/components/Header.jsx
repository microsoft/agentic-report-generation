// src/components/Header.jsx
import React from 'react';
import { Link } from 'react-router-dom';

export default function Header({ appName }) {
  return (
    <header className="w-full bg-primary text-white py-3 px-6 flex items-center justify-between shadow-md">
      <Link to="/" className="text-xl font-bold tracking-wide">
        {appName}
      </Link>
      <nav className="hidden md:flex gap-4 items-center">
        <Link to="/" className="hover:underline">
          Home
        </Link>
        <Link to="/about" className="hover:underline">
          About
        </Link>
        {/* Add more links if needed */}
      </nav>
    </header>
  );
}
