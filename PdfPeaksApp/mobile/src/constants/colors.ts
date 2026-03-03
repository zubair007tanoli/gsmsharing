/**
 * Color Constants for PDFPeaksApp
 * 
 * Defines the complete color palette following the design specification
 */

// Primary Colors
export const COLORS = {
  // Primary
  primary: '#2563EB',
  primaryDark: '#1D4ED8',
  primaryLight: '#3B82F6',
  
  // Secondary
  secondary: '#10B981',
  secondaryDark: '#059669',
  secondaryLight: '#34D399',
  
  // Accent
  accent: '#F59E0B',
  
  // Background & Surface
  background: '#F8FAFC',
  surface: '#FFFFFF',
  surfaceElevated: '#FFFFFF',
  
  // Text Colors
  textPrimary: '#1E293B',
  textSecondary: '#64748B',
  textTertiary: '#94A3B8',
  
  // Semantic Colors
  error: '#EF4444',
  warning: '#F59E0B',
  success: '#10B981',
  info: '#3B82F6',
  
  // Borders
  border: '#E2E8F0',
  borderFocus: '#2563EB',
  
  // Overlay
  overlay: 'rgba(0, 0, 0, 0.5)',
  overlayLight: 'rgba(0, 0, 0, 0.3)',
  
  // White & Black
  white: '#FFFFFF',
  black: '#000000',
  
  // Transparent
  transparent: 'transparent',
} as const;

// Gradient configurations
export const GRADIENTS = {
  primary: [COLORS.primary, COLORS.primaryDark],
  secondary: [COLORS.secondary, COLORS.secondaryDark],
  accent: [COLORS.accent, '#D97706'],
} as const;

// Shadows
export const SHADOWS = {
  small: {
    shadowColor: COLORS.black,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 4,
    elevation: 2,
  },
  medium: {
    shadowColor: COLORS.black,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
    elevation: 4,
  },
  large: {
    shadowColor: COLORS.black,
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.12,
    shadowRadius: 16,
    elevation: 8,
  },
} as const;

// Border Radius
export const RADIUS = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 24,
  full: 9999,
} as const;

// Spacing
export const SPACING = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 24,
  xxl: 32,
  xxxl: 48,
} as const;

// Typography Sizes
export const FONT_SIZES = {
  xs: 10,
  sm: 12,
  md: 14,
  lg: 16,
  xl: 18,
  xxl: 20,
  xxxl: 24,
  display: 32,
} as const;

// Type exports
export type ColorKey = keyof typeof COLORS;
export type RadiusKey = keyof typeof RADIUS;
export type SpacingKey = keyof typeof SPACING;
export type FontSizeKey = keyof typeof FONT_SIZES;
