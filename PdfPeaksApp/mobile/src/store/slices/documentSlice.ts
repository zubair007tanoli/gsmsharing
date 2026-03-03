/**
 * Document Slice - Redux State Management
 * 
 * Handles document-related state including CRUD operations
 */

import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { Document, Page } from '../../types';

interface DocumentState {
  items: Document[];
  currentDocument: Document | null;
  selectedPages: string[];
  isLoading: boolean;
  error: string | null;
  searchQuery: string;
  filterTags: string[];
  sortBy: 'date' | 'name' | 'size';
  sortOrder: 'asc' | 'desc';
}

const initialState: DocumentState = {
  items: [],
  currentDocument: null,
  selectedPages: [],
  isLoading: false,
  error: null,
  searchQuery: '',
  filterTags: [],
  sortBy: 'date',
  sortOrder: 'desc',
};

// Async Thunks
export const fetchDocuments = createAsyncThunk(
  'documents/fetchAll',
  async (_, { rejectWithValue }) => {
    try {
      // TODO: Implement API call
      // const response = await documentService.getAll();
      // return response.data;
      return [];
    } catch (error) {
      return rejectWithValue('Failed to fetch documents');
    }
  }
);

export const createDocument = createAsyncThunk(
  'documents/create',
  async (document: Partial<Document>, { rejectWithValue }) => {
    try {
      // TODO: Implement API call
      // const response = await documentService.create(document);
      // return response.data;
      return { ...document, id: Date.now().toString() } as Document;
    } catch (error) {
      return rejectWithValue('Failed to create document');
    }
  }
);

export const deleteDocument = createAsyncThunk(
  'documents/delete',
  async (documentId: string, { rejectWithValue }) => {
    try {
      // TODO: Implement API call
      // await documentService.delete(documentId);
      return documentId;
    } catch (error) {
      return rejectWithValue('Failed to delete document');
    }
  }
);

// Slice
const documentSlice = createSlice({
  name: 'documents',
  initialState,
  reducers: {
    setDocuments: (state, action: PayloadAction<Document[]>) => {
      state.items = action.payload;
    },
    addDocument: (state, action: PayloadAction<Document>) => {
      state.items.unshift(action.payload);
    },
    updateDocument: (state, action: PayloadAction<Document>) => {
      const index = state.items.findIndex(doc => doc.id === action.payload.id);
      if (index !== -1) {
        state.items[index] = action.payload;
      }
    },
    removeDocument: (state, action: PayloadAction<string>) => {
      state.items = state.items.filter(doc => doc.id !== action.payload);
    },
    setCurrentDocument: (state, action: PayloadAction<Document | null>) => {
      state.currentDocument = action.payload;
    },
    selectPage: (state, action: PayloadAction<string>) => {
      if (!state.selectedPages.includes(action.payload)) {
        state.selectedPages.push(action.payload);
      }
    },
    deselectPage: (state, action: PayloadAction<string>) => {
      state.selectedPages = state.selectedPages.filter(id => id !== action.payload);
    },
    clearSelectedPages: (state) => {
      state.selectedPages = [];
    },
    toggleFavorite: (state, action: PayloadAction<string>) => {
      const doc = state.items.find(d => d.id === action.payload);
      if (doc) {
        doc.isFavorite = !doc.isFavorite;
      }
    },
    addTagToDocument: (state, action: PayloadAction<{ documentId: string; tag: string }>) => {
      const doc = state.items.find(d => d.id === action.payload.documentId);
      if (doc && !doc.tags.includes(action.payload.tag)) {
        doc.tags.push(action.payload.tag);
      }
    },
    removeTagFromDocument: (state, action: PayloadAction<{ documentId: string; tag: string }>) => {
      const doc = state.items.find(d => d.id === action.payload.documentId);
      if (doc) {
        doc.tags = doc.tags.filter(t => t !== action.payload.tag);
      }
    },
    setSearchQuery: (state, action: PayloadAction<string>) => {
      state.searchQuery = action.payload;
    },
    setFilterTags: (state, action: PayloadAction<string[]>) => {
      state.filterTags = action.payload;
    },
    setSortBy: (state, action: PayloadAction<'date' | 'name' | 'size'>) => {
      state.sortBy = action.payload;
    },
    setSortOrder: (state, action: PayloadAction<'asc' | 'desc'>) => {
      state.sortOrder = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch Documents
      .addCase(fetchDocuments.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchDocuments.fulfilled, (state, action) => {
        state.isLoading = false;
        state.items = action.payload;
      })
      .addCase(fetchDocuments.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Create Document
      .addCase(createDocument.fulfilled, (state, action) => {
        state.items.unshift(action.payload);
      })
      // Delete Document
      .addCase(deleteDocument.fulfilled, (state, action) => {
        state.items = state.items.filter(doc => doc.id !== action.payload);
      });
  },
});

// Export actions
export const {
  setDocuments,
  addDocument,
  updateDocument,
  removeDocument,
  setCurrentDocument,
  selectPage,
  deselectPage,
  clearSelectedPages,
  toggleFavorite,
  addTagToDocument,
  removeTagFromDocument,
  setSearchQuery,
  setFilterTags,
  setSortBy,
  setSortOrder,
  clearError,
} = documentSlice.actions;

// Export reducer
export default documentSlice.reducer;
