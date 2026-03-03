/**
 * Favorites Screen - Starred Documents
 * 
 * Displays all favorited/starred documents
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  TouchableOpacity,
  Image,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootState } from '../store';
import { RootStackParamList, Document } from '../types';
import { toggleFavorite } from '../store/slices/documentSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

export const FavoritesScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const navigation = useNavigation<NavigationProp>();
  const dispatch = useDispatch();
  
  const documents = useSelector((state: RootState) => 
    state.documents.items.filter(doc => doc.isFavorite)
  );
  
  // Handle document press
  const handleDocumentPress = (document: Document) => {
    navigation.navigate('DocumentDetail', { documentId: document.id });
  };
  
  // Handle document open
  const handleDocumentOpen = (document: Document) => {
    navigation.navigate('PDFViewer', { documentId: document.id });
  };
  
  // Handle unfavorite
  const handleUnfavorite = (documentId: string) => {
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
          style={styles.unfavoriteButton}
          onPress={() => handleUnfavorite(item.id)}
        >
          <Text style={styles.unfavoriteIcon}>⭐</Text>
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );
  
  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Starred</Text>
        <Text style={styles.headerSubtitle}>
          {documents.length} document{documents.length !== 1 ? 's' : ''}
        </Text>
      </View>
      
      {/* Documents List */}
      {documents.length > 0 ? (
        <FlatList
          data={documents}
          renderItem={renderDocumentItem}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.documentList}
          showsVerticalScrollIndicator={false}
        />
      ) : (
        <View style={styles.emptyState}>
          <Text style={styles.emptyIcon}>⭐</Text>
          <Text style={styles.emptyText}>No starred documents</Text>
          <Text style={styles.emptySubtext}>
            Star your important documents for quick access
          </Text>
        </View>
      )}
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
    paddingHorizontal: SPACING.lg,
    paddingVertical: SPACING.lg,
  },
  headerTitle: {
    fontSize: FONT_SIZES.xxxl,
    fontWeight: '700',
    color: COLORS.textPrimary,
    marginBottom: SPACING.xs,
  },
  headerSubtitle: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  documentList: {
    paddingHorizontal: SPACING.lg,
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
  unfavoriteButton: {
    justifyContent: 'center',
    paddingLeft: SPACING.sm,
  },
  unfavoriteIcon: {
    fontSize: 20,
  },
  emptyState: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: SPACING.xxl,
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
