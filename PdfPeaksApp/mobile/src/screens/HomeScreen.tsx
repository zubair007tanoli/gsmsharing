/**
 * Home Screen - Dashboard for PDFPeaksApp
 * 
 * Displays recent documents, quick actions, and storage info
 */

import React, { useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Image,
  FlatList,
  RefreshControl,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootState } from '../store';
import { RootStackParamList, Document } from '../types';
import { fetchDocuments } from '../store/slices/documentSlice';

// Navigation type
type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

export const HomeScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const navigation = useNavigation<NavigationProp>();
  const dispatch = useDispatch();
  
  // Redux state
  const documents = useSelector((state: RootState) => state.documents.items);
  const isLoading = useSelector((state: RootState) => state.documents.isLoading);
  const storageInfo = useSelector((state: RootState) => state.settings.storageInfo);
  
  // Get recent documents (last 6)
  const recentDocuments = documents.slice(0, 6);
  
  // Handle refresh
  const onRefresh = useCallback(() => {
    dispatch(fetchDocuments());
  }, [dispatch]);
  
  // Navigate to scanner
  const handleScanPress = () => {
    navigation.navigate('Scanner', { mode: 'single' });
  };
  
  // Navigate to batch scan
  const handleBatchScanPress = () => {
    navigation.navigate('Scanner', { mode: 'batch' });
  };
  
  // Navigate to document detail
  const handleDocumentPress = (document: Document) => {
    navigation.navigate('DocumentDetail', { documentId: document.id });
  };
  
  // Navigate to PDF viewer
  const handleDocumentOpen = (document: Document) => {
    navigation.navigate('PDFViewer', { documentId: document.id });
  };
  
  // Render document card
  const renderDocumentCard = ({ item }: { item: Document }) => (
    <TouchableOpacity
      style={styles.documentCard}
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
      <View style={styles.documentInfo}>
        <Text style={styles.documentTitle} numberOfLines={2}>
          {item.title}
        </Text>
        <Text style={styles.documentMeta}>
          {item.pageCount} page{item.pageCount !== 1 ? 's' : ''} • {formatFileSize(item.fileSize)}
        </Text>
        <Text style={styles.documentDate}>
          {formatDate(item.updatedAt)}
        </Text>
      </View>
    </TouchableOpacity>
  );
  
  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Header */}
      <View style={styles.header}>
        <View>
          <Text style={styles.greeting}>Welcome to</Text>
          <Text style={styles.appName}>PDFPeaksApp</Text>
        </View>
        <TouchableOpacity style={styles.searchButton}>
          <Text style={styles.searchIcon}>🔍</Text>
        </TouchableOpacity>
      </View>
      
      <ScrollView
        style={styles.content}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl
            refreshing={isLoading}
            onRefresh={onRefresh}
            tintColor={COLORS.primary}
          />
        }
      >
        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Quick Actions</Text>
          <View style={styles.actionButtons}>
            <TouchableOpacity
              style={styles.actionButton}
              onPress={handleScanPress}
              activeOpacity={0.8}
            >
              <View style={[styles.actionIcon, { backgroundColor: COLORS.primary }]}>
                <Text style={styles.actionIconText}>📷</Text>
              </View>
              <Text style={styles.actionLabel}>Scan</Text>
            </TouchableOpacity>
            
            <TouchableOpacity
              style={styles.actionButton}
              onPress={handleBatchScanPress}
              activeOpacity={0.8}
            >
              <View style={[styles.actionIcon, { backgroundColor: COLORS.secondary }]}>
                <Text style={styles.actionIconText}>📑</Text>
              </View>
              <Text style={styles.actionLabel}>Batch Scan</Text>
            </TouchableOpacity>
            
            <TouchableOpacity
              style={styles.actionButton}
              activeOpacity={0.8}
            >
              <View style={[styles.actionIcon, { backgroundColor: COLORS.accent }]}>
                <Text style={styles.actionIconText}>📥</Text>
              </View>
              <Text style={styles.actionLabel}>Import</Text>
            </TouchableOpacity>
            
            <TouchableOpacity
              style={styles.actionButton}
              activeOpacity={0.8}
            >
              <View style={[styles.actionIcon, { backgroundColor: '#8B5CF6' }]}>
                <Text style={styles.actionIconText}>☁️</Text>
              </View>
              <Text style={styles.actionLabel}>Cloud</Text>
            </TouchableOpacity>
          </View>
        </View>
        
        {/* Storage Info */}
        <View style={styles.section}>
          <View style={styles.storageCard}>
            <View style={styles.storageInfo}>
              <Text style={styles.storageTitle}>Storage</Text>
              <Text style={styles.storageText}>
                {formatFileSize(storageInfo.used)} / {formatFileSize(storageInfo.total)}
              </Text>
            </View>
            <View style={styles.storageBar}>
              <View
                style={[
                  styles.storageProgress,
                  { width: `${storageInfo.percentUsed}%` },
                ]}
              />
            </View>
          </View>
        </View>
        
        {/* Recent Documents */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Recent Documents</Text>
            <TouchableOpacity>
              <Text style={styles.seeAllText}>See All</Text>
            </TouchableOpacity>
          </View>
          
          {recentDocuments.length > 0 ? (
            <FlatList
              data={recentDocuments}
              renderItem={renderDocumentCard}
              keyExtractor={(item) => item.id}
              horizontal
              showsHorizontalScrollIndicator={false}
              contentContainerStyle={styles.documentList}
            />
          ) : (
            <View style={styles.emptyState}>
              <Text style={styles.emptyIcon}>📭</Text>
              <Text style={styles.emptyText}>No documents yet</Text>
              <Text style={styles.emptySubtext}>
                Tap "Scan" to create your first document
              </Text>
            </View>
          )}
        </View>
        
        {/* Tips Section */}
        <View style={[styles.section, styles.tipsSection]}>
          <Text style={styles.sectionTitle}>Tips</Text>
          <View style={styles.tipCard}>
            <Text style={styles.tipIcon}>💡</Text>
            <View style={styles.tipContent}>
              <Text style={styles.tipTitle}>Pro Tip</Text>
              <Text style={styles.tipText}>
                Use batch scanning to capture multiple pages at once and save them as a single PDF.
              </Text>
            </View>
          </View>
        </View>
      </ScrollView>
    </View>
  );
};

