/**
 * Documents Screen - File Management
 * 
 * Displays all documents with folder organization
 */

import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  TextInput,
  Image,
  Alert,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootState } from '../store';
import { RootStackParamList, Document } from '../types';
import { setSearchQuery, toggleFavorite, deleteDocument } from '../store/slices/documentSlice';
import { setViewMode, showDeleteConfirm } from '../store/slices/uiSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

export const DocumentsScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const navigation = useNavigation<NavigationProp>();
  const dispatch = useDispatch();
  
  const documents = useSelector((state: RootState) => state.documents.items);
  const searchQuery = useSelector((state: RootState) => state.documents.searchQuery);
  const viewMode = useSelector((state: RootState) => state.ui.viewMode);
  const folders = useSelector((state: RootState) => state.folders.items);
  
  // Filter documents based on search
  const filteredDocuments = documents.filter(doc =>
    doc.title.toLowerCase().includes(searchQuery.toLowerCase())
  );
  
  // Handle search
  const handleSearch = (text: string) => {
    dispatch(setSearchQuery(text));
  };
  
  // Handle document press
  const handleDocumentPress = (document: Document) => {
    navigation.navigate('DocumentDetail', { documentId: document.id });
  };
  
  // Handle document open
  const handleDocumentOpen = (document: Document) => {
    navigation.navigate('PDFViewer', { documentId: document.id });
  };
  
  // Handle favorite toggle
  const handleFavoritePress = (documentId: string) => {
    dispatch(toggleFavorite(documentId));
  };
  
  // Handle delete
  const handleDeletePress = (document: Document) => {
    dispatch(showDeleteConfirm({
      type: 'document',
      id: document.id,
      name: document.title,
    }));
  };
  
  // Handle folder press
  const handleFolderPress = (folderId: string) => {
    navigation.navigate('FolderDetail', { folderId });
  };
  
  // Render document item
  const renderDocumentItem = ({ item }: { item: Document }) => (
    <TouchableOpacity
      style={styles.documentItem}
      onPress={() => handleDocumentPress(item)}
      onLongPress={() => handleDocumentOpen(item)}
      activeOpacity={0.7}
    >
      <View style={styles.documentThumbnail}>
        {item.thumbnailUrl ? (
          <Image source={{ uri: item.thumbnailUrl }} style={styles.thumbnailImage} />
        ) : (
          <View style={styles.placeholderThumbnail}>
            <Text style={styles.placeholderText}>
              {item.type === 'pdf' ? '📄' : '🖼️'}
            </Text>
          </View>
        )}
      </View>
      <View style={styles.documentContent}>
        <View style={styles.documentInfo}>
          <Text style={styles.documentTitle} numberOfLines={1}>
            {item.title}
          </Text>
          <Text style={styles.documentMeta}>
            {item.pageCount} page{item.pageCount !== 1 ? 's' : ''} • {formatFileSize(item.fileSize)}
          </Text>
          <Text style={styles.documentDate}>
            {formatDate(item.updatedAt)}
          </Text>
        </View>
        <View style={styles.documentActions}>
          <TouchableOpacity
            style={styles.actionIconButton}
            onPress={() => handleFavoritePress(item.id)}
          >
            <Text style={styles.actionIcon}>{item.isFavorite ? '⭐' : '☆'}</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={styles.actionIconButton}
            onPress={() => handleDeletePress(item)}
          >
            <Text style={styles.actionIcon}>🗑️</Text>
          </TouchableOpacity>
        </View>
      </View>
    </TouchableOpacity>
  );
  
  // Render folder item
  const renderFolderItem = ({ item }: { item: any }) => (
    <TouchableOpacity
      style={styles.folderItem}
      onPress={() => handleFolderPress(item.id)}
      activeOpacity={0.7}
    >
      <View style={styles.folderIcon}>
        <Text style={styles.folderIconText}>📁</Text>
      </View>
      <View style={styles.folderInfo}>
        <Text style={styles.folderName}>{item.name}</Text>
        <Text style={styles.folderCount}>{item.documentCount} items</Text>
      </View>
      <Text style={styles.chevron}>›</Text>
    </TouchableOpacity>
  );
  
  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>My Documents</Text>
        <View style={styles.headerActions}>
          <TouchableOpacity
            style={styles.viewModeButton}
            onPress={() => dispatch(setViewMode(viewMode === 'grid' ? 'list' : 'grid'))}
          >
            <Text style={styles.viewModeIcon}>{viewMode === 'grid' ? '☰' : '⊞'}</Text>
          </TouchableOpacity>
        </View>
      </View>
      
      {/* Search Bar */}
      <View style={styles.searchContainer}>
        <View style={styles.searchBar}>
          <Text style={styles.searchIcon}>🔍</Text>
          <TextInput
            style={styles.searchInput}
            placeholder="Search documents..."
            placeholderTextColor={COLORS.textTertiary}
            value={searchQuery}
            onChangeText={handleSearch}
          />
          {searchQuery.length > 0 && (
            <TouchableOpacity onPress={() => handleSearch('')}>
              <Text style={styles.clearIcon}>✕</Text>
            </TouchableOpacity>
          )}
        </View>
      </View>
      
      {/* Folders Section */}
      {folders.length > 0 && (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Folders</Text>
          <FlatList
            data={folders}
            renderItem={renderFolderItem}
            keyExtractor={(item) => item.id}
            scrollEnabled={false}
          />
        </View>
      )}
      
      {/* Documents List */}
      <View style={styles.section}>
        <Text style={styles.sectionTitle}>
          All Documents ({filteredDocuments.length})
        </Text>
        {filteredDocuments.length > 0 ? (
          <FlatList
            data={filteredDocuments}
            renderItem={renderDocumentItem}
            keyExtractor={(item) => item.id}
            showsVerticalScrollIndicator={false}
            contentContainerStyle={styles.documentList}
          />
        ) : (
          <View style={styles.emptyState}>
            <Text style={styles.emptyIcon}>📭</Text>
            <Text style={styles.emptyText}>
              {searchQuery ? 'No documents found' : 'No documents yet'}
            </Text>
            <Text style={styles.emptySubtext}>
              {searchQuery ? 'Try a different search term' : 'Scan your first document to get started'}
            </Text>
          </View>
        )}
      </View>
    </View>
  );
};

