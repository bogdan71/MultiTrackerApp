import { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import { useToast } from '../components/Toast';
import { ItemForm, PRIORITY_OPTIONS, type FormField } from '../components/ItemForm';
import { ConfirmDialog } from '../components/ConfirmDialog';
import type { TodoItem } from '../types';

const FIELDS: FormField[] = [
    { name: 'title', label: 'Title', type: 'text', required: true },
    { name: 'description', label: 'Description', type: 'textarea' },
    { name: 'dueDate', label: 'Due Date', type: 'date' },
    { name: 'priority', label: 'Priority', type: 'select', options: PRIORITY_OPTIONS },
    { name: 'category', label: 'Category', type: 'text' },
];

export function TodosPage() {
    const [todos, setTodos] = useState<TodoItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [showForm, setShowForm] = useState(false);
    const [editing, setEditing] = useState<TodoItem | null>(null);
    const [deleting, setDeleting] = useState<TodoItem | null>(null);
    const { showToast } = useToast();

    const load = useCallback(async () => {
        try {
            setLoading(true);
            setTodos(await api.getTodos());
        } catch { showToast('Failed to load todos', 'error'); }
        finally { setLoading(false); }
    }, [showToast]);

    useEffect(() => { load(); }, [load]);

    const handleSubmit = async (values: Record<string, any>) => {
        try {
            if (editing) {
                await api.updateTodo(editing.id, { ...editing, ...values });
                showToast('Todo updated');
            } else {
                await api.createTodo(values);
                showToast('Todo added');
            }
            setShowForm(false);
            setEditing(null);
            load();
        } catch { showToast('Failed to save todo', 'error'); }
    };

    const handleToggle = async (todo: TodoItem) => {
        try {
            await api.toggleTodo(todo.id);
            showToast(todo.isCompleted ? 'Marked as pending' : 'Marked as completed');
            load();
        } catch { showToast('Failed to toggle todo', 'error'); }
    };

    const handleDelete = async () => {
        if (!deleting) return;
        try {
            await api.deleteTodo(deleting.id);
            showToast('Todo deleted');
            setDeleting(null);
            load();
        } catch { showToast('Failed to delete todo', 'error'); }
    };

    return (
        <div>
            <div className="page-header">
                <div>
                    <h2>✅ Todos</h2>
                    <p className="subtitle">{todos.filter(t => !t.isCompleted).length} pending • {todos.filter(t => t.isCompleted).length} completed</p>
                </div>
                <button className="btn btn-primary" onClick={() => { setEditing(null); setShowForm(true); }}>
                    ➕ Add Todo
                </button>
            </div>

            {loading ? (
                <div className="loading"><div className="spinner" /> Loading todos...</div>
            ) : todos.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">✅</div>
                    <p>No todos yet. Start by adding one!</p>
                    <button className="btn btn-primary" onClick={() => setShowForm(true)}>➕ Add Your First Todo</button>
                </div>
            ) : (
                <div className="items-grid">
                    {todos.map(todo => (
                        <div key={todo.id} className={`card item-card ${todo.isCompleted ? 'todo-completed' : ''}`}>
                            <div className="item-header">
                                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 12 }}>
                                    <button
                                        className={`todo-check ${todo.isCompleted ? 'checked' : ''}`}
                                        onClick={() => handleToggle(todo)}
                                        title={todo.isCompleted ? 'Mark as pending' : 'Mark as completed'}
                                    >
                                        {todo.isCompleted ? '✓' : ''}
                                    </button>
                                    <div>
                                        <div className="item-title">{todo.title}</div>
                                        {todo.description && <div className="item-subtitle">{todo.description}</div>}
                                    </div>
                                </div>
                                <span className={`badge badge-priority-${todo.priority.toLowerCase()}`}>{todo.priority}</span>
                            </div>
                            <div className="item-meta">
                                {todo.category && <span className="badge badge-genre">{todo.category}</span>}
                                {todo.dueDate && <span className="badge badge-upcoming">📅 {todo.dueDate}</span>}
                            </div>
                            <div className="item-actions">
                                <button className="btn btn-secondary btn-sm" onClick={() => { setEditing(todo); setShowForm(true); }}>✏️ Edit</button>
                                <button className="btn btn-danger btn-sm" onClick={() => setDeleting(todo)}>🗑️ Delete</button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {showForm && (
                <ItemForm
                    title={editing ? '✏️ Edit Todo' : '✅ Add New Todo'}
                    fields={FIELDS}
                    initialValues={editing || undefined}
                    onSubmit={handleSubmit}
                    onCancel={() => { setShowForm(false); setEditing(null); }}
                />
            )}

            {deleting && (
                <ConfirmDialog
                    title="Delete Todo"
                    message={`Are you sure you want to delete "${deleting.title}"?`}
                    onConfirm={handleDelete}
                    onCancel={() => setDeleting(null)}
                />
            )}
        </div>
    );
}
