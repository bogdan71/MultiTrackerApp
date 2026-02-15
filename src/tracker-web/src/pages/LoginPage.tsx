import { useState } from 'react';
import { useAuth } from '../components/AuthContext';

export function LoginPage() {
    const { login, register } = useAuth();
    const [isRegisterMode, setIsRegisterMode] = useState(false);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError('');

        if (isRegisterMode && password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }

        setIsSubmitting(true);
        try {
            if (isRegisterMode) {
                await register(email, password);
            } else {
                await login(email, password);
            }
        } catch (err: any) {
            setError(err.message || 'Something went wrong');
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div className="login-page">
            <div className="login-card">
                <div className="login-header">
                    <div className="login-logo">🚀</div>
                    <h1>Tracker</h1>
                    <p>{isRegisterMode ? 'Create your account' : 'Welcome back'}</p>
                </div>

                <form onSubmit={handleSubmit} className="login-form">
                    {error && <div className="login-error">{error}</div>}

                    <div className="form-group">
                        <label htmlFor="email">Email</label>
                        <input
                            id="email"
                            type="email"
                            value={email}
                            onChange={e => setEmail(e.target.value)}
                            placeholder="you@example.com"
                            required
                            autoFocus
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="password">Password</label>
                        <input
                            id="password"
                            type="password"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            placeholder="••••••••"
                            required
                            minLength={6}
                        />
                    </div>

                    {isRegisterMode && (
                        <div className="form-group">
                            <label htmlFor="confirmPassword">Confirm Password</label>
                            <input
                                id="confirmPassword"
                                type="password"
                                value={confirmPassword}
                                onChange={e => setConfirmPassword(e.target.value)}
                                placeholder="••••••••"
                                required
                                minLength={6}
                            />
                        </div>
                    )}

                    <button type="submit" className="login-submit" disabled={isSubmitting}>
                        {isSubmitting ? 'Please wait...' : isRegisterMode ? 'Create Account' : 'Sign In'}
                    </button>
                </form>

                <div className="login-footer">
                    <button
                        type="button"
                        className="login-toggle"
                        onClick={() => { setIsRegisterMode(!isRegisterMode); setError(''); }}
                    >
                        {isRegisterMode ? 'Already have an account? Sign in' : "Don't have an account? Register"}
                    </button>
                </div>
            </div>
        </div>
    );
}
