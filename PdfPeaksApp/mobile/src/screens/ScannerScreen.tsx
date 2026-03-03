/**
 * Scanner Screen - Camera-based Document Scanning
 * 
 * Full-screen camera interface for document capture
 */

import React, { useState, useCallback, useRef, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Image,
  Alert,
  Dimensions,
} from 'react-native';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { useDispatch, useSelector } from 'react-redux';

import { COLORS, SPACING, RADIUS, FONT_SIZES, SHADOWS } from '../constants/colors';
import { RootStackParamList, Page } from '../types';
import { RootState } from '../store';
import { startSession, endSession, addPage, toggleFlash, setCaptureMode } from '../store/slices/scanSlice';

type NavigationProp = NativeStackNavigationProp<RootStackParamList>;
type ScannerRouteProp = RouteProp<RootStackParamList, 'Scanner'>;

const { width, height } = Dimensions.get('window');

export const ScannerScreen: React.FC = () => {
  const navigation = useNavigation<NavigationProp>();
  const route = useRoute<ScannerRouteProp>();
  const dispatch = useDispatch();
  
  const { mode } = route.params || { mode: 'single' };
  
  const { flashEnabled, captureMode, currentSession } = useSelector((state: RootState) => state.scan);
  const preferences = useSelector((state: RootState) => state.settings.preferences);
  
  const [capturedImage, setCapturedImage] = useState<string | null>(null);
  const [isCapturing, setIsCapturing] = useState(false);
  
  // Initialize scan session
  useEffect(() => {
    dispatch(startSession({ mode }));
    
    return () => {
      dispatch(endSession());
    };
  }, [dispatch, mode]);
  
  // Handle capture
  const handleCapture = useCallback(() => {
    if (isCapturing) return;
    
    setIsCapturing(true);
    
    // Simulate capture - in real app, use react-native-vision-camera
    setTimeout(() => {
      // Create a mock page
      const newPage: Page = {
        id: Date.now().toString(),
        documentId: currentSession?.id || '',
        order: (currentSession?.pages.length || 0) + 1,
        imageUrl: 'file://mock_capture.jpg',
        thumbnailUrl: 'file://mock_thumb.jpg',
        width: 1080,
        height: 1920,
        fileSize: 0,
        enhancements: {
          brightness: 0,
          contrast: 0,
          saturation: 0,
          sharpness: 0,
          rotation: 0,
          filter: preferences.autoEnhanceOnScan ? 'document' : 'none',
        },
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      
      dispatch(addPage(newPage));
      setCapturedImage('file://mock_capture.jpg');
      setIsCapturing(false);
      
      // Auto-apply enhancements if enabled
      if (preferences.autoEnhanceOnScan) {
        // Enhancement would be applied here
      }
    }, 500);
  }, [isCapturing, currentSession, preferences.autoEnhanceOnScan, dispatch]);
  
  // Handle retake
  const handleRetake = () => {
    setCapturedImage(null);
  };
  
  // Handle continue to editor
  const handleContinue = () => {
    if (currentSession?.pages.length) {
      navigation.navigate('Editor', { pageId: currentSession.pages[0].id });
    }
  };
  
  // Handle close
  const handleClose = () => {
    Alert.alert(
      'Discard Scan?',
      'Are you sure you want to discard this scan?',
      [
        { text: 'Cancel', style: 'cancel' },
        { text: 'Discard', style: 'destructive', onPress: () => navigation.goBack() },
      ]
    );
  };
  
  // Toggle flash
  const handleToggleFlash = () => {
    dispatch(toggleFlash());
  };
  
  // Toggle capture mode
  const handleToggleMode = () => {
    dispatch(setCaptureMode(captureMode === 'auto' ? 'manual' : 'auto'));
  };
  
  return (
    <View style={styles.container}>
      {/* Camera Preview Area */}
      <View style={styles.cameraPreview}>
        {capturedImage ? (
          <View style={styles.previewContainer}>
            <Image source={{ uri: capturedImage }} style={styles.previewImage} />
            <View style={styles.previewOverlay}>
              <Text style={styles.previewText}>Preview</Text>
            </View>
          </View>
        ) : (
          <View style={styles.cameraPlaceholder}>
            <Text style={styles.cameraPlaceholderText}>📷</Text>
            <Text style={styles.cameraPlaceholderLabel}>Camera Preview</Text>
            <Text style={styles.cameraPlaceholderSubtext}>
              Point at document and tap capture
            </Text>
          </View>
        )}
      </View>
      
      {/* Top Controls */}
      <View style={styles.topControls}>
        <TouchableOpacity style={styles.controlButton} onPress={handleClose}>
          <Text style={styles.controlIcon}>✕</Text>
        </TouchableOpacity>
        
        <View style={styles.modeIndicator}>
          <Text style={styles.modeText}>
            {mode === 'batch' ? 'Batch Mode' : 'Single Mode'}
          </Text>
        </View>
        
        <TouchableOpacity style={styles.controlButton} onPress={handleToggleFlash}>
          <Text style={styles.controlIcon}>{flashEnabled ? '⚡' : '⚡'}</Text>
        </TouchableOpacity>
      </View>
      
      {/* Document Frame Guide */}
      <View style={styles.frameGuide}>
        <View style={[styles.corner, styles.topLeft]} />
        <View style={[styles.corner, styles.topRight]} />
        <View style={[styles.corner, styles.bottomLeft]} />
        <View style={[styles.corner, styles.bottomRight]} />
      </View>
      
      {/* Bottom Controls */}
      <View style={styles.bottomControls}>
        {/* Gallery Button */}
        <TouchableOpacity style={styles.sideButton}>
          <Text style={styles.sideButtonIcon}>🖼️</Text>
        </TouchableOpacity>
        
        {/* Capture Button */}
        <TouchableOpacity
          style={[styles.captureButton, isCapturing && styles.captureButtonActive]}
          onPress={capturedImage ? handleRetake : handleCapture}
          disabled={isCapturing}
        >
          <View style={styles.captureButtonInner}>
            {isCapturing ? (
              <Text style={styles.captureButtonText}>...</Text>
            ) : (
              <Text style={styles.captureButtonIcon}>{capturedImage ? '↩️' : '⭕'}</Text>
            )}
          </View>
        </TouchableOpacity>
        
        {/* Done/Continue Button */}
        <TouchableOpacity
          style={[styles.sideButton, !capturedImage && styles.sideButtonDisabled]}
          onPress={handleContinue}
          disabled={!capturedImage}
        >
          <Text style={styles.sideButtonIcon}>{capturedImage ? '➡️' : '⏸️'}</Text>
        </TouchableOpacity>
      </View>
      
      {/* Mode Toggle */}
      <TouchableOpacity style={styles.modeToggle} onPress={handleToggleMode}>
        <Text style={styles.modeToggleText}>
          {captureMode === 'auto' ? 'Auto' : 'Manual'}
        </Text>
      </TouchableOpacity>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.black,
  },
  cameraPreview: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  cameraPlaceholder: {
    alignItems: 'center',
  },
  cameraPlaceholderText: {
    fontSize: 64,
    marginBottom: SPACING.md,
  },
  cameraPlaceholderLabel: {
    fontSize: FONT_SIZES.lg,
    color: COLORS.white,
    fontWeight: '600',
    marginBottom: SPACING.xs,
  },
  cameraPlaceholderSubtext: {
    fontSize: FONT_SIZES.md,
    color: COLORS.textTertiary,
    textAlign: 'center',
  },
  previewContainer: {
    flex: 1,
    width: '100%',
    height: '100%',
  },
  previewImage: {
    flex: 1,
    resizeMode: 'contain',
  },
  previewOverlay: {
    position: 'absolute',
    top: SPACING.lg,
    left: SPACING.lg,
    backgroundColor: COLORS.overlay,
    paddingHorizontal: SPACING.md,
    paddingVertical: SPACING.xs,
    borderRadius: RADIUS.md,
  },
  previewText: {
    color: COLORS.white,
    fontSize: FONT_SIZES.sm,
    fontWeight: '500',
  },
  topControls: {
    position: 'absolute',
    top: 50,
    left: 0,
    right: 0,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: SPACING.lg,
  },
  controlButton: {
    width: 44,
    height: 44,
    borderRadius: RADIUS.full,
    backgroundColor: COLORS.overlay,
    justifyContent: 'center',
    alignItems: 'center',
  },
  controlIcon: {
    fontSize: 18,
    color: COLORS.white,
  },
  modeIndicator: {
    backgroundColor: COLORS.overlay,
    paddingHorizontal: SPACING.md,
    paddingVertical: SPACING.xs,
    borderRadius: RADIUS.full,
  },
  modeText: {
    color: COLORS.white,
    fontSize: FONT_SIZES.sm,
    fontWeight: '500',
  },
  frameGuide: {
    position: 'absolute',
    top: '15%',
    left: '5%',
    right: '5%',
    bottom: '25%',
  },
  corner: {
    position: 'absolute',
    width: 30,
    height: 30,
    borderColor: COLORS.white,
  },
  topLeft: {
    top: 0,
    left: 0,
    borderTopWidth: 3,
    borderLeftWidth: 3,
  },
  topRight: {
    top: 0,
    right: 0,
    borderTopWidth: 3,
    borderRightWidth: 3,
  },
  bottomLeft: {
    bottom: 0,
    left: 0,
    borderBottomWidth: 3,
    borderLeftWidth: 3,
  },
  bottomRight: {
    bottom: 0,
    right: 0,
    borderBottomWidth: 3,
    borderRightWidth: 3,
  },
  bottomControls: {
    position: 'absolute',
    bottom: 50,
    left: 0,
    right: 0,
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
    paddingHorizontal: SPACING.xl,
  },
  sideButton: {
    width: 56,
    height: 56,
    borderRadius: RADIUS.lg,
    backgroundColor: COLORS.overlay,
    justifyContent: 'center',
    alignItems: 'center',
  },
  sideButtonDisabled: {
    opacity: 0.4,
  },
  sideButtonIcon: {
    fontSize: 24,
  },
  captureButton: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: COLORS.white,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 4,
    borderColor: COLORS.primary,
  },
  captureButtonActive: {
    opacity: 0.7,
  },
  captureButtonInner: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: COLORS.primary,
    justifyContent: 'center',
    alignItems: 'center',
  },
  captureButtonIcon: {
    fontSize: 28,
  },
  captureButtonText: {
    fontSize: FONT_SIZES.xl,
    color: COLORS.white,
    fontWeight: '700',
  },
  modeToggle: {
    position: 'absolute',
    bottom: 140,
    alignSelf: 'center',
    backgroundColor: COLORS.overlay,
    paddingHorizontal: SPACING.lg,
    paddingVertical: SPACING.sm,
    borderRadius: RADIUS.full,
  },
  modeToggleText: {
    color: COLORS.white,
    fontSize: FONT_SIZES.sm,
    fontWeight: '500',
  },
});