// Helper functions
function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function formatDate(date: Date | string): string {
  const d = new Date(date);
  const now = new Date();
  const diff = now.getTime() - d.getTime();
  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  
  if (days === 0) return 'Today';
  if (days === 1) return 'Yesterday';
  if (days < 7) return `${days} days ago`;
  
  return d.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
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
  greeting: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  appName: {
    fontSize: FONT_SIZES.xxl,
    fontWeight: '700',
    color: COLORS.textPrimary,
  },
  searchButton: {
    width: 44,
    height: 44,
    borderRadius: RADIUS.full,
    backgroundColor: COLORS.surface,
    justifyContent: 'center',
    alignItems: 'center',
    ...SHADOWS.small,
  },
  searchIcon: {
    fontSize: 18,
  },
  content: {
    flex: 1,
  },
  section: {
    paddingHorizontal: SPACING.lg,
    marginBottom: SPACING.xl,
  },
  sectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: SPACING.md,
  },
  sectionTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.md,
  },
  seeAllText: {
    fontSize: FONT_SIZES.md,
    color: COLORS.primary,
    fontWeight: '500',
  },
  actionButtons: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  actionButton: {
    alignItems: 'center',
    width: 72,
  },
  actionIcon: {
    width: 56,
    height: 56,
    borderRadius: RADIUS.lg,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: SPACING.xs,
  },
  actionIconText: {
    fontSize: 24,
  },
  actionLabel: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    fontWeight: '500',
  },
  storageCard: {
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    ...SHADOWS.small,
  },
  storageInfo: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: SPACING.sm,
  },
  storageTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  storageText: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  storageBar: {
    height: 8,
    backgroundColor: COLORS.border,
    borderRadius: RADIUS.full,
    overflow: 'hidden',
  },
  storageProgress: {
    height: '100%',
    backgroundColor: COLORS.primary,
    borderRadius: RADIUS.full,
  },
  documentList: {
    paddingRight: SPACING.lg,
  },
  documentCard: {
    width: 140,
    marginRight: SPACING.md,
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    overflow: 'hidden',
    ...SHADOWS.small,
  },
  documentThumbnail: {
    width: '100%',
    height: 140,
    backgroundColor: COLORS.border,
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
    fontSize: 40,
  },
  documentInfo: {
    padding: SPACING.sm,
  },
  documentTitle: {
    fontSize: FONT_SIZES.sm,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: 2,
  },
  documentMeta: {
    fontSize: FONT_SIZES.xs,
    color: COLORS.textSecondary,
  },
  documentDate: {
    fontSize: FONT_SIZES.xs,
    color: COLORS.textTertiary,
    marginTop: 2,
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
  tipsSection: {
    paddingBottom: SPACING.xxxl,
  },
  tipCard: {
    flexDirection: 'row',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    ...SHADOWS.small,
  },
  tipIcon: {
    fontSize: 24,
    marginRight: SPACING.md,
  },
  tipContent: {
    flex: 1,
  },
  tipTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.xs,
  },
  tipText: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    lineHeight: 20,
  },
});
