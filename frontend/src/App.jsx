import React from 'react';
import { Routes, Route } from 'react-router-dom';
import CompanyList from './pages/CompanyList';
import CompanyDetail from './pages/CompanyDetail';

function App() {
  return (
    <Routes>
      <Route path="/" element={<CompanyList />} />
      <Route path="/detail/:companyId" element={<CompanyDetail />} />
    </Routes>
  );
}

export default App;
