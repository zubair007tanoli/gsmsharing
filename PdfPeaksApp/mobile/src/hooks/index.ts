/**
 * Custom Hooks for PDFPeaksApp
 * 
 * Reusable hooks for common functionality
 */

import { useState, useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RootState, AppDispatch } from '../store';

/**
 * Hook for accessing Redux dispatch
 */
export const useAppDispatch = () => useDispatch<AppDispatch>();

/**
 * Hook for accessing Redux state
 */
export const useAppSelector = <T>(selector: (state: RootState) => T): T => 
  useSelector(selector);

/**
 * Hook for managing loading states
 */
export const useLoading = (initialState = false) => {
  const [isLoading, setIsLoading] = useState(initialState);
  const [error, setError] = useState<string | null>(null);

  const startLoading = useCallback(() => {
    setIsLoading(true);
    setError(null);
  }, []);

  const stopLoading = useCallback(() => {
    setIsLoading(false);
  }, []);

  const setErrorMessage = useCallback((message: string) => {
    setError(message);
    setIsLoading(false);
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    isLoading,
    error,
    startLoading,
    stopLoading,
    setErrorMessage,
    clearError,
  };
};

/**
 * Hook for document filtering and sorting
 */
export const useDocuments = () => {
  const documents = useAppSelector((state) => state.documents.items);
  const searchQuery = useAppSelector((state) => state.documents.searchQuery);
  const filterTags = useAppSelector((state) => state.documents.filterTags);
  const sortBy = useAppSelector((state) => state.documents.sortBy);
  const sortOrder = useAppSelector((state) => state.documents.sortOrder);

  const filteredDocuments = documents
    .filter(doc => {
      // Filter by search query
      if (searchQuery && !doc.title.toLowerCase().includes(searchQuery.toLowerCase())) {
        return false;
      }
      // Filter by tags
      if (filterTags.length > 0 && !filterTags.some(tag => doc.tags.includes(tag))) {
        return false;
      }
      return true;
    })
    .sort((a, b) => {
      let comparison = 0;
      switch (sortBy) {
        case 'name':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'size':
          comparison = a.fileSize - b.fileSize;
          break;
        case 'date':
        default:
          comparison = new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime();
          break;
      }
      return sortOrder === 'asc' ? comparison : -comparison;
    });

  return {
    documents: filteredDocuments,
    totalCount: documents.length,
    filteredCount: filteredDocuments.length,
  };
};

/**
 * Hook for scan session management
 */
export const useScanSession = () => {
  const session = useAppSelector((state) => state.scan.currentSession);
  const pendingPages = useAppSelector((state) => state.scan.pendingPages);
  const isCapturing = useAppSelector((state) => state.scan.isCapturing);
  const isProcessing = useAppSelector((state) => state.scan.isProcessing);

  const pageCount = session?.pages.length || 0;
  const hasPages = pageCount > 0;

  return {
    session,
    pageCount,
    hasPages,
    pendingPages,
    isCapturing,
    isProcessing,
  };
};

/**
 * Hook for user preferences
 */
export const usePreferences = () => {
  const preferences = useAppSelector((state) => state.settings.preferences);
  return preferences;
};

/**
 * Hook for storage info
 */
export const useStorage = () => {
  const storageInfo = useAppSelector((state) => state.settings.storageInfo);
  return storageInfo;
};

/**
 * Hook for camera/flash state
 */
export const useCamera = () => {
  const flashEnabled = useAppSelector((state) => state.scan.flashEnabled);
  const captureMode = useAppSelector((state) => state.scan.captureMode);
  
  return {
    flashEnabled,
    captureMode,
    isAutoMode: captureMode === 'auto',
  };
};

/**
 * Debounce hook for search input
 */
export const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
};

/**
 * Hook for handling back navigation
 */
export const useBackHandler = (handler: () => boolean) => {
  useEffect(() => {
    const backHandler = () => handler();
    
    // Add event listener for hardware back button
    // In React Native, you'd use BackHandler.addEventListener
    
    return () => {
      // Remove event listener
    };
  }, [handler]);
};
