/**
 * Folder Slice - Redux State Management
 * 
 * Handles folder organization and navigation
 */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Folder } from '../../types';

interface FolderState {
  items: Folder[];
  currentFolderId: string | null;
  breadcrumbs: Folder[];
  isLoading: boolean;
}

const initialState: FolderState = {
  items: [],
  currentFolderId: null,
  breadcrumbs: [],
  isLoading: false,
};

const folderSlice = createSlice({
  name: 'folders',
  initialState,
  reducers: {
    setFolders: (state, action: PayloadAction<Folder[]>) => {
      state.items = action.payload;
    },
    addFolder: (state, action: PayloadAction<Folder>) => {
      state.items.push(action.payload);
    },
    updateFolder: (state, action: PayloadAction<Folder>) => {
      const index = state.items.findIndex(f => f.id === action.payload.id);
      if (index !== -1) {
        state.items[index] = action.payload;
      }
    },
    removeFolder: (state, action: PayloadAction<string>) => {
      state.items = state.items.filter(f => f.id !== action.payload);
    },
    setCurrentFolder: (state, action: PayloadAction<string | null>) => {
      state.currentFolderId = action.payload;
    },
    setBreadcrumbs: (state, action: PayloadAction<Folder[]>) => {
      state.breadcrumbs = action.payload;
    },
    addToBreadcrumbs: (state, action: PayloadAction<Folder>) => {
      if (!state.breadcrumbs.find(f => f.id === action.payload.id)) {
        state.breadcrumbs.push(action.payload);
      }
    },
    popBreadcrumb: (state) => {
      state.breadcrumbs.pop();
    },
    clearBreadcrumbs: (state) => {
      state.breadcrumbs = [];
    },
    setIsLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
    },
    incrementDocumentCount: (state, action: PayloadAction<string>) => {
      const folder = state.items.find(f => f.id === action.payload);
      if (folder) {
        folder.documentCount += 1;
      }
    },
    decrementDocumentCount: (state, action: PayloadAction<string>) => {
      const folder = state.items.find(f => f.id === action.payload);
      if (folder && folder.documentCount > 0) {
        folder.documentCount -= 1;
      }
    },
  },
});

export const {
  setFolders,
  addFolder,
  updateFolder,
  removeFolder,
  setCurrentFolder,
  setBreadcrumbs,
  addToBreadcrumbs,
  popBreadcrumb,
  clearBreadcrumbs,
  setIsLoading,
  incrementDocumentCount,
  decrementDocumentCount,
} = folderSlice.actions;

export default folderSlice.reducer;
