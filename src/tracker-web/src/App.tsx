import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { ToastProvider } from './components/Toast';
import { Dashboard } from './pages/Dashboard';
import { BooksPage } from './pages/BooksPage';
import { MoviesPage } from './pages/MoviesPage';
import { SongsPage } from './pages/SongsPage';
import { TodosPage } from './pages/TodosPage';

function App() {
  return (
    <ToastProvider>
      <BrowserRouter>
        <div className="app-layout">
          <aside className="sidebar">
            <div className="sidebar-logo">
              <div className="logo-icon">🚀</div>
              <h1>Tracker</h1>
            </div>
            <nav className="sidebar-nav">
              <NavLink to="/" end>
                <span className="nav-icon">📊</span>
                <span>Dashboard</span>
              </NavLink>
              <NavLink to="/books">
                <span className="nav-icon">📚</span>
                <span>Books</span>
              </NavLink>
              <NavLink to="/movies">
                <span className="nav-icon">🎬</span>
                <span>Movies</span>
              </NavLink>
              <NavLink to="/songs">
                <span className="nav-icon">🎵</span>
                <span>Songs</span>
              </NavLink>
              <NavLink to="/todos">
                <span className="nav-icon">✅</span>
                <span>Todos</span>
              </NavLink>
            </nav>
          </aside>
          <main className="main-content">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/books" element={<BooksPage />} />
              <Route path="/movies" element={<MoviesPage />} />
              <Route path="/songs" element={<SongsPage />} />
              <Route path="/todos" element={<TodosPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </ToastProvider>
  );
}

export default App;
