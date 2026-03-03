/**
 * App Navigator - Main Navigation Configuration
 * 
 * Defines the navigation structure for PDFPeaksApp
 */

import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { View, Text, StyleSheet } from 'react-native';
import { COLORS, FONT_SIZES } from '../constants/colors';

// Screens
import { HomeScreen } from '../screens/HomeScreen';
import { DocumentsScreen } from '../screens/DocumentsScreen';
import { ScanScreen } from '../screens/ScanScreen';
import { FavoritesScreen } from '../screens/FavoritesScreen';
import { SettingsScreen } from '../screens/SettingsScreen';
import { ScannerScreen } from '../screens/ScannerScreen';
import { EditorScreen } from '../screens/EditorScreen';
import { PDFViewerScreen } from '../screens/PDFViewerScreen';
import { DocumentDetailScreen } from '../screens/DocumentDetailScreen';
import { FolderDetailScreen } from '../screens/FolderDetailScreen';

// Types
import { RootStackParamList, MainTabParamList } from '../types';

// Icons (using simple text for now - would use react-native-vector-icons in production)
const TabIcon: React.FC<{ name: string; focused: boolean }> = ({ name, focused }) => (
  <View style={styles.iconContainer}>
    <Text style={[styles.iconText, focused && styles.iconTextFocused]}>{name}</Text>
  </View>
);

const Stack = createNativeStackNavigator<RootStackParamList>();
const Tab = createBottomTabNavigator<MainTabParamList>();

/**
 * Main Tab Navigator
 * Bottom tab navigation for primary app sections
 */
const MainTabs: React.FC = () => {
  return (
    <Tab.Navigator
      screenOptions={{
        tabBarActiveTintColor: COLORS.primary,
        tabBarInactiveTintColor: COLORS.textTertiary,
        tabBarStyle: styles.tabBar,
        tabBarLabelStyle: styles.tabBarLabel,
        headerShown: false,
      }}
    >
      <Tab.Screen
        name="Home"
        component={HomeScreen}
        options={{
          tabBarLabel: 'Home',
          tabBarIcon: ({ focused }) => <TabIcon name="🏠" focused={focused} />,
        }}
      />
      <Tab.Screen
        name="Documents"
        component={DocumentsScreen}
        options={{
          tabBarLabel: 'Files',
          tabBarIcon: ({ focused }) => <TabIcon name="📁" focused={focused} />,
        }}
      />
      <Tab.Screen
        name="Scan"
        component={ScanScreen}
        options={{
          tabBarLabel: 'Scan',
          tabBarIcon: ({ focused }) => <TabIcon name="📷" focused={focused} />,
        }}
      />
      <Tab.Screen
        name="Favorites"
        component={FavoritesScreen}
        options={{
          tabBarLabel: 'Starred',
          tabBarIcon: ({ focused }) => <TabIcon name="⭐" focused={focused} />,
        }}
      />
      <Tab.Screen
        name="Settings"
        component={SettingsScreen}
        options={{
          tabBarLabel: 'Settings',
          tabBarIcon: ({ focused }) => <TabIcon name="⚙️" focused={focused} />,
        }}
      />
    </Tab.Navigator>
  );
};

/**
 * Main App Navigator
 * Root navigation stack
 */
export const AppNavigator: React.FC = () => {
  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: COLORS.surface },
        headerTintColor: COLORS.textPrimary,
        headerTitleStyle: { fontWeight: '600' },
        headerShadowVisible: false,
        contentStyle: { backgroundColor: COLORS.background },
      }}
    >
      <Stack.Screen
        name="Main"
        component={MainTabs}
        options={{ headerShown: false }}
      />
      <Stack.Screen
        name="Scanner"
        component={ScannerScreen}
        options={{
          headerShown: false,
          presentation: 'fullScreenModal',
          animation: 'slide_from_bottom',
        }}
      />
      <Stack.Screen
        name="Editor"
        component={EditorScreen}
        options={{
          title: 'Edit',
          presentation: 'modal',
          animation: 'slide_from_right',
        }}
      />
      <Stack.Screen
        name="PDFViewer"
        component={PDFViewerScreen}
        options={{
          title: 'PDF Viewer',
          animation: 'slide_from_right',
        }}
      />
      <Stack.Screen
        name="DocumentDetail"
        component={DocumentDetailScreen}
        options={{
          title: 'Document',
          animation: 'slide_from_right',
        }}
      />
      <Stack.Screen
        name="FolderDetail"
        component={FolderDetailScreen}
        options={{
          title: 'Folder',
          animation: 'slide_from_right',
        }}
      />
    </Stack.Navigator>
  );
};

const styles = StyleSheet.create({
  tabBar: {
    backgroundColor: COLORS.surface,
    borderTopColor: COLORS.border,
    borderTopWidth: 1,
    height: 60,
    paddingBottom: 8,
    paddingTop: 8,
  },
  tabBarLabel: {
    fontSize: FONT_SIZES.xs,
    fontWeight: '500',
  },
  iconContainer: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  iconText: {
    fontSize: 20,
    opacity: 0.6,
  },
  iconTextFocused: {
    opacity: 1,
  },
});
