/**
 * Scan Screen - Scan Entry Point
 * 
 * Provides options to start scanning in different modes
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootStackParamList } from '../types';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;

export const ScanScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const navigation = useNavigation<NavigationProp>();
  
  // Handle single page scan
  const handleSingleScan = () => {
    navigation.navigate('Scanner', { mode: 'single' });
  };
  
  // Handle batch scan
  const handleBatchScan = () => {
    navigation.navigate('Scanner', { mode: 'batch' });
  };
  
  // Handle import from gallery
  const handleImport = () => {
    // TODO: Implement gallery import
    navigation.navigate('Scanner', { mode: 'single' });
  };
  
  // Handle document import
  const handleImportPDF = () => {
    // TODO: Implement PDF import
    navigation.navigate('Scanner', { mode: 'single' });
  };
  
  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Scan</Text>
        <Text style={styles.headerSubtitle}>Choose your scan method</Text>
      </View>
      
      <ScrollView 
        style={styles.content}
        showsVerticalScrollIndicator={false}
      >
        {/* Main Scan Options */}
        <View style={styles.section}>
          <TouchableOpacity
            style={styles.mainScanButton}
            onPress={handleSingleScan}
            activeOpacity={0.8}
          >
            <View style={styles.scanIconContainer}>
              <Text style={styles.scanIcon}>📷</Text>
            </View>
            <View style={styles.scanInfo}>
              <Text style={styles.scanTitle}>Single Page</Text>
              <Text style={styles.scanDescription}>
                Scan one page at a time with automatic edge detection
              </Text>
            </View>
            <Text style={styles.scanArrow}>→</Text>
          </TouchableOpacity>
          
          <TouchableOpacity
            style={styles.mainScanButton}
            onPress={handleBatchScan}
            activeOpacity={0.8}
          >
            <View style={[styles.scanIconContainer, { backgroundColor: COLORS.secondary }]}>
              <Text style={styles.scanIcon}>📑</Text>
            </View>
            <View style={styles.scanInfo}>
              <Text style={styles.scanTitle}>Batch Scan</Text>
              <Text style={styles.scanDescription}>
                Scan multiple pages continuously and save as one PDF
              </Text>
            </View>
            <Text style={styles.scanArrow}>→</Text>
          </TouchableOpacity>
        </View>
        
        {/* Import Options */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Import</Text>
          
          <TouchableOpacity
            style={styles.importButton}
            onPress={handleImport}
            activeOpacity={0.7}
          >
            <View style={[styles.importIconContainer, { backgroundColor: COLORS.accent }]}>
              <Text style={styles.importIcon}>🖼️</Text>
            </View>
            <View style={styles.importInfo}>
              <Text style={styles.importTitle}>From Gallery</Text>
              <Text style={styles.importDescription}>
                Import existing photos
              </Text>
            </View>
          </TouchableOpacity>
          
          <TouchableOpacity
            style={styles.importButton}
            onPress={handleImportPDF}
            activeOpacity={0.7}
          >
            <View style={[styles.importIconContainer, { backgroundColor: '#8B5CF6' }]}>
              <Text style={styles.importIcon}>📄</Text>
            </View>
            <View style={styles.importInfo}>
              <Text style={styles.importTitle}>Import PDF</Text>
              <Text style={styles.importDescription}>
                Add pages to existing PDF
              </Text>
            </View>
          </TouchableOpacity>
        </View>
        
        {/* Features Preview */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Features</Text>
          
          <View style={styles.featuresGrid}>
            <View style={styles.featureCard}>
              <Text style={styles.featureIcon}>✨</Text>
              <Text style={styles.featureTitle}>Auto Enhance</Text>
              <Text style={styles.featureDescription}>
                Automatic brightness & contrast optimization
              </Text>
            </View>
            
            <View style={styles.featureCard}>
              <Text style={styles.featureIcon}>📝</Text>
              <Text style={styles.featureTitle}>OCR</Text>
              <Text style={styles.featureDescription}>
                Extract text from scanned documents
              </Text>
            </View>
            
            <View style={styles.featureCard}>
              <Text style={styles.featureIcon}>🔍</Text>
              <Text style={styles.featureTitle}>Filters</Text>
              <Text style={styles.featureDescription}>
                Document, B&W, Magic Color and more
              </Text>
            </View>
            
            <View style={styles.featureCard}>
              <Text style={styles.featureIcon}>📤</Text>
              <Text style={styles.featureTitle}>Export</Text>
              <Text style={styles.featureDescription}>
                PDF, JPG, PNG, DOCX and more
              </Text>
            </View>
          </View>
        </View>
        
        {/* Tips */}
        <View style={[styles.section, styles.tipsSection]}>
          <View style={styles.tipCard}>
            <Text style={styles.tipIcon}>💡</Text>
            <View style={styles.tipContent}>
              <Text style={styles.tipTitle}>Pro Tip</Text>
              <Text style={styles.tipText}>
                Use batch scanning for multi-page documents like contracts, receipts, or notes. It's faster and keeps all pages organized in one PDF.
              </Text>
            </View>
          </View>
        </View>
      </ScrollView>
    </View>
  );
};

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
  content: {
    flex: 1,
  },
  section: {
    paddingHorizontal: SPACING.lg,
    marginBottom: SPACING.xl,
  },
  sectionTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: SPACING.md,
  },
  mainScanButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    marginBottom: SPACING.md,
    ...SHADOWS.medium,
  },
  scanIconContainer: {
    width: 56,
    height: 56,
    borderRadius: RADIUS.lg,
    backgroundColor: COLORS.primary,
    justifyContent: 'center',
    alignItems: 'center',
  },
  scanIcon: {
    fontSize: 24,
  },
  scanInfo: {
    flex: 1,
    marginLeft: SPACING.md,
  },
  scanTitle: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: 2,
  },
  scanDescription: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    lineHeight: 18,
  },
  scanArrow: {
    fontSize: 24,
    color: COLORS.textTertiary,
  },
  importButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.md,
    marginBottom: SPACING.sm,
    ...SHADOWS.small,
  },
  importIconContainer: {
    width: 44,
    height: 44,
    borderRadius: RADIUS.md,
    justifyContent: 'center',
    alignItems: 'center',
  },
  importIcon: {
    fontSize: 20,
  },
  importInfo: {
    flex: 1,
    marginLeft: SPACING.md,
  },
  importTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
  },
  importDescription: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
  },
  featuresGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  featureCard: {
    width: '48%',
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.md,
    marginBottom: SPACING.md,
    ...SHADOWS.small,
  },
  featureIcon: {
    fontSize: 24,
    marginBottom: SPACING.xs,
  },
  featureTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textPrimary,
    marginBottom: 2,
  },
  featureDescription: {
    fontSize: FONT_SIZES.xs,
    color: COLORS.textSecondary,
    lineHeight: 16,
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
