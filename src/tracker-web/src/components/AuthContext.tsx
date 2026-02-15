import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';

interface AuthContextType {
    user: { email: string } | null;
    token: string | null;
    isLoading: boolean;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error('useAuth must be used within AuthProvider');
    return ctx;
}

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<{ email: string } | null>(null);
    const [token, setToken] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const savedToken = localStorage.getItem('tracker_token');
        const savedEmail = localStorage.getItem('tracker_email');
        if (savedToken && savedEmail) {
            setToken(savedToken);
            setUser({ email: savedEmail });
        }
        setIsLoading(false);
    }, []);

    async function login(email: string, password: string) {
        const res = await fetch('/api/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        });
        if (!res.ok) {
            const body = await res.text();
            throw new Error(body || `Login failed: ${res.status}`);
        }
        const data = await res.json();
        const accessToken = data.accessToken;
        localStorage.setItem('tracker_token', accessToken);
        localStorage.setItem('tracker_email', email);
        setToken(accessToken);
        setUser({ email });
    }

    async function register(email: string, password: string) {
        const res = await fetch('/api/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        });
        if (!res.ok) {
            const body = await res.json().catch(() => null);
            if (body?.errors) {
                const messages = Object.values(body.errors).flat().join(', ');
                throw new Error(messages || 'Registration failed');
            }
            throw new Error('Registration failed');
        }
        // Auto-login after registration
        await login(email, password);
    }

    function logout() {
        localStorage.removeItem('tracker_token');
        localStorage.removeItem('tracker_email');
        setToken(null);
        setUser(null);
    }

    return (
        <AuthContext.Provider value={{ user, token, isLoading, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
}
