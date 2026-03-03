/**
 * Settings Screen - App Configuration
 * 
 * User preferences and app settings
 */

import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Switch,
  Alert,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useSelector, useDispatch } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootState } from '../store';
import {
  toggleAutoEnhance,
  toggleCloudSync,
  toggleBiometricAuth,
  toggleDarkMode,
  setDefaultExportFormat,
  setDefaultPageSize,
  setDefaultOCRLanguage,
  resetSettings,
} from '../store/slices/settingsSlice';

export const SettingsScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const dispatch = useDispatch();
  
  const preferences = useSelector((state: RootState) => state.settings.preferences);
  const storageInfo = useSelector((state: RootState) => state.settings.storageInfo);
  
  // Handle export format change
  const handleExportFormatChange = () => {
    const formats: Array<'pdf' | 'jpeg' | 'png'> = ['pdf', 'jpeg', 'png'];
    const currentIndex = formats.indexOf(preferences.defaultExportFormat);
    const nextIndex = (currentIndex + 1) % formats.length;
    dispatch(setDefaultExportFormat(formats[nextIndex]));
  };
  
  // Handle page size change
  const handlePageSizeChange = () => {
    const sizes: Array<'letter' | 'a4' | 'legal'> = ['letter', 'a4', 'legal'];
    const currentIndex = sizes.indexOf(preferences.defaultPageSize);
    const nextIndex = (currentIndex + 1) % sizes.length;
    dispatch(setDefaultPageSize(sizes[nextIndex]));
  };
  
  // Handle reset settings
  const handleResetSettings = () => {
    Alert.alert(
      'Reset Settings',
      'Are you sure you want to reset all settings to default?',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Reset', style: 'destructive', onPress: () => dispatch(resetSettings()) },
      ]
    );
  };
  
  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Settings</Text>
      </View>
      
      <ScrollView 
        style={styles.content}
        showsVerticalScrollIndicator={false}
      >
        {/* Storage Section */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Storage</Text>
          <View style={styles.card}>
            <View style={styles.storageInfo}>
              <Text style={styles.storageLabel}>Used Space</Text>
              <Text style={styles.storageValue}>
                {formatFileSize(storageInfo.used)} / {formatFileSize(storageInfo.total)}
              </Text>
            </View>
            <View style={styles.storageBar}>
              <View
                style={[
                  styles.storageProgress,
                  { width: `${Math.min(storageInfo.percentUsed, 100)}%` },
                ]}
              />
            </View>
          </View>
        </View>
        
        {/* Scan Settings */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Scan Settings</Text>
          <View style={styles.card}>
            <View style={styles.settingRow}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Auto Enhance</Text>
                <Text style={styles.settingDescription}>
                  Automatically optimize scanned images
                </Text>
              </View>
              <Switch
                value={preferences.autoEnhanceOnScan}
                onValueChange={() => dispatch(toggleAutoEnhance())}
                trackColor={{ false: COLORS.border, true: COLORS.primaryLight }}
                thumbColor={preferences.autoEnhanceOnScan ? COLORS.primary : COLORS.surface}
              />
            </View>
            
            <View style={styles.divider} />
            
            <TouchableOpacity style={styles.settingRow} onPress={handlePageSizeChange}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Default Page Size</Text>
                <Text style={styles.settingDescription}>
                  Default size for new PDFs
                </Text>
              </View>
              <Text style={styles.settingValue}>{preferences.defaultPageSize.toUpperCase()}</Text>
            </TouchableOpacity>
          </View>
        </View>
        
        {/* Export Settings */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Export Settings</Text>
          <View style={styles.card}>
            <TouchableOpacity style={styles.settingRow} onPress={handleExportFormatChange}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Default Format</Text>
                <Text style={styles.settingDescription}>
                  Default export format
                </Text>
              </View>
              <Text style={styles.settingValue}>{preferences.defaultExportFormat.toUpperCase()}</Text>
            </TouchableOpacity>
            
            <View style={styles.divider} />
            
            <TouchableOpacity style={styles.settingRow}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>OCR Language</Text>
                <Text style={styles.settingDescription}>
                  Language for text recognition
                </Text>
              </View>
              <Text style={styles.settingValue}>{preferences.defaultOCRLanguage.toUpperCase()}</Text>
            </TouchableOpacity>
          </View>
        </View>
        
        {/* Sync & Security */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Sync & Security</Text>
          <View style={styles.card}>
            <View style={styles.settingRow}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Cloud Sync</Text>
                <Text style={styles.settingDescription}>
                  Sync documents across devices
                </Text>
              </View>
              <Switch
                value={preferences.cloudSyncEnabled}
                onValueChange={() => dispatch(toggleCloudSync())}
                trackColor={{ false: COLORS.border, true: COLORS.primaryLight }}
                thumbColor={preferences.cloudSyncEnabled ? COLORS.primary : COLORS.surface}
              />
            </View>
            
            <View style={styles.divider} />
            
            <View style={styles.settingRow}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Biometric Auth</Text>
                <Text style={styles.settingDescription}>
                  Use Face ID or Touch ID
                </Text>
              </View>
              <Switch
                value={preferences.biometricAuthEnabled}
                onValueChange={() => dispatch(toggleBiometricAuth())}
                trackColor={{ false: COLORS.border, true: COLORS.primaryLight }}
                thumbColor={preferences.biometricAuthEnabled ? COLORS.primary : COLORS.surface}
              />
            </View>
          </View>
        </View>
        
        {/* Appearance */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Appearance</Text>
          <View style={styles.card}>
            <View style={styles.settingRow}>
              <View style={styles.settingInfo}>
                <Text style={styles.settingLabel}>Dark Mode</Text>
                <Text style={styles.settingDescription}>
                  Use dark color scheme
                </Text>
              </View>
              <Switch
                value={preferences.darkModeEnabled}
                onValueChange={() => dispatch(toggleDarkMode())}
                trackColor={{ false: COLORS.border, true: COLORS.primaryLight }}
                thumbColor={preferences.darkModeEnabled ? COLORS.primary : COLORS.surface}
              />
            </View>
          </View>
        </View>
        
        {/* About & Support */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>About</Text>
          <View style={styles.card}>
            <TouchableOpacity style={styles.settingRow}>
              <Text style={styles.settingLabel}>Privacy Policy</Text>
              <Text style={styles.chevron}>›</Text>
            </TouchableOpacity>
            
            <View style={styles.divider} />
            
            <TouchableOpacity style={styles.settingRow}>
              <Text style={styles.settingLabel}>Terms of Service</Text>
              <Text style={styles.chevron}>›</Text>
            </TouchableOpacity>
            
            <View style={styles.divider} />
            
            <TouchableOpacity style={styles.settingRow}>
              <Text style={styles.settingLabel}>Version</Text>
              <Text style={styles.settingValue}>1.0.0</Text>
            </TouchableOpacity>
          </View>
        </View>
        
        {/* Reset */}
        <View style={styles.section}>
          <TouchableOpacity
            style={styles.resetButton}
            onPress={handleResetSettings}
          >
            <Text style={styles.resetButtonText}>Reset to Defaults</Text>
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
  },
  content: {
    flex: 1,
  },
  section: {
    paddingHorizontal: SPACING.lg,
    marginBottom: SPACING.xl,
  },
  sectionTitle: {
    fontSize: FONT_SIZES.md,
    fontWeight: '600',
    color: COLORS.textSecondary,
    marginBottom: SPACING.sm,
    textTransform: 'uppercase',
    letterSpacing: 0.5,
  },
  card: {
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    ...SHADOWS.small,
  },
  settingRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: SPACING.lg,
  },
  settingInfo: {
    flex: 1,
    marginRight: SPACING.md,
  },
  settingLabel: {
    fontSize: FONT_SIZES.md,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  settingDescription: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  settingValue: {
    fontSize: FONT_SIZES.md,
    color: COLORS.primary,
    fontWeight: '500',
  },
  divider: {
    height: 1,
    backgroundColor: COLORS.border,
    marginLeft: SPACING.lg,
  },
  chevron: {
    fontSize: 20,
    color: COLORS.textTertiary,
  },
  storageInfo: {
    padding: SPACING.lg,
    paddingBottom: SPACING.sm,
  },
  storageLabel: {
    fontSize: FONT_SIZES.md,
    fontWeight: '500',
    color: COLORS.textPrimary,
  },
  storageValue: {
    fontSize: FONT_SIZES.sm,
    color: COLORS.textSecondary,
    marginTop: 2,
  },
  storageBar: {
    height: 6,
    backgroundColor: COLORS.border,
    marginHorizontal: SPACING.lg,
    marginBottom: SPACING.lg,
    borderRadius: RADIUS.full,
    overflow: 'hidden',
  },
  storageProgress: {
    height: '100%',
    backgroundColor: COLORS.primary,
    borderRadius: RADIUS.full,
  },
  resetButton: {
    backgroundColor: COLORS.surface,
    borderRadius: RADIUS.lg,
    padding: SPACING.lg,
    alignItems: 'center',
    borderWidth: 1,
    borderColor: COLORS.error,
  },
  resetButtonText: {
    fontSize: FONT_SIZES.md,
    fontWeight: '500',
    color: COLORS.error,
  },
});
