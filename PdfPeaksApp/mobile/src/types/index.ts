/**
 * Type Definitions for PDFPeaksApp
 * 
 * Core type definitions for documents, pages, folders, and other entities
 */

// Document Types
export type DocumentType = 'pdf' | 'image';
export type CloudSyncStatus = 'synced' | 'pending' | 'error';
export type DocumentStatus = 'active' | 'archived' | 'deleted';

// Enhancement Types
export interface EnhancementSettings {
  brightness: number;
  contrast: number;
  saturation: number;
  sharpness: number;
  rotation: number;
  filter: ImageFilter;
}

export type ImageFilter = 
  | 'none' 
  | 'document' 
  | 'photo' 
  | 'grayscale' 
  | 'magicColor' 
  | 'blackWhite'
  | 'enhanced';

// Page Interface
export interface Page {
  id: string;
  documentId: string;
  order: number;
  imageUrl: string;
  thumbnailUrl: string;
  width: number;
  height: number;
  fileSize: number;
  enhancements: EnhancementSettings;
  ocrText?: string;
  createdAt: Date;
  updatedAt: Date;
}

// Document Interface
export interface Document {
  id: string;
  title: string;
  type: DocumentType;
  pages: Page[];
  folderId: string | null;
  tags: string[];
  createdAt: Date;
  updatedAt: Date;
  thumbnailUrl: string;
  fileSize: number;
  pageCount: number;
  isFavorite: boolean;
  isArchived: boolean;
  ocrText?: string;
  cloudSyncStatus: CloudSyncStatus;
  status: DocumentStatus;
}

// Folder Interface
export interface Folder {
  id: string;
  name: string;
  parentId: string | null;
  color: string;
  createdAt: Date;
  documentCount: number;
  isDefault: boolean;
}

// Tag Interface
export interface Tag {
  id: string;
  name: string;
  color: string;
  documentCount: number;
}

// Scan Session
export interface ScanSession {
  id: string;
  pages: Page[];
  status: 'capturing' | 'editing' | 'saving';
  startedAt: Date;
}

// OCR Result
export interface OCRResult {
  pageId: string;
  text: string;
  confidence: number;
  language: string;
  words: OCRWord[];
}

export interface OCRWord {
  text: string;
  confidence: number;
  bbox: {
    x0: number;
    y0: number;
    x1: number;
    y1: number;
  };
}

// Export Options
export interface ExportOptions {
  format: 'pdf' | 'jpeg' | 'png' | 'tiff' | 'docx' | 'txt';
  quality: 'low' | 'medium' | 'high' | 'original';
  pdfSettings?: {
    pageSize: 'letter' | 'a4' | 'legal' | 'custom';
    compression: number; // 0-100
    ocrEnabled: boolean;
  };
}

// PDF Operations
export interface PDFMergeOptions {
  documentIds: string[];
  outputTitle: string;
}

export interface PDFSplitOptions {
  documentId: string;
  pageRanges: string[]; // e.g., ["1-3", "5", "7-10"]
  outputTitles: string[];
}

// User Preferences
export interface UserPreferences {
  defaultExportFormat: 'pdf' | 'jpeg' | 'png';
  defaultPageSize: 'letter' | 'a4' | 'legal';
  autoEnhanceOnScan: boolean;
  cloudSyncEnabled: boolean;
  defaultOCRLanguage: string;
  biometricAuthEnabled: boolean;
  darkModeEnabled: boolean;
}

// Navigation Types
export type RootStackParamList = {
  Main: undefined;
  Scanner: { mode?: 'single' | 'batch' };
  Editor: { pageId: string; documentId?: string };
  PDFViewer: { documentId: string };
  DocumentDetail: { documentId: string };
  Settings: undefined;
  Export: { documentId: string };
  FolderDetail: { folderId: string };
  Search: undefined;
};

export type MainTabParamList = {
  Home: undefined;
  Documents: undefined;
  Scan: undefined;
  Favorites: undefined;
  Settings: undefined;
};

// API Response Types
export interface APIResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

// Storage Types
export interface StorageInfo {
  used: number;
  total: number;
  percentUsed: number;
}
