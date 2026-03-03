/**
 * Folder Detail Screen - Folder Contents
 * 
 * Shows contents of a specific folder
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  Image,
  Alert,
} from 'react-native';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootStackParamList, Document, Folder } from '../types';
import { RootState } from '../store';
import { toggleFavorite } from '../store/slices/documentSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;
type FolderDetailRouteProp = RouteProp<RootStackParamList, 'FolderDetail'>;

export const FolderDetailScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>();
  const route = useRoute<FolderDetailRouteProp>();
  const dispatch = useDispatch();
  
  const { folderId } = route.params;
  
  const folder = useSelector((state: RootState) =>
    state.folders.items.find(f => f.id === folderId)
  );
  
  const documents = useSelector((state: RootState) =>
    state.documents.items.filter(doc => doc.folderId === folderId)
  );
  
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
        <TouchableOpacity
          style={styles.favoriteButton}
          onPress={() => handleFavoritePress(item.id)}
        >
          <Text style={styles.favoriteIcon}>{item.isFavorite ? '⭐' : '☆'}</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );
  
  // Render empty state
  const renderEmpty = () => (
    <View style={styles.emptyState}>
      <Text style={styles.emptyIcon}>📂</Text>
      <Text style={styles.emptyText}>This folder is empty</Text>
      <Text style={styles.emptySubtext}>
        Scan or import documents to add them here
      </Text>
    </View>
  );
  
  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity 
          style={styles.backButton}
          onPress={() => navigation.goBack()}
        >
          <Text style={styles.backIcon}>←</Text>
        </TouchableOpacity>
        
        <View style={styles.headerInfo}>
          <View style={styles.folderIcon}>
            <Text style={styles.folderIconText}>📁</Text>
          </View>
          <View style={styles.headerText}>
            <Text style={styles.headerTitle}>{folder?.name || 'Folder'}</Text>
            <Text style={styles.headerSubtitle}>
              {documents.length} item{documents.length !== 1 ? 's' : ''}
            </Text>
          </View>
        </View>
        
        <TouchableOpacity style={styles.moreButton}>
          <Text style={styles.moreIcon}>⋮</Text>
        </TouchableOpacity>
      </View>
      
      {/* Documents List */}
      <FlatList
        data={documents}
        renderItem={renderDocumentItem}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.documentList}
        showsVerticalScrollIndicator={false}
        ListEmptyComponent={renderEmpty}
      />
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
    alignItems: 'center',
    paddingHorizontal: SPACING.md,
    paddingVertical: SPACING.md,
    backgroundColor: COLORS.surface,
    borderBottomWidth: 1,
    borderBottomColor: COLORS.border,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  backIcon: {
    fontSize: 20,
    color: COLORS.textPrimary,
  },
  headerInfo: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: SPACING.md,
  },
  folderIcon: {
    width: 40,
    height: 40,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  folderIconText: {
    fontSize: 20,
  },
  headerText: {
    marginLeft: SPACING.sm,
  },
  headerTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  headerSubtitle: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
  },
  moreButton: {
    width: 40,
    height: 40,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  moreIcon: {
    fontSize: 18,
    color: COLORS.textPrimary,
  },
  documentList: {
    padding: SPACING.lg,
    paddingBottom: SPACING.xxxl,
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
  favoriteButton: {
    justifyContent: 'center',
    paddingLeft: SPACING.sm,
  },
  favoriteIcon: {
    fontSize: 20,
  },
  emptyState: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingVertical: SPACING.xxxl,
  },
  emptyIcon: {
    fontSize: 64,
    marginBottom: SPACING.lg,
  },
  emptyText: {
    fontSize: FONT_SIZES.xl,
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
