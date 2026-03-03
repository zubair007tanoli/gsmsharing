/**
 * Editor Screen - Image Enhancement & Editing
 * 
 * Provides tools for enhancing and editing scanned images
 */

import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Image,
  ScrollView,
  Dimensions,
} from 'react-native';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useDispatch, useSelector } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootStackParamList, EnhancementSettings } from '../types';
import { RootState } from '../store';
import { setPageEnhancements, applyFilter, rotatePage, removePage } from '../store/slices/scanSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;
type EditorRouteProp = RouteProp<RootStackParamList, 'Editor'>;

const { width } = Dimensions.get('window');

type FilterOption = {
  id: EnhancementSettings['filter'];
  label: string;
  icon: string;
};

const FILTERS: FilterOption[] = [
  { id: 'none', label: 'Original', icon: '🖼️' },
  { id: 'document', label: 'Document', icon: '📄' },
  { id: 'photo', label: 'Photo', icon: '📷' },
  { id: 'grayscale', label: 'Gray', icon: '⬛' },
  { id: 'blackWhite', label: 'B&W', icon: '⚫' },
  { id: 'magicColor', label: 'Magic', icon: '✨' },
];

export const EditorScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>();
  const route = useRoute<EditorRouteProp>();
  const dispatch = useDispatch();
  
  const { pageId } = route.params;
  
  const { currentSession } = useSelector((state: RootState) => state.scan);
  const page = currentSession?.pages.find(p => p.id === pageId);
  
  const [activeTool, setActiveTool] = useState<'filters' | 'adjust' | 'crop'>('filters');
  const [currentFilter, setCurrentFilter] = useState<EnhancementSettings['filter']>(
    page?.enhancements.filter || 'none'
  );
  
  // Handle filter selection
  const handleFilterSelect = (filterId: EnhancementSettings['filter']) => {
    setCurrentFilter(filterId);
    if (pageId) {
      dispatch(applyFilter({ pageId, filter: filterId }));
    }
  };
  
  // Handle rotation
  const handleRotate = () => {
    if (pageId) {
      dispatch(rotatePage({ pageId, degrees: 90 }));
    }
  };
  
  // Handle delete page
  const handleDelete = () => {
    if (pageId) {
      dispatch(removePage(pageId));
      navigation.goBack();
    }
  };
  
  // Handle save
  const handleSave = () => {
    // TODO: Save enhancements and navigate
    navigation.goBack();
  };
  
  if (!page) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorText}>Page not found</Text>
      </View>
    );
  }
  
  return (
    <View style={styles.container}>
      {/* Preview Area */}
      <View style={styles.previewContainer}>
        <Image
          source={{ uri: page.imageUrl }}
          style={styles.previewImage}
          resizeMode="contain"
        />
      </View>
      
      {/* Tools Tabs */}
      <View style={styles.toolsTabs}>
        <TouchableOpacity
          style={[styles.toolTab, activeTool === 'filters' && styles.toolTabActive]}
          onPress={() => setActiveTool('filters')}
        >
          <Text style={[styles.toolTabText, activeTool === 'filters' && styles.toolTabTextActive]}>
            Filters
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.toolTab, activeTool === 'adjust' && styles.toolTabActive]}
          onPress={() => setActiveTool('adjust')}
        >
          <Text style={[styles.toolTabText, activeTool === 'adjust' && styles.toolTabTextActive]}>
            Adjust
          </Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={[styles.toolTab, activeTool === 'crop' && styles.toolTabActive]}
          onPress={() => setActiveTool('crop')}
        >
          <Text style={[styles.toolTabText, activeTool === 'crop' && styles.toolTabTextActive]}>
            Crop
          </Text>
        </TouchableOpacity>
      </View>
      
      {/* Tools Content */}
      <View style={styles.toolsContent}>
        {activeTool === 'filters' && (
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            <View style={styles.filtersRow}>
              {FILTERS.map((filter) => (
                <TouchableOpacity
                  key={filter.id}
                  style={[
                    styles.filterButton,
                    currentFilter === filter.id && styles.filterButtonActive,
                  ]}
                  onPress={() => handleFilterSelect(filter.id)}
                >
                  <Text style={styles.filterIcon}>{filter.icon}</Text>
                  <Text style={[
                    styles.filterLabel,
                    currentFilter === filter.id && styles.filterLabelActive,
                  ]}>
                    {filter.label}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </ScrollView>
        )}
        
        {activeTool === 'adjust' && (
          <View style={styles.adjustContainer}>
            <Text style={styles.adjustPlaceholder}>
              Adjustment sliders would go here
            </Text>
            <View style={styles.adjustSliders}>
              <View style={styles.sliderRow}>
                <Text style={styles.sliderLabel}>Brightness</Text>
                <Text style={styles.sliderValue}>0</Text>
              </View>
              <View style={styles.sliderRow}>
                <Text style={styles.sliderLabel}>Contrast</Text>
                <Text style={styles.sliderValue}>0</Text>
              </View>
              <View style={styles.sliderRow}>
                <Text style={styles.sliderLabel}>Saturation</Text>
                <Text style={styles.sliderValue}>0</Text>
              </View>
            </View>
          </View>
        )}
        
        {activeTool === 'crop' && (
          <View style={styles.cropContainer}>
            <Text style={styles.cropPlaceholder}>Crop & Rotate</Text>
            <View style={styles.cropActions}>
              <TouchableOpacity style={styles.cropButton} onPress={handleRotate}>
                <Text style={styles.cropButtonIcon}>↻</Text>
                <Text style={styles.cropButtonText}>Rotate</Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.cropButton}>
                <Text style={styles.cropButtonIcon}>⛶</Text>
                <Text style={styles.cropButtonText}>Crop</Text>
              </TouchableOpacity>
            </View>
          </View>
        )}
      </View>
      
      {/* Bottom Actions */}
      <View style={styles.bottomActions}>
        <TouchableOpacity style={styles.deleteButton} onPress={handleDelete}>
          <Text style={styles.deleteIcon}>🗑️</Text>
        </TouchableOpacity>
        
        <TouchableOpacity style={styles.saveButton} onPress={handleSave}>
          <Text style={styles.saveButtonText}>Save</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.black,
  },
  previewContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  previewImage: {
    width: width,
    height: width * 1.4,
  },
  toolsTabs: {
    flexDirection: 'row',
    backgroundColor: COLORS.surface,
    borderTopLeftRadius: RADIUS.xl,
    borderTopRightRadius: RADIUS.xl,
  },
  toolTab: {
    flex: 1,
    paddingVertical: SPACING.md,
    alignItems: 'center',
  },
  toolTabActive: {
    borderBottomWidth: 2,
    borderBottomColor: COLORS.primary,
  },
  toolTabText: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
    fontWeight: '500',
  },
  toolTabTextActive: {
    color: COLORS.primary,
    fontWeight: '600',
  },
  toolsContent: {
    backgroundColor: COLORS.surface,
    paddingVertical: SPACING.md,
    minHeight: 100,
  },
  filtersRow: {
    flexDirection: 'row',
    paddingHorizontal: SPACING.md,
    gap: SPACING.sm,
  },
  filterButton: {
    alignItems: 'center',
    padding: SPACING.sm,
    borderRadius: RADIUS.md,
    minWidth: 70,
    backgroundColor: COLORS.background,
  },
  filterButtonActive: {
    backgroundColor: COLORS.primaryLight + '20',
    borderWidth: 1,
    borderColor: COLORS.primary,
  },
  filterIcon: {
    fontSize: 24,
    marginBottom: SPACING.xs,
  },
  filterLabel: {
    fontSize: FONT_SIZES.xs,
    color: COLORS.textSecondary,
  },
  filterLabelActive: {
    color: COLORS.primary,
    fontWeight: '600',
  },
  adjustContainer: {
    paddingHorizontal: SPACING.lg,
  },
  adjustPlaceholder: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textTertiary,
    textAlign: 'center',
    marginBottom: SPACING.md,
  },
  adjustSliders: {
    gap: SPACING.sm,
  },
  sliderRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  sliderLabel: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textPrimary,
  },
  sliderValue: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textSecondary,
  },
  cropContainer: {
    alignItems: 'center',
    paddingHorizontal: SPACING.lg,
  },
  cropPlaceholder: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textTertiary,
    marginBottom: SPACING.md,
  },
  cropActions: {
    flexDirection: 'row',
    gap: SPACING.lg,
  },
  cropButton: {
    alignItems: 'center',
    padding: SPACING.md,
  },
  cropButtonIcon: {
    fontSize: 24,
    marginBottom: SPACING.xs,
  },
  cropButtonText: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textPrimary,
  },
  bottomActions: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: SPACING.lg,
    paddingVertical: SPACING.md,
    backgroundColor: COLORS.surface,
    borderTopWidth: 1,
    borderTopColor: COLORS.border,
  },
  deleteButton: {
    width: 48,
    height: 48,
    borderRadius: RADIUS.md,
    backgroundColor: COLORS.background,
    justifyContent: 'center',
    alignItems: 'center',
  },
  deleteIcon: {
    fontSize: 20,
  },
  saveButton: {
    flex: 1,
    marginLeft: SPACING.md,
    backgroundColor: COLORS.primary,
    paddingVertical: SPACING.md,
    borderRadius: RADIUS.lg,
    alignItems: 'center',
  },
  saveButtonText: {
    fontSize: FONT_SIZES.lg,
    fontWeight: '600',
    color: COLORS.white,
  },
  errorText: {
    fontSize: FONT_SIZES.lg,
    color: COLORS.error,
    textAlign: 'center',
    marginTop: SPACING.xxxl,
  },
});
