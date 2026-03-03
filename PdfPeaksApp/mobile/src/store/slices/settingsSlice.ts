/**
 * Settings Slice - Redux State Management
 * 
 * Handles user preferences and app settings
 */

import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { UserPreferences, StorageInfo } from '../../types';

interface SettingsState {
  preferences: UserPreferences;
  storageInfo: StorageInfo;
  isFirstLaunch: boolean;
  appVersion: string;
  lastSyncTime: Date | null;
}

const initialState: SettingsState = {
  preferences: {
    defaultExportFormat: 'pdf',
    defaultPageSize: 'a4',
    autoEnhanceOnScan: true,
    cloudSyncEnabled: false,
    defaultOCRLanguage: 'eng',
    biometricAuthEnabled: false,
    darkModeEnabled: false,
  },
  storageInfo: {
    used: 0,
    total: 0,
    percentUsed: 0,
  },
  isFirstLaunch: true,
  appVersion: '1.0.0',
  lastSyncTime: null,
};

const settingsSlice = createSlice({
  name: 'settings',
  initialState,
  reducers: {
    updatePreferences: (state, action: PayloadAction<Partial<UserPreferences>>) => {
      state.preferences = { ...state.preferences, ...action.payload };
    },
    setDefaultExportFormat: (state, action: PayloadAction<'pdf' | 'jpeg' | 'png'>) => {
      state.preferences.defaultExportFormat = action.payload;
    },
    setDefaultPageSize: (state, action: PayloadAction<'letter' | 'a4' | 'legal'>) => {
      state.preferences.defaultPageSize = action.payload;
    },
    toggleAutoEnhance: (state) => {
      state.preferences.autoEnhanceOnScan = !state.preferences.autoEnhanceOnScan;
    },
    toggleCloudSync: (state) => {
      state.preferences.cloudSyncEnabled = !state.preferences.cloudSyncEnabled;
    },
    setDefaultOCRLanguage: (state, action: PayloadAction<string>) => {
      state.preferences.defaultOCRLanguage = action.payload;
    },
    toggleBiometricAuth: (state) => {
      state.preferences.biometricAuthEnabled = !state.preferences.biometricAuthEnabled;
    },
    toggleDarkMode: (state) => {
      state.preferences.darkModeEnabled = !state.preferences.darkModeEnabled;
    },
    setStorageInfo: (state, action: PayloadAction<StorageInfo>) => {
      state.storageInfo = action.payload;
    },
    setFirstLaunch: (state, action: PayloadAction<boolean>) => {
      state.isFirstLaunch = action.payload;
    },
    setLastSyncTime: (state, action: PayloadAction<Date>) => {
      state.lastSyncTime = action.payload;
    },
    resetSettings: (state) => {
      state.preferences = initialState.preferences;
    },
  },
});

export const {
  updatePreferences,
  setDefaultExportFormat,
  setDefaultPageSize,
  toggleAutoEnhance,
  toggleCloudSync,
  setDefaultOCRLanguage,
  toggleBiometricAuth,
  toggleDarkMode,
  setStorageInfo,
  setFirstLaunch,
  setLastSyncTime,
  resetSettings,
} = settingsSlice.actions;

export default settingsSlice.reducer;
