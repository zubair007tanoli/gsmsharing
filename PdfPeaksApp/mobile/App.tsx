/**
 * PDFPeaksApp - Professional Document Scanner & PDF Manager
 * 
 * A comprehensive document scanning application combining features
 * from pdfpeaks and camscanner.com
 * 
 * @version 1.0.0
 * @author PDFPeaksApp Team
 */

import React from 'react';
import { StatusBar } from 'expo-status-bar';
import { StyleSheet, View, Text } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Provider } from 'react-redux';
import { GestureHandlerRootView } from 'react-native-gesture-handler';

import { store } from './src/store';
import { AppNavigator } from './src/navigation/AppNavigator';
import { COLORS } from './src/constants/colors';

/**
 * Main App Component
 * 
 * Initializes all providers and navigation structure
 */
const App: React.FC = () => {
  return (
    <GestureHandlerRootView style={styles.container}>
      <Provider store={store}>
        <SafeAreaProvider>
          <NavigationContainer>
            <StatusBar style="dark" backgroundColor={COLORS.background} />
            <AppNavigator />
          </NavigationContainer>
        </SafeAreaProvider>
      </Provider>
    </GestureHandlerRootView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
});

export default App;
