/**
 * Redux Store Configuration
 * 
 * Central state management for PDFPeaksApp
 */

import { configureStore, combineReducers } from '@reduxjs/toolkit';
import documentReducer from './slices/documentSlice';
import folderReducer from './slices/folderSlice';
import scanReducer from './slices/scanSlice';
import uiReducer from './slices/uiSlice';
import settingsReducer from './slices/settingsSlice';

// Combine all reducers
const rootReducer = combineReducers({
  documents: documentReducer,
  folders: folderReducer,
  scan: scanReducer,
  ui: uiReducer,
  settings: settingsReducer,
});

// Configure store
export const store = configureStore({
  reducer: rootReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        // Ignore Date objects in state
        ignoredActions: [
          'documents/addDocument',
          'documents/updateDocument',
          'documents/setDocuments',
          'folders/addFolder',
          'folders/setFolders',
          'scan/addPage',
        ],
        ignoredPaths: [
          'documents.items',
          'folders.items',
          'scan.currentSession.pages',
        ],
      },
    }),
});

// Export types
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

// Re-export actions
export * from './slices/documentSlice';
export * from './slices/folderSlice';
export * from './slices/scanSlice';
export * from './slices/uiSlice';
export * from './slices/settingsSlice';
