import { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import { useToast } from '../components/Toast';
import { ItemForm, STATUS_OPTIONS, type FormField } from '../components/ItemForm';
import { ConfirmDialog } from '../components/ConfirmDialog';
import type { Song } from '../types';

const FIELDS: FormField[] = [
    { name: 'title', label: 'Title', type: 'text', required: true },
    { name: 'artist', label: 'Artist', type: 'text' },
    { name: 'album', label: 'Album', type: 'text' },
    { name: 'genre', label: 'Genre', type: 'text' },
    { name: 'releaseDate', label: 'Release Date', type: 'date' },
    { name: 'status', label: 'Status', type: 'select', options: STATUS_OPTIONS },
    { name: 'albumArtUrl', label: 'Album Art URL', type: 'text' },
    { name: 'notes', label: 'Notes', type: 'textarea' },
];

export function SongsPage() {
    const [songs, setSongs] = useState<Song[]>([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editing, setEditing] = useState<Song | null>(null);
    const [deleting, setDeleting] = useState<Song | null>(null);
    const { showToast } = useToast();

    const load = useCallback(async () => {
        try {
            setLoading(true);
            setSongs(await api.getSongs());
        } catch { showToast('Failed to load songs', 'error'); }
        finally { setLoading(false); }
    }, [showToast]);

    useEffect(() => { load(); }, [load]);

    const handleSubmit = async (values: Record<string, any>) => {
        try {
            if (editing) {
                await api.updateSong(editing.id, { ...editing, ...values });
                showToast('Song updated');
            } else {
                await api.createSong(values);
                showToast('Song added');
            }
            setShowForm(false);
            setEditing(null);
            load();
        } catch { showToast('Failed to save song', 'error'); }
    };

    const handleDelete = async () => {
        if (!deleting) return;
        try {
            await api.deleteSong(deleting.id);
            showToast('Song deleted');
            setDeleting(null);
            load();
        } catch { showToast('Failed to delete song', 'error'); }
    };

    const getStatusClass = (status: string) => `badge badge-${status.toLowerCase()}`;

    return (
        <div>
            <div className="page-header">
                <div>
                    <h2>🎵 Songs</h2>
                    <p className="subtitle">{songs.length} songs tracked</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setEditing(null); setShowForm(true); }}>
                    ➕ Add Song
                </button>
            </div>

            {loading ? (
                <div className="loading"><div className="spinner" /> Loading songs...</div>
            ) : songs.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">🎵</div>
                    <p>No songs tracked yet. Start by adding one!</p>
                    <button className="btn btn-primary" onClick={() => setShowForm(true)}>➕ Add Your First Song</button>
                </div>
            ) : (
                <div className="items-grid">
                    {songs.map(song => (
                        <div key={song.id} className="card item-card">
                            <div className="item-header">
                                <div>
                                    <div className="item-title">{song.title}</div>
                                    <div className="item-subtitle">{song.artist || 'Unknown Artist'}{song.album ? ` • ${song.album}` : ''}</div>
                                </div>
                                <span className={getStatusClass(song.status)}>{song.status}</span>
                            </div>
                            <div className="item-meta">
                                {song.genre && <span className="badge badge-genre">{song.genre}</span>}
                                {song.releaseDate && <span className="badge badge-upcoming">📅 {song.releaseDate}</span>}
                            </div>
                            {song.notes && <p style={{ fontSize: 13, color: 'var(--text-muted)', marginTop: 10, lineHeight: 1.5 }}>{song.notes}</p>}
                            <div className="item-actions">
                                <button className="btn btn-secondary btn-sm" onClick={() => { setEditing(song); setShowForm(true); }}>✏️ Edit</button>
                                <button className="btn btn-danger btn-sm" onClick={() => setDeleting(song)}>🗑️ Delete</button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {showForm && (
                <ItemForm
                    title={editing ? '✏️ Edit Song' : '🎵 Add New Song'}
                    fields={FIELDS}
                    initialValues={editing || undefined}
                    onSubmit={handleSubmit}
                    onCancel={() => { setShowForm(false); setEditing(null); }}
                />
            )}

            {deleting && (
                <ConfirmDialog
                    title="Delete Song"
                    message={`Are you sure you want to delete "${deleting.title}"?`}
                    onConfirm={handleDelete}
                    onCancel={() => setDeleting(null)}
                />
            )}
        </div>
    );
}
