/**
 * Scan Slice - Redux State Management
 * 
 * Handles scanning session state and page management
 */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Page, EnhancementSettings, ScanSession } from '../../types';

interface ScanState {
  currentSession: ScanSession | null;
  isCapturing: boolean;
  isProcessing: boolean;
  flashEnabled: boolean;
  captureMode: 'auto' | 'manual';
  batchMode: boolean;
  pendingPages: Page[];
  editedPageId: string | null;
}

const defaultEnhancements: EnhancementSettings = {
  brightness: 0,
  contrast: 0,
  saturation: 0,
  sharpness: 0,
  rotation: 0,
  filter: 'none',
};

const initialState: ScanState = {
  currentSession: null,
  isCapturing: false,
  isProcessing: false,
  flashEnabled: false,
  captureMode: 'auto',
  batchMode: false,
  pendingPages: [],
  editedPageId: null,
};

const scanSlice = createSlice({
  name: 'scan',
  initialState,
  reducers: {
    startSession: (state, action: PayloadAction<{ mode: 'single' | 'batch' }>) => {
      state.currentSession = {
        id: Date.now().toString(),
        pages: [],
        status: 'capturing',
        startedAt: new Date(),
      };
      state.batchMode = action.payload.mode === 'batch';
      state.pendingPages = [];
    },
    endSession: (state) => {
      state.currentSession = null;
      state.pendingPages = [];
      state.isCapturing = false;
      state.isProcessing = false;
    },
    addPage: (state, action: PayloadAction<Page>) => {
      if (state.currentSession) {
        state.currentSession.pages.push(action.payload);
        state.pendingPages.push(action.payload);
      }
    },
    removePage: (state, action: PayloadAction<string>) => {
      if (state.currentSession) {
        state.currentSession.pages = state.currentSession.pages.filter(
          p => p.id !== action.payload
        );
        state.pendingPages = state.pendingPages.filter(
          p => p.id !== action.payload
        );
      }
    },
    reorderPages: (state, action: PayloadAction<string[]>) => {
      if (state.currentSession) {
        const pageMap = new Map(state.currentSession.pages.map(p => [p.id, p]));
        state.currentSession.pages = action.payload
          .map(id => pageMap.get(id))
          .filter((p): p is Page => p !== undefined);
      }
    },
    setPageEnhancements: (state, action: PayloadAction<{ pageId: string; enhancements: Partial<EnhancementSettings> }>) => {
      const { pageId, enhancements } = action.payload;
      const page = state.currentSession?.pages.find(p => p.id === pageId);
      if (page) {
        page.enhancements = { ...page.enhancements, ...enhancements };
      }
      const pendingPage = state.pendingPages.find(p => p.id === pageId);
      if (pendingPage) {
        pendingPage.enhancements = { ...pendingPage.enhancements, ...enhancements };
      }
    },
    applyFilter: (state, action: PayloadAction<{ pageId: string; filter: EnhancementSettings['filter'] }>) => {
      const { pageId, filter } = action.payload;
      const page = state.currentSession?.pages.find(p => p.id === pageId);
      if (page) {
        page.enhancements.filter = filter;
      }
    },
    rotatePage: (state, action: PayloadAction<{ pageId: string; degrees: number }>) => {
      const { pageId, degrees } = action.payload;
      const page = state.currentSession?.pages.find(p => p.id === pageId);
      if (page) {
        page.enhancements.rotation = (page.enhancements.rotation + degrees) % 360;
      }
    },
    cropPage: (state, action: PayloadAction<{ pageId: string; cropData: { x: number; y: number; width: number; height: number } }>) => {
      // Crop functionality - store crop data
      const { pageId, cropData } = action.payload;
      const page = state.currentSession?.pages.find(p => p.id === pageId);
      if (page) {
        // Crop would be applied when saving
      }
    },
    setIsCapturing: (state, action: PayloadAction<boolean>) => {
      state.isCapturing = action.payload;
    },
    setIsProcessing: (state, action: PayloadAction<boolean>) => {
      state.isProcessing = action.payload;
    },
    toggleFlash: (state) => {
      state.flashEnabled = !state.flashEnabled;
    },
    setCaptureMode: (state, action: PayloadAction<'auto' | 'manual'>) => {
      state.captureMode = action.payload;
    },
    setEditedPage: (state, action: PayloadAction<string | null>) => {
      state.editedPageId = action.payload;
    },
    clearPendingPages: (state) => {
      state.pendingPages = [];
    },
    setSessionStatus: (state, action: PayloadAction<ScanSession['status']>) => {
      if (state.currentSession) {
        state.currentSession.status = action.payload;
      }
    },
  },
});

export const {
  startSession,
  endSession,
  addPage,
  removePage,
  reorderPages,
  setPageEnhancements,
  applyFilter,
  rotatePage,
  cropPage,
  setIsCapturing,
  setIsProcessing,
  toggleFlash,
  setCaptureMode,
  setEditedPage,
  clearPendingPages,
  setSessionStatus,
} = scanSlice.actions;

export default scanSlice.reducer;
