
import { useState } from 'react';
import { api } from '../api';
import { useToast } from './Toast';

interface CreateCategoryModalProps {
    isOpen: boolean;
    onClose: () => void;
    onCreated: () => void;
}

export function CreateCategoryModal({ isOpen, onClose, onCreated }: CreateCategoryModalProps) {
    const [name, setName] = useState('');
    const [slug, setSlug] = useState('');
    const [icon, setIcon] = useState('📁');
    const [description, setDescription] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { showToast } = useToast();

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        try {
            await api.createCategory({ name, slug, icon, description });
            showToast('Category created!', 'success');
            onCreated();
            onClose();
            // Reset form
            setName('');
            setSlug('');
            setIcon('📁');
            setDescription('');
        } catch (error) {
            console.error(error);
            showToast('Failed to create category', 'error');
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.value;
        setName(val);
        // Auto-generate slug
        setSlug(val.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, ''));
    };

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <div className="modal-header">
                    <h2>Add New Category</h2>
                    <button className="close-btn" onClick={onClose}>×</button>
                </div>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label>Name</label>
                        <input
                            type="text"
                            value={name}
                            onChange={handleNameChange}
                            required
                            placeholder="e.g. Apps, Games"
                        />
                    </div>
                    <div className="form-group">
                        <label>Slug (URL)</label>
                        <input
                            type="text"
                            value={slug}
                            onChange={(e) => setSlug(e.target.value)}
                            required
                        />
                    </div>
                    <div className="form-group">
                        <label>Icon (Emoji)</label>
                        <input
                            type="text"
                            value={icon}
                            onChange={(e) => setIcon(e.target.value)}
                            maxLength={5}
                            placeholder="e.g. 📱"
                        />
                    </div>
                    <div className="form-group">
                        <label>Description</label>
                        <textarea
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            rows={3}
                        />
                    </div>
                    <div className="modal-actions">
                        <button type="button" onClick={onClose} disabled={isSubmitting}>Cancel</button>
                        <button type="submit" disabled={isSubmitting}>create</button>
                    </div>
                </form>
            </div>
        </div>
    );
}
