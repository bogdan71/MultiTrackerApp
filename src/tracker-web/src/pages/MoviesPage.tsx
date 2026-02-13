import { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import { useToast } from '../components/Toast';
import { ItemForm, STATUS_OPTIONS, type FormField } from '../components/ItemForm';
import { ConfirmDialog } from '../components/ConfirmDialog';
import type { Movie } from '../types';

const FIELDS: FormField[] = [
    { name: 'title', label: 'Title', type: 'text', required: true },
    { name: 'director', label: 'Director', type: 'text' },
    { name: 'genre', label: 'Genre', type: 'text' },
    { name: 'releaseDate', label: 'Release Date', type: 'date' },
    { name: 'status', label: 'Status', type: 'select', options: STATUS_OPTIONS },
    { name: 'posterUrl', label: 'Poster URL', type: 'text' },
    { name: 'notes', label: 'Notes', type: 'textarea' },
];

export function MoviesPage() {
    const [movies, setMovies] = useState<Movie[]>([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editing, setEditing] = useState<Movie | null>(null);
    const [deleting, setDeleting] = useState<Movie | null>(null);
    const { showToast } = useToast();

    const load = useCallback(async () => {
        try {
            setLoading(true);
            setMovies(await api.getMovies());
        } catch { showToast('Failed to load movies', 'error'); }
        finally { setLoading(false); }
    }, [showToast]);

    useEffect(() => { load(); }, [load]);

    const handleSubmit = async (values: Record<string, any>) => {
        try {
            if (editing) {
                await api.updateMovie(editing.id, { ...editing, ...values });
                showToast('Movie updated');
            } else {
                await api.createMovie(values);
                showToast('Movie added');
            }
            setShowForm(false);
            setEditing(null);
            load();
        } catch { showToast('Failed to save movie', 'error'); }
    };

    const handleDelete = async () => {
        if (!deleting) return;
        try {
            await api.deleteMovie(deleting.id);
            showToast('Movie deleted');
            setDeleting(null);
            load();
        } catch { showToast('Failed to delete movie', 'error'); }
    };

    const getStatusClass = (status: string) => `badge badge-${status.toLowerCase()}`;

    return (
        <div>
            <div className="page-header">
                <div>
                    <h2>🎬 Movies</h2>
                    <p className="subtitle">{movies.length} movies tracked</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setEditing(null); setShowForm(true); }}>
                    ➕ Add Movie
                </button>
            </div>

            {loading ? (
                <div className="loading"><div className="spinner" /> Loading movies...</div>
            ) : movies.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">🎬</div>
                    <p>No movies tracked yet. Start by adding one!</p>
                    <button className="btn btn-primary" onClick={() => setShowForm(true)}>➕ Add Your First Movie</button>
                </div>
            ) : (
                <div className="items-grid">
                    {movies.map(movie => (
                        <div key={movie.id} className="card item-card">
                            <div className="item-header">
                                <div>
                                    <div className="item-title">{movie.title}</div>
                                    <div className="item-subtitle">{movie.director || 'Unknown Director'}</div>
                                </div>
                                <span className={getStatusClass(movie.status)}>{movie.status}</span>
                            </div>
                            <div className="item-meta">
                                {movie.genre && <span className="badge badge-genre">{movie.genre}</span>}
                                {movie.releaseDate && <span className="badge badge-upcoming">📅 {movie.releaseDate}</span>}
                            </div>
                            {movie.notes && <p style={{ fontSize: 13, color: 'var(--text-muted)', marginTop: 10, lineHeight: 1.5 }}>{movie.notes}</p>}
                            <div className="item-actions">
                                <button className="btn btn-secondary btn-sm" onClick={() => { setEditing(movie); setShowForm(true); }}>✏️ Edit</button>
                                <button className="btn btn-danger btn-sm" onClick={() => setDeleting(movie)}>🗑️ Delete</button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {showForm && (
                <ItemForm
                    title={editing ? '✏️ Edit Movie' : '🎬 Add New Movie'}
                    fields={FIELDS}
                    initialValues={editing || undefined}
                    onSubmit={handleSubmit}
                    onCancel={() => { setShowForm(false); setEditing(null); }}
                />
            )}

            {deleting && (
                <ConfirmDialog
                    title="Delete Movie"
                    message={`Are you sure you want to delete "${deleting.title}"?`}
                    onConfirm={handleDelete}
                    onCancel={() => setDeleting(null)}
                />
            )}
        </div>
    );
}
