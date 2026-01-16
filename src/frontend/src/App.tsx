import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Navbar } from './components/Navbar';
import { AuthProvider } from './contexts/AuthContext';
import { CompetitionCreate } from './pages/CompetitionCreate';
import { CompetitionList } from './pages/CompetitionList';
import { Home } from './pages/Home';
import { OrganizerRegister } from './pages/OrganizerRegister';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <div className="min-h-screen bg-gray-50">
            <Navbar />
            <main className="container mx-auto px-4 py-8">
              <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/register" element={<OrganizerRegister />} />
                <Route path="/competitions" element={<CompetitionList />} />
                <Route path="/competitions/create" element={<CompetitionCreate />} />
              </Routes>
            </main>
            <Toaster position="top-right" />
          </div>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
