import { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import { useToast } from '../components/Toast';
import type { DashboardData } from '../types';

export function Dashboard() {
    const [data, setData] = useState<DashboardData | null>(null);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();

    const load = useCallback(async () => {
        try {
            setLoading(true);
            const d = await api.getDashboard();
            setData(d);
        } catch {
            showToast('Failed to load dashboard', 'error');
        } finally {
            setLoading(false);
        }
    }, [showToast]);

    useEffect(() => { load(); }, [load]);

    if (loading) return <div className="loading"><div className="spinner" /> Loading dashboard...</div>;
    if (!data) return null;

    const totalBooks = data.summary.books.reduce((a, b) => a + b.count, 0);
    const totalMovies = data.summary.movies.reduce((a, b) => a + b.count, 0);
    const totalSongs = data.summary.songs.reduce((a, b) => a + b.count, 0);

    return (
        <div>
            <div className="page-header">
                <div>
                    <h2>📊 Dashboard</h2>
                    <p className="subtitle">Your tracking overview at a glance</p>
                </div>
            </div>

            <div className="dashboard-stats">
                <div className="card stat-card books">
                    <div className="stat-icon">📚</div>
                    <div className="stat-value">{totalBooks}</div>
                    <div className="stat-label">Books Tracked</div>
                </div>
                <div className="card stat-card movies">
                    <div className="stat-icon">🎬</div>
                    <div className="stat-value">{totalMovies}</div>
                    <div className="stat-label">Movies Tracked</div>
                </div>
                <div className="card stat-card songs">
                    <div className="stat-icon">🎵</div>
                    <div className="stat-value">{totalSongs}</div>
                    <div className="stat-label">Songs Tracked</div>
                </div>
                <div className="card stat-card todos">
                    <div className="stat-icon">✅</div>
                    <div className="stat-value">{data.summary.todos.pending}</div>
                    <div className="stat-label">Pending Todos</div>
                </div>
                {/* Dynamic Categories Stats */}
                {data.summary.categories?.map(cat => (
                    <div key={cat.slug} className="card stat-card" style={{ position: 'relative' }}>
                        <div className="stat-icon">{cat.icon || '📁'}</div>
                        <div className="stat-value">{cat.count}</div>
                        <div className="stat-label">{cat.name}</div>
                        <div style={{
                            position: 'absolute',
                            bottom: 0,
                            left: 0,
                            right: 0,
                            height: '2px',
                            background: 'linear-gradient(90deg, var(--accent-blue), var(--accent-purple))'
                        }} />
                    </div>
                ))}
            </div>

            <div className="dashboard-sections">
                <div className="dashboard-section">
                    <h3>📚 Upcoming Books</h3>
                    {data.upcoming.books.length === 0 && <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>No upcoming books</p>}
                    {data.upcoming.books.map(b => (
                        <div key={b.id} className="section-item">
                            <div>
                                <div className="section-item-title">{b.title}</div>
                                <div className="section-item-sub">{b.author}{b.releaseDate ? ` • ${b.releaseDate}` : ''}</div>
                            </div>
                            <span className="badge badge-upcoming">Upcoming</span>
                        </div>
                    ))}
                </div>

                <div className="dashboard-section">
                    <h3>🎬 Upcoming Movies</h3>
                    {data.upcoming.movies.length === 0 && <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>No upcoming movies</p>}
                    {data.upcoming.movies.map(m => (
                        <div key={m.id} className="section-item">
                            <div>
                                <div className="section-item-title">{m.title}</div>
                                <div className="section-item-sub">{m.director}{m.releaseDate ? ` • ${m.releaseDate}` : ''}</div>
                            </div>
                            <span className="badge badge-upcoming">Upcoming</span>
                        </div>
                    ))}
                </div>

                <div className="dashboard-section">
                    <h3>🎵 Upcoming Songs</h3>
                    {data.upcoming.songs.length === 0 && <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>No upcoming songs</p>}
                    {data.upcoming.songs.map(s => (
                        <div key={s.id} className="section-item">
                            <div>
                                <div className="section-item-title">{s.title}</div>
                                <div className="section-item-sub">{s.artist}{s.releaseDate ? ` • ${s.releaseDate}` : ''}</div>
                            </div>
                            <span className="badge badge-upcoming">Upcoming</span>
                        </div>
                    ))}
                </div>

                <div className="dashboard-section">
                    <h3>🆕 Recent Items</h3>
                    {(!data.upcoming.recentItems || data.upcoming.recentItems.length === 0) && <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>No recent items</p>}
                    {data.upcoming.recentItems?.map(i => (
                        <div key={i.id} className="section-item">
                            <div>
                                <div className="section-item-title">{i.title}</div>
                                <div className="section-item-sub">{i.categoryName} • {new Date(i.createdAt).toLocaleDateString()}</div>
                            </div>
                            <span className="badge badge-genre">{i.status}</span>
                        </div>
                    ))}
                </div>

                <div className="dashboard-section">
                    <h3>📝 Pending Todos</h3>
                    {data.pendingTodos.length === 0 && <p style={{ color: 'var(--text-muted)', fontSize: 14 }}>All done! 🎉</p>}
                    {data.pendingTodos.map(t => (
                        <div key={t.id} className="section-item">
                            <div>
                                <div className="section-item-title">{t.title}</div>
                                <div className="section-item-sub">{t.category}{t.dueDate ? ` • Due: ${t.dueDate}` : ''}</div>
                            </div>
                            <span className={`badge badge-priority-${t.priority.toLowerCase()}`}>{t.priority}</span>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
