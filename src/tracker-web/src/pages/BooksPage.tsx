import { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import { useToast } from '../components/Toast';
import { ItemForm, STATUS_OPTIONS, type FormField } from '../components/ItemForm';
import { ConfirmDialog } from '../components/ConfirmDialog';
import type { Book } from '../types';

const FIELDS: FormField[] = [
    { name: 'title', label: 'Title', type: 'text', required: true },
    { name: 'author', label: 'Author', type: 'text' },
    { name: 'genre', label: 'Genre', type: 'text' },
    { name: 'releaseDate', label: 'Release Date', type: 'date' },
    { name: 'status', label: 'Status', type: 'select', options: STATUS_OPTIONS },
    { name: 'coverImageUrl', label: 'Cover Image URL', type: 'text' },
    { name: 'notes', label: 'Notes', type: 'textarea' },
];

export function BooksPage() {
    const [books, setBooks] = useState<Book[]>([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editing, setEditing] = useState<Book | null>(null);
    const [deleting, setDeleting] = useState<Book | null>(null);
    const { showToast } = useToast();

    const load = useCallback(async () => {
        try {
            setLoading(true);
            setBooks(await api.getBooks());
        } catch { showToast('Failed to load books', 'error'); }
        finally { setLoading(false); }
    }, [showToast]);

    useEffect(() => { load(); }, [load]);

    const handleSubmit = async (values: Record<string, any>) => {
        try {
            if (editing) {
                await api.updateBook(editing.id, { ...editing, ...values });
                showToast('Book updated');
            } else {
                await api.createBook(values);
                showToast('Book added');
            }
            setShowForm(false);
            setEditing(null);
            load();
        } catch { showToast('Failed to save book', 'error'); }
    };

    const handleDelete = async () => {
        if (!deleting) return;
        try {
            await api.deleteBook(deleting.id);
            showToast('Book deleted');
            setDeleting(null);
            load();
        } catch { showToast('Failed to delete book', 'error'); }
    };

    const getStatusClass = (status: string) => `badge badge-${status.toLowerCase()}`;

    return (
        <div>
            <div className="page-header">
                <div>
                    <h2>📚 Books</h2>
                    <p className="subtitle">{books.length} books tracked</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setEditing(null); setShowForm(true); }}>
                    ➕ Add Book
                </button>
            </div>

            {loading ? (
                <div className="loading"><div className="spinner" /> Loading books...</div>
            ) : books.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">📚</div>
                    <p>No books tracked yet. Start by adding one!</p>
                    <button className="btn btn-primary" onClick={() => setShowForm(true)}>➕ Add Your First Book</button>
                </div>
            ) : (
                <div className="items-grid">
                    {books.map(book => (
                        <div key={book.id} className="card item-card">
                            <div className="item-header">
                                <div>
                                    <div className="item-title">{book.title}</div>
                                    <div className="item-subtitle">{book.author || 'Unknown Author'}</div>
                                </div>
                                <span className={getStatusClass(book.status)}>{book.status}</span>
                            </div>
                            <div className="item-meta">
                                {book.genre && <span className="badge badge-genre">{book.genre}</span>}
                                {book.releaseDate && <span className="badge badge-upcoming">📅 {book.releaseDate}</span>}
                            </div>
                            {book.notes && <p style={{ fontSize: 13, color: 'var(--text-muted)', marginTop: 10, lineHeight: 1.5 }}>{book.notes}</p>}
                            <div className="item-actions">
                                <button className="btn btn-secondary btn-sm" onClick={() => { setEditing(book); setShowForm(true); }}>✏️ Edit</button>
                                <button className="btn btn-danger btn-sm" onClick={() => setDeleting(book)}>🗑️ Delete</button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {showForm && (
                <ItemForm
                    title={editing ? '✏️ Edit Book' : '📚 Add New Book'}
                    fields={FIELDS}
                    initialValues={editing || undefined}
                    onSubmit={handleSubmit}
                    onCancel={() => { setShowForm(false); setEditing(null); }}
                />
            )}

            {deleting && (
                <ConfirmDialog
                    title="Delete Book"
                    message={`Are you sure you want to delete "${deleting.title}"?`}
                    onConfirm={handleDelete}
                    onCancel={() => setDeleting(null)}
                />
            )}
        </div>
    );
}
