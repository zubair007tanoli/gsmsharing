/**
 * PDF Viewer Screen - Document Viewing
 * 
 * Displays PDF documents with zoom and navigation
 */

import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Dimensions,
} from 'react-native';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useSelector } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES } from '../constants/colors';
import { RootStackParamList } from '../types';
import { RootState } from '../store';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;
type PDFViewerRouteProp = RouteProp<RootStackParamList, 'PDFViewer'>;

const { width, height } = Dimensions.get('window');

export const PDFViewerScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>();
  const route = useRoute<PDFViewerRouteProp>();
  
  const { documentId } = route.params;
  
  const document = useSelector((state: RootState) =>
    state.documents.items.find(doc => doc.id === documentId)
  );
  
  const [currentPage, setCurrentPage] = useState(1);
  const [zoomLevel, setZoomLevel] = useState(1);
  
  const totalPages = document?.pageCount || 1;
  
  // Handle close
  const handleClose = () => {
    navigation.goBack();
  };
  
  // Handle page navigation
  const goToPrevPage = () => {
    if (currentPage > 1) {
      setCurrentPage(currentPage - 1);
    }
  };
  
  const goToNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(currentPage + 1);
    }
  };
  
  // Handle zoom
  const handleZoomIn = () => {
    setZoomLevel(Math.min(zoomLevel + 0.5, 3));
  };
  
  const handleZoomOut = () => {
    setZoomLevel(Math.max(zoomLevel - 0.5, 1));
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
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.headerButton} onPress={handleClose}>
          <Text style={styles.headerButtonText}>✕</Text>
        </TouchableOpacity>
        
        <View style={styles.headerCenter}>
          <Text style={styles.headerTitle} numberOfLines={1}>
            {document.title}
          </Text>
          <Text style={styles.headerSubtitle}>
            Page {currentPage} of {totalPages}
          </Text>
        </View>
        
        <TouchableOpacity style={styles.headerButton}>
          <Text style={styles.headerButtonText}>⋮</Text>
        </TouchableOpacity>
      </View>
      
      {/* PDF Content Area */}
      <View style={styles.pdfContainer}>
        <View style={styles.pdfPlaceholder}>
          <Text style={styles.pdfPlaceholderIcon}>📄</Text>
          <Text style={styles.pdfPlaceholderText}>
            PDF Page {currentPage}
          </Text>
          <Text style={styles.pdfPlaceholderSubtext}>
            {document.title}
          </Text>
        </View>
      </View>
      
      {/* Bottom Controls */}
      <View style={styles.bottomControls}>
        {/* Zoom Controls */}
        <View style={styles.zoomControls}>
          <TouchableOpacity
            style={[styles.zoomButton, zoomLevel <= 1 && styles.zoomButtonDisabled]}
            onPress={handleZoomOut}
            disabled={zoomLevel <= 1}
          >
            <Text style={styles.zoomButtonText}>−</Text>
          </TouchableOpacity>
          
          <Text style={styles.zoomLevel}>{Math.round(zoomLevel * 100)}%</Text>
          
          <TouchableOpacity
            style={[styles.zoomButton, zoomLevel >= 3 && styles.zoomButtonDisabled]}
            onPress={handleZoomIn}
            disabled={zoomLevel >= 3}
          >
            <Text style={styles.zoomButtonText}>+</Text>
          </TouchableOpacity>
        </View>
        
        {/* Page Navigation */}
        <View style={styles.pageNavigation}>
          <TouchableOpacity
            style={[styles.navButton, currentPage <= 1 && styles.navButtonDisabled]}
            onPress={goToPrevPage}
            disabled={currentPage <= 1}
          >
            <Text style={styles.navButtonText}>←</Text>
          </TouchableOpacity>
          
          <View style={styles.pageIndicator}>
            <Text style={styles.pageIndicatorText}>
              {currentPage} / {totalPages}
            </Text>
          </View>
          
          <TouchableOpacity
            style={[styles.navButton, currentPage >= totalPages && styles.navButtonDisabled]}
            onPress={goToNextPage}
            disabled={currentPage >= totalPages}
          >
            <Text style={styles.navButtonText}>→</Text>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: SPACING.md,
    paddingVertical: SPACING.md,
    backgroundColor: COLORS.surface,
    borderBottomWidth: 1,
    borderBottomColor: COLORS.border,
  },
  headerButton: {
    width: 40,
    height: 40,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  headerButtonText: {
    fontSize: 18,
    color: COLORS.textPrimary,
  },
  headerCenter: {
    flex: 1,
    alignItems: 'center',
    paddingHorizontal: SPACING.sm,
  },
  headerTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  headerSubtitle: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  pdfContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.border,
  },
  pdfPlaceholder: {
    alignItems: 'center',
    padding: SPACING.xl,
  },
  pdfPlaceholderIcon: {
    fontSize: 64,
    marginBottom: SPACING.md,
  },
  pdfPlaceholderText: {
    fontSize: FONT_SIZES.xl,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.xs,
  },
  pdfPlaceholderSubtext: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  bottomControls: {
    backgroundColor: COLORS.surface,
    paddingHorizontal: SPACING.lg,
    paddingVertical: SPACING.md,
    borderTopWidth: 1,
    borderTopColor: COLORS.border,
  },
  zoomControls: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: SPACING.md,
  },
  zoomButton: {
    width: 36,
    height: 36,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  zoomButtonDisabled: {
    opacity: 0.4,
  },
  zoomButtonText: {
    fontSize: 20,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  zoomLevel: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
    marginHorizontal: SPACING.md,
    minWidth: 50,
    textAlign: 'center',
  },
  pageNavigation: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  navButton: {
    width: 44,
    height: 44,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.primary,
    justifyContent: 'center',
    alignItems: 'center',
  },
  navButtonDisabled: {
    backgroundColor: COLORS.border,
  },
  navButtonText: {
    fontSize: 18,
    fontWeight: '600',
    color: COLORS.white,
  },
  pageIndicator: {
    paddingHorizontal: SPACING.lg,
  },
  pageIndicatorText: {
    fontSize: FONT_SIZES.md,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  errorText: {
    fontSize: FONT_SIZES.lg,
    color: COLORS.error,
    textAlign: 'center',
    marginTop: SPACING.xxxl,
  },
});
