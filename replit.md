# PdfPeaks Mobile App

A feature-rich PDF scanner and document management mobile app built with Expo React Native. Competitor to pdfpeaks.com and CamScanner.

## Architecture

- **Frontend**: Expo Router with file-based routing, React Native
- **Backend**: Express.js (port 5000) - serves API and landing page
- **State Management**: React Context (DocumentsContext) + AsyncStorage for persistence
- **Styling**: React Native StyleSheet with dark/light theme support
- **Fonts**: Inter (via @expo-google-fonts/inter)

## Key Features

- **Document Scanner** - Scan via camera or import from gallery/files
- **PDF Document Library** - Browse, organize, search, sort documents
- **Folder Organization** - Create color-coded folders for document management
- **20+ PDF Tools** - Merge, split, compress, convert, sign, OCR, watermark, etc.
- **Home Dashboard** - Stats, recent docs, quick actions
- **Document Viewer** - Preview documents with info tab and quick actions
- **Dark/Light Mode** - Full theme support

## Project Structure

```
app/
  _layout.tsx           # Root layout with fonts, providers
  (tabs)/
    _layout.tsx         # Tab navigation (NativeTabs + liquid glass)
    index.tsx           # Home/Dashboard screen
    scan.tsx            # Document scanner screen
    documents.tsx       # Document library screen
    tools.tsx           # PDF tools screen
  document/
    [id].tsx            # Document viewer screen
context/
  DocumentsContext.tsx  # Global state for documents & folders
constants/
  colors.ts             # Theme colors (blue accent, navy dark mode)
server/
  index.ts              # Express server
  routes.ts             # API routes
```

## Color Theme

- Primary: `#2563EB` (blue)
- Accent: `#EF4444` (red for PDF)
- Dark background: `#0B1628` (deep navy)
- Light background: `#F8FAFC`

## Running the App

- Backend: `npm run server:dev` (port 5000)
- Frontend: `npm run expo:dev` (port 8081)

## Notes

- Uses AsyncStorage for document persistence (with sample data on first load)
- Camera and gallery permissions handled with expo-image-picker
- PDF import via expo-document-picker
- Sample documents pre-loaded for demonstration
