/**
 * UI Slice - Redux State Management
 * 
 * Handles UI-related state like modals, loading states, toasts
 */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

interface UIState {
  isLoading: boolean;
  loadingMessage: string | null;
  toasts: Toast[];
  modalVisible: boolean;
  modalType: string | null;
  modalData: any;
  isSearchActive: boolean;
  isSidebarOpen: boolean;
  selectedDocumentIds: string[];
  viewMode: 'grid' | 'list';
  deleteConfirmVisible: boolean;
  deleteConfirmData: { type: string; id: string; name: string } | null;
}

const initialState: UIState = {
  isLoading: false,
  loadingMessage: null,
  toasts: [],
  modalVisible: false,
  modalType: null,
  modalData: null,
  isSearchActive: false,
  isSidebarOpen: false,
  selectedDocumentIds: [],
  viewMode: 'grid',
  deleteConfirmVisible: false,
  deleteConfirmData: null,
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    setLoading: (state, action: PayloadAction<{ isLoading: boolean; message?: string }>) => {
      state.isLoading = action.payload.isLoading;
      state.loadingMessage = action.payload.message || null;
    },
    showToast: (state, action: PayloadAction<Omit<Toast, 'id'>>) => {
      const id = Date.now().toString();
      state.toasts.push({ ...action.payload, id });
    },
    hideToast: (state, action: PayloadAction<string>) => {
      state.toasts = state.toasts.filter(t => t.id !== action.payload);
    },
    clearToasts: (state) => {
      state.toasts = [];
    },
    showModal: (state, action: PayloadAction<{ type: string; data?: any }>) => {
      state.modalVisible = true;
      state.modalType = action.payload.type;
      state.modalData = action.payload.data || null;
    },
    hideModal: (state) => {
      state.modalVisible = false;
      state.modalType = null;
      state.modalData = null;
    },
    setSearchActive: (state, action: PayloadAction<boolean>) => {
      state.isSearchActive = action.payload;
    },
    toggleSidebar: (state) => {
      state.isSidebarOpen = !state.isSidebarOpen;
    },
    setSidebarOpen: (state, action: PayloadAction<boolean>) => {
      state.isSidebarOpen = action.payload;
    },
    toggleDocumentSelection: (state, action: PayloadAction<string>) => {
      const index = state.selectedDocumentIds.indexOf(action.payload);
      if (index === -1) {
        state.selectedDocumentIds.push(action.payload);
      } else {
        state.selectedDocumentIds.splice(index, 1);
      }
    },
    selectDocuments: (state, action: PayloadAction<string[]>) => {
      state.selectedDocumentIds = action.payload;
    },
    clearDocumentSelection: (state) => {
      state.selectedDocumentIds = [];
    },
    setViewMode: (state, action: PayloadAction<'grid' | 'list'>) => {
      state.viewMode = action.payload;
    },
    showDeleteConfirm: (state, action: PayloadAction<{ type: string; id: string; name: string }>) => {
      state.deleteConfirmVisible = true;
      state.deleteConfirmData = action.payload;
    },
    hideDeleteConfirm: (state) => {
      state.deleteConfirmVisible = false;
      state.deleteConfirmData = null;
    },
  },
});

export const {
  setLoading,
  showToast,
  hideToast,
  clearToasts,
  showModal,
  hideModal,
  setSearchActive,
  toggleSidebar,
  setSidebarOpen,
  toggleDocumentSelection,
  selectDocuments,
  clearDocumentSelection,
  setViewMode,
  showDeleteConfirm,
  hideDeleteConfirm,
} = uiSlice.actions;

export default uiSlice.reducer;
