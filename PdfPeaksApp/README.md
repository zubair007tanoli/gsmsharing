# PDFPeaksApp

A professional document scanning and PDF management application combining features from pdfpeaks and camscanner.com.

## Features

### Scanning Features (from CamScanner)
- 📷 Camera-based document scanning (single & batch modes)
- ✨ Auto-capture with edge detection
- ⚡ Flash control
- 🎨 Image filters (Document, Photo, Grayscale, B&W, Magic Color)
- 📐 Crop and rotate functionality
- 🖼️ Gallery import

### PDF Management (from pdfpeaks)
- 📄 PDF viewing with zoom and navigation
- 📁 Document organization (folders, tags, favorites)
- 🔍 Search functionality
- 📤 Export to multiple formats (PDF, JPG, PNG, DOCX)

### Additional Features
- 🔤 OCR text recognition support
- ☁️ Cloud sync ready architecture
- 🌙 Dark mode support
- 🔐 Biometric authentication ready

## Prerequisites

Before building, ensure you have the following installed:

1. **Node.js** (v18 or higher)
   - Download from: https://nodejs.org/
   - Or install via: `nvm install 18`

2. **npm** or **yarn** (comes with Node.js)

3. **Java Development Kit (JDK)** (for Android)
   - Download JDK 17 from: https://adoptium.net/
   - Set JAVA_HOME environment variable

4. **Android Studio** (for Android development)
   - Download from: https://developer.android.com/studio
   - Install Android SDK with API level 34

5. **Xcode** (for iOS development - macOS only)
   - Download from Mac App Store

## Quick Start

### 1. Install Dependencies

```bash
# Navigate to mobile directory
cd pdfpeaksapp/mobile

# Install dependencies
npm install

# Or with yarn
yarn install
```

### 2. Run on Android

```bash
# Start Metro bundler
npm start

# In another terminal, run on Android
npm run android
```

### 3. Run on iOS (macOS only)

```bash
# Install iOS pods
cd ios && pod install && cd ..

# Start Metro bundler
npm start

# In another terminal, run on iOS
npm run ios
```

## Project Structure

```
pdfpeaksapp/
├── SPEC.md                    # Detailed specification
├── README.md                   # This file
└── mobile/
    ├── package.json            # Dependencies
    ├── App.tsx                 # Main entry point
    ├── src/
    │   ├── constants/          # Colors, theme
    │   ├── types/              # TypeScript types
    │   ├── store/              # Redux state management
    │   ├── navigation/         # React Navigation
    │   ├── screens/            # All app screens
    │   └── hooks/              # Custom hooks
    ├── android/                # Android native code
    └── ios/                    # iOS native code
```

## Tech Stack

- **Framework**: React Native 0.74
- **Language**: TypeScript
- **State Management**: Redux Toolkit
- **Navigation**: React Navigation (Stack + Tabs)
- **Camera**: react-native-vision-camera
- **PDF**: react-native-pdf
- **Image Processing**: react-native-image-crop-picker

## Building for Production

### Android APK

```bash
cd pdfpeaksapp/mobile

# Build debug APK
cd android && ./gradlew assembleDebug

# APK will be at: android/app/build/outputs/apk/debug/app-debug.apk

# Build release APK
cd android && ./gradlew assembleRelease
```

### iOS (for App Store)

```bash
cd pdfpeaksapp/mobile/ios

# Open in Xcode
open PDFPeaksApp.xcworkspace

# Or build from command line
xcodebuild -workspace PDFPeaksApp.xcworkspace \
  -scheme PDFPeaksApp \
  -configuration Release \
  -archivePath PDFPeaksApp.xcarchive \
  archive
```

## Configuration

### Android Permissions

The following permissions are required in `android/app/src/main/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-feature android:name="android.hardware.camera" android:required="false" />
<uses-feature android:name="android.hardware.camera.autofocus" android:required="false" />
```

### iOS Permissions

Add to `ios/PDFPeaksApp/Info.plist`:

```xml
<key>NSCameraUsageDescription</key>
<string>PDFPeaksApp needs camera access to scan documents</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>PDFPeaksApp needs photo library access to import images</string>
<key>NSPhotoLibraryAddUsageDescription</key>
<string>PDFPeaksApp needs to save scanned documents</string>
```

## Troubleshooting

### Metro Bundler Issues
```bash
# Clear Metro cache
npx react-native start --reset-cache

# Or
npm start -- --reset-cache
```

### Android Build Issues
```bash
# Clean gradle cache
cd android && ./gradlew clean

# Rebuild
cd .. && npx react-native run-android
```

### iOS Build Issues
```bash
# Remove Pods and reinstall
cd ios && rm -rf Pods Podfile.lock
pod install
```

## License

MIT License - See LICENSE file for details.
