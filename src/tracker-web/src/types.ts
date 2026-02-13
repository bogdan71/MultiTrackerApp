export interface Book {
    id: number;
    title: string;
    author?: string;
    genre?: string;
    releaseDate?: string;
    status: TrackingStatus;
    notes?: string;
    coverImageUrl?: string;
    createdAt: string;
    updatedAt: string;
}

export interface Movie {
    id: number;
    title: string;
    director?: string;
    genre?: string;
    releaseDate?: string;
    status: TrackingStatus;
    notes?: string;
    posterUrl?: string;
    createdAt: string;
    updatedAt: string;
}

export interface Song {
    id: number;
    title: string;
    artist?: string;
    album?: string;
    genre?: string;
    releaseDate?: string;
    status: TrackingStatus;
    notes?: string;
    albumArtUrl?: string;
    createdAt: string;
    updatedAt: string;
}

export interface TodoItem {
    id: number;
    title: string;
    description?: string;
    dueDate?: string;
    priority: Priority;
    isCompleted: boolean;
    category?: string;
    createdAt: string;
    updatedAt: string;
}

export type TrackingStatus = 'Upcoming' | 'InProgress' | 'Completed' | 'Dropped';
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface DashboardData {
    summary: {
        books: { status: string; count: number }[];
        movies: { status: string; count: number }[];
        songs: { status: string; count: number }[];
        todos: { total: number; completed: number; pending: number; highPriority: number };
        categories: { name: string; icon?: string; slug: string; count: number }[];
    };
    upcoming: {
        books: Book[];
        movies: Movie[];
        songs: Song[];
        recentItems: RecentItem[];
    };
    pendingTodos: TodoItem[];
}

export interface RecentItem {
    id: number;
    title: string;
    status: string;
    categoryName: string;
    categorySlug: string;
    createdAt: string;
}

export interface Category {
    id: number;
    name: string;
    slug: string;
    icon?: string;
    description?: string;
}

export interface Item {
    id: number;
    categoryId: number;
    title: string;
    description?: string;
    status: string;
    properties?: string;
    createdAt: string;
    updatedAt: string;
}
