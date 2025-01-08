import React from 'react';
import { Link } from 'react-router-dom';

export default function Header({ appName }) {
  return (
    <header className="flex justify-between items-center p-4 bg-white shadow-md">
      <Link to="/" className="text-xl font-bold text-blue-500">
        {appName}
      </Link>
    </header>
  );
}
