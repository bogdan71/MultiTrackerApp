const BASE = '/api';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
    const res = await fetch(`${BASE}${url}`, {
        headers: { 'Content-Type': 'application/json' },
        ...options,
    });
    if (!res.ok) {
        throw new Error(`API error: ${res.status} ${res.statusText}`);
    }
    if (res.status === 204) return undefined as T;
    return res.json();
}

export const api = {
    // Books
    getBooks: (params?: string) => request<any[]>(`/books${params ? `?${params}` : ''}`),
    getBook: (id: number) => request<any>(`/books/${id}`),
    createBook: (data: any) => request<any>('/books', { method: 'POST', body: JSON.stringify(data) }),
    updateBook: (id: number, data: any) => request<any>(`/books/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    deleteBook: (id: number) => request<void>(`/books/${id}`, { method: 'DELETE' }),

    // Movies
    getMovies: (params?: string) => request<any[]>(`/movies${params ? `?${params}` : ''}`),
    getMovie: (id: number) => request<any>(`/movies/${id}`),
    createMovie: (data: any) => request<any>('/movies', { method: 'POST', body: JSON.stringify(data) }),
    updateMovie: (id: number, data: any) => request<any>(`/movies/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    deleteMovie: (id: number) => request<void>(`/movies/${id}`, { method: 'DELETE' }),

    // Songs
    getSongs: (params?: string) => request<any[]>(`/songs${params ? `?${params}` : ''}`),
    getSong: (id: number) => request<any>(`/songs/${id}`),
    createSong: (data: any) => request<any>('/songs', { method: 'POST', body: JSON.stringify(data) }),
    updateSong: (id: number, data: any) => request<any>(`/songs/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    deleteSong: (id: number) => request<void>(`/songs/${id}`, { method: 'DELETE' }),

    // Todos
    getTodos: (params?: string) => request<any[]>(`/todos${params ? `?${params}` : ''}`),
    getTodo: (id: number) => request<any>(`/todos/${id}`),
    createTodo: (data: any) => request<any>('/todos', { method: 'POST', body: JSON.stringify(data) }),
    updateTodo: (id: number, data: any) => request<any>(`/todos/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    toggleTodo: (id: number) => request<any>(`/todos/${id}/toggle`, { method: 'PATCH' }),
    deleteTodo: (id: number) => request<void>(`/todos/${id}`, { method: 'DELETE' }),

    // Dashboard
    getDashboard: () => request<any>('/dashboard'),

    // Categories
    getCategories: () => request<any[]>('/categories'),
    createCategory: (data: any) => request<any>('/categories', { method: 'POST', body: JSON.stringify(data) }),
    deleteCategory: (id: number) => request<void>(`/categories/${id}`, { method: 'DELETE' }),
    getCategory: (slug: string) => request<any>(`/categories/${slug}`),

    // Items
    getItems: (slug: string) => request<any[]>(`/categories/${slug}/items`),
    createItem: (slug: string, data: any) => request<any>(`/categories/${slug}/items`, { method: 'POST', body: JSON.stringify(data) }),
    updateItem: (slug: string, id: number, data: any) => request<void>(`/categories/${slug}/items/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    deleteItem: (slug: string, id: number) => request<void>(`/categories/${slug}/items/${id}`, { method: 'DELETE' }),
};
