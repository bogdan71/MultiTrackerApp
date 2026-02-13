interface ConfirmDialogProps {
    title: string;
    message: string;
    onConfirm: () => void;
    onCancel: () => void;
}

export function ConfirmDialog({ title, message, onConfirm, onCancel }: ConfirmDialogProps) {
    return (
        <div className="modal-overlay" onClick={onCancel}>
            <div className="modal confirm-dialog" onClick={e => e.stopPropagation()}>
                <h3>⚠️ {title}</h3>
                <p>{message}</p>
                <div className="confirm-actions" style={{ marginTop: 20 }}>
                    <button className="btn btn-secondary" onClick={onCancel}>Cancel</button>
                    <button className="btn btn-danger" onClick={onConfirm}>Delete</button>
                </div>
            </div>
        </div>
    );
}
