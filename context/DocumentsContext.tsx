import React, { createContext, useContext, useState, useEffect, useMemo, ReactNode } from "react";
import AsyncStorage from "@react-native-async-storage/async-storage";

export type DocumentType = "pdf" | "image" | "word" | "excel";

export interface Document {
  id: string;
  name: string;
  type: DocumentType;
  size: string;
  pages: number;
  createdAt: number;
  updatedAt: number;
  uri?: string;
  thumbnail?: string;
  folderId?: string;
  starred: boolean;
  tags: string[];
}

export interface Folder {
  id: string;
  name: string;
  color: string;
  createdAt: number;
  documentCount: number;
}

interface DocumentsContextValue {
  documents: Document[];
  folders: Folder[];
  isLoading: boolean;
  addDocument: (doc: Omit<Document, "id" | "createdAt" | "updatedAt">) => Promise<Document>;
  deleteDocument: (id: string) => Promise<void>;
  renameDocument: (id: string, name: string) => Promise<void>;
  toggleStar: (id: string) => Promise<void>;
  addFolder: (name: string, color: string) => Promise<void>;
  deleteFolder: (id: string) => Promise<void>;
  getDocumentsInFolder: (folderId?: string) => Document[];
  recentDocuments: Document[];
  starredDocuments: Document[];
  totalSize: string;
}

const DocumentsContext = createContext<DocumentsContextValue | null>(null);

const STORAGE_KEY = "pdfpeaks_documents";
const FOLDERS_KEY = "pdfpeaks_folders";

const SAMPLE_DOCUMENTS: Document[] = [
  { id: "1", name: "Project Proposal 2026.pdf", type: "pdf", size: "2.4 MB", pages: 12, createdAt: Date.now() - 86400000 * 2, updatedAt: Date.now() - 86400000 * 2, starred: true, tags: ["work"] },
  { id: "2", name: "Invoice #1042.pdf", type: "pdf", size: "0.8 MB", pages: 2, createdAt: Date.now() - 86400000 * 5, updatedAt: Date.now() - 86400000 * 5, starred: false, tags: ["finance"], folderId: "f1" },
  { id: "3", name: "Scanned Contract.pdf", type: "pdf", size: "3.1 MB", pages: 8, createdAt: Date.now() - 86400000 * 1, updatedAt: Date.now() - 86400000 * 1, starred: true, tags: ["legal"], folderId: "f2" },
  { id: "4", name: "Meeting Notes.pdf", type: "pdf", size: "0.5 MB", pages: 3, createdAt: Date.now() - 86400000 * 7, updatedAt: Date.now() - 86400000 * 3, starred: false, tags: ["work"] },
  { id: "5", name: "Tax Return 2025.pdf", type: "pdf", size: "1.2 MB", pages: 6, createdAt: Date.now() - 86400000 * 14, updatedAt: Date.now() - 86400000 * 14, starred: false, tags: ["finance"], folderId: "f1" },
];

const SAMPLE_FOLDERS: Folder[] = [
  { id: "f1", name: "Finance", color: "#10B981", createdAt: Date.now() - 86400000 * 30, documentCount: 2 },
  { id: "f2", name: "Legal", color: "#8B5CF6", createdAt: Date.now() - 86400000 * 20, documentCount: 1 },
  { id: "f3", name: "Work", color: "#F59E0B", createdAt: Date.now() - 86400000 * 10, documentCount: 0 },
];

export function DocumentsProvider({ children }: { children: ReactNode }) {
  const [documents, setDocuments] = useState<Document[]>([]);
  const [folders, setFolders] = useState<Folder[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [docsRaw, foldersRaw] = await Promise.all([
          AsyncStorage.getItem(STORAGE_KEY),
          AsyncStorage.getItem(FOLDERS_KEY),
        ]);
        if (docsRaw) {
          setDocuments(JSON.parse(docsRaw));
        } else {
          setDocuments(SAMPLE_DOCUMENTS);
          await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(SAMPLE_DOCUMENTS));
        }
        if (foldersRaw) {
          setFolders(JSON.parse(foldersRaw));
        } else {
          setFolders(SAMPLE_FOLDERS);
          await AsyncStorage.setItem(FOLDERS_KEY, JSON.stringify(SAMPLE_FOLDERS));
        }
      } catch (e) {
        setDocuments(SAMPLE_DOCUMENTS);
        setFolders(SAMPLE_FOLDERS);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  const saveDocs = async (docs: Document[]) => {
    setDocuments(docs);
    await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(docs));
  };

  const saveFolders = async (flds: Folder[]) => {
    setFolders(flds);
    await AsyncStorage.setItem(FOLDERS_KEY, JSON.stringify(flds));
  };

  const addDocument = async (doc: Omit<Document, "id" | "createdAt" | "updatedAt">): Promise<Document> => {
    const newDoc: Document = {
      ...doc,
      id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
      createdAt: Date.now(),
      updatedAt: Date.now(),
    };
    const updated = [newDoc, ...documents];
    await saveDocs(updated);
    return newDoc;
  };

  const deleteDocument = async (id: string) => {
    await saveDocs(documents.filter((d) => d.id !== id));
  };

  const renameDocument = async (id: string, name: string) => {
    await saveDocs(documents.map((d) => d.id === id ? { ...d, name, updatedAt: Date.now() } : d));
  };

  const toggleStar = async (id: string) => {
    await saveDocs(documents.map((d) => d.id === id ? { ...d, starred: !d.starred } : d));
  };

  const addFolder = async (name: string, color: string) => {
    const folder: Folder = {
      id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
      name,
      color,
      createdAt: Date.now(),
      documentCount: 0,
    };
    await saveFolders([...folders, folder]);
  };

  const deleteFolder = async (id: string) => {
    await saveFolders(folders.filter((f) => f.id !== id));
    await saveDocs(documents.map((d) => d.folderId === id ? { ...d, folderId: undefined } : d));
  };

  const getDocumentsInFolder = (folderId?: string) => {
    if (!folderId) return documents.filter((d) => !d.folderId);
    return documents.filter((d) => d.folderId === folderId);
  };

  const recentDocuments = useMemo(
    () => [...documents].sort((a, b) => b.updatedAt - a.updatedAt).slice(0, 5),
    [documents]
  );

  const starredDocuments = useMemo(
    () => documents.filter((d) => d.starred),
    [documents]
  );

  const totalSize = useMemo(() => {
    const totalBytes = documents.reduce((acc, d) => {
      const num = parseFloat(d.size);
      const unit = d.size.includes("MB") ? 1024 * 1024 : 1024;
      return acc + num * unit;
    }, 0);
    if (totalBytes > 1024 * 1024) return `${(totalBytes / (1024 * 1024)).toFixed(1)} MB`;
    return `${(totalBytes / 1024).toFixed(1)} KB`;
  }, [documents]);

  const value = useMemo(() => ({
    documents,
    folders,
    isLoading,
    addDocument,
    deleteDocument,
    renameDocument,
    toggleStar,
    addFolder,
    deleteFolder,
    getDocumentsInFolder,
    recentDocuments,
    starredDocuments,
    totalSize,
  }), [documents, folders, isLoading]);

  return <DocumentsContext.Provider value={value}>{children}</DocumentsContext.Provider>;
}

export function useDocuments() {
  const ctx = useContext(DocumentsContext);
  if (!ctx) throw new Error("useDocuments must be used within DocumentsProvider");
  return ctx;
}
