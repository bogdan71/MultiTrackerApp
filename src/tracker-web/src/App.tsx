import { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, NavLink } from 'react-router-dom';
import { ToastProvider } from './components/Toast';
import { Dashboard } from './pages/Dashboard';
import { BooksPage } from './pages/BooksPage';
import { MoviesPage } from './pages/MoviesPage';
import { SongsPage } from './pages/SongsPage';
import { TodosPage } from './pages/TodosPage';
import { DynamicCategoryPage } from './pages/DynamicCategoryPage';
import { CreateCategoryModal } from './components/CreateCategoryModal';
import { api } from './api';
import type { Category } from './types';

function App() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    loadCategories();
  }, []);

  async function loadCategories() {
    try {
      const data = await api.getCategories();
      setCategories(data);
    } catch (error) {
      console.error('Failed to load categories', error);
    }
  }

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

              <div className="nav-divider"></div>

              {categories.map(cat => (
                <NavLink key={cat.id} to={`/category/${cat.slug}`}>
                  <span className="nav-icon">{cat.icon || '📁'}</span>
                  <span>{cat.name}</span>
                </NavLink>
              ))}

              <button className="add-category-btn" onClick={() => setIsModalOpen(true)}>
                + Add Category
              </button>
            </nav>
          </aside>
          <main className="main-content">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/books" element={<BooksPage />} />
              <Route path="/movies" element={<MoviesPage />} />
              <Route path="/songs" element={<SongsPage />} />
              <Route path="/todos" element={<TodosPage />} />
              <Route path="/category/:slug" element={<DynamicCategoryPage />} />
            </Routes>
          </main>
        </div>
        <CreateCategoryModal
          isOpen={isModalOpen}
          onClose={() => setIsModalOpen(false)}
          onCreated={loadCategories}
        />
      </BrowserRouter>
    </ToastProvider>
  );
}

export default App;
