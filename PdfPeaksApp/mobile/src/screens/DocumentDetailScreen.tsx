/**
 * Document Detail Screen - Document Actions
 * 
 * Shows document details with actions like view, share, export
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Image,
  ScrollView,
  Alert,
} from 'react-native';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootStackParamList } from '../types';
import { RootState } from '../store';
import { toggleFavorite, deleteDocument } from '../store/slices/documentSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;
type DocumentDetailRouteProp = RouteProp<RootStackParamList, 'DocumentDetail'>;

export const DocumentDetailScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>();
  const route = useRoute<DocumentDetailRouteProp>();
  const dispatch = useDispatch();
  
  const { documentId } = route.params;
  
  const document = useSelector((state: RootState) =>
    state.documents.items.find(doc => doc.id === documentId)
  );
  
  // Handle view PDF
  const handleViewPDF = () => {
    if (document) {
      navigation.navigate('PDFViewer', { documentId: document.id });
    }
  };
  
  // Handle share
  const handleShare = () => {
    Alert.alert('Share', 'Share functionality would open native share sheet');
  };
  
  // Handle export
  const handleExport = () => {
    Alert.alert('Export', 'Export options: PDF, JPG, PNG, DOCX');
  };
  
  // Handle favorite toggle
  const handleToggleFavorite = () => {
    if (document) {
      dispatch(toggleFavorite(document.id));
    }
  };
  
  // Handle delete
  const handleDelete = () => {
    Alert.alert(
      'Delete Document',
      `Are you sure you want to delete "${document?.title}"?`,
      [
        { text: 'Cancel', style: 'cancel' },
        { 
          text: 'Delete', 
          style: 'destructive',
          onPress: () => {
            if (document) {
              dispatch(deleteDocument(document.id));
              navigation.goBack();
            }
          }
        },
      ]
    );
  };
  
  // Handle edit
  const handleEdit = () => {
    if (document && document.pages.length > 0) {
      navigation.navigate('Editor', { pageId: document.pages[0].id, documentId: document.id });
    }
  };
  
  if (!document) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorText}>Document not found</Text>
      </View>
    );
  }
  
  return (
    <View style={styles.container}>
      <ScrollView style={styles.content} showsVerticalScrollIndicator={false}>
        {/* Document Preview */}
        <View style={styles.previewContainer}>
          <View style={styles.previewPlaceholder}>
            <Text style={styles.previewIcon}>
              {document.type === 'pdf' ? '📄' : '🖼️'}
            </Text>
            <Text style={styles.previewPageCount}>
              {document.pageCount} page{document.pageCount !== 1 ? 's' : ''}
            </Text>
          </View>
        </View>
        
        {/* Document Info */}
        <View style={styles.infoSection}>
          <Text style={styles.documentTitle}>{document.title}</Text>
          
          <View style={styles.metaRow}>
            <View style={styles.metaItem}>
              <Text style={styles.metaLabel}>Size</Text>
              <Text style={styles.metaValue}>{formatFileSize(document.fileSize)}</Text>
            </View>
            <View style={styles.metaItem}>
              <Text style={styles.metaLabel}>Created</Text>
              <Text style={styles.metaValue}>{formatDate(document.createdAt)}</Text>
            </View>
            <View style={styles.metaItem}>
              <Text style={styles.metaLabel}>Modified</Text>
              <Text style={styles.metaValue}>{formatDate(document.updatedAt)}</Text>
            </View>
          </View>
        </View>
        
        {/* Quick Actions */}
        <View style={styles.actionsSection}>
          <Text style={styles.sectionTitle}>Actions</Text>
          
          <View style={styles.actionsGrid}>
            <TouchableOpacity style={styles.actionCard} onPress={handleViewPDF}>
              <Text style={styles.actionIcon}>👁️</Text>
              <Text style={styles.actionLabel}>View</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleEdit}>
              <Text style={styles.actionIcon}>✏️</Text>
              <Text style={styles.actionLabel}>Edit</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleShare}>
              <Text style={styles.actionIcon}>📤</Text>
              <Text style={styles.actionLabel}>Share</Text>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard} onPress={handleExport}>
              <Text style={styles.actionIcon}>📥</Text>
              <Text style={styles.actionLabel}>Export</Text>
            </TouchableOpacity>
          </View>
        </View>
        
        {/* More Options */}
        <View style={styles.optionsSection}>
          <Text style={styles.sectionTitle}>More Options</Text>
          
          <TouchableOpacity style={styles.optionRow} onPress={handleToggleFavorite}>
            <View style={styles.optionInfo}>
              <Text style={styles.optionIcon}>{document.isFavorite ? '⭐' : '☆'}</Text>
              <Text style={styles.optionLabel}>
                {document.isFavorite ? 'Remove from Favorites' : 'Add to Favorites'}
              </Text>
            </View>
          </TouchableOpacity>
          
          <TouchableOpacity style={styles.optionRow}>
            <View style={styles.optionInfo}>
              <Text style={styles.optionIcon}>🏷️</Text>
              <Text style={styles.optionLabel}>Add Tags</Text>
            </View>
          </TouchableOpacity>
          
          <TouchableOpacity style={styles.optionRow}>
            <View style={styles.optionInfo}>
              <Text style={styles.optionIcon}>📁</Text>
              <Text style={styles.optionLabel}>Move to Folder</Text>
            </View>
          </TouchableOpacity>
          
          <TouchableOpacity style={[styles.optionRow, styles.dangerOption]} onPress={handleDelete}>
            <View style={styles.optionInfo}>
              <Text style={styles.optionIcon}>🗑️</Text>
              <Text style={styles.optionLabelDanger}>Delete Document</Text>
            </View>
          </TouchableOpacity>
        </View>
      </ScrollView>
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
  content: {
    flex: 1,
  },
  previewContainer: {
    backgroundColor: COLORS.surface,
    paddingVertical: SPACING.xxl,
    alignItems: 'center',
  },
  previewPlaceholder: {
    width: 200,
    height: 280,
    backgroundColor: COLORS.background,
    borderRadius: RADIUS.lg,
    justifyContent: 'center',
    alignItems: 'center',
    ...SHADOWS.medium,
  },
  previewIcon: {
    fontSize: 64,
    marginBottom: SPACING.sm,
  },
  previewPageCount: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  infoSection: {
    padding: SPACING.lg,
  },
  documentTitle: {
    fontSize: FONT_SIZES.xxl,
    fontWeight: '700',
    color: COLORS.textPrimary,
    marginBottom: SPACING.md,
  },
  metaRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  metaItem: {
    alignItems: 'center',
  },
  metaLabel: {
    fontSize: FONT_SIZES.xs,
    color: COLORS.textTertiary,
    marginBottom: 2,
  },
  metaValue: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textPrimary,
    fontWeight: '500',
  },
  actionsSection: {
    padding: SPACING.lg,
  },
  sectionTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.md,
  },
  actionsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  actionCard: {
    width: '48%',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    alignItems: 'center',
    marginBottom: SPACING.md,
    ...SHADOWS.small,
  },
  actionIcon: {
    fontSize: 28,
    marginBottom: SPACING.xs,
  },
  actionLabel: {
    fontSize: FONT_SIZES.md,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  optionsSection: {
    padding: SPACING.lg,
    paddingBottom: SPACING.xxxl,
  },
  optionRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    marginBottom: SPACING.sm,
    ...SHADOWS.small,
  },
  dangerOption: {
    borderWidth: 1,
    borderColor: COLORS.error,
  },
  optionInfo: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  optionIcon: {
    fontSize: 20,
    marginRight: SPACING.md,
  },
  optionLabel: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textPrimary,
  },
  optionLabelDanger: {
    fontSize: FONT_SIZES.md,
    color: COLORS.error,
  },
  errorText: {
    fontSize: FONT_SIZES.lg,
    color: COLORS.error,
    textAlign: 'center',
    marginTop: SPACING.xxxl,
  },
});
