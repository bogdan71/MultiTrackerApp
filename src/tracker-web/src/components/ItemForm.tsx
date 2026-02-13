import { useState, useEffect } from 'react';


export interface FormField {
    name: string;
    label: string;
    type: 'text' | 'date' | 'select' | 'textarea';
    required?: boolean;
    options?: { value: string; label: string }[];
}

interface ItemFormProps {
    title: string;
    fields: FormField[];
    initialValues?: Record<string, any>;
    onSubmit: (values: Record<string, any>) => void;
    onCancel: () => void;
}

const STATUS_OPTIONS = [
    { value: 'Upcoming', label: 'Upcoming' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Dropped', label: 'Dropped' },
];

const PRIORITY_OPTIONS = [
    { value: 'Low', label: 'Low' },
    { value: 'Medium', label: 'Medium' },
    { value: 'High', label: 'High' },
    { value: 'Critical', label: 'Critical' },
];

export { STATUS_OPTIONS, PRIORITY_OPTIONS };

export function ItemForm({ title, fields, initialValues, onSubmit, onCancel }: ItemFormProps) {
    const [values, setValues] = useState<Record<string, any>>({});

    useEffect(() => {
        if (initialValues) {
            setValues(initialValues);
        }
    }, [initialValues]);

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSubmit(values);
    };

    return (
        <div className="modal-overlay" onClick={onCancel}>
            <div className="modal" onClick={e => e.stopPropagation()}>
                <h3>{title}</h3>
                <form onSubmit={handleSubmit}>
                    {fields.map(field => (
                        <div className="form-group" key={field.name}>
                            <label htmlFor={field.name}>{field.label}</label>
                            {field.type === 'textarea' ? (
                                <textarea
                                    id={field.name}
                                    value={values[field.name] || ''}
                                    onChange={e => setValues(v => ({ ...v, [field.name]: e.target.value }))}
                                    required={field.required}
                                />
                            ) : field.type === 'select' ? (
                                <select
                                    id={field.name}
                                    value={values[field.name] || ''}
                                    onChange={e => setValues(v => ({ ...v, [field.name]: e.target.value }))}
                                    required={field.required}
                                >
                                    <option value="">Select...</option>
                                    {(field.options || []).map(opt => (
                                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                                    ))}
                                </select>
                            ) : (
                                <input
                                    id={field.name}
                                    type={field.type}
                                    value={values[field.name] || ''}
                                    onChange={e => setValues(v => ({ ...v, [field.name]: e.target.value }))}
                                    required={field.required}
                                />
                            )}
                        </div>
                    ))}
                    <div className="form-actions">
                        <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
                        <button type="submit" className="btn btn-primary">
                            {initialValues ? '💾 Save Changes' : '➕ Add Item'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
