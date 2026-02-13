
import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../api';
import type { Category, Item } from '../types';
import { useToast } from '../components/Toast';

export function DynamicCategoryPage() {
    const { slug } = useParams<{ slug: string }>();
    const [category, setCategory] = useState<Category | null>(null);
    const [items, setItems] = useState<Item[]>([]);
    const [loading, setLoading] = useState(true);
    const { showToast } = useToast();

    // Form state
    const [newItemTitle, setNewItemTitle] = useState('');

    useEffect(() => {
        loadData();
    }, [slug]);

    async function loadData() {
        if (!slug) return;
        setLoading(true);
        try {
            const cat = await api.getCategory(slug);
            setCategory(cat);
            const data = await api.getItems(slug);
            setItems(data);
        } catch (error) {
            console.error(error);
            showToast('Failed to load data', 'error');
        } finally {
            setLoading(false);
        }
    }

    async function handleAddItem(e: React.FormEvent) {
        e.preventDefault();
        if (!slug || !newItemTitle.trim()) return;

        try {
            const newItem = await api.createItem(slug, {
                title: newItemTitle,
                status: 'Active'
            });
            setItems([...items, newItem]);
            setNewItemTitle('');
            showToast('Item added!', 'success');
        } catch (error) {
            console.error(error);
            showToast('Failed to add item', 'error');
        }
    }

    async function handleDeleteItem(id: number) {
        if (!slug || !confirm('Are you sure?')) return;
        try {
            await api.deleteItem(slug, id);
            setItems(items.filter(i => i.id !== id));
            showToast('Item deleted', 'success');
        } catch (error) {
            console.error(error);
            showToast('Failed to delete item', 'error');
        }
    }

    if (loading) return <div>Loading...</div>;
    if (!category) return <div>Category not found</div>;

    return (
        <div className="page-container">
            <header className="page-header">
                <h2>{category.icon} {category.name}</h2>
                <p>{category.description}</p>
            </header>

            <div className="add-item-form">
                <form onSubmit={handleAddItem} className="inline-form">
                    <input
                        type="text"
                        value={newItemTitle}
                        onChange={(e) => setNewItemTitle(e.target.value)}
                        placeholder={`Add new ${category.name.slice(0, -1)}...`} // simplistic singularization
                        required
                    />
                    <button type="submit">Add</button>
                </form>
            </div>

            <div className="items-grid">
                {items.map(item => (
                    <div key={item.id} className="item-card">
                        <div className="item-content">
                            <h3>{item.title}</h3>
                            <p className="item-status">{item.status}</p>
                            {item.description && <p>{item.description}</p>}
                        </div>
                        <div className="item-actions">
                            <button onClick={() => handleDeleteItem(item.id)} className="delete-btn">Delete</button>
                        </div>
                    </div>
                ))}
                {items.length === 0 && (
                    <div className="empty-state">
                        <p>No items yet. Add one above!</p>
                    </div>
                )}
            </div>
        </div>
    );
}