function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function formatDate(date: Date | string): string {
  const d = new Date(date);
  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: SPACING.lg,
    paddingVertical: SPACING.md,
  },
  headerTitle: {
    fontSize: FONT_SIZES.xxl,
    fontWeight: '700',
    color: COLORS.textPrimary,
  },
  headerActions: {
    flexDirection: 'row',
    gap: SPACING.sm,
  },
  viewModeButton: {
    width: 40,
    height: 40,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.surface,
    justifyContent: 'center',
    alignItems: 'center',
  },
  viewModeIcon: {
    fontSize: 18,
  },
  searchContainer: {
    paddingHorizontal: SPACING.lg,
    marginBottom: SPACING.md,
  },
  searchBar: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    paddingHorizontal: SPACING.md,
    height: 48,
    ...SHADOWS.small,
  },
  searchIcon: {
    fontSize: 16,
    marginRight: SPACING.sm,
  },
  searchInput: {
    flex: 1,
    fontSize: FONT_SIZES.md,
    color: COLORS.textPrimary,
  },
  clearIcon: {
    fontSize: 14,
    color: COLORS.textTertiary,
    padding: SPACING.xs,
  },
  section: {
    paddingHorizontal: SPACING.lg,
    marginBottom: SPACING.lg,
  },
  sectionTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.md,
  },
  documentList: {
    paddingBottom: SPACING.xxl,
  },
  documentItem: {
    flexDirection: 'row',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    marginBottom: SPACING.md,
    overflow: 'hidden',
    ...SHADOWS.small,
  },
  documentThumbnail: {
    width: 80,
    height: 80,
  },
  thumbnailImage: {
    width: '100%',
    height: '100%',
    resizeMode: 'cover',
  },
  placeholderThumbnail: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.background,
  },
  placeholderText: {
    fontSize: 28,
  },
  documentContent: {
    flex: 1,
    flexDirection: 'row',
    padding: SPACING.md,
  },
  documentInfo: {
    flex: 1,
    justifyContent: 'center',
  },
  documentTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: 2,
  },
  documentMeta: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
  },
  documentDate: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textTertiary,
    marginTop: 2,
  },
  documentActions: {
    justifyContent: 'center',
    gap: SPACING.xs,
  },
  actionIconButton: {
    padding: SPACING.xs,
  },
  actionIcon: {
    fontSize: 18,
  },
  folderItem: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.md,
    marginBottom: SPACING.sm,
    ...SHADOWS.small,
  },
  folderIcon: {
    width: 44,
    height: 44,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  folderIconText: {
    fontSize: 22,
  },
  folderInfo: {
    flex: 1,
    marginLeft: SPACING.md,
  },
  folderName: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  folderCount: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
  },
  chevron: {
    fontSize: 24,
    color: COLORS.textTertiary,
  },
  emptyState: {
    alignItems: 'center',
    paddingVertical: SPACING.xxxl,
  },
  emptyIcon: {
    fontSize: 48,
    marginBottom: SPACING.md,
  },
  emptyText: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.xs,
  },
  emptySubtext: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
    textAlign: 'center',
  },
});
