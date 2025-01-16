# Frontend Implementation Documentation

## 1. Overview

The frontend application is a React-based system that provides an interactive interface for exploring company information and generating reports. It consists of two main views:

1. **Company List View**: A searchable grid of companies with basic information
2. **Company Detail View**: A detailed view of each company featuring:
   - Company overview and metadata
   - News feed
   - Interactive chat interface for generating reports and analyzing company data

The application employs a responsive design and integrates with a backend API for data retrieval and report generation.

## 2. Technical Stack

### Core Technologies
- **React (v18.3.1)**: Main UI framework
- **Vite (v6.0.5)**: Build tool and development server
- **React Router (v7.1.1)**: Client-side routing

### UI and Styling
- **Tailwind CSS (v3.4.17)**: Utility-first CSS framework
- **Lucide React (v0.469.0)**: Icon library
- **PostCSS (v8.4.49)**: CSS processing
- **Autoprefixer (v10.4.20)**: CSS compatibility

### Data Processing
- **Axios (v1.7.9)**: HTTP client
- **React Markdown (v9.0.3)**: Markdown rendering
- **MDX (v3.1.0)**: Enhanced Markdown support
- **Remark plugins**: 
  - remark-breaks
  - remark-gfm

### Development Tools
- **ESLint (v9.17.0)**: Code linting
- **Various ESLint plugins**: React-specific linting rules

## 3. Features and Implementation

### 3.1 Company Listing

#### Implementation Details
```javascript
// CompanyList.jsx
const [companiesData, setCompaniesData] = useState([]);

useEffect(() => {
  const fetchCompanies = async () => {
    const data = await getCompanies();
    setCompaniesData(data);
  };
  fetchCompanies();
}, []);
```

#### API Integration
- **Endpoint**: `GET /api/v1/ReportGeneration/all-companies`
- **Response Format**:
```javascript
[{
  id: string,
  company_name: string,
  company_description: string,
  thumbnail: string,
  news_data: Array<NewsItem>
}]
```

### 3.2 Company Detail View

#### Component Structure
```javascript
<div className="min-h-screen flex flex-col md:flex-row">
  <MainContent />       // Company info and news
  <ChatInterface />     // Resizable chat sidebar
</div>
```

#### News Feed Implementation
```javascript
// CompanyDetail.jsx
useEffect(() => {
  const currentCompany = companiesData.find((c) => c.id === companyId);
  setCompany(currentCompany);

  async function getNews() {
    const news = currentCompany.news_data;
    setNews(news);
  }

  if (currentCompany) {
    getNews();
  }
}, [companyId, companiesData]);
```

### 3.3 Chat Interface

#### Core Functionality
The chat interface (`ChatInterface.jsx`) provides real-time interaction for company analysis and report generation.

#### Message Flow
1. **User Input**:
```javascript
const handleSend = async () => {
  if (!inputValue.trim()) return;
  setIsLoading(true);

  try {
    const prompt = `company name: ${company_name}\n${inputValue}`;
    const response = await reportGeneration(prompt);

    const userMessage = {
      id: messages.length + 1,
      type: 'user',
      content: inputValue,
    };

    const aiResponse = {
      id: messages.length + 2,
      type: 'ai',
      content: response,
    };

    setMessages((prev) => [...prev, userMessage, aiResponse]);
  } catch (error) {
    console.error('Error:', error);
  } finally {
    setIsLoading(false);
  }
};
```

#### API Integration
- **Endpoint**: `POST /api/v1/ReportGeneration/report-generator`
- **Request Format**:
```javascript
{
  sessionId: string,
  useId: string,
  prompt: string
}
```

#### Quick Queries
Predefined queries available for common requests:
```javascript
const quickQueries = [
  'Generate an executive summary',
  'Summarize executive & board changes',
  `Summarize ${company_name} activity (3 years)`,
  'Confirm ASN status',
  'Summarize financials',
  'Summarize corporate timeline',
];
```

### 3.4 State Management

#### Local State
- Component-level state using React hooks
- No global state management library required due to component hierarchy

#### Data Flow
1. Company data fetched at CompanyList level
2. Passed down to CompanyCard components
3. Individual company data accessed in CompanyDetail
4. Chat state maintained within ChatInterface

### 3.5 API Helper Implementation

```javascript
// helpers/index.js
const api = axios.create({
  baseURL: 'http://localhost:5266/api/v1/ReportGeneration',
});

export const getCompanies = async () => {
  try {
    const response = await api.get('/all-companies');
    return response.data;
  } catch (error) {
    console.error(error);
    return [];
  }
};

export const reportGeneration = async (prompt) => {
  try {
    const response = await api.post('/report-generator', {
      sessionId: "1",
      useId: "1",
      prompt: prompt,
    });
    return response.data;
  } catch (error) {
    console.error(error);
    return [];
  }
};
```

### 3.6 Responsive Design Implementation

#### Layout Structure
- Mobile-first approach using Tailwind breakpoints
- Flexible grid system for company cards
- Resizable chat interface implementation:

```javascript
const [chatWidth, setChatWidth] = useState(400);
const [isResizing, setIsResizing] = useState(false);

useEffect(() => {
  const handleMouseMove = (e) => {
    if (!isResizing) return;
    const newWidth = window.innerWidth - e.clientX;
    setChatWidth(Math.min(Math.max(newWidth, 200), window.innerWidth * 0.9));
  };

  if (isResizing) {
    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', () => setIsResizing(false));
  }

  return () => {
    document.removeEventListener('mousemove', handleMouseMove);
    document.removeEventListener('mouseup', () => setIsResizing(false));
  };
}, [isResizing]);
```

### 3.7 Error Handling

#### Implementation
- API calls wrapped in try-catch blocks
- Error states maintained at component level
- Fallback UI components for error states

```javascript
const [error, setError] = useState(null);

// Error handling in API calls
try {
  const data = await getCompanies();
  setCompaniesData(data);
} catch (error) {
  setError(error.message);
  console.error('Failed to fetch companies:', error);
}
```

### 3.8 Loading States

#### Implementation
Custom loading animation for chat interface:
```css
.dot-flashing {
  position: relative;
  width: 4px;
  height: 4px;
  border-radius: 2px;
  background-color: #666;
  animation: dotFlashing 1s infinite linear alternate;
  animation-delay: 0.5s;
}
```

Loading states tracked per component:
```javascript
const [isLoading, setIsLoading] = useState(false);
// Used in API calls and user interactions
```